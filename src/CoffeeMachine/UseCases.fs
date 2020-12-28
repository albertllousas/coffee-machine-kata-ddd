namespace CoffeeMachine

open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.Domain.Model.Dependencies

module UseCases =

    type DrinkRequest = { DrinkId: string; Sugar: int; Money: decimal; ExtraHot: bool }

    let serveDrink (findDrink: FindDrink)
                   (handleDrinkNotServed: HandleDrinkNotServed)
                   (prepareOrder: PrepareOrder)
                   (makeDrink: MakeDrink)
                   (updateStatistics: UpdateStatistics)
                   (request: DrinkRequest)
                   =
        findDrink (DrinkId request.DrinkId)
        |> Result.bind (prepareOrder request.Sugar request.Money request.ExtraHot)
        |> Result.bind makeDrink
        |> Result.map updateStatistics
        |> Result.mapError handleDrinkNotServed
        |> ignore


    let printReport (printStatistics: PrintStatistics) = printStatistics
