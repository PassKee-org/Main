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
        new("secret-key", "Emergency Secret Key", "fa-solid fa-key"),
        new("device-login", "Sign-In & Quick Unlock", "fa-solid fa-laptop-medical"),
        new("sharing", "Encrypted Link Sharing", "fa-solid fa-share-nodes"),
        new("keepass-import", "KeePass (.kdbx) Import & Backup", "fa-solid fa-file-import"),
        new("session-security", "Auto-Lock & Zero-Knowledge", "fa-solid fa-shield-halved")
    ];

    public static readonly IReadOnlyList<FaqItemModel> FaqItems =
    [
        new(
            Id: "what-is-secret-key",
            CategoryId: "secret-key",
            CategoryLabel: "Emergency Secret Key",
            Question: "What is the Emergency Secret Key and when is it generated?",
            Answer: "Your Emergency Secret Key is a unique 128-bit cryptographic code (formatted as PK-XXXX-XXXX-XXXX-XXXX-XXXX-XXXX) generated directly in your browser when you create your PassKee account. It combines with your Master Password via Argon2id so no one can sign in to your vault from a new device using your password alone. During registration, you copy and confirm that you have saved this Secret Key before proceeding to sign in.",
            Keywords: ["secret key", "emergency secret key", "128-bit", "pk-", "registration", "copy secret key"],
            ShowOnHome: true),

        new(
            Id: "lost-password-or-secret-key",
            CategoryId: "secret-key",
            CategoryLabel: "Emergency Secret Key",
            Question: "What should I do if I forgot my Master Password or lost my Secret Key?",
            Answer: "Because PassKee is built on strict zero-knowledge encryption, our servers never store your Master Password or Secret Key and cannot reset or recover them. If you lost your Secret Key but still have an unlocked session on an authorized device, immediately open Vault Actions (⋮) -> Backup -> Export to KeePass (.kdbx) to save your data, then create a new account. If both your Master Password and Secret Key are lost with no active session, your encrypted vault cannot be decrypted by anyone.",
            Keywords: ["forgot password", "lost secret key", "recovery", "reset password", "no remote reset", "backup", "kdbx"],
            ShowOnHome: true),

        new(
            Id: "enter-secret-key-every-time",
            CategoryId: "device-login",
            CategoryLabel: "Sign-In & Quick Unlock",
            Question: "Do I have to enter my Emergency Secret Key every time I unlock my vault?",
            Answer: "No. When signing in, check 'Remember Secret Key for quick unlock in this session'. PassKee encrypts your Secret Key in isolated browser sessionStorage using a key derived from your Master Password and Auth Salt via Argon2id. When your session locks after inactivity, the 'Unlock Session' screen only asks for your Master Password.",
            Keywords: ["every time", "new device", "quick unlock", "sessionstorage", "remember secret key", "unlock session"],
            ShowOnHome: true),

        new(
            Id: "inactivity-auto-lock",
            CategoryId: "session-security",
            CategoryLabel: "Auto-Lock & Zero-Knowledge",
            Question: "What happens after 15 minutes of inactivity or when I click Lock?",
            Answer: "After 15 minutes of inactivity—or immediately when you click the Lock button in the header—PassKee closes all open modals, zeroes out your decrypted Private Key and active Vault AES Key in memory (RAM), and navigates to the Unlock Session screen. If you close the browser tab or click Logout, sessionStorage is completely cleared.",
            Keywords: ["auto-lock", "15 minutes", "inactivity", "close tab", "lock", "logout", "ram", "timeout"],
            ShowOnHome: false),

        new(
            Id: "one-time-link-sharing",
            CategoryId: "sharing",
            CategoryLabel: "Encrypted Link Sharing",
            Question: "How does 'Share a link to this item' protect my shared credentials?",
            Answer: "When you share an item, your browser strips any file attachments, generates a fresh 256-bit AES-GCM key, encrypts the item locally, and places the 64-character hex decryption key after the URL hash fragment (/share/{token}#{hexKey}). Browsers never send the # fragment to the server. You can set expiration to 1 hour, 1 day, 7 days, or 30 days, and optionally enable 'Can be viewed only once'.",
            Keywords: ["share link", "url fragment", "hash", "one-time view", "can be viewed only once", "expires after", "files attached"],
            ShowOnHome: false),

        new(
            Id: "keepass-kdbx-migration",
            CategoryId: "keepass-import",
            CategoryLabel: "KeePass (.kdbx) Import & Backup",
            Question: "Which formats are supported for KeePass (.kdbx) Import and Backup?",
            Answer: "From the Vault Actions (⋮) menu in the sidebar, you can import KeePass KDBX 3 and KDBX 4 databases (up to 100 MB) unlocked with a database password. All decrypting happens locally in your browser via WebAssembly, preserving directories, entries, tags, and file attachments. You can also choose Backup -> Export to KeePass (.kdbx) at any time to download an encrypted KDBX 4 archive (with archived items placed in a dedicated Archive directory).",
            Keywords: ["keepass", "kdbx", "kdbx 3", "kdbx 4", "import", "backup", "export", "100 mb", "archive"],
            ShowOnHome: false),

        new(
            Id: "server-breach-protection",
            CategoryId: "session-security",
            CategoryLabel: "Auto-Lock & Zero-Knowledge",
            Question: "Can PassKee servers or administrators read my vault or folder names?",
            Answer: "No. PassKee servers store only raw binary ciphertext (PostgreSQL bytea). Each vault has its own 256-bit AES-GCM key wrapped with your Curve25519 (X25519) keypair, and both directory names and credential payloads are encrypted on your device before transmission.",
            Keywords: ["administrators", "breach", "zero-knowledge", "server", "encryption", "aes-256-gcm", "x25519", "argon2id"],
            ShowOnHome: false),

        new(
            Id: "supported-credential-types",
            CategoryId: "device-login",
            CategoryLabel: "Sign-In & Quick Unlock",
            Question: "What credential types and generators are built into PassKee?",
            Answer: "PassKee supports 8 credential types: Login, Secure Note, Card, Password, File, Database, SSH Key, and Server—complete with custom icons, custom fields, sections, tags, file attachments, and an Archive folder. The top header also provides instant generators for random Passwords, PIN codes, and developer GUID/UUID v4/v7, Hex, and Base64 tokens.",
            Keywords: ["types", "login", "secure note", "card", "password", "file", "database", "ssh key", "server", "guid", "uuid"],
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
                description = "Zero-knowledge cloud password and secrets manager with Two-Key protection (Master Password + 128-bit Emergency Secret Key), client-side Argon2id, Curve25519 (X25519), and AES-256-GCM encryption, local KeePass (.kdbx) import/backup, and encrypted #fragment link sharing.",
                featureList = new[]
                {
                    "Zero-Knowledge Client-Side AES-256-GCM & X25519 Encryption",
                    "Two-Key Protection (Master Password + 128-bit Emergency Secret Key)",
                    "Argon2id Key Derivation & Session Quick Unlock",
                    "8 Credential Types: Login, Secure Note, Card, Password, File, Database, SSH Key, Server",
                    "Local In-Browser KeePass (.kdbx) KDBX 3/4 Import and KDBX 4 Backup",
                    "Zero-Knowledge Link Sharing via URL Fragment (#)",
                    "Built-in Password, PIN, UUID v4/v7, Hex & Base64 Generators",
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
                headline = "PassKee Security Architecture — Zero-Knowledge & Two-Key Protection",
                description = "Detailed explanation of PassKee's Zero-Knowledge architecture, Two-Key protection (Master Password + 128-bit Emergency Secret Key), Curve25519 + AES-256-GCM encryption, and URL fragment encrypted sharing.",
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
                name = "How to Create and Set Up Your Encrypted PassKee Vault in 3 Steps",
                description = "Step-by-step quick start guide from creating your account and saving your Emergency Secret Key to storing or importing your first encrypted credentials.",
                step = new[]
                {
                    new
                    {
                        @type = "HowToStep",
                        position = 1,
                        name = "Create PassKee Account & Master Password",
                        text = "Enter your Email Address and create a Master Password (at least 10 characters with uppercase, lowercase Latin letters, and digits). PassKee generates your 128-bit Emergency Secret Key on your device."
                    },
                    new
                    {
                        @type = "HowToStep",
                        position = 2,
                        name = "Copy & Confirm Your Emergency Secret Key",
                        text = "On the 'Save Your Emergency Secret Key' screen, click Copy Secret Key, store it in a safe place, and check 'I have saved my Secret Key and understand PassKee cannot recover it'."
                    },
                    new
                    {
                        @type = "HowToStep",
                        position = 3,
                        name = "Sign In & Add Items or Import KeePass (.kdbx)",
                        text = "Sign in to your vault, organize directories, create items across 8 credential types, or import an existing KeePass (.kdbx) file locally in your browser."
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
