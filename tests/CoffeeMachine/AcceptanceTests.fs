module CoffeMachine.AcceptanceTests

open Xunit
open CoffeeMachine.Infrastructure.FakeApplication
open CoffeeMachine

[<Fact>]
let ``process a customer order`` () =
    let app = App()
    let request: UseCases.DrinkRequest =
        { DrinkId = "tea"
          Sugar = 1
          ExtraHot = false
          Money = 0.5m }

    app.Pad.ProcessCustomerOrder request

    Assert.Equal("T:1:O", List.head app.DrinkMaker.ReceivedCommands)

[<Fact>]
let ``display an error message when not enough money is provided`` () =
    let app = App()
    let request: UseCases.DrinkRequest =
        { DrinkId = "tea"
          Sugar = 1
          ExtraHot = false
          Money = 0.1m }

    app.Pad.ProcessCustomerOrder request

    Assert.Equal("M:Not enough money, please add 0.3 more", List.head app.DrinkMaker.ReceivedCommands)
