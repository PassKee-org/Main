@Library('common')
import com.shared.jenkins.docker.DockerHelper
import com.shared.jenkins.docker.DockerContainer

def effectiveEnvironment = params.ENVIRONMENT ?: 'Development'
def environmentKey = effectiveEnvironment.toLowerCase()
def containerSharedDir = "/mnt/local_share/docker_images/timevic"
def imageName = "latest"
def imageWebTmpName = "${containerSharedDir}/${environmentKey}_web_latest"
def imageCommonTmpName = "${containerSharedDir}/${environmentKey}_common_latest"
def currentBranchName = ''
def shouldRunDeployment = true

def dockerHelper = new DockerHelper(this)
public Map<String, String> envVariables = new HashMap<String, String>()

def mainContainer = new DockerContainer(
    name: "timevic-main-${environmentKey}",
    dockerFile: 'devops/publish_native/common/Dockerfile',
);

def migrationContainer = new DockerContainer(
    name: "timevic-main-${environmentKey}",
    dockerFile: 'devops/publish_native/common/Dockerfile',
    isRunAlways: false,
    isRunInBackground: false,
);
def webAppContainer = new DockerContainer(
    name: "timevic-web-${environmentKey}",
    dockerFile: 'devops/publish_native/web/Dockerfile',
);

def repositoryUrl = scm.userRemoteConfigs[0].url;
def gitCredentials="gitea-jenkins-ssh-key"

properties([
    pipelineTriggers([
        githubPush()
    ]),
    parameters([
        // https://plugins.jenkins.io/git-parameter/
        gitParameter (name: 'GIT_TAG', type: 'PT_TAG', sortMode: 'DESCENDING_SMART', selectedValue: 'NONE', defaultValue: 'main'),
        string (name: 'NEW_VERSION', defaultValue: '', description: 'Provide version to create GIT tag'),
        choice(name: 'ENVIRONMENT', choices: ['Development', 'Production'], description: 'Select environment to deploy'),
    ]),
    disableConcurrentBuilds()
])

node('build-node') {

    stage('Show deployment parameters') {
        echo "Repository: ${repositoryUrl}"
        echo "Requested environment: ${params.ENVIRONMENT}"
        echo "Tag: ${params.GIT_TAG}"
    }

    if (!params.GIT_TAG?.trim())
    {
        stage('Switch to GIT tag') {
            git branch: "${params.BRANCH}", url: repositoryUrl
        }    
    }

    stage('Checkout') {
        cleanWs()
        sh """
            git config --global http.postBuffer 2048M
            git config --global http.maxRequestBuffer 1024M
            git config --global core.compression 0
        """
        checkout scm
    }

    stage('Resolve trigger context') {
        currentBranchName = resolveBranchName()
        def isAutoBuildForPush = isAutoTriggeredPushBuild()

        if (isAutoBuildForPush && currentBranchName != 'main') {
            shouldRunDeployment = false
            currentBuild.result = 'NOT_BUILT'
            echo "Skipping auto deployment for branch '${currentBranchName}'. Only 'main' is deployed automatically."
        }

        if (isAutoBuildForPush && currentBranchName == 'main') {
            effectiveEnvironment = 'Development'
        }

        environmentKey = effectiveEnvironment.toLowerCase()
        imageWebTmpName = "${containerSharedDir}/${environmentKey}_web_latest"
        imageCommonTmpName = "${containerSharedDir}/${environmentKey}_common_latest"
        mainContainer.name = "timevic-main-${environmentKey}"
        migrationContainer.name = "timevic-main-${environmentKey}"
        webAppContainer.name = "timevic-web-${environmentKey}"

        echo "Branch: ${currentBranchName}"
        echo "Auto-triggered push build: ${isAutoBuildForPush}"
        echo "Effective environment: ${effectiveEnvironment}"
    }

    if (!shouldRunDeployment) {
        stage('Skip deployment') {
            echo "Deployment pipeline skipped."
        }
        return
    }

    stage('Set environment vars') {
        // Redis
        envVariables.put('Redis__Server', '10.10.0.2:6379')
        
        envVariables.put('Serilog__IsSendEmailIfError', 'false')
        envVariables.put('Serilog__MinimumLevel__Default', 'Debug')
        
        mainContainer.buildVariables.put('ENVIRONMENT', effectiveEnvironment)
        envVariables.put('ASPNETCORE_ENVIRONMENT', effectiveEnvironment)
        
        webAppContainer.buildVariables.put('ENVIRONMENT', effectiveEnvironment)
        webAppContainer.envVariables.put('ASPNETCORE_ENVIRONMENT', effectiveEnvironment)

        // GrayLog
        envVariables.put('App__Logging__GrayLog__Host', '192.168.88.30')
        envVariables.put('App__Logging__GrayLog__Port', '12201')

        def dbName = ''
        def dbPort = ''
        def dbHost = ''
        if (effectiveEnvironment == 'Production')
        {
            envVariables.put('App__FrontendUrl', 'https://timevic.com')
            dbName = 'timevic'
            dbPort = '5432'
            dbHost = '192.168.88.41'
        }
        else if (effectiveEnvironment == 'Development')
        {
            envVariables.put('App__FrontendUrl', 'https://dev.timevic.com')
            dbName = 'timevic_dev'
            dbPort = '5432'
            dbHost = '192.168.88.42'
        }

        // Common
        withCredentials([
                usernamePassword(credentialsId: "timevic_production_smtp_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('Smtp__UserName', USER_NAME)
            envVariables.put('Smtp__Password', PASSWORD)
        }
        withCredentials([string(credentialsId: "timevic_production_recaptcha_secret", variable: 'AUTH_SECRET')]) {
            envVariables.put('ReCaptcha__Secret', AUTH_SECRET)
        }

        withCredentials([
                usernamePassword(credentialsId: "timevic_${environmentKey}_db_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put(
                'ConnectionStrings__DefaultConnection',
                "User ID=${USER_NAME};Password=${PASSWORD};Host=${dbHost};Port=${dbPort};Database=${dbName};Pooling=true;"
            )
        }
        withCredentials([string(credentialsId: "timevic_${environmentKey}_user_jwt", variable: 'AUTH_SECRET')]) {
            envVariables.put('App__Auth__SymmetricSecurityKey', AUTH_SECRET)
        }
        
        // withCredentials([string(credentialsId: "timevic_${environmentKey}_google__storage_project_id", variable: 'AUTH_SECRET')]) {
        //     envVariables.put('Google__Storage__ProjectId', AUTH_SECRET)
        // }

        // withCredentials([string(credentialsId: "timevic_${environmentKey}_google__storage_bucket_name", variable: 'AUTH_SECRET')]) {
        //     envVariables.put('Google__Storage__BucketName', AUTH_SECRET)
        // }
        
        withCredentials([
            usernamePassword(credentialsId: "timevic_${environmentKey}_aws_s3_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('AWS__S3__AccessKey', USER_NAME)
            envVariables.put('AWS__S3__SecretKey', PASSWORD)
        }
        envVariables.put('AWS__S3__BucketName', "timevic-${environmentKey}")

        withCredentials([
            usernamePassword(credentialsId: "timevic_${environmentKey}_garage_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('Garage__AccessKey', USER_NAME)
            envVariables.put('Garage__SecretKey', PASSWORD)
        }
    }

    stage('Build web image') {
        dockerHelper.buildAndSave(webAppContainer, imageWebTmpName)
    }

    stage('Build main image') {
        withCredentials([file(credentialsId: 'timevic_production_gcloud_credentials', variable: 'FILE')]) {
            sh 'cp $FILE .credentials/google.json'
        }
        withCredentials([file(credentialsId: 'timevic_production_firebase_credentials', variable: 'FILE')]) {
            sh 'cp $FILE .credentials/firebase-credentials.json'
        }
        dockerHelper.buildAndSave(mainContainer, imageCommonTmpName)
    }

    if (params.NEW_VERSION) {
        stage('Create GIT tag') {
            def (VER_MAJOR, VER_MINOR, VER_PATCH, VER_BUILD) = params.NEW_VERSION.tokenize('.').collect { it.toInteger() }
            env.VERSION_INCREMENT = VER_MAJOR + "." + VER_MINOR + "." + VER_PATCH + "." + VER_BUILD

            withCredentials([sshUserPrivateKey(credentialsId: gitCredentials, keyFileVariable: 'key')]) {
                sh '''
                    git config core.sshCommand 'ssh -i ${key}'
                    git config user.email "lampego@gmail.com"
                    git config user.name "lampego"
                    git tag "${VERSION_INCREMENT}"
                    git push --tags
                '''
            }
        }
    }

    stage("Clean workspace") {
        cleanWs()
    }
    
    stage('CleanUp Docker') {
        sh 'docker system prune -f'
    }
}

if (shouldRunDeployment) {
node('web-node') {

    stage('Load container') {
        dockerHelper.loadFromFile(imageCommonTmpName)
        dockerHelper.loadFromFile(imageWebTmpName)
    }

    stage('Stop containers') {
        dockerHelper.stopContainer(webAppContainer)

        mainContainer.tagName = "timevic-api-${environmentKey}";
        dockerHelper.stopContainer(mainContainer)

        mainContainer.tagName = "timevic-worker-${environmentKey}";
        dockerHelper.stopContainer(mainContainer)
    }

    stage('Run migrations') {
        dockerHelper.stopContainer(migrationContainer)

        migrationContainer.envVariables = envVariables.clone()
        migrationContainer.envVariables.put('PROJECT_DIR', 'PassKee.Migrations')
        dockerHelper.runContainer(migrationContainer)
    }

    stage('Run common API') {
        mainContainer.tagName = "timevic-api-${environmentKey}";
         if (effectiveEnvironment == 'Production')
        {
            mainContainer.port = '6200:80';
        }
        else if (effectiveEnvironment == 'Development')
        {
            mainContainer.port = '8215:80';
        }

        mainContainer.envVariables = envVariables.clone()
        mainContainer.envVariables.put('PROJECT_DIR', 'PassKee.Api')
        dockerHelper.runContainer(mainContainer)
    }

    stage('Run worker') {
        mainContainer.tagName = "timevic-worker-${environmentKey}";
        mainContainer.port = '';

        mainContainer.envVariables = envVariables.clone()
        mainContainer.envVariables.put('PROJECT_DIR', 'PassKee.WorkerServices')
        dockerHelper.runContainer(mainContainer)
    }

    stage('Run web app') {
        if (effectiveEnvironment == 'Production')
        {
            webAppContainer.port = '6201:80';
        }
        else if (effectiveEnvironment == 'Development')
        {
            webAppContainer.port = '8216:80';
        }
        dockerHelper.runContainer(webAppContainer)
    }
    
//     stage('CleanUp') {
//         sh '''
//             rm ${imageCommonTmpName}
//             rm ${imageWebTmpName}
//         '''
//     }   
    }
}

def resolveBranchName() {
    def rawBranchName = env.BRANCH_NAME ?: env.GIT_BRANCH
    if (!rawBranchName?.trim()) {
        rawBranchName = sh(script: 'git rev-parse --abbrev-ref HEAD', returnStdout: true).trim()
    }

    return rawBranchName
        .replaceFirst(/^origin\//, '')
        .replaceFirst(/^refs\/heads\//, '')
}

def isAutoTriggeredPushBuild() {
    if (currentBuild.rawBuild.getCause(hudson.model.Cause$UserIdCause) != null) {
        return false
    }

    return currentBuild.rawBuild.getCauses().any { cause ->
        def causeName = cause.class.simpleName
        return causeName != null && (causeName.contains('GitHubPush')
            || causeName.contains('Gitea')
            || causeName.contains('SCMTrigger'))
    }
}
