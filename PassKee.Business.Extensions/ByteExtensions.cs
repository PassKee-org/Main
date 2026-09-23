using System.Text;

namespace PassKee.Business.Extensions
{
    public static class ByteExtensions
    {
        public static string GetString(this byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        public static string? ToHexString(this byte[]? hex, bool ToUpper = false)
        {
            if (hex == null)
            {
                return null;
            }
            if (hex.Length == 0)
            {
                return string.Empty;
            }
            var s = new StringBuilder();
            foreach (byte b in hex)
            {
                if (ToUpper)
                    s.Append(b.ToString("x2").ToUpper());
                else
                    s.Append(b.ToString("x2").ToUpper());
            }
            return s.ToString();
        }

        /// <summary>  
        ///  Returns true if arrays are equals
        /// </summary>  
        public static bool CompareTo(this byte[]? bytes, byte[]? toCompare)
        {
            if (bytes == null || toCompare == null)
            {
                return false;
            }
            if (toCompare.Length != bytes.Length)
                return false;

            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] != toCompare[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
