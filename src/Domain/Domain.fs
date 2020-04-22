namespace ShoppingCart.Domain

type ShoppingItem =
    | Bacon
    | Egg

module ShoppingItem =
    open FParsec
    let pShoppingItem : Parser<ShoppingItem, unit> =
        choice [(stringCIReturn "Egg" Egg)
                (stringCIReturn "Bacon" Bacon)]

module ShoppingDSL =
    open FSharpPlus
    open FSharpPlus.Data

    open System.Collections.Generic

    type ShoppingDSL<'next> =
        | SetItemCount of (ShoppingItem * uint32) * 'next
        | GetItemCount of ShoppingItem * (uint32 option -> 'next)
    with
        static member Map(x, f) =
            match x with
            | SetItemCount (args, next) -> SetItemCount(args, f next)
            | GetItemCount (args, next) -> GetItemCount(args, next >> f)

    type ShoppingProgram<'t> = Free<ShoppingDSL<'t>, 't>

    let rec interpret
        (items : Dictionary<ShoppingItem, uint32>)
        (program : ShoppingProgram<'a>) =
        match Free.run program with
        | Pure x -> x
        | Roll (SetItemCount ((item, count), next)) ->
            if count = 0u && items.ContainsKey item
            then items.Remove(item) |> ignore
            else if count > 0u
                then items.[item] <- count
            next |> interpret items
        | Roll (GetItemCount (item, next)) ->
            match items.TryGetValue item with
            | (true, value) -> Some value
            | (false, _ ) -> None
            |> next |> interpret items

    let setItemCount item count : ShoppingProgram<_> = Free.liftF (SetItemCount((item, count), ()))
    let getItemCount item : ShoppingProgram<_> = Free.liftF (GetItemCount(item, id))

    let clearItem item = setItemCount item 0u

    let addItem count item = monad {
        let! amount = getItemCount item
        match amount with
        | Some x -> do! setItemCount item (x + count)
        | None -> do! setItemCount item count
    }

    let subItem count item = monad {
        let! amount = getItemCount item
        match amount with
        | Some x when x >= count -> do! setItemCount item (x - count)
        | Some _ -> do! clearItem item
        | None -> ()
    }

    open FParsec

    type InstructionType<'a> =
        | Action of ShoppingProgram<unit>
        | Func of ShoppingProgram<'a>

    let pInstruction =
        let item = ShoppingItem.pShoppingItem .>> spaces
        let strCI s = pstringCI s .>> spaces
        let uint = puint32 .>> spaces

        let itemCount = (uint .>>. item)
                        <|> (item |>> fun x -> (1u, x))

        let pAdd = strCI "add" >>. itemCount
                   |>> fun (count, item) -> addItem count item |> Action

        let pSub = strCI "sub" >>. itemCount
                   |>> fun (count, item) -> subItem count item |> Action

        let pSetItemCount = pipe2 (strCI "set" >>. item) (strCI "to" >>. uint) (fun x y -> (setItemCount x y) |> Action)

        let pGetItemCount = (strCI "get" >>. item) |>> (getItemCount >> Func)

        let pClear = (strCI "clear" >>. item) |>> (clearItem >> Action)

        spaces
        >>. choice [ pAdd
                     pSub
                     pSetItemCount
                     pGetItemCount
                     pClear ]

    let execute items str =
        match run pInstruction str with
        | Success(result, _, _) ->
            match result with
            | Func x ->
                interpret items x |> Choice1Of2 |> Result.Ok
            | Action x ->
                interpret items x |> Choice2Of2 |> Result.Ok
        | Failure(error, _, _) -> Result.Error error
