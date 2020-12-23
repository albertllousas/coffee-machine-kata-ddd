module CoffeeMachine.Domain.ModelTests

open Xunit
open CoffeeMachine.Domain.Model.Types

type ``drink order aggregate should``() =

    [<Fact>]
    let ``prepare a valid drink order`` () =
        let coffeeWithNoSugar = DrinkOrder.reconstitute Coffee NoSugar NoStick

        let result = DrinkOrder.prepareWith Coffee NoSugar
        
        Assert.Equal(coffeeWithNoSugar, result)

    [<Fact>]
    let ``add an stick when the preparing an order with sugar`` () =
        let coffeeWithStick = DrinkOrder.reconstitute Coffee WithOneSugar WithStick

        let result = DrinkOrder.prepareWith Coffee WithOneSugar
        
        Assert.Equal(coffeeWithStick, result)

