using System.Text.RegularExpressions;

namespace KDG.Boilerplate.Server.Validation;

/// <summary>
/// Transforms validation error keys to a consistent, client-friendly format.
/// Converts array bracket notation to dot notation.
/// Example: Items[0].Quantity -> Items.0.Quantity
/// </summary>
public static partial class ErrorKeyTransformer
{
    [GeneratedRegex(@"\[(\d+)\]")]
    private static partial Regex ArrayBracketRegex();

    /// <summary>
    /// Transforms a single error key to dot notation.
    /// </summary>
    public static string Transform(string key)
    {
        // Replace array bracket notation [n] with .n
        return ArrayBracketRegex().Replace(key, ".$1");
    }

    /// <summary>
    /// Transforms all keys in an error dictionary.
    /// </summary>
    public static Dictionary<string, string[]> TransformKeys(IDictionary<string, string[]> errors)
    {
        return errors.ToDictionary(
            kvp => Transform(kvp.Key),
            kvp => kvp.Value
        );
    }
}

