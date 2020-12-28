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

[<Fact>]
let ``print report`` () =
    let app = App()
        
    app.Pad.ProcessCustomerOrder { DrinkId = "tea"; Sugar = 1; ExtraHot = false; Money = 0.6m }
    app.Pad.ProcessCustomerOrder { DrinkId = "tea"; Sugar = 1; ExtraHot = false; Money = 0.6m }
    
    app.Pad.PrintReport()

    Assert.Equal("total = 0.8", List.head app.Printer.PrintedLines)
    Assert.Equal("Tea = 2", List.last app.Printer.PrintedLines)

[<Fact>]
let ``send and email notification and display a message when there is a drink shortage`` () =
    let app = App()
    app.BeverageQuantityChecker.Empty <- true
    let request: UseCases.DrinkRequest =
        { DrinkId = "tea"
          Sugar = 1
          ExtraHot = false
          Money = 0.5m }

    app.Pad.ProcessCustomerOrder request

    Assert.Equal("M:Sorry, there is no more tea, a notification to refill it has been sent", List.head app.DrinkMaker.ReceivedCommands)
    Assert.Equal("tea", List.head app.EmailNotifier.ReceivedEmails)
