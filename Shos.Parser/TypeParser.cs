namespace Shos.Parser;

using System;
using System.Globalization;
using System.Reflection;

/// <summary>
/// Provides methods for parsing strings to various .NET types using reflection and type conversion.
/// Supports both nullable and non-nullable types with intelligent fallback mechanisms.
/// </summary>
/// <remarks>
/// This class uses a dual approach for type parsing:
/// 1. First attempts to find and invoke a static Parse(string) method on the target type using reflection
/// 2. Falls back to Convert.ChangeType with InvariantCulture for types without Parse methods
/// 
/// Special handling is provided for nullable types, allowing empty strings to be converted to null values.
/// </remarks>
public class TypeParser
{
    /// <summary>
    /// Parses a string value to the specified type using reflection to find the Parse method,
    /// or falls back to Convert.ChangeType if no Parse method is available.
    /// </summary>
    /// <param name="type">
    /// The target type to parse the string into. Cannot be null.
    /// Supports both nullable and non-nullable types.
    /// </param>
    /// <param name="text">
    /// The string value to parse. Cannot be null, except for nullable types where 
    /// null or empty strings will return null.
    /// </param>
    /// <returns>
    /// The parsed value as an object of the specified type, or null for nullable types 
    /// with empty/null strings. The returned object will need to be cast to the appropriate type.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="type"/> is null, or when <paramref name="text"/> is null 
    /// for non-nullable types.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when the string cannot be parsed to the target type due to invalid format.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when the parsed value is outside the valid range of the target type.
    /// </exception>
    /// <exception cref="TargetInvocationException">
    /// Thrown when the reflection-based Parse method invocation fails.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method handles nullable types by extracting the underlying type using 
    /// Nullable.GetUnderlyingType() and returning null for empty strings.
    /// </para>
    /// <para>
    /// The parsing strategy:
    /// 1. Validates input parameters
    /// 2. Handles nullable type extraction
    /// 3. Returns null for nullable types with empty input
    /// 4. Uses reflection to find a static Parse(string) method
    /// 5. Falls back to Convert.ChangeType with InvariantCulture
    /// </para>
    /// <para>
    /// InvariantCulture is used to ensure consistent parsing behavior regardless of 
    /// the system's current culture settings.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Basic type parsing
    /// var intValue = TypeParser.Parse(typeof(int), "123");           // Returns 123
    /// var doubleValue = TypeParser.Parse(typeof(double), "123.45");  // Returns 123.45
    /// var boolValue = TypeParser.Parse(typeof(bool), "true");        // Returns true
    /// 
    /// // DateTime and Guid parsing
    /// var dateValue = TypeParser.Parse(typeof(DateTime), "2024-01-01");
    /// var guidValue = TypeParser.Parse(typeof(Guid), "12345678-1234-1234-1234-123456789abc");
    /// 
    /// // Nullable type handling
    /// var nullableInt = TypeParser.Parse(typeof(int?), "123");       // Returns 123
    /// var nullValue = TypeParser.Parse(typeof(int?), "");            // Returns null
    /// 
    /// // String parsing (no conversion needed)
    /// var stringValue = TypeParser.Parse(typeof(string), "hello");   // Returns "hello"
    /// </code>
    /// </example>
    public static object? Parse(Type type, string text)
    {
        // Input validation - ensure required parameters are not null
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (text is null)
            throw new ArgumentNullException(nameof(text));

        // Handle nullable types by extracting the underlying type
        // For example: int? -> int, DateTime? -> DateTime
        // If the type is not nullable, targetType will be the same as the original type
        var targetType = Nullable.GetUnderlyingType(type) ?? type;

        // Special case for nullable types: return null when given an empty string
        // This allows nullable types to gracefully handle empty input by converting to null
        // We check if type != targetType to determine if the original type was nullable
        if (string.IsNullOrEmpty(text) && type != targetType)
            return null;

        // Attempt to find a static Parse method that accepts a single string parameter
        // This covers most built-in .NET types like int, double, DateTime, Guid, etc.
        // BindingFlags.Public | BindingFlags.Static ensures we only find public static methods
        var parseMethod = targetType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static,
                                               null, new[] { typeof(string) }, null);

        // If a Parse method was found, invoke it with the input text
        // This uses reflection to call the method dynamically at runtime
        if (parseMethod is not null)
            return parseMethod.Invoke(null, new object[] { text });

        // Fallback mechanism: if no Parse method exists, use Convert.ChangeType
        // This handles types that don't have a Parse method but can be converted
        // InvariantCulture ensures consistent behavior regardless of system locale
        return Convert.ChangeType(text, targetType, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Attempts to parse a string value to the specified type without throwing exceptions.
    /// This is a safe wrapper around the Parse method that provides exception-free parsing.
    /// </summary>
    /// <param name="type">
    /// The target type to parse the string into. Should follow the same rules as the Parse method.
    /// </param>
    /// <param name="text">
    /// The string value to parse. Should follow the same rules as the Parse method.
    /// </param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    /// <item><description>canParse: true if parsing succeeded, false if any exception occurred</description></item>
    /// <item><description>result: the parsed value if successful, null if parsing failed</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method internally calls the Parse method and catches all exceptions,
    /// converting them into a boolean success indicator. This provides a safe way to
    /// attempt parsing without having to handle exceptions at the call site.
    /// </para>
    /// <para>
    /// All exceptions from the Parse method (ArgumentNullException, FormatException, 
    /// OverflowException, TargetInvocationException, etc.) are caught and result in 
    /// a return value of (false, null).
    /// </para>
    /// <para>
    /// This method is particularly useful when you need to attempt parsing multiple 
    /// values and want to continue processing even if some fail, or when you need
    /// to validate input without disrupting program flow.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Safe parsing with success checking
    /// var (success, value) = TypeParser.TryParse(typeof(int), "123");
    /// if (success) {
    ///     Console.WriteLine($"Parsed value: {value}");
    /// } else {
    ///     Console.WriteLine("Parsing failed");
    /// }
    /// 
    /// // Using tuple deconstruction for cleaner code
    /// var (canParseBool, boolResult) = TypeParser.TryParse(typeof(bool), "true");
    /// var (canParseInt, intResult) = TypeParser.TryParse(typeof(int), "invalid");
    /// 
    /// // canParseBool will be true, boolResult will be true
    /// // canParseInt will be false, intResult will be null
    /// 
    /// // Batch processing example
    /// string[] inputs = { "123", "invalid", "456" };
    /// foreach (var input in inputs) {
    ///     var (canParse, result) = TypeParser.TryParse(typeof(int), input);
    ///     if (canParse) {
    ///         Console.WriteLine($"Successfully parsed: {result}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public static (bool canParse, object? result) TryParse(Type type, string text)
    {
        try {
            // Attempt to parse using the main Parse method
            // If successful, return true with the parsed result
            return (true, Parse(type, text));
        } catch (Exception) {
            // If any exception occurs during parsing, return false with null result
            // This includes all possible exceptions from Parse method:
            // - ArgumentNullException: null parameters
            // - FormatException: invalid format
            // - OverflowException: value out of range
            // - TargetInvocationException: reflection method invocation failed
            // - InvalidCastException: conversion not supported
            // - Any other exceptions from custom Parse methods
            return (false, null);
        }
    }
}
