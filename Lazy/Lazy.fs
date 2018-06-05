open System
open System.Threading

type ILazy<'a> =
    abstract member Get: unit -> 'a

type SingleThreadedLazy<'a> (supplier : unit -> 'a) =
    let mutable isFirst = true
    let mutable result =  Unchecked.defaultof<'a>
    interface ILazy<'a> with
        member this.Get() = 
            if (isFirst)
            then
                isFirst <- false
                result <- supplier()
            result
   
type MultiThreadedLazy<'a> (supplier : unit -> 'a) =
    let mutable isFirst = true
    let mutable result =  Unchecked.defaultof<'a>
    let lockObj = new Object()

    interface ILazy<'a> with
        member this.Get() = 
            if (isFirst)
            then 
                lock lockObj (fun() ->
                    if (isFirst)
                    then
                        isFirst <- false
                        result <- (supplier())
                )
            result   

type MultiThreadedLockFreeLazy<'a when 'a : not struct and 'a : equality> (supplier : unit -> 'a) =
    let mutable isFirst = true
    let mutable result =  Unchecked.defaultof<'a>

    interface ILazy<'a> with
        member this.Get() = 
            if (Volatile.Read(&isFirst) = true)
            then
                let mutable compRes = supplier()
                let mutable curRes = result

                while (Interlocked.CompareExchange(&result, compRes, curRes) <> curRes) do
                    compRes <- supplier()
                    curRes <- result

            Volatile.Write(&isFirst, false)
            result
        
type LazyFactory =
    static member CreateSingleThreadedLazy (supplier : unit -> 'a) = 
        new SingleThreadedLazy<'a> (supplier) :> ILazy<'a>

    static member CreateMultiThreadedLazy (supplier : unit -> 'a) =
        new MultiThreadedLazy<'a> (supplier) :> ILazy<'a>
    
    static member CreateMultiThreadedLockFreeLazy (supplier : unit -> 'a) =
        new MultiThreadedLockFreeLazy<'a> (supplier) :> ILazy<'a>

[<EntryPoint>]
let main argv =
    0
