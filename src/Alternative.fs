namespace Alternative

module Option =
    let takeFirstSome options =
        options
        |> Seq.tryFind Option.isSome
        |> Option.defaultValue None
    
    let (<|>) a b =
        match a with
        | Some _ -> a
        | _ -> b

module Result =
    let isOk = function
        | Ok _ -> true
        | _ -> false

    let takeFirstOk error results =
        match results |> Seq.tryFind isOk with
        | Some r -> r
        | None ->
            results
            |> Seq.tryHead
            |> Option.defaultValue (Error error)

    let (<|>) a b =
        match a with
        | Ok _ -> a
        | _ ->
            match b with
            | Ok _ -> b
            | _ -> a

module Async =
    let map f a =
        async {
            let! a' = a
            return f a'
        }

    let sequence (t : Async<'a> seq) : Async<'a seq> =
        async {
            let! ct = Async.CancellationToken
            return seq {
                use enum = t.GetEnumerator ()
                while enum.MoveNext () do
                    yield Async.RunSynchronously (enum.Current, cancellationToken = ct)
            }
        }

module AsyncResult =
    let takeFirstOk error =
        Async.sequence >> Async.map (Result.takeFirstOk error)

    let (<|>) a b =
        async {
            match! a with
            | Ok _ -> return! a
            | _ ->
                match! b with
                | Ok _ -> return! b
                | _ -> return! a
        }
