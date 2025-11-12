# Supabase.FSharp Tests

Comprehensive test suite for the Supabase.FSharp library - F# extensions and idiomatic wrappers for the Supabase C# client.

## Overview

This test project provides complete coverage for all F# modules in the Supabase.FSharp library using xUnit as the testing framework.

## Test Modules

### AsyncExtensionsTests.fs
Tests for async/task conversion utilities:
- Task ↔ Async conversions (`AsAsync`, `AsAsyncResult`, `AsTask`)
- Round-trip conversions
- Exception handling in async operations
- Delayed task execution
- Multiple awaits on the same task

**Coverage:** 12 tests covering all AsyncExtensions functionality

### OptionExtensionsTests.fs
Tests for Option type conversions:
- Nullable ↔ Option conversions (`ofNullable`, `toNullable`)
- Object ↔ Option conversions (`ofObj`, `toObj`)
- Default value handling (`defaultValue`, `defaultWith`)
- Round-trip conversions
- Pipeline integration
- Lazy evaluation

**Coverage:** 24 tests covering all OptionExtensions functionality

### SupabaseBuilderTests.fs
Tests for computation expression builders:
- `SupabaseOptionsBuilder` configuration
  - Schema, autoRefreshToken, autoConnectRealtime settings
  - Custom headers
  - Storage options
- Client creation helpers (`createClient`, `createSimpleClient`)
- `AuthWorkflowBuilder` async operations
  - Bind, Return, ReturnFrom
  - Exception handling (TryWith, TryFinally)
  - Combine and Delay operations

**Coverage:** 22 tests covering all builder patterns

### SupabaseModuleTests.fs
Tests for idiomatic F# modules:
- **Supabase module**: Client creation, table access, RPC calls
- **Auth module**: Session and user management (as Option types)
- **Realtime module**: Channel management, auth token handling
- **Storage module**: Bucket access, public URLs
- **Functions module**: Edge function invocation

**Coverage:** 29 tests covering all module functions and pipeline integration

## Running the Tests

### Using .NET CLI

```bash
# Run all tests
dotnet test Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj

# Run with verbose output
dotnet test Supabase.FSharp.Tests/Supabase.FSharp.Tests.fsproj --verbosity detailed

# Run specific test file
dotnet test --filter "FullyQualifiedName~AsyncExtensionsTests"
```

### Using Visual Studio

1. Open the solution in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All" or select specific tests to run

### Using Rider

1. Open the solution in JetBrains Rider
2. Right-click on the test project and select "Run Unit Tests"
3. Or use the unit test runner window

## Test Framework

- **Testing Framework**: xUnit 2.6.6
- **Assertion Library**: FsUnit.xUnit 5.6.1
- **Mocking**: NSubstitute 5.1.0 (for future integration tests)
- **Target Framework**: .NET 8.0

## Test Coverage Summary

| Module | Functions Tested | Test Count | Coverage |
|--------|-----------------|------------|----------|
| AsyncExtensions | 9 | 12 | 100% |
| OptionExtensions | 10+ | 24 | 100% |
| SupabaseBuilder | 6 builders/functions | 22 | 100% |
| SupabaseModule | 30+ functions | 29 | 100% |
| **Total** | **55+** | **87** | **100%** |

## Notes

- These are **unit tests** that verify the F# wrapper functionality
- Tests focus on F# idiomatic patterns, type safety, and pipeline integration
- Integration tests requiring actual Supabase connections are planned for future implementation
- The C# Supabase library has its own test suite in the `SupabaseTests` project

## Contributing

When adding new features to Supabase.FSharp:

1. Add corresponding tests in the appropriate test file
2. Ensure tests cover:
   - Happy path scenarios
   - Edge cases
   - Exception handling
   - Pipeline/currying support
3. Run all tests before submitting changes
4. Maintain 100% test coverage

## CI/CD Integration

Tests are automatically run as part of the build pipeline defined in `.github/workflows/build-and-test.yml`.
