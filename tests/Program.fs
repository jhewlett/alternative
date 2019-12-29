module Alternative.Tests.Main

open Expecto
open Alternative

module Expect' =
    let isNone actual =
        Expect.isNone actual ""

    let equal expected actual =
        Expect.equal actual expected ""

[<EntryPoint>]
let main argv =
    let tests =
        testList "Unit tests" [
            testList "Option" [
                testList "requireAny" [
                    test "All nones" {
                        seq {
                            None
                            None
                        }
                        |> Option.requireAny
                        |> Expect'.isNone                
                    }

                    test "Single Some" {
                        seq {
                            None
                            Some 1
                        }
                        |> Option.requireAny
                        |> Expect'.equal (Some 1)                
                    }

                    test "Multiple Some, takes first" {
                        let result =
                            seq {
                                Some 1
                                Some 2
                            }
                            |> Option.requireAny
                        Expect.equal result (Some 1) "Expected first Some"                
                    }

                    test "Evaluates lazily" {
                        let mutable evaluatedEagerly = false
                        let result =
                            seq {
                                Some 1
                                evaluatedEagerly <- true
                            }
                            |> Option.requireAny
                        Expect.isFalse evaluatedEagerly "Expected to stop evaluating after seeing a Some"                
                    }

                    test "Empty list" {
                        let result = [ ] |> Option.requireAny
                        
                        Expect.isNone result "Expected None for an empty list"                
                    }      
                ]

                testList "requireAll" [
                    test "All nones" {
                        let result =
                            seq {
                                None
                            }
                            |> Option.requireAll
                        Expect.isNone result "Expected None"                
                    }

                    test "None and Some" {
                        let result =
                            seq {
                                None
                                Some 1
                            }
                            |> Option.requireAll
                        Expect.isNone result "Expected None"                
                    }

                    test "Multiple Some, takes first" {
                        let result =
                            seq {
                                Some 1
                                Some 2
                            }
                            |> Option.requireAll
                        Expect.equal result (Some 1) "Expected first Some"                
                    }

                    test "Evaluates lazily" {
                        let mutable evaluatedEagerly = false
                        let result =
                            seq {
                                None
                                evaluatedEagerly <- true
                            }
                            |> Option.requireAll
                        Expect.isFalse evaluatedEagerly "Expected to stop evaluating after seeing a None"                
                    }

                    test "Empty list" {
                        let result = [ ] |> Option.requireAll
                        
                        Expect.isNone result "Expected None for an empty list"                
                    }      
                ]
            ]
        
            testList "AsyncResult" [
                testAsync "requireAny" {
                    let! result =
                        seq {
                            async { return Error "message" }
                        }
                        |> AsyncResult.requireAny "default"
                        
                    Expect.equal result (Error "message") ""
                }

                testAsync "requireAny2" {
                    let! result =
                        seq {
                            async { return Error "message" }
                            async { return Ok () }
                        }
                        |> AsyncResult.requireAny "default"
                        
                    Expect.equal result (Ok ()) ""
                }
                
                testAsync "requireAll" {
                    let! result =
                        seq {
                            async { return Ok () }
                            async { return Error "message" }
                        }
                        |> AsyncResult.requireAll "default"

                    Expect.equal result (Error "message") ""
                }
            ]
        ]        

    //let tests =
        // match argv with
        // | [| "--unit" |] -> unitTests
        // | [| "--integration" |] -> integrationTests
        // | _ -> testList "all tests" [unitTests; integrationTests]
    
    runTests defaultConfig tests