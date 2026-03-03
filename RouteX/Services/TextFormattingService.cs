using System.Globalization;

namespace RouteX.Services
{
    public interface ITextFormattingService
    {
        string CapitalizeEachWord(string input);
        string CapitalizeFirstLetter(string input);
        string FormatName(string input);
    }

    public class TextFormattingService : ITextFormattingService
    {
        public string CapitalizeEachWord(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var words = input.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }
            return string.Join(" ", words);
        }

        public string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }

        public string FormatName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Handle special cases like "McDonald", "O'Neill", etc.
            var name = input.ToLower().Trim();
            
            // Handle common name prefixes and special cases
            var specialCases = new Dictionary<string, string>
            {
                {"mc", "Mc"},
                {"o'", "O'"},
                {"mac", "Mac"},
                {"van", "van"},
                {"de", "de"},
                {"von", "von"}
            };

            foreach (var special in specialCases)
            {
                if (name.StartsWith(special.Key))
                {
                    var remaining = name.Substring(special.Key.Length);
                    return special.Value + CapitalizeFirstLetter(remaining);
                }
            }

            return CapitalizeFirstLetter(name);
        }
    }
}
