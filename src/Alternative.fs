namespace Alternative

//todo: why do the requires take a default error. Just use the first one? (assum seq is non-empty or only use if seq is empty)

module AsyncSeq =
    let foldIf predicate folder state (xs : Async<'a> seq) =
        async {
            let mutable acc = state
            use e = xs.GetEnumerator()

            while predicate acc xs && e.MoveNext() do
                let! current = e.Current
                acc <- folder acc current

            return acc
        }

module Seq =
    let foldIf predicate folder state (xs : 'a seq) =
        let mutable acc = state
        use e = xs.GetEnumerator()

        while predicate acc xs && e.MoveNext() do
            acc <- folder acc e.Current

        acc

module Result =
    let isOk = function
        | Ok _ -> true
        | _ -> false

    let isError = function
        | Error _ -> true
        | _ -> false

    let (<|>) a b =
        match a with
        | Ok _ -> a
        | _ -> b

    let requireAny error results =
        Seq.foldIf
            (fun acc _ -> isError acc)
            (<|>)
            (Error error)
            results

module AsyncResult =
    let (<|>) a b =
        async {
            match! a with
            | Ok _ -> return! a
            | _ -> return! b
        }

    let requireAny error results =        
        AsyncSeq.foldIf
            (fun acc _ -> Result.isError acc)
            Result.(<|>)
            (Error error)
            results

module Option =
    let (<|>) a b =
        match a with
        | Some _ -> a
        | _ -> b

    let requireAny options =
        Seq.foldIf
            (fun acc _ -> Option.isNone acc)
            (<|>)
            None
            options
