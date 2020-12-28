module CoffeeMachine.InfrastructureTests

open Xunit
open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.Infrastructure.OutboundAdapters
open CoffeeMachine.Infrastructure.OutboundAdapters.DrinkMakerWithStringProtocolAdapter
open FSharp.Reflection


let tea: Drink =
    { DrinkId = DrinkId "tea"
      DrinkType = Tea
      Price = Price 0.4m }

let coffee: Drink =
    { DrinkId = DrinkId "coffee"
      DrinkType = Coffee
      Price = Price 0.6m }

let chocolate: Drink =
    { DrinkId = DrinkId "chocolate"
      DrinkType = Chocolate
      Price = Price 0.5m }

let orangeJuice: Drink =
    { DrinkId = DrinkId "orange-juice"
      DrinkType = OrangeJuice
      Price = Price 0.6m }

let buildDrinkOrderOf drink sugar stick extraHot totalOfMoney =
    DrinkOrder.reconstitute drink sugar stick extraHot (TotalOfMoney totalOfMoney)

type CrashDrinkMaker() =
    inherit DrinkMakerDriver()
    override this.ReceiveCommand _ = raise (System.Exception("boom!"))

type TestData() =
    static member Translations =
        [ (buildDrinkOrderOf tea NoSugar NoStick Normal 0.4m, "T::")
          (buildDrinkOrderOf tea WithOneSugar AddStick Normal 0.4m, "T:1:O")
          (buildDrinkOrderOf tea WithTwoSugars AddStick Normal 0.4m, "T:2:O")
          (buildDrinkOrderOf tea WithTwoSugars AddStick ExtraHot 0.4m, "Th:2:O")
          (buildDrinkOrderOf chocolate NoSugar NoStick Normal 0.5m, "H::")
          (buildDrinkOrderOf chocolate WithOneSugar AddStick Normal 0.5m, "H:1:O")
          (buildDrinkOrderOf chocolate WithTwoSugars AddStick Normal 0.5m, "H:2:O")
          (buildDrinkOrderOf chocolate WithTwoSugars AddStick ExtraHot 0.5m, "Hh:2:O")
          (buildDrinkOrderOf coffee NoSugar NoStick Normal 0.6m, "C::")
          (buildDrinkOrderOf coffee WithOneSugar AddStick Normal 0.6m, "C:1:O")
          (buildDrinkOrderOf coffee WithTwoSugars AddStick Normal 0.6m, "C:2:O")
          (buildDrinkOrderOf coffee WithTwoSugars AddStick ExtraHot 0.6m, "Ch:2:O")
          (buildDrinkOrderOf orangeJuice NoSugar NoStick Normal 0.6m, "O::")
          (buildDrinkOrderOf orangeJuice WithOneSugar AddStick Normal 0.6m, "O:1:O")
          (buildDrinkOrderOf orangeJuice WithTwoSugars AddStick Normal 0.6m, "O:2:O") ]
        |> Seq.map FSharpValue.GetTupleFields


type ``drink maker with string protocol adapter should``() =

    [<Theory; MemberData("Translations", MemberType = typeof<TestData>)>]
    let ``translate from drink order to a valid drink maker string command`` (drinkOrder: DrinkOrder) (expectedCommand: string) =
        let drinkMaker = FakeDrinkMaker()

        let result = makeDrink drinkMaker drinkOrder

        Assert.Equal(result, Ok drinkOrder)
        Assert.Equal(expectedCommand, List.head drinkMaker.ReceivedCommands)


    [<Fact>]
    let ``fail if drink maker crashes``() =
        let drinkMaker = CrashDrinkMaker()
        let drinkOrder = buildDrinkOrderOf tea NoSugar NoStick Normal 0.4m

        let result = makeDrink drinkMaker drinkOrder

        Assert.Equal(Error(DrinkMakerError(error = "boom!")), result)
