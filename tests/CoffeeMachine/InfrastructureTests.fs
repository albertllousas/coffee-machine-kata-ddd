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

let buildDrinkOrderOf drink sugar stick extraHot totalOfMoney=
    DrinkOrder.reconstitute drink sugar stick extraHot (TotalOfMoney totalOfMoney)

type TestData() =
    static member Translations =
        [ (buildDrinkOrderOf Tea NoSugar NoStick Normal 0.4m,                   "T::")
          (buildDrinkOrderOf Tea WithOneSugar AddStick Normal 0.4m,             "T:1:O")
          (buildDrinkOrderOf Tea WithTwoSugars AddStick Normal 0.4m,            "T:2:O")
          (buildDrinkOrderOf Tea WithTwoSugars AddStick ExtraHot 0.4m,          "Th:2:O")
          (buildDrinkOrderOf Chocolate NoSugar NoStick Normal 0.5m,             "H::")
          (buildDrinkOrderOf Chocolate WithOneSugar AddStick Normal 0.5m,       "H:1:O")
          (buildDrinkOrderOf Chocolate WithTwoSugars AddStick Normal 0.5m,      "H:2:O")
          (buildDrinkOrderOf Chocolate WithTwoSugars AddStick ExtraHot 0.5m,    "Hh:2:O")
          (buildDrinkOrderOf Coffee NoSugar NoStick Normal 0.6m,                "C::")
          (buildDrinkOrderOf Coffee WithOneSugar AddStick Normal 0.6m,          "C:1:O")
          (buildDrinkOrderOf Coffee WithTwoSugars AddStick Normal 0.6m,         "C:2:O")
          (buildDrinkOrderOf Coffee WithTwoSugars AddStick ExtraHot 0.6m,       "Ch:2:O")
          (buildDrinkOrderOf OrangeJuice NoSugar NoStick Normal 0.6m,           "O::")
          (buildDrinkOrderOf OrangeJuice WithOneSugar AddStick Normal 0.6m,     "O:1:O")
          (buildDrinkOrderOf OrangeJuice WithTwoSugars AddStick Normal 0.6m,    "O:2:O") ]
        |> Seq.map FSharpValue.GetTupleFields


type ``drink maker with string protocol adapter should``() =

    [<Theory; MemberData("Translations", MemberType = typeof<TestData>)>]
    let ``translate from drink order to a valid drink maker string command`` (drinkOrder: DrinkOrder) (expectedCommand: string) =
        let drinkMaker = FakeDrinkMaker()

        makeDrink drinkMaker drinkOrder

        Assert.Equal(expectedCommand, List.head drinkMaker.ReceivedCommands)
