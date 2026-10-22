namespace OzFwBaseHelpers
{
    public static class StringExtensions
    {
        public static string Mask(this string value, string mask)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            string result = value;
            var charLength = result.Length / 2;
            result = result.Remove(charLength, result.Length - charLength);


            for (int i = charLength; i < value.Length; i++)
            {
                result += mask;
            }

            return result;
        }
    }
}
