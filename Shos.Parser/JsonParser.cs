namespace Shos.Parser;

using System;
using System.Reflection;

/// <summary>
/// Provides functionality to parse JSON-like strings and create instances of specified types
/// using constructor parameter matching with key-value pairs.
/// </summary>
/// <remarks>
/// This class supports creating objects from simple JSON-like strings (key:value,key:value format)
/// by matching the keys to constructor parameter names and parsing the values using TypeParser.
/// </remarks>
public class JsonParser
{
    /// <summary>
    /// Creates an instance of type T from a JSON-like string containing comma-separated key:value pairs.
    /// </summary>
    /// <typeparam name="T">The type to create an instance of.</typeparam>
    /// <param name="jsonString">
    /// A string containing comma-separated key:value pairs (e.g., "name:John,age:25").
    /// Keys should match constructor parameter names (case-insensitive).
    /// </param>
    /// <returns>
    /// An instance of type T if a matching constructor is found and all parameters can be parsed,
    /// otherwise the default value of T (null for reference types).
    /// </returns>
    /// <remarks>
    /// This method parses the JSON-like string into key-value pairs and delegates to the
    /// overloaded Parse method that accepts an array of tuples. The parsing is lenient
    /// and will ignore malformed key-value pairs.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Person {
    ///     public Person(string name, int age) { Name = name; Age = age; }
    ///     public string Name { get; }
    ///     public int Age { get; }
    /// }
    /// 
    /// var person = JsonParser.Parse&lt;Person&gt;("name:John,age:25");
    /// // Creates: new Person("John", 25)
    /// </code>
    /// </example>
    public static T? Parse<T>(string jsonString)
    {
        // Parse the JSON-like string into key-value pairs
        // Split by comma to get individual key:value pairs
        var keyValueTexts = jsonString.Split(',')
                            // Split each part by colon to separate key from value
                            .Select(text => text.Split(':'))
                            // Trim whitespace from each part
                            .Select(texts => texts.Select(text => text.Trim()).ToArray())
                            // Only keep pairs that have exactly 2 parts (key and value)
                            .Where(texts => texts.Length == 2)
                            // Convert to tuple format for easier handling
                            .Select(texts => (texts[0], texts[1]))
                            .ToArray();

        // Delegate to the overloaded method that handles the actual object creation
        return Parse<T>(keyValueTexts);
    }

    /// <summary>
    /// Creates an instance of type T using constructor parameter matching with the provided key-value pairs.
    /// </summary>
    /// <typeparam name="T">The type to create an instance of.</typeparam>
    /// <param name="keyValueTexts">
    /// An array of tuples containing parameter names (keys) and their string values.
    /// Parameter names are matched case-insensitively with constructor parameter names.
    /// </param>
    /// <returns>
    /// An instance of type T if a matching constructor is found and all parameters can be parsed,
    /// otherwise the default value of T (null for reference types).
    /// </returns>
    /// <remarks>
    /// This method finds the best matching constructor by:
    /// 1. Filtering constructors that can have all parameters matched and parsed successfully
    /// 2. Ordering by the number of parameters (preferring constructors with more parameters)
    /// 3. Using the first (best) matching constructor to create the instance
    /// 
    /// Parameter matching is case-insensitive and uses TypeParser.TryParse for value conversion.
    /// All constructor parameters must be successfully matched and parsed for the constructor to be viable.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class Person {
    ///     public Person(string name, int age) { Name = name; Age = age; }
    ///     public string Name { get; }
    ///     public int Age { get; }
    /// }
    /// 
    /// var keyValues = new[] { ("name", "John"), ("age", "25") };
    /// var person = JsonParser.Parse&lt;Person&gt;(keyValues);
    /// // Creates: new Person("John", 25)
    /// </code>
    /// </example>
    public static T? Parse<T>((string key, string valueText)[] keyValueTexts)
    {
        // Get the target type and all its public constructors
        var type = typeof(T);
        var constructors = type.GetConstructors();

        // Find the best matching constructor by evaluating each one
        var candidateConstructors = constructors
            // For each constructor, determine if it can be satisfied with the provided key-value pairs
            .Select(constructor => (constructor, CreateParameterKeyValues(constructor, keyValueTexts)))
            // Only keep constructors where all parameters can be matched and parsed
            .Where(pair => pair.Item2.isMatch)
            // Prefer constructors with more parameters (more specific matches)
            .OrderByDescending(pair => pair.Item2.parameterKeyValues.Length)
            // Extract the constructor and parameter values for easier handling
            .Select(pair => (pair.Item1, pair.Item2.parameterKeyValues))
            .ToArray();

        // If we found at least one viable constructor, use the best one
        if (candidateConstructors.Length > 0) {
            var (constructor, parameterKeyValues) = candidateConstructors[0];
            // Invoke the constructor with the parsed parameter values
            var parameterValues = parameterKeyValues.Select(parameterKeyValue => parameterKeyValue.value).ToArray();
            var item = (T?)constructor.Invoke(parameterValues);
            if (item is not null)
                SetProperties(item, parameterKeyValues, keyValueTexts);
            return item;
        }

        // No suitable constructor found, return default value
        return default;
    }

    /// <summary>
    /// Creates parameter values for a specific constructor by matching key-value pairs to parameter names.
    /// </summary>
    /// <param name="constructor">The constructor to create parameter values for.</param>
    /// <param name="keyValueTexts">The key-value pairs to match against parameter names.</param>
    /// <returns>
    /// A tuple containing:
    /// - isMatch: true if all parameters can be matched and parsed successfully, false otherwise
    /// - parameterValues: an array of parsed parameter values if successful, empty array if failed
    /// </returns>
    /// <remarks>
    /// Only public, non-static constructors are considered for parameter matching.
    /// This method delegates to the parameter-specific overload for the actual matching logic.
    /// </remarks>
    static (bool isMatch, (string key, object value)[] parameterKeyValues) CreateParameterKeyValues(ConstructorInfo constructor, (string key, string valueText)[] keyValueTexts)
    {
        // Only consider public, non-static constructors
        // Static constructors and non-public constructors are not suitable for object instantiation
        if (!constructor.IsPublic || constructor.IsStatic)
            return (false, []);

        // Get the constructor's parameters and delegate to the parameter matching logic
        var parameters = constructor.GetParameters() ?? [];
        return CreateParameterKeyValues(parameters, keyValueTexts);
    }

    /// <summary>
    /// Creates parameter values by matching key-value pairs to parameter information.
    /// This is the core logic for determining if a constructor can be satisfied with the given key-value pairs.
    /// </summary>
    /// <param name="parameters">The parameter information from a constructor to match against.</param>
    /// <param name="keyValueTexts">The key-value pairs to match against parameter names.</param>
    /// <returns>
    /// A tuple containing:
    /// - isMatch: true if all parameters can be matched and parsed successfully, false otherwise
    /// - parameterValues: an array of parsed parameter values if successful, empty array if failed
    /// </returns>
    /// <remarks>
    /// Parameter matching is case-insensitive. All parameters must be successfully matched and parsed
    /// for the method to return a successful match. Uses TypeParser.TryParse for value conversion.
    /// If any parameter cannot be matched or parsed, the entire match fails.
    /// </remarks>
    static (bool isMatch, (string key, object value)[] parameterKeyValues) CreateParameterKeyValues(ParameterInfo[] parameters, (string key, string valueText)[] keyValueTexts)
    {
        // Collection to store successfully parsed parameter values
        List<(string key, object value)> keyValues = new();

        // Process each parameter in the constructor
        foreach (var parameter in parameters) {
            // Try to find a matching key-value pair for this parameter
            // Matching is case-insensitive using ToLower()
            var matchedValueText = keyValueTexts.FirstOrDefault(keyValueText =>
                (parameter?.Name?.ToLower() ?? "") == keyValueText.key.ToLower());

            if (matchedValueText != default) {
                // Found a matching key-value pair, try to parse the value
                var (canParse, value) = TypeParser.TryParse(parameter.ParameterType, matchedValueText.valueText);

                // Only proceed if parsing succeeded and we got a non-null value
                // Note: This means nullable parameters with null values will fail the match
                if (canParse && value is not null) {
                    keyValues.Add((matchedValueText.key, value));
                } else {
                    // Parsing failed or resulted in null, this constructor can't be used
                    return (false, []);
                }
            } else {
                // No matching key found for this parameter, constructor can't be satisfied
                return (false, []);
            }
        }

        // All parameters were successfully matched and parsed
        return (true, keyValues.ToArray());
    }

    static void SetProperties(object item, (string key, object value)[] parameterKeyValues, (string key, string valueText)[] keyValueTexts)
    {
        var parameterValues = parameterKeyValues.Select(parameterKeyValues => parameterKeyValues.key).ToArray();
        var remainKeyValueTexts = keyValueTexts.Where(keyValueText => !parameterValues.Contains(keyValueText.key));
        remainKeyValueTexts.ForEach(remainKeyValueText => SetProperty(item, remainKeyValueText));
    }

    static void SetProperty(object item, (string key, string valueText) keyValueText)
    {
        var type = item.GetType();
        var property = type.GetProperty(keyValueText.key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);
        if (property is null)
            property = type.GetProperty(keyValueText.key.ToPascalCase(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty);

        if (property is null)
            return;

        var (canParse, value) = TypeParser.TryParse(property.PropertyType, keyValueText.valueText);
        if (!canParse || value is null)
            return;

        property.SetValue(item, value);
    }
}
