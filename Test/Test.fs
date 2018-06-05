open Lazy
open NUnit.Framework
open FsUnit
open System.Threading.Tasks

[<Test>]
let ``test of SingleThreadedLazy class 1``() =
    let mutable i = 0
    let c = LazyFactory.CreateSingleThreadedLazy (fun() -> i <- i + 1)
    c.Get()
    c.Get()
    i |> should equal 1


[<Test>]
let ``test of SingleThreadedLazy class 2``() =
    let c = LazyFactory.CreateSingleThreadedLazy (fun() -> [1..10] |> List.sum)
    c.Get() |> should equal 55

[<Test>]
let ``test of MultiThreadedLazy class 1``() =
    let mutable i = 0
    let c = LazyFactory.CreateMultiThreadedLazy (fun() -> i <- i + 1)
    Parallel.For(1, 30, fun num -> c.Get()) |> ignore
    i |> should equal 1

[<Test>]
let ``test of MultiThreadedLazy class 2``() =
    let c = LazyFactory.CreateMultiThreadedLazy (fun() -> [1..10] |> List.sum)
    c.Get() |> should equal 55

[<Test>]
let ``test of MultiThreadedLockFreeLazy class 1``() =
    let mutable i = 0
    let c = LazyFactory.CreateMultiThreadedLockFreeLazy (fun() -> i <- i + 1)
    Parallel.For(1, 30, fun num -> c.Get()) |> ignore
    i |> should equal 1

[<Test>]
let ``test of MultiThreadedLockFreeLazy class 2``() =
    let c = LazyFactory.CreateMultiThreadedLockFreeLazy (fun() -> [1..10] |> List.map (fun i -> i))
    c.Get() |> List.sum |> should equal 55


[<EntryPoint>]
let main argv =
    ``test of SingleThreadedLazy class 1``()
    ``test of SingleThreadedLazy class 2``()

    ``test of MultiThreadedLazy class 1``()
    ``test of MultiThreadedLazy class 2``()

    ``test of MultiThreadedLockFreeLazy class 1``()
    ``test of MultiThreadedLockFreeLazy class 2``()
    0 
