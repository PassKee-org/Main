@Library('common')
import com.shared.jenkins.docker.DockerHelper
import com.shared.jenkins.docker.DockerContainer

def effectiveEnvironment = params.ENVIRONMENT ?: 'Development'
def environmentKey = effectiveEnvironment.toLowerCase()
def containerSharedDir = "/mnt/local_share/docker_images/passkee"
def imageName = "latest"
def imageWebTmpName = "${containerSharedDir}/${environmentKey}_web_latest"
def imageCommonTmpName = "${containerSharedDir}/${environmentKey}_common_latest"
def currentBranchName = ''
def shouldRunDeployment = true

def dockerHelper = new DockerHelper(this)
public Map<String, String> envVariables = new HashMap<String, String>()

def mainContainer = new DockerContainer(
    name: "passkee-main-${environmentKey}",
    dockerFile: 'devops/publish/common/Dockerfile',
);

def apiContainer = new DockerContainer(
    name: "passkee-main-${environmentKey}",
    tagName: "passkee-api-${environmentKey}",
    dockerFile: 'devops/publish/common/Dockerfile',
);

def workerContainer = new DockerContainer(
    name: "passkee-main-${environmentKey}",
    tagName: "passkee-worker-${environmentKey}",
    dockerFile: 'devops/publish/common/Dockerfile',
);

def migrationContainer = new DockerContainer(
    name: "passkee-main-${environmentKey}",
    dockerFile: 'devops/publish/common/Dockerfile',
    isRunAlways: false,
    isRunInBackground: false,
);
def webAppContainer = new DockerContainer(
    name: "passkee-web-${environmentKey}",
    dockerFile: 'devops/publish/web/Dockerfile',
);

def repositoryUrl = scm.userRemoteConfigs[0].url;
def gitCredentials="gitea-jenkins-ssh-key"

properties([
    pipelineTriggers([
        pollSCM(''),
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

    stage('Checkout') {
        cleanWs()
        sh """
            git config --global http.postBuffer 2048M
            git config --global http.maxRequestBuffer 1024M
            git config --global core.compression 0
        """
        checkout scm

        if (params.GIT_TAG?.trim() && params.GIT_TAG != 'NONE' && params.GIT_TAG != 'main') {
            sh "git checkout ${params.GIT_TAG}"
        }
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
        mainContainer.name = "passkee-main-${environmentKey}"
        apiContainer.name = "passkee-main-${environmentKey}"
        apiContainer.tagName = "passkee-api-${environmentKey}"
        workerContainer.name = "passkee-main-${environmentKey}"
        workerContainer.tagName = "passkee-worker-${environmentKey}"
        migrationContainer.name = "passkee-main-${environmentKey}"
        webAppContainer.name = "passkee-web-${environmentKey}"

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
        envVariables.put('Serilog__IsSendEmailIfError', 'false')
        envVariables.put('Serilog__MinimumLevel__Default', 'Debug')
        
        mainContainer.buildVariables.put('ENVIRONMENT', effectiveEnvironment)
        envVariables.put('ASPNETCORE_ENVIRONMENT', effectiveEnvironment)
        
        webAppContainer.buildVariables.put('ENVIRONMENT', effectiveEnvironment)
        webAppContainer.envVariables.put('ASPNETCORE_ENVIRONMENT', effectiveEnvironment)

        dbName = 'passkee'
        def dbPort = '5432'
        def dbHost = ''
        if (effectiveEnvironment == 'Production')
        {
            envVariables.put('App__FrontendUrl', 'https://passkee.org')
            dbHost = '192.168.88.41'
        }
        else if (effectiveEnvironment == 'Development')
        {
            envVariables.put('App__FrontendUrl', 'https://dev.passkee.org')
            dbHost = '192.168.88.42'
        }

        // Grtaylog
        withCredentials([string(credentialsId: "passkee_graylog_ip", variable: 'AUTH_SECRET')]) {
            envVariables.put('App__Logging__GrayLog__Host', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "passkee_graylog_port", variable: 'AUTH_SECRET')]) {
            envVariables.put('App__Logging__GrayLog__Port', AUTH_SECRET)
        }

        // Common
        envVariables.put('Smtp__EnableSsl', 'true')
        withCredentials([
                usernamePassword(credentialsId: "passkee_production_smtp_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('Smtp__UserName', USER_NAME)
            envVariables.put('Smtp__Password', PASSWORD)
        }
        withCredentials([string(credentialsId: "passkee_production_smtp_server", variable: 'AUTH_SECRET')]) {
            envVariables.put('Smtp__Server', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "passkee_production_smtp_port", variable: 'AUTH_SECRET')]) {
            envVariables.put('Smtp__Port', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "passkee_production_smtp_from_name", variable: 'AUTH_SECRET')]) {
            envVariables.put('Smtp__From__Name', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "passkee_production_smtp_from_email", variable: 'AUTH_SECRET')]) {
            envVariables.put('Smtp__From__Email', AUTH_SECRET)
        }

        withCredentials([string(credentialsId: "passkee_production_recaptcha_secret", variable: 'AUTH_SECRET')]) {
            envVariables.put('ReCaptcha__Secret', AUTH_SECRET)
        }

        withCredentials([
                usernamePassword(credentialsId: "passkee_${environmentKey}_db_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put(
                'ConnectionStrings__DefaultConnection',
                "User ID=${USER_NAME};Password=${PASSWORD};Host=${dbHost};Port=${dbPort};Database=${dbName};Pooling=true;"
            )
        }
        withCredentials([string(credentialsId: "passkee_${environmentKey}_user_jwt", variable: 'AUTH_SECRET')]) {
            envVariables.put('App__Auth__SymmetricSecurityKey', AUTH_SECRET)
        }

        withCredentials([
            usernamePassword(credentialsId: "passkee_${environmentKey}_garage_credentials", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
        ]) {
            envVariables.put('Garage__AccessKey', USER_NAME)
            envVariables.put('Garage__SecretKey', PASSWORD)
        }
        withCredentials([string(credentialsId: "passkee_garage_url", variable: 'AUTH_SECRET')]) {
            envVariables.put('Garage__Url', AUTH_SECRET)
        }
        withCredentials([string(credentialsId: "passkee_${environmentKey}_garage_bucket_name", variable: 'AUTH_SECRET')]) {
            envVariables.put('Garage__BucketName', AUTH_SECRET)
        }
    }

    parallel(
        'Build main image': {
            stage('Build main image') {
                dockerHelper.buildAndSave(mainContainer, imageCommonTmpName)
            }
        },
        'Build web image': {
            stage('Build web image') {
                dockerHelper.buildAndSave(webAppContainer, imageWebTmpName)
            }
        }
    )

    if (params.NEW_VERSION) {
        stage('Create GIT tag') {
            def (VER_MAJOR, VER_MINOR, VER_PATCH, VER_BUILD) = params.NEW_VERSION.tokenize('.').collect { it.toInteger() }
            env.VERSION_INCREMENT = VER_MAJOR + "." + VER_MINOR + "." + VER_PATCH + "." + VER_BUILD

            withCredentials([sshUserPrivateKey(credentialsId: gitCredentials, keyFileVariable: 'key')]) {
                sh '''
                    git config core.sshCommand 'ssh -i ${key}'
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

    stage('Purge Cloudflare cache') {
        if (effectiveEnvironment == 'Production')
        {
            withCredentials([
                usernamePassword(credentialsId: "passkee_cloudflaire_api_token", usernameVariable: 'USER_NAME', passwordVariable: 'PASSWORD')
            ]) {
                sh '''
                    set -e

                    RESPONSE=$(curl -sS -X POST "https://api.cloudflare.com/client/v4/zones/${USER_NAME}/purge_cache" \
                    -H "Authorization: Bearer ${PASSWORD}" \
                    -H "Content-Type: application/json" \
                    --data '{"purge_everything":true}')

                    echo "$RESPONSE" | grep -q '"success":true'
                '''
            }
        }

    }
}

if (shouldRunDeployment) {
node('web-node') {

    stage('Load container') {
        parallel(
            'Load main image': {
                dockerHelper.loadFromFile(imageCommonTmpName)
            },
            'Load web image': {
                dockerHelper.loadFromFile(imageWebTmpName)
            }
        )
    }

    stage('Stop containers') {
        parallel(
            'Stop web app': {
                dockerHelper.stopContainer(webAppContainer)
            },
            'Stop API': {
                dockerHelper.stopContainer(apiContainer)
            },
            'Stop worker': {
                dockerHelper.stopContainer(workerContainer)
            }
        )
    }

    stage('Run migrations') {
        dockerHelper.stopContainer(migrationContainer)

        migrationContainer.envVariables = envVariables.clone()
        migrationContainer.envVariables.put('PROJECT_DIR', 'PassKee.Migrations')
        dockerHelper.runContainer(migrationContainer)
    }

    parallel(
        'Run common API': {
            stage('Run common API') {
                if (effectiveEnvironment == 'Production')
                {
                    apiContainer.port = '8222:80';
                }
                else if (effectiveEnvironment == 'Development')
                {
                    apiContainer.port = '8223:80';
                }

                apiContainer.envVariables = envVariables.clone()
                apiContainer.envVariables.put('PROJECT_DIR', 'PassKee.Api')
                apiContainer.envVariables.put('App__Name', 'Api')
                dockerHelper.runContainer(apiContainer)
            }
        },
        'Run worker': {
            stage('Run worker') {
                workerContainer.port = '';

                workerContainer.envVariables = envVariables.clone()
                workerContainer.envVariables.put('PROJECT_DIR', 'PassKee.WorkerServices')
                workerContainer.envVariables.put('App__Name', 'Worker')
                dockerHelper.runContainer(workerContainer)
            }
        },
        'Run web app': {
            stage('Run web app') {
                if (effectiveEnvironment == 'Production')
                {
                    webAppContainer.port = '8224:80';
                }
                else if (effectiveEnvironment == 'Development')
                {
                    webAppContainer.port = '8225:80';
                }
                webAppContainer.envVariables.put('App__Name', 'WebClient')
                dockerHelper.runContainer(webAppContainer)
            }
        }
    )
    
//     stage('CleanUp') {
//         sh '''
//             rm ${imageCommonTmpName}
//             rm ${imageWebTmpName}
//         '''
//     }   
    }
}

def resolveBranchName() {
    if (env.GIT_REF?.trim()) {
        return cleanBranchName(env.GIT_REF)
    }

    def rawBranchName = env.BRANCH_NAME ?: env.GIT_BRANCH
    if (rawBranchName?.trim() && rawBranchName != 'HEAD') {
        return cleanBranchName(rawBranchName)
    }

    // When Jenkins checkouts commit directly (detached HEAD), determine branch from git references
    try {
        def pointsAt = sh(script: "git for-each-ref --points-at=HEAD --format='%(refname:short)' refs/remotes/origin refs/heads", returnStdout: true).trim()
        if (pointsAt) {
            def branches = pointsAt.split('\n').collect { cleanBranchName(it) }
            if (branches.contains('main')) {
                return 'main'
            }
            if (!branches.isEmpty() && branches[0] != 'HEAD') {
                return branches[0]
            }
        }

        def containsBranches = sh(script: 'git branch -r --contains HEAD', returnStdout: true).trim()
        if (containsBranches) {
            def branches = containsBranches.split('\n').collect { cleanBranchName(it) }
            if (branches.contains('main')) {
                return 'main'
            }
            if (!branches.isEmpty() && branches[0] != 'HEAD') {
                return branches[0]
            }
        }
    } catch (Exception e) {
        echo "Failed to resolve branch from git refs: ${e.message}"
    }

    if (scm?.branches && scm.branches[0]?.name) {
        def scmBranch = cleanBranchName(scm.branches[0].name)
        if (scmBranch && scmBranch != 'HEAD') {
            return scmBranch
        }
    }

    def abbrev = sh(script: 'git rev-parse --abbrev-ref HEAD', returnStdout: true).trim()
    return cleanBranchName(abbrev)
}

def cleanBranchName(String branch) {
    if (!branch) return ''
    return branch
        .replaceFirst(/^\*\s*/, '')
        .replaceFirst(/^origin\//, '')
        .replaceFirst(/^refs\/remotes\/origin\//, '')
        .replaceFirst(/^refs\/heads\//, '')
        .replaceFirst(/^refs\/tags\//, '')
        .trim()
}

def isAutoTriggeredPushBuild() {
    if (currentBuild.rawBuild.getCause(hudson.model.Cause$UserIdCause) != null) {
        return false
    }

    return currentBuild.rawBuild.getCauses().any { cause ->
        def causeName = cause.class.simpleName
        return causeName != null && (causeName.contains('GitHubPush')
            || causeName.contains('Gitea')
            || causeName.contains('SCMTrigger')
            || causeName.contains('RemoteCause')
            || causeName.contains('BranchIndexingCause')
            || causeName.contains('GenericCause'))
    }
}
