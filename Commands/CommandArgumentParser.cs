using MP3PlayerV2.Models;
using System.Globalization;

namespace MP3PlayerV2.Commands
{
    /// <summary>
    /// Provides utility methods for parsing strings into various data types, such as booleans, integers, doubles,
    /// enumerations, ranges, and time intervals.
    /// </summary>
    /// <remarks>This static class includes methods for parsing strings into common data types with additional
    /// handling for edge cases, such as null or whitespace input, and optional constraints like clamping values to a
    /// specified range. The methods are designed to simplify parsing operations and improve robustness in scenarios
    /// where input data may be user-provided or otherwise unstructured.</remarks>
    public static class CommandArgumentParser
    {
        #region Boolean Parsing
        
        /// <summary>
        /// Attempts to parse the specified string as a boolean value.
        /// </summary>
        /// <remarks>The method recognizes the following string values as <see langword="true"/>: "1",
        /// "true", "yes", "on". It recognizes the following string values as <see langword="false"/>: "0", "false",
        /// "no", "off". Any other value will result in a return value of <see langword="false"/> and <paramref
        /// name="result"/> will be set to <see langword="false"/>.</remarks>
        /// <param name="value">The string to parse. The comparison is case-insensitive and ignores leading or trailing whitespace.</param>
        /// <param name="result">When this method returns, contains the parsed boolean value if the conversion succeeded; otherwise, <see
        /// langword="false"/>.</param>
        /// <returns><see langword="true"/> if the string was successfully parsed as a boolean value; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool TryParseBool(this string? value, out bool result)
        {
            result = false;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            switch (value.Trim().ToLowerInvariant())
            {
                case "1":
                case "true":
                case "yes":
                case "on":
                    result = true;
                    return true;

                case "0":
                case "false":
                case "no":
                case "off":
                    result = false;
                    return true;

                default:
                    return false;
            }
        }

        #endregion Boolean Parsing

        #region Integer Parsing

        /// <summary>
        /// Attempts to parse the specified string as an integer, clamping the result to the specified range if
        /// successful.
        /// </summary>
        /// <remarks>If the input string is successfully parsed as an integer, the resulting value is
        /// clamped to the range specified by <paramref name="min"/> and <paramref name="max"/>. If the input string is
        /// <see langword="null"/> or cannot be parsed as an integer, the method returns <see langword="false"/> and
        /// sets <paramref name="result"/> to 0.</remarks>
        /// <param name="value">The string to parse. Can be <see langword="null"/> or empty.</param>
        /// <param name="result">When this method returns, contains the parsed integer value clamped to the range specified by <paramref
        /// name="min"/> and <paramref name="max"/>, if the parsing succeeds; otherwise, contains the default value of
        /// <see cref="int"/> (0).</param>
        /// <param name="min">The minimum allowable value for the parsed integer. Defaults to <see cref="int.MinValue"/>.</param>
        /// <param name="max">The maximum allowable value for the parsed integer. Defaults to <see cref="int.MaxValue"/>.</param>
        /// <returns><see langword="true"/> if the string was successfully parsed as an integer; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool TryParseInt(this string? value, out int result, int min = int.MinValue, int max = int.MaxValue)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                result = Math.Clamp(result, min, max);
                return true;
            }

            result = default;
            return false;
        }

        #endregion Integer Parsing

        #region Double Parsing

        /// <summary>
        /// Attempts to parse the specified string as a double-precision floating-point number.
        /// </summary>
        /// <remarks>The parsing operation uses the invariant culture and allows floating-point and
        /// thousands separators. If the parsed value exceeds the specified range, it will be clamped to the <paramref
        /// name="min"/> and <paramref name="max"/> values.</remarks>
        /// <param name="value">The string to parse. Can be null or empty.</param>
        /// <param name="result">When this method returns, contains the parsed double value, clamped to the specified range if parsing is
        /// successful; otherwise, contains the default value of <see cref="double"/> (0.0).</param>
        /// <param name="min">The minimum allowable value for the parsed result. Defaults to <see cref="double.MinValue"/>.</param>
        /// <param name="max">The maximum allowable value for the parsed result. Defaults to <see cref="double.MaxValue"/>.</param>
        /// <returns><see langword="true"/> if the string was successfully parsed as a double; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool TryParseDouble(this string? value, out double result, double min = double.MinValue, double max = double.MaxValue)
        {
            if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result))
            {
                result = Math.Clamp(result, min, max);
                return true;
            }

            result = default;
            return false;
        }

        #endregion Double Parsing

        #region Range Parsing

        /// <summary>
        /// Parses the range string from the <see cref="PlayerCommand"/> instance and returns the start and end values
        /// as a tuple.
        /// </summary>
        /// <remarks>The range string is expected to be in the format "start-end", where both "start" and
        /// "end" are optional integers.  If only one value is provided, it is treated as the start value, and the end
        /// value will be <see langword="null"/>.  If both values are provided, they are parsed as the start and end of
        /// the range, respectively.</remarks>
        /// <param name="cmd">The <see cref="PlayerCommand"/> instance containing the range string to parse.</param>
        /// <returns>A tuple containing the start and end values of the range, where each value is nullable.  Returns <see
        /// langword="null"/> if the range string is empty, whitespace, or contains unsupported values such as "all" or
        /// "selected".</returns>
        public static (int? Start, int? End)? ParseRange(this PlayerCommand cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd.Range))
                return null;

            var range = cmd.Range.Trim().ToLowerInvariant();

            if (range is "all" or "selected")
                return null;

            var parts = range.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int? start = null, end = null;

            if (parts.Length == 1 && int.TryParse(parts[0], out var single))
            {
                start = single;
            }
            else if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out var s)) start = s;
                if (int.TryParse(parts[1], out var e)) end = e;
            }

            return (start, end);
        }

        #endregion Range Parsing

        #region Enum Parsing

        /// <summary>
        /// Attempts to parse the specified string representation of an enumeration value into the corresponding enum
        /// type.
        /// </summary>
        /// <remarks>This method is a wrapper around <see cref="Enum.TryParse{TEnum}(string, bool, out
        /// TEnum)"/> and provides additional handling for null or whitespace input by returning <see langword="false"/>
        /// in such cases.</remarks>
        /// <typeparam name="TEnum">The type of the enumeration. Must be a struct and an enumeration type.</typeparam>
        /// <param name="value">The string representation of the enumeration value to parse. Can be null or whitespace.</param>
        /// <param name="result">When this method returns, contains the parsed enumeration value of type <typeparamref name="TEnum"/> if the
        /// parsing succeeds; otherwise, the default value of <typeparamref name="TEnum"/>.</param>
        /// <param name="ignoreCase">A boolean value indicating whether the case of the string representation should be ignored during parsing.
        /// The default value is <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if the parsing succeeds; otherwise, <see langword="false"/>.</returns>
        public static bool TryParseEnum<TEnum>(this string? value, out TEnum result, bool ignoreCase = true)
            where TEnum : struct, Enum
        {
            if (!string.IsNullOrWhiteSpace(value) &&
                Enum.TryParse(value, ignoreCase, out result))
            {
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Parses the specified string to an enumeration value of type <typeparamref name="TEnum"/>. Returns a fallback
        /// value if the parsing fails.
        /// </summary>
        /// <remarks>This method attempts to parse the provided string into the specified enumeration
        /// type.  If the string does not represent a valid enumeration value, the method returns the provided fallback
        /// value.</remarks>
        /// <typeparam name="TEnum">The enumeration type to parse the string into.</typeparam>
        /// <param name="value">The string representation of the enumeration value to parse. Can be null.</param>
        /// <param name="fallback">The fallback value to return if parsing fails.</param>
        /// <param name="ignoreCase">A boolean value indicating whether to ignore case during parsing. Defaults to <see langword="true"/>.</param>
        /// <returns>The parsed enumeration value of type <typeparamref name="TEnum"/> if successful; otherwise, the specified
        /// fallback value.</returns>
        public static TEnum ParseEnumOrDefault<TEnum>(this string? value, TEnum fallback, bool ignoreCase = true)
            where TEnum : struct, Enum
            => value.TryParseEnum(out TEnum parsed, ignoreCase) ? parsed : fallback;

        #endregion Enum Parsing

        #region TimeSpan Parsing

        /// <summary>
        /// Attempts to parse the specified string representation of a time interval into a <see cref="TimeSpan"/>
        /// object.
        /// </summary>
        /// <remarks>This method supports parsing both standard <see cref="TimeSpan"/> formats and
        /// shorthand formats where the string ends with: <list type="bullet"> <item><description>'s' for seconds (e.g.,
        /// "30s" for 30 seconds).</description></item> <item><description>'m' for minutes (e.g., "5m" for 5
        /// minutes).</description></item> </list> The parsing is case-insensitive and trims any leading or trailing
        /// whitespace from the input string.</remarks>
        /// <param name="value">The string representation of the time interval to parse. The string can be in a standard <see
        /// cref="TimeSpan"/> format or a shorthand format ending with 's' (seconds) or 'm' (minutes).</param>
        /// <param name="result">When this method returns, contains the <see cref="TimeSpan"/> value equivalent to the time interval
        /// contained in <paramref name="value"/>, if the conversion succeeded; otherwise, <see cref="TimeSpan.Zero"/>.
        /// This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the string was successfully parsed into a <see cref="TimeSpan"/>; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool TryParseTimeSpan(this string? value, out TimeSpan result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result))
                return true;

            value = value.Trim().ToLowerInvariant();

            if (value.EndsWith("s") && double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
            {
                result = TimeSpan.FromSeconds(seconds);
                return true;
            }
            else if (value.EndsWith("m") && double.TryParse(value[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out double minutes))
            {
                result = TimeSpan.FromMinutes(minutes);
                return true;
            }

            return false;
        }

        #endregion TimeSpan Parsing
    }
}
