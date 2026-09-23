using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PassKee.Business.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string str)
        {
            return System.Text.UTF8Encoding.Default.GetBytes(str);
        }

        public static byte[]? ToHexBytes(this string? hex)
        {
            if (hex == null)
            {
                return null;
            }
            if (hex.Length == 0)
            {
                return new byte[0];
            }
            int l = hex.Length / 2;
            var b = new byte[l];
            for (int i = 0; i < l; ++i)
            {
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return b;
        }

        public static string ToHexString(this string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string FromHexString(this string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }

        public static byte[] HexStringToByteArray(this string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }
            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return HexAsBytes;
        }

        public static String CleanToLowedString(this string str)
        {
            char[] arr = str.ToCharArray();

            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c))));
            return new string(arr);
        }

        public static string StripHTML(this string input)
        {
            return Regex.Replace(input, "<.*?>", String.Empty);
        }

        public static string GetMBtcString(this decimal amount)
        {
            return $"{amount.ToString("0.#####")} mBTC";
        }

        public static string? Truncate(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string? TruncateAndAddDots(this string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        public static string TrimLastSlash(this string value)
        {
            if (value.EndsWith("/"))
            {
                value = value.Remove(value.LastIndexOf('/'), 1);
            }
            return value;
        }

        public static string MySubstring(this string s, int index, int length)
        {
            if (s.Length < index + length)
                length = s.Length - index;
            return s.Substring(index, length);
        }

        public static decimal ToDecimal(this string value)
        {
            try
            {
                // unify string (no spaces, only . )
                string output = value.Trim().Replace(" ", "").Replace(",", ".");

                // split it on points
                string[] split = output.Split('.');

                if (split.Count() > 1)
                {
                    // take all parts except last
                    output = string.Join("", split.Take(split.Count() - 1).ToArray());

                    // combine token parts with last part
                    output = string.Format("{0}.{1}", output, split.Last());
                }

                // parse double invariant
                return decimal.Parse(output, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return 0m;
            }
        }

        public static string? EnsureLeadingSlash(this string? url)
        {
            if (url != null && !url.StartsWith("/"))
            {
                return "/" + url;
            }

            return url;
        }

        public static string? EnsureTrailingSlash(this string? url)
        {
            if (url != null && !url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }

        public static string? RemoveLeadingSlash(this string? url)
        {
            if (url != null && url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            return url;
        }

        public static string? RemoveTrailingSlash(this string? url)
        {
            if (url != null && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static string? RemoveLeadingPathSeparator(this string? url)
        {
            if (url != null && (url.StartsWith("/") || url.StartsWith("\\")))
            {
                url = url.Substring(1);
            }

            return url;
        }
        
        public static string CleanUrlPath(this string url)
        {
            if (String.IsNullOrWhiteSpace(url)) url = "/";

            if (url != "/" && url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static string ToUpperFirstChar(this string value)
        {
            char[] array = value.ToCharArray();
            if (array.Length >= 1)
            {
                if (char.IsLower(array[0]))
                {
                    array[0] = char.ToUpper(array[0]);
                }
            }
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i - 1] == ' ')
                {
                    if (char.IsLower(array[i]))
                    {
                        array[i] = char.ToUpper(array[i]);
                    }
                }
            }
            return new string(array);
        }

        public static string RemoveNewLineSymbols(this string str)
        {
            return str.Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "");
        }
        
        public static string RemoveNewLineSymbolsWithChars(this string str)
        {
            return str.RemoveNewLineSymbols()
                .Replace("\\n", "\n")
                .Replace("\\r", "\n");
        }
        
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.ToLower().Substring(1)
            };
        
        public static string? Truncate(this string? value, int maxLength, bool isAddDots = true)
        {
            if (string.IsNullOrEmpty(value)) 
                return value;
            var dots = isAddDots ? "..." : "";
            return value.Length <= maxLength ? value : $"{value[..maxLength]}{dots}"; 
        }

        public static byte[] GetUtf8Bytes(this string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }
        
        public static string FirstChar(this string? value)
        {
            if (String.IsNullOrEmpty(value))
                return string.Empty;

            return value.Substring(0, 1);
        }
        
        public static int CountLines(this string str)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (str == string.Empty)
                return 0;
            int index = -1;
            int count = 0;
            while (-1 != (index = str.IndexOf(Environment.NewLine, index + 1, StringComparison.Ordinal)))
                count++;

            return count + 1;
        }
        
        public static string GetFirstUpperLetters(this string str, int maxLetters = 2)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException(nameof(str));
            var letters = str.Split([" "], StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.FirstChar().ToUpper())
                .Take(maxLetters);
            return string.Join("", letters);
        }
    }
}
