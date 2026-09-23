namespace PassKee.Business.Common.Utils
{
    public static class Base64Utils
    {
        public static bool IsValidBase64(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return false;
            } 
            return Convert.TryFromBase64String(
                base64, 
                CalculateBase64Buffer(base64),
                out _
            );
        }
        
        private static byte[] CalculateBase64Buffer(string base64)
        {
            // Magic block. Buffer calculation.
            return new byte[
                (
                    (base64.Length * 3) + 3
                ) / 4 - (
                    base64.Length > 0 && base64[base64.Length - 1] == '=' 
                        ? base64.Length > 1 && base64[base64.Length - 2] == '=' 
                            ? 2 
                            : 1 
                        : 0
                )
            ];
        }
    }
}
