using System.Security.Cryptography;

namespace PassKee.Business.Common.Utils
{
    public static class SecurityUtil
    {
        private static readonly string BASE_58_ALBHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        private static readonly string FULL_ALBHABET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        private static readonly int SALT_SIZE = 32;
        private static readonly int PASSWORD_SIZE = 12;
        private static readonly int HASH_SIZE = 32;
        private static readonly int HASH_ITERATIONS = 100_000;

        public static byte[] GenerateSalt(int? size = null)
        {
            var saltSize = size ?? SALT_SIZE;
            var data = new byte[saltSize];
            RandomNumberGenerator.Fill(data);
            return data;
        }
        
        public static string GenerateSaltAsString(int? size = null)
        {
            return Convert.ToBase64String(GenerateSalt(size));
        }

        public static string GeneratePassword(int size)
        {
            return GetBase58RandomString(size);
        }
        
        public static string GeneratePassword(int? size = null)
        {
            var passwordSize = size ?? PASSWORD_SIZE;
            return GetBase58RandomString(passwordSize);
        }

        private const string LowercaseLetters = "abcdefghjkmnpqrstuvwxyz";
        private const string UppercaseLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const string DigitChars = "23456789";
        private const string SpecialCharacters = "!@#$%&*_-+=";
        private const string CrockfordBase32Alphabet = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        public static string GenerateStrongPassword(int length = 16, bool includeSpecialChars = true)
        {
            if (length < 8)
            {
                length = 8;
            }

            var pool = LowercaseLetters + UppercaseLetters + DigitChars + (includeSpecialChars ? SpecialCharacters : string.Empty);
            var chars = new char[length];

            chars[0] = LowercaseLetters[RandomNumberGenerator.GetInt32(LowercaseLetters.Length)];
            chars[1] = UppercaseLetters[RandomNumberGenerator.GetInt32(UppercaseLetters.Length)];
            chars[2] = DigitChars[RandomNumberGenerator.GetInt32(DigitChars.Length)];

            int nextIndex = 3;
            if (includeSpecialChars && length >= 4)
            {
                chars[3] = SpecialCharacters[RandomNumberGenerator.GetInt32(SpecialCharacters.Length)];
                nextIndex = 4;
            }

            for (int i = nextIndex; i < length; i++)
            {
                chars[i] = pool[RandomNumberGenerator.GetInt32(pool.Length)];
            }

            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }

        public static string GenerateSecretKey()
        {
            var chars = new char[24];
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = CrockfordBase32Alphabet[RandomNumberGenerator.GetInt32(CrockfordBase32Alphabet.Length)];
            }

            return $"PK-{new string(chars, 0, 4)}-{new string(chars, 4, 4)}-{new string(chars, 8, 4)}-{new string(chars, 12, 4)}-{new string(chars, 16, 4)}-{new string(chars, 20, 4)}";
        }

        public static byte[] GeneratePasswordHash(string password, byte[] salt)
        {
            return GeneratePasswordHash(password, salt, HASH_ITERATIONS);
        }

        public static byte[] GeneratePasswordHash(string password, byte[] salt, int iterations)
        {
            return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HASH_SIZE);
        }

        public static string GetBase58RandomString(int length)
        {
            return GetRandomString(length, BASE_58_ALBHABET);
        }

        public static string GetRandomString(int length)
        {
            return GetRandomString(length, FULL_ALBHABET);
        }

        private static string GetRandomString(int Length, String ValidSymbols)
        {
            return RandomNumberGenerator.GetString(ValidSymbols, Length);
        }
        
        public static string GetTimeBasedToken(bool isShort = false)
        {
            var ticksBytes = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
            byte[] randomBytes;
            if (isShort)
            {
                randomBytes = Guid.CreateVersion7().ToByteArray();
            }
            else
            {
                randomBytes = new byte[8];
                RandomNumberGenerator.Fill(randomBytes);
            }

            var combined = ticksBytes.Concat(randomBytes).ToArray();
            return Convert.ToBase64String(combined)
                .Replace('+', 'H')
                .Replace('/', 'k')
                .Replace('#', 's')
                .Replace('=', 'i');
        }
    }
}
