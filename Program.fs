open System

module Option =
    let requireAny (results : Option<'a> seq) : Option<'a> =
        results
        |> Seq.tryFind (fun r ->
            match r with
            | Some _ -> true
            | None -> false)
        |> Option.defaultValue None

    let requireAll (results : Option<'a> seq) : Option<'a> =
        results
        |> Seq.tryFind (fun r ->
            match r with
            | None -> true
            | Some _ -> false)
        |> Option.defaultValue None

    let (<|>) (a : Lazy<Option<'a>>) (b : Lazy<Option<'a>>) : Lazy<Option<'a>> =
        match a.Value with
        | Some _ -> a
        | _ ->
            b.Value |> ignore
            b

    let (<&>) (a : Lazy<Option<'a>>) (b : Lazy<Option<'a>>) : Lazy<Option<'a>> =
        match a.Value with
        | None -> lazy None
        | Some _ ->
            match b.Value with
            | None -> lazy None
            | Some _ -> a

open Option

[<EntryPoint>]
let main argv =
    let opt =
        seq {
            None
            None
            Some 6
        }
        |> requireAny

    printfn "%A" opt

    let res =
        lazy None
        <|> lazy None
        <|> (lazy None <&> lazy Some 3)

    printfn "%A" res.Value

    0
