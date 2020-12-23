module CoffeMachine.AcceptanceTests

open Xunit
open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.Domain.Model.Dependencies
open CoffeeMachine.Infrastructure.OutboundAdapters
open CoffeeMachine

module FakeApplication =

    type FakeDrinkMaker() =
        inherit DrinkMakerDriver()
        member val ReceivedCommands: string list = [] with get, set
        override this.ReceiveCommand command =
            this.ReceivedCommands <- this.ReceivedCommands @ [ command ]
    
    let drinkMaker = FakeDrinkMaker()

    let makeDrink: MakeDrink = DrinkMakerWithStringProtocolAdapter.makeDrink drinkMaker

    let prepareOrder: PrepareOrder = DrinkOrder.prepareWith
 
    let serveDrinkUseCase = UseCases.serveDrink prepareOrder makeDrink

    type FakePad(serveDrinkUseCase: (UseCases.DrinkRequest -> unit)) =
        member this.ProcessCustomerOrder order = serveDrinkUseCase order
    
    let pad = FakePad(serveDrinkUseCase)

open FakeApplication

[<Fact>]
let ``process a customer order`` () =
        pad.ProcessCustomerOrder { Drink = Coffee; Sugar = WithTwoSugars }

        Assert.Equal("C:2:O", List.head drinkMaker.ReceivedCommands)



    

