module Tests

open System.Collections.Generic
open ShoppingCart.Domain

open TickSpec
open Expecto

type ShoppingCart = { Items : Dictionary<ShoppingItem, uint32>}

let [<Given>] ``an empty shopping cart`` () = { Items = Dictionary() }

let [<When>] ``I (.*)`` (command : string) (cart : ShoppingCart ) =
    ShoppingDSL.execute cart.Items command |> ignore

let [<Then>] ``there should be (.*) (.*) in the shopping cart`` (count : uint32) (item : string) (cart : ShoppingCart) =
    let expectedCount =
        match count with
        | 0u -> None
        | x -> Some x

    let result =
        sprintf "get %s" item
        |> ShoppingDSL.execute cart.Items

    let value = Expect.wantOk result "Command didn't execute as expected"

    match value with
    | Choice1Of2 x -> Expect.equal x expectedCount "Unexpected amount of item"
    | Choice2Of2 () -> failwith "Shouldn't reach this"
