module OptionExtensionsTests

open System
open Xunit
open FsUnit.Xunit
open Supabase.FSharp

[<Fact>]
let ``ofNullable converts nullable with value to Some`` () =
    // Arrange
    let nullableInt = Nullable<int>(42)

    // Act
    let result = ofNullable nullableInt

    // Assert
    result |> should equal (Some 42)

[<Fact>]
let ``ofNullable converts null nullable to None`` () =
    // Arrange
    let nullableInt = Nullable<int>()

    // Act
    let result = ofNullable nullableInt

    // Assert
    result |> should equal None

[<Fact>]
let ``ofNullable handles different nullable types`` () =
    // Arrange
    let nullableDouble = Nullable<float>(3.14)
    let nullableBool = Nullable<bool>(true)
    let nullableDateTime = Nullable<DateTime>(DateTime(2024, 1, 1))

    // Act & Assert
    ofNullable nullableDouble |> should equal (Some 3.14)
    ofNullable nullableBool |> should equal (Some true)
    ofNullable nullableDateTime |> should equal (Some (DateTime(2024, 1, 1)))

[<Fact>]
let ``ofObj converts non-null object to Some`` () =
    // Arrange
    let str = "Hello, F#!"

    // Act
    let result = ofObj str

    // Assert
    result |> should equal (Some "Hello, F#!")

[<Fact>]
let ``ofObj converts null object to None`` () =
    // Arrange
    let str: string = null

    // Act
    let result = ofObj str

    // Assert
    result |> should equal None

[<Fact>]
let ``toNullable converts Some to nullable with value`` () =
    // Arrange
    let optionValue = Some 123

    // Act
    let result = toNullable optionValue

    // Assert
    result.HasValue |> should equal true
    result.Value |> should equal 123

[<Fact>]
let ``toNullable converts None to null nullable`` () =
    // Arrange
    let optionValue: int option = None

    // Act
    let result = toNullable optionValue

    // Assert
    result.HasValue |> should equal false

[<Fact>]
let ``toObj converts Some to object`` () =
    // Arrange
    let optionValue = Some "Test string"

    // Act
    let result = toObj optionValue

    // Assert
    result |> should equal "Test string"

[<Fact>]
let ``toObj converts None to null`` () =
    // Arrange
    let optionValue: string option = None

    // Act
    let result = toObj optionValue

    // Assert
    result |> should equal null

[<Fact>]
let ``defaultValue returns value when Some`` () =
    // Arrange
    let optionValue = Some 42
    let defaultVal = 99

    // Act
    let result = defaultValue defaultVal optionValue

    // Assert
    result |> should equal 42

[<Fact>]
let ``defaultValue returns default when None`` () =
    // Arrange
    let optionValue: int option = None
    let defaultVal = 99

    // Act
    let result = defaultValue defaultVal optionValue

    // Assert
    result |> should equal 99

[<Fact>]
let ``defaultWith returns value when Some`` () =
    // Arrange
    let optionValue = Some "existing"
    let mutable called = false
    let defaultThunk () =
        called <- true
        "default"

    // Act
    let result = defaultWith defaultThunk optionValue

    // Assert
    result |> should equal "existing"
    called |> should equal false

[<Fact>]
let ``defaultWith computes default when None`` () =
    // Arrange
    let optionValue: string option = None
    let mutable called = false
    let defaultThunk () =
        called <- true
        "computed default"

    // Act
    let result = defaultWith defaultThunk optionValue

    // Assert
    result |> should equal "computed default"
    called |> should equal true

[<Fact>]
let ``Round trip conversion ofNullable and toNullable with value`` () =
    // Arrange
    let original = Nullable<int>(456)

    // Act
    let option = ofNullable original
    let result = toNullable option

    // Assert
    result.HasValue |> should equal true
    result.Value |> should equal 456

[<Fact>]
let ``Round trip conversion ofNullable and toNullable with null`` () =
    // Arrange
    let original = Nullable<int>()

    // Act
    let option = ofNullable original
    let result = toNullable option

    // Assert
    result.HasValue |> should equal false

[<Fact>]
let ``Round trip conversion ofObj and toObj with value`` () =
    // Arrange
    let original = "Original string"

    // Act
    let option = ofObj original
    let result = toObj option

    // Assert
    result |> should equal "Original string"

[<Fact>]
let ``Round trip conversion ofObj and toObj with null`` () =
    // Arrange
    let original: string = null

    // Act
    let option = ofObj original
    let result = toObj option

    // Assert
    result |> should equal null

[<Fact>]
let ``defaultValue works with different types`` () =
    // Arrange & Act & Assert
    defaultValue "default" (Some "value") |> should equal "value"
    defaultValue "default" None |> should equal "default"

    defaultValue 0 (Some 100) |> should equal 100
    defaultValue 0 None |> should equal 0

    defaultValue false (Some true) |> should equal true
    defaultValue false None |> should equal false

[<Fact>]
let ``defaultWith is lazy and only evaluates when needed`` () =
    // Arrange
    let mutable evaluationCount = 0
    let expensiveComputation () =
        evaluationCount <- evaluationCount + 1
        "expensive result"

    // Act
    let result1 = defaultWith expensiveComputation (Some "quick")
    let result2 = defaultWith expensiveComputation None
    let result3 = defaultWith expensiveComputation None

    // Assert
    result1 |> should equal "quick"
    result2 |> should equal "expensive result"
    result3 |> should equal "expensive result"
    evaluationCount |> should equal 2  // Only evaluated for None cases

[<Fact>]
let ``ofNullable and ofObj work in pipeline`` () =
    // Arrange
    let nullableValue = Nullable<int>(42)
    let stringValue = "test"

    // Act & Assert
    nullableValue
    |> ofNullable
    |> Option.map ((*) 2)
    |> should equal (Some 84)

    stringValue
    |> ofObj
    |> Option.map (fun s -> s.ToUpper())
    |> should equal (Some "TEST")

[<Fact>]
let ``toNullable and toObj work in pipeline`` () =
    // Arrange
    let optInt = Some 100
    let optString = Some "lower"

    // Act
    let resultInt = optInt |> Option.map ((*) 2) |> toNullable
    let resultString = optString |> Option.map (fun s -> s.ToUpper()) |> toObj

    // Assert
    resultInt.Value |> should equal 200
    resultString |> should equal "LOWER"
