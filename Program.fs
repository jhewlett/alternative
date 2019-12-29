open System

//todo: write tests
//todo: is Seq.isEmpty and Seq.head the most efficient/straightforward
//todo: why do the requires take a default error. Just use the first one? (assum seq is non-empty or only use if seq is empty)

module Result =
    let isOk = function
        | Ok _ -> true
        | _ -> false

    let isError = function
        | Error _ -> true
        | _ -> false
    
    let requireAny error results =
        results
        |> Seq.tryFind isOk
        |> Option.defaultValue (Error error)

    let requireAll error results =
        let hasError = results |> Seq.exists isError

        match hasError with
        | true -> Error error
        | _ ->
            match Seq.isEmpty results with
            | true -> Error error
            | false -> Seq.head results

module AsyncResult =
    //todo: make an async version of tryFind, using a while loop?

    let requireAny error (results : Async<Result<'a, 'b>> seq) : Async<Result<'a, 'b>> =        
        async {
            use e = results.GetEnumerator()
            let mutable res = None
            while (Option.isNone res && e.MoveNext()) do
                let! c = e.Current
                if Result.isOk c then res <- Some c        

            match res with
            | Some c -> return c
            | None -> return Error error
        }

module Option =
    let requireAny options =
        options
        |> Seq.tryFind Option.isSome
        |> Option.defaultValue None

    let requireAll (options : Option<'a> seq) : Option<'a> = 
        let hasNone = options |> Seq.exists Option.isNone

        match hasNone with
        | true -> None
        | _ ->
            match Seq.isEmpty options with
            | true -> None
            | false -> Seq.head options

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

[<EntryPoint>]
let main argv =
    let opt : Async<Result<int, string>> =
        seq {
            async { return Error "nothing" }
            async { return Error "another error" }
            //async { return Ok 4 }
        }
        |> AsyncResult.requireAny "expected an OK"

    async {
        let! res = opt        
        printfn "%A" res
    } |> Async.RunSynchronously

    let opt2 =
        seq {
            Error "hi"
            Error "boo"            
        }
        |> Result.requireAny "dont do this"

    printfn "%A" opt2        

    // let res =
    //     lazy None
    //     <|> lazy None
    //     <|> (lazy None <&> lazy Some 3)

    // printfn "%A" res.Value

    0
