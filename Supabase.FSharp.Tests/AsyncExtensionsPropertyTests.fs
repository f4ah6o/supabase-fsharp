module AsyncExtensionsPropertyTests

open System
open System.Threading.Tasks
open FsCheck
open FsCheck.Xunit
open Supabase.FSharp

module Async =
    let map f computation = async {
        let! value = computation
        return f value
    }

// ==============================================
// Property-Based Tests for AsyncExtensions
// ==============================================
// These tests verify invariants for Task/Async conversions
// that should hold for any input value.

// ==============================================
// Round-trip Properties
// ==============================================

[<Property>]
let ``Task to Async to Task preserves int values`` (value: int) =
    let originalTask = Task.FromResult(value)
    let asyncOp = AsyncExtensions.AsAsyncResult originalTask
    let resultTask = AsyncExtensions.AsTask asyncOp
    resultTask.Result = value

[<Property>]
let ``Task to Async to Task preserves string values`` (value: string) =
    value <> null ==> lazy (
        let originalTask = Task.FromResult(value)
        let asyncOp = AsyncExtensions.AsAsyncResult originalTask
        let resultTask = AsyncExtensions.AsTask asyncOp
        resultTask.Result = value
    )

[<Property>]
let ``Task to Async to Task preserves bool values`` (value: bool) =
    let originalTask = Task.FromResult(value)
    let asyncOp = AsyncExtensions.AsAsyncResult originalTask
    let resultTask = AsyncExtensions.AsTask asyncOp
    resultTask.Result = value

[<Property>]
let ``Task to Async to Task preserves float values`` (value: float) =
    not (Double.IsNaN value) ==> lazy (
        let originalTask = Task.FromResult(value)
        let asyncOp = AsyncExtensions.AsAsyncResult originalTask
        let resultTask = AsyncExtensions.AsTask asyncOp
        resultTask.Result = value
    )

[<Property>]
let ``Async to Task to Async preserves int values`` (value: int) =
    let originalAsync = async { return value }
    let task = AsyncExtensions.AsTask originalAsync
    let resultAsync = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously resultAsync
    result = value

[<Property>]
let ``Async to Task to Async preserves string values`` (value: string) =
    value <> null ==> lazy (
        let originalAsync = async { return value }
        let task = AsyncExtensions.AsTask originalAsync
        let resultAsync = AsyncExtensions.AsAsyncResult task
        let result = Async.RunSynchronously resultAsync
        result = value
    )

[<Property>]
let ``Async to Task to Async preserves bool values`` (value: bool) =
    let originalAsync = async { return value }
    let task = AsyncExtensions.AsTask originalAsync
    let resultAsync = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously resultAsync
    result = value

[<Property>]
let ``Async to Task to Async preserves DateTime values`` (value: DateTime) =
    let originalAsync = async { return value }
    let task = AsyncExtensions.AsTask originalAsync
    let resultAsync = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously resultAsync
    result = value

// ==============================================
// Determinism Properties
// ==============================================

[<Property>]
let ``AsTask result can be awaited multiple times with same result`` (value: int) =
    let asyncOp = async { return value }
    let task = AsyncExtensions.AsTask asyncOp
    let result1 = task.Result
    let result2 = task.Result
    let result3 = task.Result
    result1 = value && result2 = value && result3 = value

[<Property>]
let ``AsAsyncResult can be run multiple times with same result`` (value: string) =
    value <> null ==> lazy (
        let task = Task.FromResult(value)
        let asyncOp = AsyncExtensions.AsAsyncResult task
        let result1 = Async.RunSynchronously asyncOp
        let result2 = Async.RunSynchronously asyncOp
        result1 = value && result2 = value
    )

// ==============================================
// Composition Properties
// ==============================================

[<Property>]
let ``Mapping over Async before and after Task conversion is equivalent`` (value: int) =
    let f x = x + 1
    let originalAsync = async { return value }

    // Map before conversion
    let leftTask =
        originalAsync
        |> Async.map f
        |> AsyncExtensions.AsTask

    // Convert then map
    let rightTask =
        originalAsync
        |> AsyncExtensions.AsTask
        |> (fun t -> task {
            let! result = t
            return f result
        })

    leftTask.Result = rightTask.Result

[<Property>]
let ``AsAsyncResult followed by bind is equivalent to async bind`` (value: int) =
    let f x = async { return x * 2 }
    let task = Task.FromResult(value)

    let left =
        task
        |> AsyncExtensions.AsAsyncResult
        |> (fun a -> async.Bind(a, f))
        |> Async.RunSynchronously

    let right =
        async.Bind(async { return value }, f)
        |> Async.RunSynchronously

    left = right

// ==============================================
// Identity Properties
// ==============================================

[<Property>]
let ``Task from completed result to Async preserves value immediately`` (value: int) =
    let task = Task.FromResult(value)
    let asyncOp = AsyncExtensions.AsAsyncResult task
    Async.RunSynchronously asyncOp = value

[<Property>]
let ``Async with immediate return to Task preserves value`` (value: int) =
    let asyncOp = async { return value }
    let task = AsyncExtensions.AsTask asyncOp
    task.Result = value

// ==============================================
// Type Preservation Properties
// ==============================================

[<Property>]
let ``AsAsyncResult preserves result type for tuples`` (x: int, y: string) =
    y <> null ==> lazy (
        let task = Task.FromResult((x, y))
        let asyncOp = AsyncExtensions.AsAsyncResult task
        let result = Async.RunSynchronously asyncOp
        result = (x, y)
    )

[<Property>]
let ``AsTask preserves result type for tuples`` (x: int, y: bool) =
    let asyncOp = async { return (x, y) }
    let task = AsyncExtensions.AsTask asyncOp
    task.Result = (x, y)

[<Property>]
let ``AsAsyncResult preserves result type for lists`` (xs: int list) =
    let task = Task.FromResult(xs)
    let asyncOp = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously asyncOp
    result = xs

[<Property>]
let ``AsTask preserves result type for lists`` (xs: NonNull<string> list) =
    let values = xs |> List.map (fun x -> x.Get)
    let asyncOp = async { return values }
    let task = AsyncExtensions.AsTask asyncOp
    task.Result = values

// ==============================================
// Sequential Conversion Properties
// ==============================================

[<Property>]
let ``Multiple round-trips preserve value`` (value: int) =
    // Task -> Async -> Task -> Async -> Task
    let originalTask = Task.FromResult(value)
    let result =
        originalTask
        |> AsyncExtensions.AsAsyncResult
        |> AsyncExtensions.AsTask
        |> AsyncExtensions.AsAsyncResult
        |> AsyncExtensions.AsTask
    result.Result = value

[<Property>]
let ``Chained async operations preserve composition`` (x: int) =
    // Create a chain of async operations
    let add1 y = async { return y + 1 }
    let mul2 y = async { return y * 2 }

    let chain = async {
        let! step1 = add1 x
        let! step2 = mul2 step1
        return step2
    }

    // Convert to Task and back
    let result =
        chain
        |> AsyncExtensions.AsTask
        |> AsyncExtensions.AsAsyncResult
        |> Async.RunSynchronously

    // Should equal (x + 1) * 2
    result = (x + 1) * 2

// ==============================================
// Consistency Properties
// ==============================================

[<Property>]
let ``AsTask completion status reflects async completion`` (value: int) =
    let asyncOp = async { return value }
    let task = AsyncExtensions.AsTask asyncOp
    // Wait for completion
    task.Wait()
    task.IsCompleted && not task.IsFaulted && not task.IsCanceled

[<Property>]
let ``Completed Task converted to Async runs immediately`` (value: int) =
    let task = Task.FromResult(value)
    let asyncOp = AsyncExtensions.AsAsyncResult task
    // Should complete without actual async delay
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
    let result = Async.RunSynchronously asyncOp
    stopwatch.Stop()
    // Should be very fast (< 100ms) since Task was already completed
    result = value && stopwatch.ElapsedMilliseconds < 100L
