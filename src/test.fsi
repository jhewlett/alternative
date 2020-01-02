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
