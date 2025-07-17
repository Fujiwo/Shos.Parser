# Shos.Parser

A powerful and flexible .NET 8 library for parsing strings to various types and creating object instances from JSON-like strings using reflection and constructor parameter matching.

## Overview

Shos.Parser provides two main parsing capabilities:

1. **TypeParser** - Parse strings to any .NET type using reflection-based type discovery
2. **JsonParser** - Create object instances from JSON-like strings by matching constructor parameters

## Features

### TypeParser
- ? **Universal Type Parsing** - Parse strings to any .NET type (int, double, DateTime, Guid, etc.)
- ? **Nullable Type Support** - Automatic handling of nullable types with empty string conversion to null
- ? **Reflection-Based Discovery** - Automatically finds and invokes static Parse methods
- ? **Intelligent Fallback** - Uses Convert.ChangeType when Parse methods are unavailable
- ? **Culture-Independent** - Uses InvariantCulture for consistent parsing behavior
- ? **Exception-Safe TryParse** - Safe parsing without exception handling

### JsonParser
- ? **JSON-like String Parsing** - Parse simple "key:value,key:value" format strings
- ? **Constructor Matching** - Automatically matches keys to constructor parameter names
- ? **Case-Insensitive Matching** - Parameter names are matched case-insensitively
- ? **Multiple Constructor Support** - Automatically selects the best matching constructor
- ? **Property Setting Support** - Can set properties after construction
- ? **Complex Type Support** - Handles DateTime, Guid, and custom types
- ? **Flexible Input** - Supports both string and key-value array inputs

## Installation

Add the project reference or copy the source files to your project targeting .NET 8.

## Quick Start

### TypeParser Usage
using Shos.Parser;

// Basic type parsing
var intValue = TypeParser.Parse(typeof(int), "123");           // Returns 123
var doubleValue = TypeParser.Parse(typeof(double), "123.45");  // Returns 123.45
var boolValue = TypeParser.Parse(typeof(bool), "true");        // Returns true

// DateTime and Guid parsing
var dateValue = TypeParser.Parse(typeof(DateTime), "2024-01-01");
var guidValue = TypeParser.Parse(typeof(Guid), "12345678-1234-1234-1234-123456789abc");

// Nullable type handling
var nullableInt = TypeParser.Parse(typeof(int?), "123");       // Returns 123
var nullValue = TypeParser.Parse(typeof(int?), "");            // Returns null

// Safe parsing with TryParse
var (success, value) = TypeParser.TryParse(typeof(int), "invalid");
if (success) {
    Console.WriteLine($"Parsed: {value}");
} else {
    Console.WriteLine("Parsing failed");
}
### JsonParser Usage
using Shos.Parser;

// Define your class
public class Person
{
    public string Name { get; }
    public int Age { get; }
    
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

// Parse from JSON-like string
var person = JsonParser.Parse<Person>("name:John,age:25");
Console.WriteLine($"{person.Name} is {person.Age} years old");

// Parse from key-value array
var keyValues = new[] { ("name", "Alice"), ("age", "30") };
var person2 = JsonParser.Parse<Person>(keyValues);

// Complex types with multiple constructors
public class Employee
{
    public string Name { get; }
    public int Age { get; }
    public string Department { get; }
    
    public Employee(string name) 
    {
        Name = name;
        Age = 0;
        Department = "Unknown";
    }
    
    public Employee(string name, int age, string department)
    {
        Name = name;
        Age = age;
        Department = department;
    }
}

// Automatically selects the constructor with more matching parameters
var employee = JsonParser.Parse<Employee>("name:Bob,age:35,department:Engineering");
## Advanced Features

### Multiple Constructor Support

JsonParser automatically selects the best matching constructor based on available parameters:
public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    public bool InStock { get; }
    
    public Product(string name) 
    {
        Name = name;
        Price = 0;
        InStock = false;
    }
    
    public Product(string name, decimal price, bool inStock)
    {
        Name = name;
        Price = price;
        InStock = inStock;
    }
}

// Uses the 3-parameter constructor
var fullProduct = JsonParser.Parse<Product>("name:Laptop,price:999.99,inStock:true");

// Uses the 1-parameter constructor when only name is provided
var basicProduct = JsonParser.Parse<Product>("name:Mouse");
### Property Setting Support

JsonParser can also set properties after construction:
public class Staff
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public Staff() { }
    
    public Staff(string name) => Name = name;
}

// Creates with constructor parameter and sets property
var staff = JsonParser.Parse<Staff>("name:John,number:101");
### Case-Insensitive Parameter Matching
// All of these work the same way
var person1 = JsonParser.Parse<Person>("name:John,age:25");
var person2 = JsonParser.Parse<Person>("NAME:John,AGE:25");
var person3 = JsonParser.Parse<Person>("Name:John,Age:25");
### Complex Type Parsing
public class ComplexObject
{
    public string Name { get; }
    public int Number { get; }
    public DateTime Date { get; }
    public Guid Id { get; }
    
    public ComplexObject(string name, int number, DateTime date, Guid id)
    {
        Name = name;
        Number = number;
        Date = date;
        Id = id;
    }
}

var complex = JsonParser.Parse<ComplexObject>(
    "name:Test,number:42,date:2024-01-01,id:12345678-1234-1234-1234-123456789abc"
);
## Supported Types

### TypeParser Supported Types
- All primitive types (int, double, float, decimal, bool, etc.)
- DateTime and DateTimeOffset
- Guid
- Enums
- Nullable versions of all above types
- Any type with a static Parse(string) method
- Any type supported by Convert.ChangeType

### JsonParser Supported Types
- All types supported by TypeParser
- Custom classes with public constructors
- Classes with parameterless constructors and settable properties
- Classes with multiple constructor overloads

## Error Handling

### TypeParser
- **Parse method**: Throws exceptions for invalid input (ArgumentNullException, FormatException, OverflowException)
- **TryParse method**: Returns (false, null) for any parsing failure

### JsonParser
- Returns `null` when no suitable constructor can be found or parameter parsing fails
- Ignores extra parameters that don't match any constructor parameters
- Handles malformed JSON-like strings gracefully

## Extension Methods

The library also includes helpful extension methods:
// String extensions
string text = "hello world";
string pascalCase = text.ToPascalCase(); // "Hello world"

// Enumerable extensions
var items = new[] { 1, 2, 3 };
items.ForEach(x => Console.WriteLine(x));
## Requirements

- .NET 8.0 or later
- C# 12.0 language features

## Contributing

This project uses modern C# features including:
- Nullable reference types
- Pattern matching
- Tuple deconstruction
- Collection expressions
- Target-typed new expressions

## License

[Add your license information here]

## Examples

Check the test files for comprehensive examples:
- `TypeParserTests.cs` - Examples of TypeParser usage
- `JsonParserTests.cs` - Examples of JsonParser usage

## Performance Notes

- TypeParser uses reflection for method discovery but caches aren't implemented - consider caching for high-performance scenarios
- JsonParser creates objects through constructor invocation which is generally fast
- InvariantCulture is used for consistent, culture-independent parsing

## API Reference

### TypeParser
public static class TypeParser
{
    public static object? Parse(Type type, string text)
    public static (bool canParse, object? result) TryParse(Type type, string text)
}
### JsonParser
public static class JsonParser
{
    public static T? Parse<T>(string jsonString)
    public static T? Parse<T>((string key, string valueText)[] keyValueTexts)
}