namespace SpektraGames.SpektraUtilities.Runtime
{
    public static class StringExtensions
    {
        public static string FirstCharacterToUpper(this string s)
        {
            if (string.IsNullOrEmpty(s) || char.IsUpper(s, 0))
            {
                return s;
            }

            return char.ToUpperInvariant(s[0]) + s[1..];
        }
    }
}