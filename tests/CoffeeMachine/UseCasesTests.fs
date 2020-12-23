module CoffeeMachine.UseCasesTests

open Xunit
open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.UseCases
open Foq

// Since Foq does not support stub or mock functions we need to wrap
type Consume<'t> =
    abstract fn: 't -> unit

type Function2<'a, 'b, 'c> =
    abstract fn: 'a -> 'b -> 'c

type ``serve a drink usecase should``() =

    [<Fact>]
    let ``success serving a drink to a customer`` () =
        let teaOrder =
            DrinkOrder.reconstitute Tea NoSugar NoStick

        let prepareOrderStub =
            Mock<Function2<Drink, Sugar, DrinkOrder>>()
                .Setup(fun mock -> <@ mock.fn Tea NoSugar @>)
                .Returns(teaOrder)
                .Create()

        let makeDrinkMock = Mock.Of<Consume<DrinkOrder>>()

        serveDrink prepareOrderStub.fn makeDrinkMock.fn { Drink = Tea; Sugar = NoSugar }

        verify <@ makeDrinkMock.fn teaOrder @>

type ``forward message usecase should``() =

    [<Fact>]
    let ``success displaying a message to the customer`` () =
        let displayMessageMock = Mock.Of<Consume<Message>>()

        forwardMessageToCustomer displayMessageMock.fn (Message("Hello"))

        verify <@ displayMessageMock.fn (Message "Hello") @>
