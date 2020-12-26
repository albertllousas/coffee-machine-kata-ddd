namespace CoffeeMachine

open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.Domain.Model.Dependencies

module UseCases =

    type DrinkRequest = { DrinkId: string; Sugar: int; Money: decimal; ExtraHot: bool }

    let serveDrink (findDrink: FindDrink)
                   (displayErrorMessage: DisplayErrorMessage)
                   (prepareOrder: PrepareOrder)
                   (makeDrink: MakeDrink)
                   (request: DrinkRequest)
                   =
        findDrink (DrinkId request.DrinkId)
        |> Result.bind (prepareOrder request.Sugar request.Money request.ExtraHot)
        |> Result.map makeDrink
        |> Result.mapError displayErrorMessage
        |> ignore
