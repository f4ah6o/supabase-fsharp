module AsyncExtensionsTests

open System
open System.Threading.Tasks
open Xunit
open FsUnit.Xunit
open Supabase.FSharp

[<Fact>]
let ``AsAsync converts Task to Async successfully`` () =
    // Arrange
    let task = Task.CompletedTask

    // Act
    let asyncOp = AsyncExtensions.AsAsync task

    // Assert
    asyncOp |> should be instanceOfType<Async<unit>>
    Async.RunSynchronously asyncOp |> should equal ()

[<Fact>]
let ``AsAsyncResult converts Task of int to Async successfully`` () =
    // Arrange
    let expectedValue = 42
    let task = Task.FromResult(expectedValue)

    // Act
    let asyncOp = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously asyncOp

    // Assert
    result |> should equal expectedValue

[<Fact>]
let ``AsAsyncResult converts Task of string to Async successfully`` () =
    // Arrange
    let expectedValue = "Hello, F#!"
    let task = Task.FromResult(expectedValue)

    // Act
    let asyncOp = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously asyncOp

    // Assert
    result |> should equal expectedValue

[<Fact>]
let ``AsTask converts Async to Task successfully`` () =
    // Arrange
    let expectedValue = 123
    let asyncOp = async { return expectedValue }

    // Act
    let task = AsyncExtensions.AsTask asyncOp
    let result = task.Result

    // Assert
    task |> should be instanceOfType<Task<int>>
    result |> should equal expectedValue

[<Fact>]
let ``AsAsync handles failed Task correctly`` () =
    // Arrange
    let expectedException = Exception("Test exception")
    let task = Task.FromException(expectedException)

    // Act
    let asyncOp = AsyncExtensions.AsAsync task

    // Assert
    (fun () -> Async.RunSynchronously asyncOp)
    |> should throw typeof<Exception>

[<Fact>]
let ``AsAsyncResult handles failed Task correctly`` () =
    // Arrange
    let expectedException = Exception("Test exception")
    let task = Task.FromException<int>(expectedException)

    // Act
    let asyncOp = AsyncExtensions.AsAsyncResult task

    // Assert
    (fun () -> Async.RunSynchronously asyncOp)
    |> should throw typeof<Exception>

[<Fact>]
let ``AsTask handles Async exception correctly`` () =
    // Arrange
    let expectedException = Exception("Test async exception")
    let asyncOp = async {
        raise expectedException
        return 1
    }

    // Act
    let task = AsyncExtensions.AsTask asyncOp

    // Assert
    (fun () -> task.Wait())
    |> should throw typeof<AggregateException>

[<Fact>]
let ``Round trip conversion Task to Async to Task`` () =
    // Arrange
    let expectedValue = "Round trip test"
    let originalTask = Task.FromResult(expectedValue)

    // Act
    let asyncOp = AsyncExtensions.AsAsyncResult originalTask
    let resultTask = AsyncExtensions.AsTask asyncOp
    let result = resultTask.Result

    // Assert
    result |> should equal expectedValue

[<Fact>]
let ``Round trip conversion Async to Task to Async`` () =
    // Arrange
    let expectedValue = 999
    let originalAsync = async { return expectedValue }

    // Act
    let task = AsyncExtensions.AsTask originalAsync
    let resultAsync = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously resultAsync

    // Assert
    result |> should equal expectedValue

[<Fact>]
let ``AsAsync with delayed Task completes successfully`` () =
    // Arrange
    let delayMs = 100
    let task = Task.Delay(delayMs)

    // Act
    let asyncOp = AsyncExtensions.AsAsync task
    let stopwatch = System.Diagnostics.Stopwatch.StartNew()
    Async.RunSynchronously asyncOp
    stopwatch.Stop()

    // Assert
    stopwatch.ElapsedMilliseconds |> should be (greaterThanOrEqualTo delayMs)

[<Fact>]
let ``AsAsyncResult with async computation completes successfully`` () =
    // Arrange
    let computation = async {
        do! Async.Sleep 50
        return "Delayed result"
    }

    // Act
    let task = AsyncExtensions.AsTask computation
    let asyncOp = AsyncExtensions.AsAsyncResult task
    let result = Async.RunSynchronously asyncOp

    // Assert
    result |> should equal "Delayed result"

[<Fact>]
let ``AsTask can be awaited multiple times`` () =
    // Arrange
    let expectedValue = 42
    let asyncOp = async { return expectedValue }
    let task = AsyncExtensions.AsTask asyncOp

    // Act
    let result1 = task.Result
    let result2 = task.Result

    // Assert
    result1 |> should equal expectedValue
    result2 |> should equal expectedValue
