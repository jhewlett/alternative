namespace Alternative

//todo: why do the requires take a default error. Just use the first one? (assum seq is non-empty or only use if seq is empty)

module Result =
    let isOk = function
        | Ok _ -> true
        | _ -> false

    let isError = function
        | Error _ -> true
        | _ -> false

    let requireAny error (results : Result<'a, 'b> seq) =
        let mutable okResult = None
        let mutable firstError = None

        use e = results.GetEnumerator()

        while (Option.isNone okResult && e.MoveNext()) do
            match e.Current with
            | Ok _ -> okResult <- Some e.Current
            | Error _ ->
                if Option.isNone firstError then
                    firstError <- Some e.Current

        match okResult with
        | Some ok -> ok
        | None ->
            firstError
            |> Option.defaultValue (Error error)

    let (<|>) a b =
        match a with
        | Ok _ -> a
        | _ ->
            match b with
            | Ok _ -> b
            | _ -> a

module AsyncResult =
    let requireAny error (results : Async<Result<'a, 'b>> seq) =        
        async {
            let mutable okResult = None
            let mutable firstError = None

            use e = results.GetEnumerator()

            while (Option.isNone okResult && e.MoveNext()) do
                let! current = e.Current
                match current with
                | Ok _ -> okResult <- Some current
                | Error _ ->
                    if Option.isNone firstError then
                        firstError <- Some current

            match okResult with
            | Some ok -> return ok
            | None ->
                return
                    firstError
                    |> Option.defaultValue (Error error)
        }

    let (<|>) a b =
        async {
            match! a with
            | Ok _ -> return! a
            | _ ->
                match! b with
                | Ok _ -> return! b
                | _ -> return! a
        }

module Option =
    let requireAny options =
        options
        |> Seq.tryFind Option.isSome
        |> Option.defaultValue None
    
    let (<|>) a b =
        match a with
        | Some _ -> a
        | _ -> b
