using Microsoft.AspNetCore.Http;
using PassKee.Business.Common.Exceptions;
using PassKee.Business.Common.Exceptions.Api;
using PassKee.Business.Common.Helpers;
using PassKee.Business.Helpers;

namespace PassKee.Business.Extensions;

public static class FormFileExtensions
{
    public static string GetExtension(this IFormFile file)
    {
        var fileExt = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(fileExt))
        {
            throw new IncorrectFileException("Invalid file extension");
        }
        var parts = fileExt.Split('.');
        if (parts.Length > 0)
        {
            return parts[1].ToLower();
        }
        return parts[0].ToLower();
    }
    
    public static string GetMimeType(IFormFile file)
    {
        return MimeTypeHelper.GetMimeTypeByExtension(file.GetExtension());
    }
}
