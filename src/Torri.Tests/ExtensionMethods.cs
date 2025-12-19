namespace Torri.Tests;

public static class ExtensionMethods
{
    public static string TrimEnd(this string input, string suffixToRemove,
        StringComparison comparisonType = StringComparison.CurrentCulture)
    {
        if (!string.IsNullOrEmpty(suffixToRemove) && input.EndsWith(suffixToRemove, comparisonType))
            return input[..^suffixToRemove.Length];

        return input;
    }
}