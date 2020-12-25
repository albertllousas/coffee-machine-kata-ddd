namespace CoffeeMachine.Infrastructure

open CoffeeMachine.Domain.Model.Types

open CoffeeMachine.Domain.Model

module OutboundAdapters =

    [<AbstractClass>]
    type DrinkMakerDriver() =
        abstract member ReceiveCommand: string -> unit
    
    type FakeDrinkMaker() =
        inherit DrinkMakerDriver()
        member val ReceivedCommands: string list = [] with get, set
        override this.ReceiveCommand command =
            this.ReceivedCommands <- this.ReceivedCommands @ [ command ]

    module DrinkMakerWithStringProtocolAdapter =

        let private translateDrink (drink: DrinkType) = 
            match drink with 
            | Tea -> "T" 
            | Coffee -> "C"
            | Chocolate -> "H"
        
        let private translateSugar (sugar: Sugar) = 
            match sugar with 
            | NoSugar -> "" 
            | WithOneSugar -> "1"
            | WithTwoSugars -> "2"

        let private translateStick (stick: Stick) =
            match stick with 
            | NoStick -> ""
            | AddStick -> "O" 

        let makeDrink (drinkMaker: DrinkMakerDriver): Dependencies.MakeDrink = fun (drinkOrder: DrinkOrder) -> 
            $"{translateDrink drinkOrder.DrinkType}:{translateSugar drinkOrder.Sugar}:{translateStick drinkOrder.Stick}"
            |> drinkMaker.ReceiveCommand

        let displayMessage (drinkMaker: DrinkMakerDriver): Dependencies.DisplayMessage = fun (Message message) ->
            drinkMaker.ReceiveCommand $"M:{message}"

    module FindDrinkAdapter =

        type InMemoryDrinkRepository() =
            member this.FindDrink: Dependencies.FindDrink = fun drinkId ->
                match drinkId with 
                | DrinkId "tea" -> Ok { DrinkId = drinkId; DrinkType = Tea; Price = Price 0.4m }
                | DrinkId "coffee" -> Ok { DrinkId = drinkId; DrinkType = Coffee; Price = Price 0.6m }
                | DrinkId "chocolate" -> Ok { DrinkId = drinkId; DrinkType = Chocolate; Price = Price 0.5m }
                | DrinkId nonExistentId -> Error (NonExistentProduct(productCode = nonExistentId))
    
    
module InboundAdapters =

    open CoffeeMachine

    type FakePad(serveDrinkUseCase: (UseCases.DrinkRequest -> unit)) =
        member this.ProcessCustomerOrder order = serveDrinkUseCase order

module FakeApplication =

    open CoffeeMachine.Domain.Model.Dependencies
    open OutboundAdapters
    open CoffeeMachine

    type FakePad(serveDrinkUseCase: (UseCases.DrinkRequest -> unit)) =
            member this.ProcessCustomerOrder order = serveDrinkUseCase order  

    type App() = 
        let drinkMaker = FakeDrinkMaker()
        let drinkRepository = FindDrinkAdapter.InMemoryDrinkRepository()
        let makeDrink: MakeDrink = DrinkMakerWithStringProtocolAdapter.makeDrink drinkMaker
        let displayMessage: DisplayMessage = DrinkMakerWithStringProtocolAdapter.displayMessage drinkMaker
        let displayErrorMessage: DisplayErrorMessage = DomainServices.displayErrorMessage displayMessage 
        let prepareOrder: PrepareOrder = DrinkOrder.prepareWith
        let serveDrinkUseCase = UseCases.serveDrink drinkRepository.FindDrink displayErrorMessage prepareOrder makeDrink     
        member this.Pad = FakePad(serveDrinkUseCase)
        member this.DrinkMaker = drinkMaker
