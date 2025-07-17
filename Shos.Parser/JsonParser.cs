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
/// Additionally, it can set properties on the created instances for values that don't match constructor parameters.
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
        // Parse the JSON-like string into key-value pairs using LINQ transformations
        // This creates a processing pipeline to convert the string format into structured data
        var keyValueTexts = jsonString.Split(',')
                            // Split each comma-separated part by colon to separate key from value
                            .Select(text => text.Split(':'))
                            // Trim whitespace from both keys and values for cleaner processing
                            .Select(texts => texts.Select(text => text.Trim()).ToArray())
                            // Filter out malformed pairs - only keep items with exactly 2 parts (key and value)
                            .Where(texts => texts.Length == 2)
                            // Convert string arrays to named tuples for easier handling
                            .Select(texts => (texts[0], texts[1]))
                            .ToArray();

        // Delegate to the overloaded method that handles the actual object creation logic
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
        // Get the target type's metadata and discover all public constructors
        var type = typeof(T);
        var constructors = type.GetConstructors();

        // Build a candidate list by evaluating each constructor's compatibility with the provided data
        // This uses a multi-step LINQ pipeline to filter and rank constructors
        var candidateConstructors = constructors
            // For each constructor, attempt to create parameter values from the key-value pairs
            .Select(constructor => (constructor, CreateParameterKeyValues(constructor, keyValueTexts)))
            // Only keep constructors where ALL parameters can be matched and parsed successfully
            .Where(pair => pair.Item2.isMatch)
            // Prioritize constructors with more parameters (more specific/complete object initialization)
            .OrderByDescending(pair => pair.Item2.parameterKeyValues.Length)
            // Simplify the tuple structure for easier consumption
            .Select(pair => (pair.Item1, pair.Item2.parameterKeyValues))
            .ToArray();

        // If we found at least one viable constructor, proceed with object creation
        if (candidateConstructors.Length > 0) {
            var (constructor, parameterKeyValues) = candidateConstructors[0];
            
            // Extract just the values from the key-value pairs for constructor invocation
            var parameterValues = parameterKeyValues.Select(parameterKeyValue => parameterKeyValue.value).ToArray();
            
            // Use reflection to invoke the constructor with the parsed parameter values
            var item = (T?)constructor.Invoke(parameterValues);
            
            // If object creation succeeded, attempt to set any remaining properties
            // that weren't handled by the constructor parameters
            if (item is not null)
                SetProperties(item, parameterKeyValues, keyValueTexts);
            
            return item;
        }

        // No suitable constructor found - return the default value for the type
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
    /// - parameterKeyValues: an array of key-value pairs with parsed values if successful, empty array if failed
    /// </returns>
    /// <remarks>
    /// Only public, non-static constructors are considered for parameter matching.
    /// This method delegates to the parameter-specific overload for the actual matching logic.
    /// </remarks>
    static (bool isMatch, (string key, object value)[] parameterKeyValues) CreateParameterKeyValues(ConstructorInfo constructor, (string key, string valueText)[] keyValueTexts)
    {
        // Filter out unsuitable constructors early to avoid unnecessary processing
        // Static constructors and non-public constructors cannot be used for object instantiation
        if (!constructor.IsPublic || constructor.IsStatic)
            return (false, []);

        // Get the constructor's parameter metadata and delegate to the core matching logic
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
    /// - parameterKeyValues: an array of key-value pairs with parsed values if successful, empty array if failed
    /// </returns>
    /// <remarks>
    /// Parameter matching is case-insensitive. All parameters must be successfully matched and parsed
    /// for the method to return a successful match. Uses TypeParser.TryParse for value conversion.
    /// If any parameter cannot be matched or parsed, the entire match fails.
    /// </remarks>
    static (bool isMatch, (string key, object value)[] parameterKeyValues) CreateParameterKeyValues(ParameterInfo[] parameters, (string key, string valueText)[] keyValueTexts)
    {
        // Initialize a collection to store successfully parsed parameter key-value pairs
        List<(string key, object value)> keyValues = new();

        // Process each parameter required by the constructor
        foreach (var parameter in parameters) {
            // Attempt to find a matching key-value pair for this parameter
            // The matching is case-insensitive to be user-friendly
            var matchedValueText = keyValueTexts.FirstOrDefault(
                keyValueText => (parameter?.Name ?? "").Equals(keyValueText.key, StringComparison.OrdinalIgnoreCase)
            );

            // Check if we found a matching key-value pair
            if (matchedValueText != default) {
                // Attempt to parse the string value to the parameter's required type
                var (canParse, value) = TypeParser.TryParse(parameter.ParameterType, matchedValueText.valueText);

                // Verify that parsing succeeded and produced a non-null result
                // Note: This approach means nullable parameters with intentional null values will cause match failure
                // This is a design choice to ensure explicit value provision
                if (canParse && value is not null) {
                    // Store the successfully parsed key-value pair for later use
                    keyValues.Add((matchedValueText.key, value));
                } else {
                    // Parsing failed or resulted in null - this constructor cannot be used
                    // Return early to avoid unnecessary processing
                    return (false, []);
                }
            } else {
                // No matching key found for this required parameter
                // The constructor cannot be satisfied with the available data
                return (false, []);
            }
        }

        // All parameters were successfully matched and parsed
        return (true, keyValues.ToArray());
    }

    /// <summary>
    /// Sets properties on the created object instance for key-value pairs that weren't used in constructor parameters.
    /// This allows for hybrid object initialization using both constructor parameters and property setters.
    /// </summary>
    /// <param name="item">The object instance to set properties on.</param>
    /// <param name="parameterKeyValues">The key-value pairs that were used for constructor parameters.</param>
    /// <param name="keyValueTexts">All available key-value pairs from the input.</param>
    /// <remarks>
    /// This method identifies which key-value pairs weren't consumed by the constructor
    /// and attempts to set them as properties on the created object instance.
    /// </remarks>
    static void SetProperties(object item, (string key, object value)[] parameterKeyValues, (string key, string valueText)[] keyValueTexts)
    {
        // Extract the keys that were already used for constructor parameters
        var parameterKeys = parameterKeyValues.Select(parameterKeyValue => parameterKeyValue.key).ToArray();
        
        // Find the remaining key-value pairs that weren't used in constructor initialization
        var remainingKeyValueTexts = keyValueTexts.Where(keyValueText => !parameterKeys.Contains(keyValueText.key));
        
        // Attempt to set each remaining key-value pair as a property on the object
        remainingKeyValueTexts.ForEach(remainingKeyValueText => SetProperty(item, remainingKeyValueText));
    }

    /// <summary>
    /// Attempts to set a single property on an object instance using reflection.
    /// Supports both exact case matching and PascalCase conversion for property names.
    /// </summary>
    /// <param name="item">The object instance to set the property on.</param>
    /// <param name="keyValueText">The key-value pair containing the property name and string value.</param>
    /// <remarks>
    /// This method first tries to find a property with the exact key name (case-sensitive),
    /// then falls back to trying the key converted to PascalCase (e.g., "firstName" -> "FirstName").
    /// The property must be public, instance-level, and have a setter to be modified.
    /// </remarks>
    static void SetProperty(object item, (string key, string valueText) keyValueText)
    {
        // Get the runtime type of the object instance for reflection operations
        var type = item.GetType();
        
        // Try to find a property with the exact key name
        var property = type.GetProperty(keyValueText.key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.IgnoreCase);

        // If no suitable property was found, exit gracefully without throwing an exception
        if (property is null)
            return;

        // Attempt to parse the string value to the property's type
        var (canParse, value) = TypeParser.TryParse(property.PropertyType, keyValueText.valueText);
        
        // Only proceed if parsing succeeded and produced a non-null value
        if (!canParse || value is null)
            return;

        // Use reflection to set the property value on the object instance
        property.SetValue(item, value);
    }
}
