# Alternative

Inspired by the `Alternative` type class from Haskell/Category theory

Finds the first `Some` in a set of `Option<'a>`, or `None` if they're all `None`.

## Usage
```fsharp
open Alternative.Option

printfn "%A" (None <|> Some 1 <|> Some 2)   //Some 1
```

If you're dealing with a list of things, or if you want to take advantage of laziness and only evaluate what's needed, then use the `takeFirstSome` version:

```fsharp
let fetchFromCache () = Some 1
let fetchFromDb () = Some 1

seq {
    fetchFromCache ()
    fetchFromDb ()      // never called
}
|> Option.takeFirstSome
|> printfn "%A"    //Some 1
```

Since this is a `seq`, values will only be produced as needed. This is very useful for things like IO (e.g. network calls, database queries). The entire sequence will only be consumed if all of the items are `None`.

There are also versions of `<|>` and `takeFirstOk` that work with `Result` and `Async<Result>`. These require you to provide a default `Error` to use in case of an empty sequence.

For example:

```fsharp
let fetchFromCache () = async { return Ok 1 }
let fetchFromDb () = async { return Ok 1 }

seq {
    fetchFromCache ()
    fetchFromDb ()    //never called
}
|> AsyncResult.takeFirstOk "Empty list"
|> Async.RunSynchronously
|> printfn "%A"    //Ok 1
```

If you want to run your `Async`s in parallel, then just use `Async.Parallel` like this:

```fsharp
module Async =
    let map f x = async { let! x' = x in return f x' }

let fetchFromCache () = async { return Ok 1 }
let fetchFromDb () = async { return Ok 1 }

seq {
    fetchFromCache ()
    fetchFromDb ()
}
|> Async.Parallel
|> Async.map (Result.takeFirstOk "Empty list")   
|> Async.RunSynchronously
|> printfn "%A"    //Ok 1
```

## Api Reference

```fsharp
namespace Alternative

module Option =
    val takeFirstSome: options:seq<'a option> -> 'a option
    val (<|>): a:'a option -> b:'a option -> 'a option

module Result =
    val takeFirstOk: error:'b -> results:seq<Result<'a, 'b>> -> Result<'a, 'b>
    val (<|>): a:Result<'a, 'b> -> b:Result<'a, 'b> -> Result<'a, 'b>

module AsyncResult =
    val takeFirstOk: error:'b -> results:seq<Async<Result<'a, 'b>>> -> Async<Result<'a, 'b>>
    val (<|>): a:Async<Result<'a, 'b>> -> b:Async<Result<'a, 'b>> -> Async<Result<'a, 'b>>
```