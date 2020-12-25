module CoffeeMachine.Domain.ModelTests

open Xunit
open CoffeeMachine.Domain.Model.Types

type ``drink order aggregate should``() =

    [<Fact>]
    let ``success preparing a valid drink order`` () =
        let coffee: Drink = {DrinkId = DrinkId "coffee"; DrinkType = Coffee; Price = Price 0.6m}
        let quantityOfSugar = 0
        let money = 0.6m

        let result = DrinkOrder.prepareWith quantityOfSugar money coffee
        
        let expectedOrder = DrinkOrder.reconstitute Coffee NoSugar NoStick (TotalOfMoney 0.6m)
        Assert.Equal(Ok expectedOrder, result)

    [<Fact>]
    let ``add an stick when the preparing an order with sugar`` () =
        let coffee: Drink = {DrinkId = DrinkId "coffee"; DrinkType = Coffee; Price = Price 0.6m}
        let quantityOfSugar = 1
        let money = 0.6m

        let result = DrinkOrder.prepareWith quantityOfSugar money coffee
        
        let expectedOrder = DrinkOrder.reconstitute Coffee WithOneSugar AddStick (TotalOfMoney 0.6m)
        Assert.Equal(Ok expectedOrder, result)

    [<Fact>]
    let ``fail preparing a drink order when not enough money is provided`` () =
        let coffee: Drink = {DrinkId = DrinkId "coffee"; DrinkType = Coffee; Price = Price 0.6m}
        let quantityOfSugar = 0
        let money = 0.5m

        let result = DrinkOrder.prepareWith quantityOfSugar money coffee
        
        Assert.Equal(Error (NotEnoughMoney(moneyMissing=0.1m)), result)

    [<Fact>]
    let ``fail preparing a drink order when invalid quanity of sugar is provided`` () =
        let coffee: Drink = {DrinkId = DrinkId "coffee"; DrinkType = Coffee; Price = Price 0.6m}
        let quantityOfSugar = 10
        let amountOfMoney = 0.6m

        let result = DrinkOrder.prepareWith quantityOfSugar amountOfMoney coffee
        
        Assert.Equal(Error InvalidQuantityOfSugar, result)

type ``sugar factory should``() =

    [<Fact>]
    let ``create a no sugar when quantity is zero`` () =
        Assert.Equal(Ok NoSugar, Sugar.from 0)

    [<Fact>]
    let ``create a sugar when quantity is one`` () =
        Assert.Equal(Ok WithOneSugar, Sugar.from 1)

    [<Fact>]
    let ``create a no sugar when quantity is two`` () =
        Assert.Equal(Ok WithTwoSugars, Sugar.from 2)

    [<Fact>]
    let ``not create a no sugar when invalid quantity is provided`` () =
        Assert.Equal(Error InvalidQuantityOfSugar, Sugar.from 10)

type ``stick factory should``() =

    [<Fact>]
    let ``create a no stick option when there is no sugar`` () =
        Assert.Equal(NoStick, Stick.from NoSugar)

    [<Fact>]
    let ``create a stick option when there is sugar`` () =
        Assert.Equal(AddStick, Stick.from WithOneSugar)

open Foq
open CoffeeMachine.Domain.Model.DomainServices

// Since Foq does not support stub or mock functions we need to wrap
type Consume<'t> =
    abstract fn: 't -> unit

type ``display error message domain service should``() =

    [<Fact>]
    let ``display a message when not enough money`` () =
        let displayMessageMock = Mock.Of<Consume<Message>>()

        displayErrorMessage displayMessageMock.fn (NotEnoughMoney(moneyMissing = 0.1m))

        verify <@ displayMessageMock.fn (Message $"Not enough money, please add 0.1 more") @>
    
    [<Fact>]
    let ``display a message when invalid quantity of sugar`` () =
        let displayMessageMock = Mock.Of<Consume<Message>>()

        displayErrorMessage displayMessageMock.fn InvalidQuantityOfSugar

        verify <@ displayMessageMock.fn (Message "Invalid quantity of sugar") @>
    
    [<Fact>]
    let ``display a message when non existent product`` () =
        let displayMessageMock = Mock.Of<Consume<Message>>()

        displayErrorMessage displayMessageMock.fn (NonExistentProduct(productCode = "coke") )

        verify <@ displayMessageMock.fn (Message $"Non existent product with code 'coke'") @>
