// Learn more about F# at http://fsharp.org

open System
open ShoppingCart.Domain
open ShoppingDSL

open System.Collections.Generic

open FSharpPlus

[<EntryPoint>]
let main argv =
    let state = Dictionary()

    let runOne command =
        execute state command
        |> function
            | Ok (Choice1Of2 x) -> sprintf "%A" x |> Some
            | Ok (Choice2Of2 ()) -> None
            | Error err -> sprintf "Error: %s" err |> Some

    printfn "BEGIN PROGRAM!!"

    seq { while true do Console.ReadLine() }
    |> Seq.map (runOne)
    |> Seq.iter(function Some x -> printfn "%s" x | None -> ())

    0 // return an integer exit code
