module Alternative.Tests.Main

open Expecto
open Alternative

[<EntryPoint>]
let main argv =
    let tests =
        testList "Unit tests" [
            testList "Option" [
                testList "takeFirstSome" [
                    test "All Nones" {
                        let result =
                            seq {
                                None
                                None
                            }
                            |> Option.takeFirstSome

                        Expect.isNone result "Expected None"          
                    }

                    test "Single Some" {
                        let result =
                            seq {
                                None
                                Some 1
                            }
                            |> Option.takeFirstSome

                        Expect.equal result (Some 1) "Expected to pass over the None and take the Some"    
                    }

                    test "Multiple Somes" {
                        let result =
                            seq {
                                Some 1
                                Some 2
                            }
                            |> Option.takeFirstSome

                        Expect.equal result (Some 1) "Expected to take the first Some"                
                    }

                    test "Lazy evaluation" {
                        let mutable evaluatedEagerly = false
                        
                        let result =
                            seq {
                                Some 1
                                evaluatedEagerly <- true
                            }
                            |> Option.takeFirstSome

                        Expect.equal result (Some 1) "Expected Some 1"
                        Expect.isFalse evaluatedEagerly "Expected to stop evaluating after seeing a Some"                
                    }

                    test "Empty list" {
                        let result = [ ] |> Option.takeFirstSome
                        
                        Expect.isNone result "Expected None for an empty list"                
                    }      
                ]
            
                testList "<|>" [
                    testProperty "Behaves the same as takeFirstSome" (fun (a: Option<int>) (b : Option<int>) ->
                        Option.(<|>) a b = Option.takeFirstSome [ a; b ]
                    )
                ]
            ]    
        
            testList "AsyncResult" [
                testList "takeFirstOk" [            
                    testAsync "All Errors" {
                        let! result =
                            seq {
                                async { return Error "message 1" }
                                async { return Error "message 2" }
                            }
                            |> AsyncResult.takeFirstOk "default"
                            
                        Expect.equal result (Error "message 1") "Expected to take the first Error"
                    }

                    testAsync "Single Ok" {
                        let! result =
                            seq {
                                async { return Error "message" }
                                async { return Ok () }
                            }
                            |> AsyncResult.takeFirstOk "default"
                            
                        Expect.equal result (Ok ()) "Expected to skip over the Error and take the Ok"
                    }

                    testAsync "Multiple Ok" {
                        let! result =
                            seq {
                                async { return Ok 1 }
                                async { return Ok 2 }
                            }
                            |> AsyncResult.takeFirstOk "default"
                            
                        Expect.equal result (Ok 1) "Expected to take the first Ok"
                    }

                    testAsync "Lazy evaluation" {
                        let mutable evaluatedEagerly = false
                        
                        let! result =
                            seq {
                                async { return Ok () }
                                evaluatedEagerly <- true
                            }
                            |> AsyncResult.takeFirstOk "error"
                        
                        Expect.equal result (Ok ()) "Expected Ok ()"
                        Expect.isFalse evaluatedEagerly "Expected to stop evaluating after seeing an Ok"                
                    }

                    testAsync "Empty list" {
                        let! result = [ ] |> AsyncResult.takeFirstOk "default"
                        
                        Expect.equal result (Error "default") "Expected default Error for an empty list"
                    }
                ]
            
                testList "<|>" [
                    testProperty "Behaves the same as takeFirstOk" (fun (a: Result<string, string>) (b : Result<string, string>) ->
                        async {
                            let! expected =
                                [
                                    async { return a }
                                    async { return b }
                                ]
                                |> AsyncResult.takeFirstOk "error"

                            let! actual =
                                AsyncResult.(<|>)
                                    (async { return a })
                                    (async { return b })

                            return actual = expected
                        }
                        |> Async.RunSynchronously
                    )
                ]
            ]

            testList "Result" [
                testList "takeFirstOk" [            
                    test "All errors" {
                        let result =
                            seq {
                                Error "message 1"
                                Error "message 2"
                            }
                            |> Result.takeFirstOk "default"
                            
                        Expect.equal result (Error "message 1") "Expected to take the first Error"
                    }

                    test "Single Ok" {
                        let result =
                            seq {
                                Error "message"
                                Ok ()
                            }
                            |> Result.takeFirstOk "default"
                            
                        Expect.equal result (Ok ()) "Expected to skip over the Error and take the Ok"
                    }

                    test "Multiple Ok" {
                        let result =
                            seq {
                                Ok 1
                                Ok 2
                            }
                            |> Result.takeFirstOk "default"
                            
                        Expect.equal result (Ok 1) "Expected to take the first Ok"
                    }

                    test "Lazy evaluation" {
                        let mutable evaluatedEagerly = false
                        
                        let result =
                            seq {
                                Ok ()
                                evaluatedEagerly <- true
                            }
                            |> Result.takeFirstOk "error"
                        
                        Expect.equal result (Ok ()) "Expected Ok ()"
                        Expect.isFalse evaluatedEagerly "Expected to stop evaluating after seeing an Ok"                
                    }

                    test "Empty list" {
                        let result = [ ] |> Result.takeFirstOk "default"
                        
                        Expect.equal result (Error "default") "Expected default Error for an empty list"
                    }
                ]
            
                testList "<|>" [
                    testProperty "Behaves the same as takeFirstOk" (fun (a: Result<string, string>) (b : Result<string, string>) ->
                        let expected = [ a; b ] |> Result.takeFirstOk "error"
                        let actual = Result.(<|>) a b

                        actual = expected
                    )
                ]
            ]
        ]
    
    runTests defaultConfig tests
    