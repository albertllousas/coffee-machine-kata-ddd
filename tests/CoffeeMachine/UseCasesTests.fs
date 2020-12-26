module CoffeeMachine.UseCasesTests

open Xunit
open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.UseCases
open CoffeeMachine.Domain.Model.Dependencies.Aliases
open Foq

// Since Foq does not support stub or mock functions we need to wrap
type Consume<'t> =
    abstract fn: 't -> unit

type Function<'a, 'b> =
    abstract fn: 'a -> 'b

type Function4<'a, 'b, 'c, 'd, 'e> =
    abstract fn: 'a -> 'b -> 'c -> 'd -> 'e

type ``serve a drink usecase should``() =

    [<Fact>]
    let ``success serving a drink to a customer`` () =
        let teaOrder = DrinkOrder.reconstitute Tea NoSugar NoStick Normal (TotalOfMoney 0.5m)
        let tea: Drink =
            { DrinkId = DrinkId "tea"
              DrinkType = Coffee
              Price = Price 0.6m }
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
        let makeDrinkMock = Mock.Of<Consume<DrinkOrder>>()
        let displayErrorMessageMock = Mock.Of<Consume<Error>>()
        let request: DrinkRequest =
            { DrinkId = "tea"
              Sugar = 0
              ExtraHot = false
              Money = 0.5m }

        serveDrink findDrinkStub.fn displayErrorMessageMock.fn prepareOrderStub.fn makeDrinkMock.fn request
        |> ignore

        verify <@ makeDrinkMock.fn teaOrder @>
