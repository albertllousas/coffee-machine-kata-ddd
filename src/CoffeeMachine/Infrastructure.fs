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
            | OrangeJuice -> "O"
        
        let private translateSugar (sugar: Sugar) = 
            match sugar with 
            | NoSugar -> "" 
            | WithOneSugar -> "1"
            | WithTwoSugars -> "2"

        let private translateStick (stick: Stick) =
            match stick with 
            | NoStick -> ""
            | AddStick -> "O" 
        
        let private translateTemperature (temperature: Temperature) =
            match temperature with 
            | Normal -> ""
            | ExtraHot -> "h" 

        let makeDrink (drinkMaker: DrinkMakerDriver): Dependencies.MakeDrink = fun (drinkOrder: DrinkOrder) -> 
            try
                $"{translateDrink drinkOrder.Drink.DrinkType}{translateTemperature drinkOrder.Temperature}:{translateSugar drinkOrder.Sugar}:{translateStick drinkOrder.Stick}"
                |> drinkMaker.ReceiveCommand
                Ok drinkOrder
            with 
            | ex -> Error(DrinkMakerError(error = ex.Message))

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

    module UpdateStatisticsAdapter =

        type InMemoryStatistics() =
            let mutable drinksSold = Map.empty
            let mutable totalEarned = 0m
            member this.Update: Dependencies.UpdateStatistics = fun drinkOrder ->
                match drinksSold.TryFind drinkOrder.Drink.DrinkType with
                | Some counter -> drinksSold <- drinksSold.Add(drinkOrder.Drink.DrinkType , counter + 1)
                | None -> drinksSold <- drinksSold.Add(drinkOrder.Drink.DrinkType , 1)

                let (Price drinkPrice) = drinkOrder.Drink.Price
                totalEarned <- totalEarned + drinkPrice
            member this.Print (print: string -> unit): Dependencies.PrintStatistics = fun () ->
                print $"total = {totalEarned}" 
                drinksSold |> Map.iter (fun key value -> print $"{key} = {value}")
                 
                
module InboundAdapters =

    open CoffeeMachine

    type FakePad(serveDrinkUseCase: (UseCases.DrinkRequest -> unit)) =
        member this.ProcessCustomerOrder order = serveDrinkUseCase order

module FakeApplication =

    open CoffeeMachine.Domain.Model.Dependencies
    open OutboundAdapters
    open CoffeeMachine

    type FakePad(serveDrinkUseCase: (UseCases.DrinkRequest -> unit), printReportUsecase: (unit -> unit)) =
            member this.ProcessCustomerOrder order = serveDrinkUseCase order
            member this.PrintReport = printReportUsecase   

     type FakePrinter() =
        member val PrintedLines: string list = [] with get, set
        member this.Print line =
            this.PrintedLines <- this.PrintedLines @ [ line ]

    type App() = 
        let printer =  FakePrinter()
        let drinkMaker = FakeDrinkMaker()
        let drinkRepository = FindDrinkAdapter.InMemoryDrinkRepository()
        let makeDrink: MakeDrink = DrinkMakerWithStringProtocolAdapter.makeDrink drinkMaker
        let displayMessage: DisplayMessage = DrinkMakerWithStringProtocolAdapter.displayMessage drinkMaker
        let handleDrinkNotServed: HandleDrinkNotServed = DomainServices.handleDrinkNotServed displayMessage 
        let inMemoryStatistics = UpdateStatisticsAdapter.InMemoryStatistics()
        let updateStatistics: UpdateStatistics = inMemoryStatistics.Update
        let printStatistics: PrintStatistics = inMemoryStatistics.Print printer.Print
        let prepareOrder: PrepareOrder = DrinkOrder.prepareWith
        let serveDrinkUseCase = UseCases.serveDrink drinkRepository.FindDrink handleDrinkNotServed prepareOrder makeDrink updateStatistics   
        let printReportUsecase = UseCases.printReport printStatistics   
        member this.Pad = FakePad(serveDrinkUseCase, printReportUsecase)
        member this.DrinkMaker = drinkMaker
        member this.Printer = printer
