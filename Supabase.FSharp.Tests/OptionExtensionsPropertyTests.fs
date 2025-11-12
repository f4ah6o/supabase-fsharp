module OptionExtensionsPropertyTests

open System
open FsCheck
open FsCheck.Xunit
open Supabase.FSharp

// ==============================================
// Property-Based Tests for OptionExtensions
// ==============================================
// These tests verify invariants and mathematical laws
// that should hold for any input value.

// ==============================================
// Round-trip Properties
// ==============================================
// Round-trip property: ofNullable >> toNullable should be identity

[<Property>]
let ``ofNullable and toNullable form a round-trip for values`` (value: int) =
    let original = Nullable<int>(value)
    let result = original |> ofNullable |> toNullable
    result.HasValue = original.HasValue && result.Value = original.Value

[<Property>]
let ``ofNullable and toNullable preserve null`` () =
    let original = Nullable<int>()
    let result = original |> ofNullable |> toNullable
    result.HasValue = false

[<Property>]
let ``toNullable and ofNullable form a round-trip for Some`` (value: int) =
    let original = Some value
    let result = original |> toNullable |> ofNullable
    result = original

[<Property>]
let ``toNullable and ofNullable form a round-trip for None`` () =
    let original: int option = None
    let result = original |> toNullable |> ofNullable
    result = original

[<Property>]
let ``ofObj and toObj form a round-trip for non-null strings`` (NonNull value) =
    let result = value |> ofObj |> toObj
    result = value

[<Property>]
let ``ofObj and toObj preserve null`` () =
    let original: string = null
    let result = original |> ofObj |> toObj
    isNull result

[<Property>]
let ``toObj and ofObj form a round-trip for Some`` (value: string) =
    value <> null ==> lazy (
        let original = Some value
        let result = original |> toObj |> ofObj
        result = original
    )

[<Property>]
let ``toObj and ofObj form a round-trip for None`` () =
    let original: string option = None
    let result = original |> toObj |> ofObj
    result = original

// ==============================================
// defaultValue Properties
// ==============================================

[<Property>]
let ``defaultValue returns the value for Some`` (defaultVal: int) (value: int) =
    let result = defaultValue defaultVal (Some value)
    result = value

[<Property>]
let ``defaultValue returns the default for None`` (defaultVal: int) =
    let result = defaultValue defaultVal None
    result = defaultVal

[<Property>]
let ``defaultValue is identity when applied to Some`` (value: int) =
    // For any default value, defaultValue d (Some x) = x
    fun defaultVal -> defaultValue defaultVal (Some value) = value

// ==============================================
// defaultWith Properties
// ==============================================

[<Property>]
let ``defaultWith returns the value for Some without calling thunk`` (value: string) =
    let mutable called = false
    let thunk () =
        called <- true
        "default"
    let result = defaultWith thunk (Some value)
    result = value && not called

[<Property>]
let ``defaultWith calls thunk exactly once for None`` () =
    let mutable callCount = 0
    let thunk () =
        callCount <- callCount + 1
        "default"
    let result = defaultWith thunk None
    result = "default" && callCount = 1

[<Property>]
let ``defaultWith is lazy - thunk is never called for Some`` (value: int) =
    let mutable called = false
    let thunk () =
        called <- true
        -1
    let result = defaultWith thunk (Some value)
    not called

// ==============================================
// Type Polymorphism Properties
// ==============================================

[<Property>]
let ``ofNullable works for different value types - int`` (value: int) =
    let nullable = Nullable<int>(value)
    let result = ofNullable nullable
    result = Some value

[<Property>]
let ``ofNullable works for different value types - float`` (value: float) =
    not (Double.IsNaN value) ==> lazy (
        let nullable = Nullable<float>(value)
        let result = ofNullable nullable
        result = Some value
    )

[<Property>]
let ``ofNullable works for different value types - bool`` (value: bool) =
    let nullable = Nullable<bool>(value)
    let result = ofNullable nullable
    result = Some value

[<Property>]
let ``ofNullable works for different value types - DateTime`` (value: DateTime) =
    let nullable = Nullable<DateTime>(value)
    let result = ofNullable nullable
    result = Some value

// ==============================================
// Functor Law Properties
// ==============================================
// Option should satisfy functor laws when combined with our extensions

[<Property>]
let ``ofNullable preserves functor identity`` (value: int) =
    // map id = id
    let nullable = Nullable<int>(value)
    let left = nullable |> ofNullable |> Option.map id
    let right = nullable |> ofNullable
    left = right

[<Property>]
let ``ofNullable preserves functor composition`` (value: int) =
    // map (f >> g) = map f >> map g
    let f x = x + 1
    let g x = x * 2
    let nullable = Nullable<int>(value)

    let left = nullable |> ofNullable |> Option.map (f >> g)
    let right = nullable |> ofNullable |> Option.map f |> Option.map g
    left = right

// ==============================================
// Consistency Properties
// ==============================================

[<Property>]
let ``ofNullable result has value iff HasValue is true`` (nullable: Nullable<int>) =
    let result = ofNullable nullable
    match result with
    | Some _ -> nullable.HasValue
    | None -> not nullable.HasValue

[<Property>]
let ``toNullable result HasValue iff option is Some`` (opt: int option) =
    let result = toNullable opt
    match opt with
    | Some _ -> result.HasValue
    | None -> not result.HasValue

[<Property>]
let ``defaultValue is equivalent to Option.defaultValue`` (defaultVal: int) (opt: int option) =
    defaultValue defaultVal opt = Option.defaultValue defaultVal opt

[<Property>]
let ``defaultWith is equivalent to Option.defaultWith`` (opt: string option) =
    let thunk () = "generated"
    defaultWith thunk opt = Option.defaultWith thunk opt

// ==============================================
// Idempotence Properties
// ==============================================

[<Property>]
let ``ofNullable >> toNullable >> ofNullable is idempotent`` (nullable: Nullable<int>) =
    let once = nullable |> ofNullable |> toNullable |> ofNullable
    let twice = once |> toNullable |> ofNullable
    once = twice

[<Property>]
let ``toNullable >> ofNullable >> toNullable is idempotent`` (opt: int option) =
    let once = opt |> toNullable |> ofNullable |> toNullable
    let twice = once |> ofNullable |> toNullable
    once.HasValue = twice.HasValue &&
    (not once.HasValue || once.Value = twice.Value)
