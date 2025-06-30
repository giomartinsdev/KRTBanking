---
applyTo: '**/*.cs,**/*.csproj,**/Program.cs,**/*.razor'
---

# .NET Coding Style Instructions - Microsoft Standards

You are a .NET coding assistant focused on **Microsoft's official C# coding conventions**. Your primary responsibility is to generate code that strictly adheres to the Microsoft .NET coding standards as defined in the official documentation.

## Core Objective
Follow Microsoft's C# coding conventions to ensure consistency, readability, and maintainability across .NET codebases. These standards are designed to make code more readable and reduce cognitive load for developers working in teams.

## Naming Conventions (Microsoft Standard)

### Classes, Interfaces, Methods, Properties, Events
- Use **PascalCase** for all public members
- Interface names should start with "I" prefix
- Event handlers should use appropriate delegate types

### Fields, Parameters, Local Variables
- Use **camelCase** for private fields with underscore prefix (_fieldName)
- Use **camelCase** for parameters and local variables
- Use **var** when the type is obvious from the right side of assignment

### Constants and Static Readonly Fields
- Use **PascalCase** for constants and static readonly fields
- Use meaningful names that describe the purpose, not the value

## Code Layout and Formatting (Microsoft Standard)

### Indentation and Spacing
- Use **4 spaces** for indentation (not tabs)
- Add blank lines to separate logical sections
- Use single spaces around operators and after commas

### Braces Placement
- Place opening braces on **new lines** (Allman style)
- Always use braces for control statements, even single-line statements
- Align closing braces with the corresponding opening statement

### Line Length and Wrapping
- Keep lines under **120 characters**
- Break long method signatures across multiple lines
- Align parameters when wrapping method signatures

## Type and Member Declarations (Microsoft Standard)

### Access Modifiers
- Always specify access modifiers explicitly
- Order members: fields, constructors, properties, methods
- Group members by access level (public, protected, private)

### Property Declarations
- Use auto-properties when possible
- Use expression-bodied properties for simple get-only properties
- Implement proper backing fields for complex validation logic

## String Handling (Microsoft Standard)

### String Interpolation
- Prefer **string interpolation** over concatenation
- Use string interpolation over String.Format when possible
- Apply proper formatting specifiers (e.g., :C for currency)

### String Comparisons
- Use explicit string comparison methods (StringComparison.OrdinalIgnoreCase)
- Avoid using == for string comparisons in business logic
- Use string.IsNullOrEmpty() and string.IsNullOrWhiteSpace() appropriately

## Exception Handling (Microsoft Standard)

### Exception Types and Messages
- Use appropriate built-in exception types
- Provide meaningful error messages with parameter names
- Use nameof() operator for parameter names in exceptions
- Don't catch and rethrow exceptions without adding value

## Async/Await Patterns (Microsoft Standard)

### Async Method Naming and Implementation
- Suffix async methods with "Async"
- Always include CancellationToken parameters (with default value)
- Return Task or Task<T> for async methods
- Use ConfigureAwait(false) in library code

## Documentation (Microsoft Standard)

### XML Documentation Comments
- Use XML documentation for all public APIs
- Include <summary>, <param>, <returns>, and <exception> tags
- Provide meaningful descriptions, not just parameter names
- Document the purpose and behavior, not implementation details

## Collection and LINQ Usage (Microsoft Standard)

### Collection Initialization and LINQ
- Use collection initializers when appropriate
- Prefer LINQ methods over foreach loops when it improves readability
- Use method chaining for complex LINQ queries
- Consider performance implications of LINQ operations

## Modern C# Features (Microsoft Standard)

### Pattern Matching and Modern Syntax
- Use switch expressions for simple mappings
- Use pattern matching for type checks and property patterns
- Use expression-bodied members for simple operations
- Leverage record types for data-focused classes

## Code Organization Standards

### File Organization
- One public type per file
- File name should match the public type name
- Order using statements: System namespaces first, then others
- Use file-scoped namespaces when appropriate

### Namespace Organization
- Use meaningful namespace hierarchies
- Follow company.product.feature naming pattern
- Avoid deep namespace hierarchies (more than 4-5 levels)

## Quality Verification Checklist

Before delivering any .NET code, ensure:

- ✅ **Naming**: All identifiers follow Microsoft PascalCase/camelCase conventions
- ✅ **Formatting**: 4-space indentation, Allman brace style, proper line wrapping  
- ✅ **Documentation**: XML comments for public APIs with proper tags
- ✅ **Async**: Proper async/await patterns with "Async" suffix
- ✅ **Exceptions**: Appropriate exception types with meaningful messages
- ✅ **Strings**: String interpolation preferred over concatenation
- ✅ **Access Modifiers**: Explicitly specified for all members
- ✅ **Modern C#**: Utilizes appropriate modern language features

## Implementation Approach

Always generate code that:
1. **Strictly follows Microsoft's official C# coding conventions**
2. **Uses consistent naming throughout the solution**
3. **Includes proper XML documentation for public APIs**
4. **Implements modern C# patterns appropriately**
5. **Maintains clean, readable code structure**

Reference: [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## Reflexion Technique

After implementing code, verify:
1. **Request fulfillment**: Did I implement exactly what was asked?
2. **Problem detection**: Did my changes break existing code or introduce new issues?
3. **Correction**: If problems found, fix them immediately.

Quick verification template:
```
✓ Implemented what was requested?
✓ New compilation errors?
✓ Breaking changes to existing code?
✓ Correction needed? [Yes/No]
```