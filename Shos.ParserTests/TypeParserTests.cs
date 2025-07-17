using System.Reflection;

namespace Shos.Parser.Tests
{
    [TestClass()]
    public class TypeParserTests
    {
        [TestMethod()]
        public void ParseTest()
        {
            // Test parsing basic types
            Assert.AreEqual(123, TypeParser.Parse(typeof(int), "123"));
            Assert.AreEqual(123L, TypeParser.Parse(typeof(long), "123"));
            Assert.AreEqual(123.45, TypeParser.Parse(typeof(double), "123.45"));
            Assert.AreEqual(123.45f, TypeParser.Parse(typeof(float), "123.45"));
            Assert.AreEqual(123.45m, TypeParser.Parse(typeof(decimal), "123.45"));
            Assert.AreEqual(true, TypeParser.Parse(typeof(bool), "true"));
            Assert.AreEqual(false, TypeParser.Parse(typeof(bool), "false"));

            // Test parsing DateTime
            var expectedDate = DateTime.Parse("2024-01-01");
            Assert.AreEqual(expectedDate, TypeParser.Parse(typeof(DateTime), "2024-01-01"));

            // Test parsing Guid
            var guidString = "12345678-1234-1234-1234-123456789abc";
            var expectedGuid = Guid.Parse(guidString);
            Assert.AreEqual(expectedGuid, TypeParser.Parse(typeof(Guid), guidString));

            // Test parsing string
            Assert.AreEqual("hello world", TypeParser.Parse(typeof(string), "hello world"));

            // Test parsing nullable types with values
            Assert.AreEqual(123, TypeParser.Parse(typeof(int?), "123"));
            Assert.AreEqual(123.45, TypeParser.Parse(typeof(double?), "123.45"));
            Assert.AreEqual(true, TypeParser.Parse(typeof(bool?), "true"));

            // Test parsing nullable types with empty string (should return null)
            Assert.AreEqual(null, TypeParser.Parse(typeof(int?), ""));
            Assert.AreEqual(null, TypeParser.Parse(typeof(double?), ""));
            Assert.AreEqual(null, TypeParser.Parse(typeof(bool?), ""));

            // Test parsing nullable types with null string (should return null)
#pragma warning disable CS8625
            Assert.ThrowsException<ArgumentNullException>(() => TypeParser.Parse(typeof(int?), null));
#pragma warning restore CS8625

            // Test ArgumentNullException for null type
#pragma warning disable CS8625
            Assert.ThrowsException<ArgumentNullException>(() => TypeParser.Parse(null, "123"));
#pragma warning restore CS8625

            // Test ArgumentNullException for null text with non-nullable type
#pragma warning disable CS8625
            Assert.ThrowsException<ArgumentNullException>(() => TypeParser.Parse(typeof(int), null));
#pragma warning restore CS8625

            // Test parsing that should throw exceptions for invalid formats
            Assert.ThrowsException<TargetInvocationException>(() => TypeParser.Parse(typeof(int), "invalid"));
            Assert.ThrowsException<TargetInvocationException>(() => TypeParser.Parse(typeof(double), "not_a_number"));
        }

        [TestMethod()]
        public void TryParseTest()
        {
            // Test successful parsing of int
            var (canParseInt, resultInt) = TypeParser.TryParse(typeof(int), "123");
            Assert.IsTrue(canParseInt);
            Assert.AreEqual(123, resultInt);

            // Test successful parsing of double
            var (canParseDouble, resultDouble) = TypeParser.TryParse(typeof(double), "123.45");
            Assert.IsTrue(canParseDouble);
            Assert.AreEqual(123.45, resultDouble);

            // Test successful parsing of bool
            var (canParseBool, resultBool) = TypeParser.TryParse(typeof(bool), "true");
            Assert.IsTrue(canParseBool);
            Assert.AreEqual(true, resultBool);

            // Test successful parsing of nullable int with value
            var (canParseNullableInt, resultNullableInt) = TypeParser.TryParse(typeof(int?), "123");
            Assert.IsTrue(canParseNullableInt);
            Assert.AreEqual(123, resultNullableInt);

            // Test successful parsing of nullable int with empty string
            var (canParseNullableEmpty, resultNullableEmpty) = TypeParser.TryParse(typeof(int?), "");
            Assert.IsTrue(canParseNullableEmpty);
            Assert.AreEqual(null, resultNullableEmpty);

            // Test failed parsing of int with invalid string
            var (canParseInvalidInt, resultInvalidInt) = TypeParser.TryParse(typeof(int), "invalid");
            Assert.IsFalse(canParseInvalidInt);
            Assert.AreEqual(null, resultInvalidInt);

            // Test failed parsing of double with invalid string
            var (canParseInvalidDouble, resultInvalidDouble) = TypeParser.TryParse(typeof(double), "not_a_number");
            Assert.IsFalse(canParseInvalidDouble);
            Assert.AreEqual(null, resultInvalidDouble);

            // Test successful parsing of string
            var (canParseString, resultString) = TypeParser.TryParse(typeof(string), "hello");
            Assert.IsTrue(canParseString);
            Assert.AreEqual("hello", resultString);
        }
    }
}