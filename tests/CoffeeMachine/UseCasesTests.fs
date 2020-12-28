module CoffeeMachine.UseCasesTests

open Xunit
open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.UseCases
open CoffeeMachine.Domain.Model.Dependencies.Aliases
open Foq

// Since Foq does not support stub or mock functions we need to wrap
type ConsumeUnit =
    abstract fn: unit -> unit

type Consume<'t> =
    abstract fn: 't -> unit

type Function<'a, 'b> =
    abstract fn: 'a -> 'b

type Function4<'a, 'b, 'c, 'd, 'e> =
    abstract fn: 'a -> 'b -> 'c -> 'd -> 'e

type ``serve a drink usecase should``() =

    [<Fact>]
    let ``success serving a drink to a customer`` () =
        let tea: Drink =
            { DrinkId = DrinkId "tea"
              DrinkType = Coffee
              Price = Price 0.6m }
        let teaOrder = DrinkOrder.reconstitute tea NoSugar NoStick Normal (TotalOfMoney 0.5m)
        let findDrinkStub =
            Mock<Function<DrinkId, Result<Drink, Error>>>()
                .Setup(fun mock -> <@ mock.fn (DrinkId "tea") @>)
                .Returns(Ok tea)
                .Create()
        let prepareOrderStub =
            Mock<Function4<QuantityOfSugar, Money, ExtraHot, Drink, Result<DrinkOrder, Error>>>()
                .Setup(fun mock -> <@ mock.fn 0 0.5m false tea @>)
                .Returns(Ok teaOrder)
                .Create()
        let makeDrinkStub = 
            Mock<Function<DrinkOrder, Result<DrinkOrder, Error>>>()
                .Setup(fun mock -> <@ mock.fn teaOrder @>)
                .Returns(Ok teaOrder)
                .Create()
        let updateStatisticsMock = Mock.Of<Consume<DrinkOrder>>()
        let handleDrinkNotServedMock = Mock.Of<Consume<Error>>()
        let request: DrinkRequest =
            { DrinkId = "tea"
              Sugar = 0
              ExtraHot = false
              Money = 0.5m }

        serveDrink findDrinkStub.fn handleDrinkNotServedMock.fn prepareOrderStub.fn makeDrinkStub.fn updateStatisticsMock.fn request
        |> ignore

        verify <@ updateStatisticsMock.fn teaOrder @> once
        verify <@ handleDrinkNotServedMock.fn (any()) @> never

    [<Fact>]
    let ``handle drink nor served when there is an error`` () =
        let findDrinkStub =
            Mock<Function<DrinkId, Result<Drink, Error>>>()
                .Setup(fun mock -> <@ mock.fn (DrinkId "teaa") @>)
                .Returns(Error(NonExistentProduct(productCode = "teaa")))
                .Create()
        let prepareOrderStub = Mock.Of<Function4<QuantityOfSugar, Money, ExtraHot, Drink, Result<DrinkOrder, Error>>>()
        let makeDrinkStub = Mock.Of<Function<DrinkOrder, Result<DrinkOrder, Error>>>()
        let updateStatisticsMock = Mock.Of<Consume<DrinkOrder>>()
        let handleDrinkNotServedMock = Mock.Of<Consume<Error>>()
        let request: DrinkRequest =
            { DrinkId = "teaa"
              Sugar = 0
              ExtraHot = false
              Money = 0.5m }

        serveDrink findDrinkStub.fn handleDrinkNotServedMock.fn (prepareOrderStub.fn) makeDrinkStub.fn updateStatisticsMock.fn request
            |> ignore

        verify <@ updateStatisticsMock.fn (any()) @> never
        verify <@ handleDrinkNotServedMock.fn (NonExistentProduct(productCode = "teaa")) @> once    

type ``print report should``() =

    [<Fact>]
    let ``print statistics`` () =
        let printStatisticsMock = Mock.Of<ConsumeUnit>()

        printReport printStatisticsMock.fn ()

        verify <@ printStatisticsMock.fn () @> once


