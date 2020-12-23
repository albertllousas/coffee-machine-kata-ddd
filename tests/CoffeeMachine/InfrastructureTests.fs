module CoffeeMachine.InfrastructureTests

open Xunit
open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.Infrastructure.OutboundAdapters
open CoffeeMachine.Infrastructure.OutboundAdapters.DrinkMakerWithStringProtocolAdapter
open FSharp.Reflection

type FakeDrinkMaker() =
    inherit DrinkMakerDriver()
    member val ReceivedCommands: string list = [] with get, set

    override this.ReceiveCommand command =
        this.ReceivedCommands <- this.ReceivedCommands @ [ command ]

let buildDrinkOrderOf drink sugar stick =
    DrinkOrder.reconstitute drink sugar stick

type TestData() =
    static member Translations =
        [ (buildDrinkOrderOf Tea NoSugar NoStick,               "T::")
          (buildDrinkOrderOf Tea WithOneSugar WithStick,        "T:1:O")
          (buildDrinkOrderOf Tea WithTwoSugars WithStick,       "T:2:O")
          (buildDrinkOrderOf Chocolate NoSugar NoStick,         "H::")
          (buildDrinkOrderOf Chocolate WithOneSugar WithStick,  "H:1:O")
          (buildDrinkOrderOf Chocolate WithTwoSugars WithStick, "H:2:O")
          (buildDrinkOrderOf Coffee NoSugar NoStick,            "C::")
          (buildDrinkOrderOf Coffee WithOneSugar WithStick,     "C:1:O")
          (buildDrinkOrderOf Coffee WithTwoSugars WithStick,    "C:2:O") ]
        |> Seq.map FSharpValue.GetTupleFields

type ``drink maker with string protocol adapter should``() =

    [<Theory; MemberData("Translations", MemberType = typeof<TestData>)>]
    let ``translate from drink order to a valid drink maker string command`` (drinkOrder: DrinkOrder) (expectedCommand: string) =
        let drinkMaker = FakeDrinkMaker()

        makeDrink drinkMaker drinkOrder

        Assert.Equal(expectedCommand, List.head drinkMaker.ReceivedCommands)
