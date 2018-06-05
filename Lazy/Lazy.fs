open System
open System.Threading

/// <summary>
/// Интерфейс ILazy<T>, предоставляющий функцию Get() для вычислений
/// </summary>
type ILazy<'a> =
    abstract member Get: unit -> 'a

/// <summary>
/// Класс SingleThreadedLazy<T>, работающий в однопоточном режиме
/// </summary>
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
   
/// <summary>
/// Класс MultiThreadedLazy<T>, работающий в многопоточном режиме
/// </summary>
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
                        result <- (supplier())
                        isFirst <- false
                )
            result   

/// <summary>
/// Класс MultiThreadedLockFreeLazy<T>, работающий в многопоточном режиме lock-free
/// </summary>
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
      
/// <summary>
/// Фабрика классов, реализующий интерфейс ILazy
/// </summary>
type LazyFactory =
    /// <summary>
    /// Однопоточный ILazy
    /// </summary>
    /// <param name="supplier">Функция, предоставляющая вычисления</param>
    static member CreateSingleThreadedLazy (supplier : unit -> 'a) = 
        new SingleThreadedLazy<'a> (supplier) :> ILazy<'a>

    /// <summary>
    /// Многопоточный ILazy
    /// </summary>
    /// <param name="supplier">Функция, предоставляющая вычисления</param>
    static member CreateMultiThreadedLazy (supplier : unit -> 'a) =
        new MultiThreadedLazy<'a> (supplier) :> ILazy<'a>
    
    /// <summary>
    /// Многопоточный lock-free ILazy
    /// </summary>
    /// <param name="supplier">Функция, предоставляющая вычисления</param>
    static member CreateMultiThreadedLockFreeLazy (supplier : unit -> 'a) =
        new MultiThreadedLockFreeLazy<'a> (supplier) :> ILazy<'a>

[<EntryPoint>]
let main argv =
    0
