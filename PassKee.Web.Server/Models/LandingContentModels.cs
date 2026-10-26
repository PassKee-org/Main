using System.Text.Encodings.Web;
using System.Text.Json;

namespace PassKee.Web.Server.Models;

public sealed record FaqItemModel(
    string Id,
    string CategoryId,
    string CategoryLabel,
    string Question,
    string Answer,
    string[] Keywords,
    bool ShowOnHome = false);

public sealed record FaqCategoryModel(
    string Id,
    string Label,
    string IconClass);

public static class LandingContentData
{
    public static readonly IReadOnlyList<FaqCategoryModel> FaqCategories =
    [
        new("all", "All Topics", "fa-solid fa-layer-group"),
        new("secret-key", "Secret Key & Emergency Kit", "fa-solid fa-key"),
        new("device-login", "New Device Sign-In", "fa-solid fa-laptop-medical"),
        new("sharing", "Secure Sharing", "fa-solid fa-share-nodes"),
        new("keepass-import", "KeePass (.kdbx) Import", "fa-solid fa-file-import"),
        new("session-security", "Auto-Lock & Privacy", "fa-solid fa-shield-halved")
    ];

    public static readonly IReadOnlyList<FaqItemModel> FaqItems =
    [
        new(
            Id: "what-is-secret-key",
            CategoryId: "secret-key",
            CategoryLabel: "Secret Key & Emergency Kit",
            Question: "What is the Secret Key and where do I find it?",
            Answer: "Your Secret Key is a unique 128-bit cryptographic code (formatted like PK-XXXX-XXXX-...) generated directly on your device when you create your PassKee account. It combines with your Master Password via Argon2id so no one can unlock your vault on a new device with your password alone. You can find your Secret Key in the Emergency Kit you saved during registration.",
            Keywords: ["secret key", "emergency kit", "128-bit", "pk-", "where to find", "registration"],
            ShowOnHome: true),

        new(
            Id: "lost-password-or-secret-key",
            CategoryId: "secret-key",
            CategoryLabel: "Secret Key & Emergency Kit",
            Question: "What should I do if I forgot my Master Password or lost my Secret Key?",
            Answer: "First, check the printed or saved Emergency Kit from when you registered—your 128-bit Secret Key is stored there. Second, if you are still signed in on another authorized browser tab or device, export your vault backup immediately. Because PassKee is built on strict zero-knowledge encryption, our servers never store your keys and cannot reset your Master Password or recover a lost Secret Key. If both your Master Password and Secret Key are permanently lost, the vault cannot be decrypted.",
            Keywords: ["forgot password", "lost secret key", "recovery", "reset password", "emergency kit", "locked out"],
            ShowOnHome: true),

        new(
            Id: "enter-secret-key-every-time",
            CategoryId: "device-login",
            CategoryLabel: "New Device Sign-In",
            Question: "Do I need to enter my Secret Key every time I sign in?",
            Answer: "No. You only enter your Secret Key when signing in from a new browser or unrecognized device. If you enable quick session unlock on your trusted device, PassKee keeps your Secret Key encrypted in isolated session storage so you only need your Master Password to unlock your vault after an inactivity lock.",
            Keywords: ["every time", "new device", "quick unlock", "session storage", "remember secret key"],
            ShowOnHome: true),

        new(
            Id: "inactivity-auto-lock",
            CategoryId: "session-security",
            CategoryLabel: "Auto-Lock & Privacy",
            Question: "What happens if I close my browser tab or step away from my computer?",
            Answer: "For your protection, PassKee automatically locks your vault after 15 minutes of inactivity and wipes decrypted keys from volatile memory (RAM). Closing the browser tab immediately clears session storage. To resume work after an inactivity lock, simply enter your Master Password.",
            Keywords: ["auto-lock", "15 minutes", "inactivity", "close tab", "session lock", "ram", "timeout"],
            ShowOnHome: false),

        new(
            Id: "one-time-link-sharing",
            CategoryId: "sharing",
            CategoryLabel: "Secure Sharing",
            Question: "Is it safe to share a password or note using a PassKee link?",
            Answer: "Yes. When you create a shareable link in PassKee, the item is encrypted in your browser with a one-time key placed after the URL hash fragment (#). By HTTP specification, browsers never send the # fragment to any server. You can also set an expiration timer and a single-view burn limit so the ciphertext is permanently deleted after the recipient views it.",
            Keywords: ["share link", "url fragment", "hash", "one-time link", "burn after reading", "sharing"],
            ShowOnHome: false),

        new(
            Id: "keepass-kdbx-migration",
            CategoryId: "keepass-import",
            CategoryLabel: "KeePass (.kdbx) Import",
            Question: "How does KeePass (.kdbx) import and export protect my data?",
            Answer: "PassKee parses and decrypts your KeePass (.kdbx) file locally inside your browser using WebAssembly. Your KDBX master password and unencrypted entries never travel over the network in plaintext—each folder, login, note, and attachment is re-encrypted locally with your PassKee Vault AES-256-GCM key before syncing.",
            Keywords: ["keepass", "kdbx", "import", "export", "migration", "bitwarden", "1password", "local"],
            ShowOnHome: false),

        new(
            Id: "server-breach-protection",
            CategoryId: "session-security",
            CategoryLabel: "Auto-Lock & Privacy",
            Question: "Can PassKee administrators read my vault or hand over my passwords?",
            Answer: "No. PassKee servers act strictly as blind storage for AES-256-GCM ciphertext and wrapped key envelopes. Key derivation (Argon2id), encryption, and decryption happen exclusively on your client device. Even if our database were inspected, there are no plaintext usernames, passwords, folder names, or decryption keys on the server.",
            Keywords: ["administrators", "breach", "zero-knowledge", "server", "encryption", "aes-256-gcm", "argon2id"],
            ShowOnHome: false),

        new(
            Id: "supported-credential-types",
            CategoryId: "device-login",
            CategoryLabel: "New Device Sign-In",
            Question: "What types of credentials and secrets can I store in PassKee?",
            Answer: "PassKee supports website Logins, Payment Cards, Secure Notes, standalone Passwords, SSH Keys, Database Connections, Server Credentials, and encrypted File Attachments—organized across multiple vaults, nested folders, and custom tags.",
            Keywords: ["types", "ssh key", "database", "cards", "notes", "files", "folders", "tags"],
            ShowOnHome: false)
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public static string BuildHomeJsonLd(string baseUrl)
    {
        var homeFaqs = FaqItems.Where(f => f.ShowOnHome).ToList();
        var graph = new object[]
        {
            new
            {
                @context = "https://schema.org",
                @type = "SoftwareApplication",
                name = "PassKee",
                url = $"{baseUrl}/",
                operatingSystem = "All",
                applicationCategory = "SecurityApplication",
                applicationSubCategory = "Password Manager",
                description = "Zero-knowledge cloud password and secrets manager with Dual-Key protection (Master Password + 128-bit Secret Key), client-side Argon2id and AES-256-GCM encryption, local KeePass (.kdbx) import/export, and encrypted link sharing.",
                featureList = new[]
                {
                    "Zero-Knowledge Client-Side AES-256-GCM Encryption",
                    "Dual-Key Security Model (Master Password + 128-bit Secret Key)",
                    "Argon2id Key Derivation Function",
                    "3-Panel Vault, Folder, and Tag Organization",
                    "Local In-Browser KeePass (.kdbx) Import and Export",
                    "Zero-Knowledge One-Time Link Sharing via URL Fragment",
                    "Automatic 15-Minute Inactivity Session Lock"
                },
                offers = new
                {
                    @type = "Offer",
                    price = "0",
                    priceCurrency = "USD"
                }
            },
            new
            {
                @context = "https://schema.org",
                @type = "FAQPage",
                mainEntity = homeFaqs.Select(f => new
                {
                    @type = "Question",
                    name = f.Question,
                    acceptedAnswer = new
                    {
                        @type = "Answer",
                        text = f.Answer
                    }
                }).ToArray()
            }
        };

        return JsonSerializer.Serialize(graph, JsonOptions);
    }

    public static string BuildSecurityJsonLd(string baseUrl)
    {
        var graph = new object[]
        {
            new
            {
                @context = "https://schema.org",
                @type = "TechArticle",
                headline = "PassKee Security Architecture — Zero-Knowledge & Dual-Key Protection",
                description = "Detailed explanation of PassKee's Zero-Knowledge architecture, Dual-Key protection (Master Password + 128-bit Secret Key), Emergency Kit risk management, and URL fragment encrypted sharing.",
                url = $"{baseUrl}/security",
                author = new
                {
                    @type = "Organization",
                    name = "PassKee"
                },
                publisher = new
                {
                    @type = "Organization",
                    name = "PassKee",
                    url = $"{baseUrl}/"
                }
            },
            new
            {
                @context = "https://schema.org",
                @type = "BreadcrumbList",
                itemListElement = new[]
                {
                    new { @type = "ListItem", position = 1, name = "PassKee", item = $"{baseUrl}/" },
                    new { @type = "ListItem", position = 2, name = "Security & Privacy", item = $"{baseUrl}/security" }
                }
            }
        };

        return JsonSerializer.Serialize(graph, JsonOptions);
    }

    public static string BuildHelpJsonLd(string baseUrl)
    {
        var graph = new object[]
        {
            new
            {
                @context = "https://schema.org",
                @type = "FAQPage",
                mainEntity = FaqItems.Select(f => new
                {
                    @type = "Question",
                    name = f.Question,
                    acceptedAnswer = new
                    {
                        @type = "Answer",
                        text = f.Answer
                    }
                }).ToArray()
            },
            new
            {
                @context = "https://schema.org",
                @type = "HowTo",
                name = "How to Set Up Your Encrypted PassKee Vault in 3 Steps",
                description = "Step-by-step quick start guide from creating your account and saving your Emergency Kit to storing or importing your first encrypted passwords.",
                step = new[]
                {
                    new
                    {
                        @type = "HowToStep",
                        position = 1,
                        name = "Create Your Account & Master Password",
                        text = "Register with your email and choose a strong Master Password. Your cryptographic keys are derived locally in your browser."
                    },
                    new
                    {
                        @type = "HowToStep",
                        position = 2,
                        name = "Save Your Emergency Kit (Secret Key)",
                        text = "Copy and safely store your 128-bit Secret Key (PK-XXXX-...). You will need it alongside your Master Password when signing in from a new device."
                    },
                    new
                    {
                        @type = "HowToStep",
                        position = 3,
                        name = "Create a Vault or Import KeePass (.kdbx)",
                        text = "Add logins, cards, SSH keys, and notes into organized folders, or import an existing .kdbx database locally inside your browser."
                    }
                }
            },
            new
            {
                @context = "https://schema.org",
                @type = "BreadcrumbList",
                itemListElement = new[]
                {
                    new { @type = "ListItem", position = 1, name = "PassKee", item = $"{baseUrl}/" },
                    new { @type = "ListItem", position = 2, name = "Help Center & FAQ", item = $"{baseUrl}/help" }
                }
            }
        };

        return JsonSerializer.Serialize(graph, JsonOptions);
    }
}

