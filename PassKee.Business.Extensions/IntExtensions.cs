namespace PassKee.Business.Extensions
{
    public static class IntExtensions
    {
        public static int Pow(this int number, uint power)
        {
            int result = 1;
            for (int i = 0; i < power; i++)
            {
                result *= number;
            }
            return result;
        }

        public static bool IsPositive(this int number)
        {
            return ((long)number).IsPositive();
        }

        public static bool IsPositive(this int? number)
        {
            return number.HasValue && number.Value.IsPositive();
        }
        
        public static bool IsPositive(this long number)
        {
            return (number > 0);
        }

        public static bool IsPositive(this long? number)
        {
            return number.HasValue && number.Value.IsPositive();
        }
    }
}
