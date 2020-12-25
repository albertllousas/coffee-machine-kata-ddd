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

let buildDrinkOrderOf drink sugar stick totalOfMoney=
    DrinkOrder.reconstitute drink sugar stick (TotalOfMoney totalOfMoney)

type TestData() =
    static member Translations =
        [ (buildDrinkOrderOf Tea NoSugar NoStick 0.4m,              "T::")
          (buildDrinkOrderOf Tea WithOneSugar AddStick 0.4m,        "T:1:O")
          (buildDrinkOrderOf Tea WithTwoSugars AddStick 0.4m,       "T:2:O")
          (buildDrinkOrderOf Chocolate NoSugar NoStick 0.5m,        "H::")
          (buildDrinkOrderOf Chocolate WithOneSugar AddStick 0.5m,  "H:1:O")
          (buildDrinkOrderOf Chocolate WithTwoSugars AddStick 0.5m, "H:2:O")
          (buildDrinkOrderOf Coffee NoSugar NoStick 0.6m,           "C::")
          (buildDrinkOrderOf Coffee WithOneSugar AddStick 0.6m,     "C:1:O")
          (buildDrinkOrderOf Coffee WithTwoSugars AddStick 0.6m,    "C:2:O") ]
        |> Seq.map FSharpValue.GetTupleFields

type ``drink maker with string protocol adapter should``() =

    [<Theory; MemberData("Translations", MemberType = typeof<TestData>)>]
    let ``translate from drink order to a valid drink maker string command`` (drinkOrder: DrinkOrder) (expectedCommand: string) =
        let drinkMaker = FakeDrinkMaker()

        makeDrink drinkMaker drinkOrder

        Assert.Equal(expectedCommand, List.head drinkMaker.ReceivedCommands)
