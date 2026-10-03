using System;
using System.Collections.Generic;
using System.IO;

namespace PassKee.Web.Core.Ui.Shared.Components.Storage;

/// <summary>
/// Picks a preview icon by file extension. Files are encrypted client-side, so no server-side thumbnails exist.
/// </summary>
public static class FileIconHelper
{
    private const string DefaultIcon = "fa-solid fa-file";

    private static readonly Dictionary<string, string> IconsByExtension = BuildMap();

    public static string GetIconClass(string? fileName)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty).TrimStart('.');
        return extension.Length > 0 && IconsByExtension.TryGetValue(extension, out var icon) ? icon : DefaultIcon;
    }

    private static Dictionary<string, string> BuildMap()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        void Add(string icon, params string[] extensions)
        {
            foreach (var extension in extensions)
            {
                map[extension] = icon;
            }
        }

        Add("fa-solid fa-file-image", "png", "jpg", "jpeg", "gif", "bmp", "webp", "svg", "ico", "heic", "tiff");
        Add("fa-solid fa-file-pdf", "pdf");
        Add("fa-solid fa-file-word", "doc", "docx", "odt", "rtf");
        Add("fa-solid fa-file-excel", "xls", "xlsx", "ods", "csv");
        Add("fa-solid fa-file-powerpoint", "ppt", "pptx", "odp");
        Add("fa-solid fa-file-lines", "txt", "md", "log");
        Add("fa-solid fa-file-zipper", "zip", "rar", "7z", "tar", "gz", "bz2", "xz");
        Add("fa-solid fa-file-audio", "mp3", "wav", "flac", "ogg", "m4a", "aac");
        Add("fa-solid fa-file-video", "mp4", "mkv", "mov", "avi", "webm");
        Add("fa-solid fa-file-code", "json", "xml", "html", "css", "js", "ts", "cs", "py", "java", "sh", "yml", "yaml", "sql");
        Add("fa-solid fa-key", "pem", "key", "pub", "crt", "cer", "p12", "pfx", "kdbx");

        return map;
    }
}
