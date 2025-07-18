namespace Shos.Parser.Tests
{
    [TestClass()]
    public class JsonParserTests
    {
        // Test classes for JsonParser testing
        public class Person
        {
            public string Name { get; } = string.Empty;
            public int Age { get; }

            public Person(string name, int age)
                => (Name, Age) = (name, age);
        }

        //public class ToDoList
        //{
        //    private List<ToDo> toDoes = new();

        //    //public List<ToDo> ToDoes { get; set; } = new();


        //    public ToDoList()
        //    { }

        //    [JsonConstructor]
        //    public ToDoList(List<ToDo> toDoes)
        //        => this.toDoes = toDoes;
        //}

        public class Name
        {
            public string FamilyName { get; set; } = string.Empty;
            public string GivenName { get; set; } = string.Empty;

            public static Name Parse(string name)
            {
                var names = name.Split(' ');
                return name.Length switch {
                    0 => new Name(),
                    1 => new Name { FamilyName = names[0], GivenName = names[1] },
                    _ => new Name { FamilyName = names[0] }
                };
            }

            public override string ToString() => $"{FamilyName} {GivenName}";
        }

        public class Staff
        {
            int number = 0;

            public int Number
            {
                get => number;
                set => number = value;
            }

            public Name Name { get; private set; } = new();

            public Staff()
            { }

            public Staff(Name name)
                => Name = name;

            //public Staff(int number, Name name)
            //    => (Number, Name) = (number, name);
        }

        public class Employee
        {
            public string Name { get; } = string.Empty;
            public int Age { get; }
            public string Department { get; } = string.Empty;
            public double Salary { get; }

            public Employee(string name, int age, string department, double salary)
                => (Name, Age, Department, Salary) = (name, age, department, salary);
        }

        public class Product
        {
            public string Name { get; } = string.Empty;
            public decimal Price { get; }
            public bool InStock { get; }

            public Product(string name, decimal price, bool inStock)
                => (Name, Price, InStock) = (name, price, inStock);
        }

        public class ComplexObject
        {
            public string Name { get; } = string.Empty;
            public int Number { get; }
            public DateTime Date { get; }
            public Guid Id { get; }

            public ComplexObject(string name, int number, DateTime date, Guid id)
                => (Name, Number, Date, Id) = (name, number, date, id);
        }

        public class MultipleConstructors
        {
            public string Name { get; } = string.Empty;
            public int Age { get; }
            public string Department { get; } = string.Empty;

            public MultipleConstructors(string name)
                => (Name, Age, Department) = (name, 0, "Unknown");

            public MultipleConstructors(string name, int age)
                => (Name, Age, Department) = (name, age, "Unknown");

            public MultipleConstructors(string name, int age, string department)
                => (Name, Age, Department) = (name, age, department);
        }

        [TestMethod()]
        public void ParseTest()
        {
            // Test basic object creation with simple types
            var personKeyValues = new[] { ("name", "John"), ("age", "25") };
            var person = JsonParser.Parse<Person>(personKeyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseWithConstuctorAndSetPropertiesTest()
        {
            // Test basic object creation with simple types
            var staffKeyValues = new[] { ("name", "Cathy Brown"), ("number", "101") };
            var staff = JsonParser.Parse<Staff>(staffKeyValues);

            Assert.IsNotNull(staff);
            //Assert.AreEqual("Cathy Brown", staff.Name.ToString()); // ToDo
            Assert.AreEqual(101, staff.Number);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_BasicTypes()
        {
            // Test with basic Person class
            var keyValues = new[] { ("name", "Alice"), ("age", "30") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("Alice", person.Name);
            Assert.AreEqual(30, person.Age);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_MultipleTypes()
        {
            // Test with Employee class having multiple parameter types
            var keyValues = new[] 
            { 
                ("name", "Bob"), 
                ("age", "35"), 
                ("department", "Engineering"), 
                ("salary", "75000.50") 
            };
            var employee = JsonParser.Parse<Employee>(keyValues);
            
            Assert.IsNotNull(employee);
            Assert.AreEqual("Bob", employee.Name);
            Assert.AreEqual(35, employee.Age);
            Assert.AreEqual("Engineering", employee.Department);
            Assert.AreEqual(75000.50, employee.Salary);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_WithBooleanAndDecimal()
        {
            // Test with Product class including boolean and decimal types
            var keyValues = new[] 
            { 
                ("name", "Laptop"), 
                ("price", "999.99"), 
                ("inStock", "true") 
            };
            var product = JsonParser.Parse<Product>(keyValues);
            
            Assert.IsNotNull(product);
            Assert.AreEqual("Laptop", product.Name);
            Assert.AreEqual(999.99m, product.Price);
            Assert.AreEqual(true, product.InStock);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_CaseInsensitiveMatching()
        {
            // Test case-insensitive parameter matching
            var keyValues = new[] { ("NAME", "Charlie"), ("AGE", "40") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("Charlie", person.Name);
            Assert.AreEqual(40, person.Age);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_MixedCaseMatching()
        {
            // Test mixed case parameter matching
            var keyValues = new[] { ("Name", "David"), ("aGe", "28") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("David", person.Name);
            Assert.AreEqual(28, person.Age);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_ComplexTypes()
        {
            // Test with DateTime and Guid types
            var testDate = "2024-01-01";
            var testGuid = "12345678-1234-1234-1234-123456789abc";
            var keyValues = new[]
            {
                ("name", "Complex"),
                ("number", "42"),
                ("date", testDate),
                ("id", testGuid)
            };
            var complexObj = JsonParser.Parse<ComplexObject>(keyValues);

            Assert.IsNotNull(complexObj);
            Assert.AreEqual("Complex", complexObj.Name);
            Assert.AreEqual(42, complexObj.Number);
            Assert.AreEqual(DateTime.Parse(testDate), complexObj.Date);
            Assert.AreEqual(Guid.Parse(testGuid), complexObj.Id);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_MultipleConstructors_PreferMoreParameters()
        {
            // Test that the constructor with more parameters is preferred
            var keyValues = new[] 
            { 
                ("name", "Employee"), 
                ("age", "30"), 
                ("department", "IT") 
            };
            var obj = JsonParser.Parse<MultipleConstructors>(keyValues);
            
            Assert.IsNotNull(obj);
            Assert.AreEqual("Employee", obj.Name);
            Assert.AreEqual(30, obj.Age);
            Assert.AreEqual("IT", obj.Department);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_PartialParameterMatch()
        {
            // Test with partial parameter matching - should use constructor with fewer parameters
            var keyValues = new[] { ("name", "PartialMatch") };
            var obj = JsonParser.Parse<MultipleConstructors>(keyValues);
            
            Assert.IsNotNull(obj);
            Assert.AreEqual("PartialMatch", obj.Name);
            Assert.AreEqual(0, obj.Age); // Default value from single-parameter constructor
            Assert.AreEqual("Unknown", obj.Department); // Default value from single-parameter constructor
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_MissingRequiredParameter()
        {
            // Test when required parameters are missing - should return null
            var keyValues = new[] { ("name", "John") }; // Missing age parameter for Person class
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_InvalidParameterValue()
        {
            // Test with invalid parameter value that cannot be parsed
            var keyValues = new[] { ("name", "John"), ("age", "invalid_number") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_ExtraParameters()
        {
            // Test with extra parameters that don't match any constructor parameter
            var keyValues = new[] 
            { 
                ("name", "John"), 
                ("age", "25"), 
                ("extraParam", "ignored") 
            };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_EmptyArray()
        {
            // Test with empty key-value array
            (string, string)[] keyValues = [];
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_DuplicateKeys()
        {
            // Test with duplicate keys - should use the first occurrence
            var keyValues = new[] 
            { 
                ("name", "FirstName"), 
                ("age", "25"), 
                ("name", "SecondName") 
            };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("FirstName", person.Name); // Should use first occurrence
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_NullOrEmptyValues()
        {
            // Test with null or empty string values
            var keyValues = new[] { ("name", ""), ("age", "25") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("", person.Name); // Empty string should be valid for string parameters
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_WhitespaceHandling()
        {
            // Test parameter names and values with whitespace (should be handled by caller)
            var keyValues = new[] { ("name", "  John  "), ("age", "  25  ") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNotNull(person);
            Assert.AreEqual("  John  ", person.Name); // Whitespace in values should be preserved
            Assert.AreEqual(25, person.Age); // TypeParser should handle whitespace in numeric values
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_BooleanParsing()
        {
            // Test various boolean value formats
            var keyValues1 = new[] { ("name", "Product1"), ("price", "10.99"), ("inStock", "true") };
            var product1 = JsonParser.Parse<Product>(keyValues1);
            
            var keyValues2 = new[] { ("name", "Product2"), ("price", "15.99"), ("inStock", "false") };
            var product2 = JsonParser.Parse<Product>(keyValues2);
            
            Assert.IsNotNull(product1);
            Assert.AreEqual(true, product1.InStock);
            
            Assert.IsNotNull(product2);
            Assert.AreEqual(false, product2.InStock);
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_NumericOverflow()
        {
            // Test with numeric values that cause overflow
            var keyValues = new[] { ("name", "Test"), ("age", "999999999999999999999") };
            var person = JsonParser.Parse<Person>(keyValues);
            
            Assert.IsNull(person); // Should fail due to numeric overflow
        }

        [TestMethod()]
        public void ParseKeyValueArrayTest_ValueTypeStructs()
        {
            // Test parsing DateTime and Guid more thoroughly
            var validDate = "2024-12-25T10:30:00";
            var validGuid = Guid.NewGuid().ToString();
            
            var keyValues = new[] 
            { 
                ("name", "Test"), 
                ("number", "123"), 
                ("date", validDate), 
                ("id", validGuid) 
            };
            var complexObj = JsonParser.Parse<ComplexObject>(keyValues);
            
            Assert.IsNotNull(complexObj);
            Assert.AreEqual(DateTime.Parse(validDate), complexObj.Date);
            Assert.AreEqual(Guid.Parse(validGuid), complexObj.Id);
        }

        [TestMethod()]
        public void ParseStringTest_BasicParsing()
        {
            // Test basic JSON-like string parsing
            var person = JsonParser.Parse<Person>("name:John, age:25");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_WithConstuctorAndSetPropertiesParsing()
        {
            var staff = JsonParser.Parse<Staff>("name: Patric Brown, number: 102");

            Assert.IsNotNull(staff);
            //Assert.AreEqual("Patric Brown", staff.Name.ToString()); // ToDo
            Assert.AreEqual(102, staff.Number);
        }

        [TestMethod()]
        public void ParseStringTest_MultipleTypes()
        {
            // Test parsing with multiple parameter types
            var employee = JsonParser.Parse<Employee>("name:Alice, age: 30,department: Engineering,salary:85000.75");
            
            Assert.IsNotNull(employee);
            Assert.AreEqual("Alice", employee.Name);
            Assert.AreEqual(30, employee.Age);
            Assert.AreEqual("Engineering", employee.Department);
            Assert.AreEqual(85000.75, employee.Salary);
        }

        [TestMethod()]
        public void ParseStringTest_WithWhitespace()
        {
            // Test parsing with whitespace around values (should be trimmed)
            var person = JsonParser.Parse<Person>("name: John , age: 25 ");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_WithExtraWhitespace()
        {
            // Test parsing with various whitespace scenarios
            var person = JsonParser.Parse<Person>("  name  :  Bob  ,  age  :  35  ");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("Bob", person.Name);
            Assert.AreEqual(35, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_BooleanAndDecimal()
        {
            // Test parsing with boolean and decimal types
            var product = JsonParser.Parse<Product>("name:Smartphone,price:599.99,inStock:true");
            
            Assert.IsNotNull(product);
            Assert.AreEqual("Smartphone", product.Name);
            Assert.AreEqual(599.99m, product.Price);
            Assert.AreEqual(true, product.InStock);
        }

        //[TestMethod()]
        //public void ParseStringTest_ComplexTypes()
        //{
        //    // Test parsing with DateTime and Guid
        //    var testDate = "2024-01-15T14:30:00";
        //    var testGuid = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
        //    var complexObj = JsonParser.Parse<ComplexObject>($"name:TestObject,number:42,date:{testDate},id:{testGuid}");
            
        //    Assert.IsNotNull(complexObj);
        //    Assert.AreEqual("TestObject", complexObj.Name);
        //    Assert.AreEqual(42, complexObj.Number);
        //    Assert.AreEqual(DateTime.Parse(testDate), complexObj.Date);
        //    Assert.AreEqual(Guid.Parse(testGuid), complexObj.Id);
        //}

        [TestMethod()]
        public void ParseStringTest_CaseInsensitiveKeys()
        {
            // Test case-insensitive key matching
            var person = JsonParser.Parse<Person>("NAME:Charlie,AGE:45");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("Charlie", person.Name);
            Assert.AreEqual(45, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_MixedCaseKeys()
        {
            // Test mixed case key matching
            var person = JsonParser.Parse<Person>("Name:Diana,aGe:28");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("Diana", person.Name);
            Assert.AreEqual(28, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_MultipleConstructorsPreferMore()
        {
            // Test that constructor with more matching parameters is preferred
            var obj = JsonParser.Parse<MultipleConstructors>("name:Manager,age:40,department:Sales");
            
            Assert.IsNotNull(obj);
            Assert.AreEqual("Manager", obj.Name);
            Assert.AreEqual(40, obj.Age);
            Assert.AreEqual("Sales", obj.Department);
        }

        [TestMethod()]
        public void ParseStringTest_PartialMatch()
        {
            // Test partial parameter matching with fewer available parameters
            var obj = JsonParser.Parse<MultipleConstructors>("name:SingleParam");
            
            Assert.IsNotNull(obj);
            Assert.AreEqual("SingleParam", obj.Name);
            Assert.AreEqual(0, obj.Age); // Default from single-parameter constructor
            Assert.AreEqual("Unknown", obj.Department); // Default from single-parameter constructor
        }

        [TestMethod()]
        public void ParseStringTest_EmptyString()
        {
            // Test with empty string
            var person = JsonParser.Parse<Person>("");
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseStringTest_InvalidFormat_NoColon()
        {
            // Test with malformed string (no colon separator)
            var person = JsonParser.Parse<Person>("name John,age 25");
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseStringTest_InvalidFormat_MultipleColons()
        {
            // Test with multiple colons in a single pair
            var person = JsonParser.Parse<Person>("name:John:Doe,age:25");
            
            // This should be parsed as name="John" and the ":Doe" part ignored, age=25
            // But since we trim and only take first two parts, it might work differently
            Assert.IsNull(person); // Expected to fail due to malformed format
        }

        [TestMethod()]
        public void ParseStringTest_MissingRequiredParameter()
        {
            // Test when required parameter is missing
            var person = JsonParser.Parse<Person>("name:John"); // Missing age
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseStringTest_InvalidParameterValue()
        {
            // Test with invalid parameter value
            var person = JsonParser.Parse<Person>("name:John,age:not_a_number");
            
            Assert.IsNull(person);
        }

        [TestMethod()]
        public void ParseStringTest_ExtraParameters()
        {
            // Test with extra parameters that don't match constructor
            var person = JsonParser.Parse<Person>("name:John,age:25,extra:ignored,another:also_ignored");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_DuplicateKeys()
        {
            // Test with duplicate keys - should use first occurrence
            var person = JsonParser.Parse<Person>("name:FirstName,age:25,name:SecondName");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("FirstName", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_EmptyValues()
        {
            // Test with empty values
            var person = JsonParser.Parse<Person>("name:,age:25");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("", person.Name); // Empty string should be valid for string parameters
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_SpecialCharactersInValues()
        {
            // Test with special characters in values
            var person = JsonParser.Parse<Person>("name:John O'Connor,age:30");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John O'Connor", person.Name);
            Assert.AreEqual(30, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_NumbersAsStrings()
        {
            // Test with numbers that should be parsed as strings
            var person = JsonParser.Parse<Person>("name:123,age:25");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("123", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_BooleanVariations()
        {
            // Test different boolean representations
            var product1 = JsonParser.Parse<Product>("name:Product1,price:10.99,inStock:true");
            var product2 = JsonParser.Parse<Product>("name:Product2,price:15.99,inStock:false");
            
            Assert.IsNotNull(product1);
            Assert.AreEqual(true, product1.InStock);
            
            Assert.IsNotNull(product2);
            Assert.AreEqual(false, product2.InStock);
        }

        [TestMethod()]
        public void ParseStringTest_DecimalPrecision()
        {
            // Test decimal precision handling
            var product = JsonParser.Parse<Product>("name:HighPrecision,price:123.456789,inStock:true");
            
            Assert.IsNotNull(product);
            Assert.AreEqual("HighPrecision", product.Name);
            Assert.AreEqual(123.456789m, product.Price);
            Assert.AreEqual(true, product.InStock);
        }

        [TestMethod()]
        public void ParseStringTest_NegativeNumbers()
        {
            // Test with negative numbers
            var person = JsonParser.Parse<Person>("name:TestUser,age:-5");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("TestUser", person.Name);
            Assert.AreEqual(-5, person.Age); // Negative age should be parsed correctly
        }

        [TestMethod()]
        public void ParseStringTest_LargeNumbers()
        {
            // Test with large numbers
            var employee = JsonParser.Parse<Employee>("name:HighEarner,age:35,department:Executive,salary:999999.99");
            
            Assert.IsNotNull(employee);
            Assert.AreEqual("HighEarner", employee.Name);
            Assert.AreEqual(35, employee.Age);
            Assert.AreEqual("Executive", employee.Department);
            Assert.AreEqual(999999.99, employee.Salary);
        }

        [TestMethod()]
        public void ParseStringTest_SingleParameter()
        {
            // Test with single parameter constructor
            var obj = JsonParser.Parse<MultipleConstructors>("name:OnlyName");
            
            Assert.IsNotNull(obj);
            Assert.AreEqual("OnlyName", obj.Name);
            Assert.AreEqual(0, obj.Age);
            Assert.AreEqual("Unknown", obj.Department);
        }

        [TestMethod()]
        public void ParseStringTest_OrderIndependence()
        {
            // Test that parameter order doesn't matter
            var person1 = JsonParser.Parse<Person>("name:John,age:25");
            var person2 = JsonParser.Parse<Person>("age:25,name:John");
            
            Assert.IsNotNull(person1);
            Assert.IsNotNull(person2);
            Assert.AreEqual(person1.Name, person2.Name);
            Assert.AreEqual(person1.Age, person2.Age);
        }

        [TestMethod()]
        public void ParseStringTest_TrailingComma()
        {
            // Test with trailing comma (should be ignored)
            var person = JsonParser.Parse<Person>("name:John,age:25,");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }

        [TestMethod()]
        public void ParseStringTest_LeadingComma()
        {
            // Test with leading comma (should be ignored)
            var person = JsonParser.Parse<Person>(",name:John,age:25");
            
            Assert.IsNotNull(person);
            Assert.AreEqual("John", person.Name);
            Assert.AreEqual(25, person.Age);
        }
    }
}