namespace CoffeeMachine

open CoffeeMachine.Domain.Model.Types
open CoffeeMachine.Domain.Model.Dependencies

module UseCases =

    type DrinkRequest = { DrinkId: string; Sugar: int; Money: decimal }

    let serveDrink (findDrink: FindDrink)
                   (displayErrorMessage: DisplayErrorMessage)
                   (prepareOrder: PrepareOrder)
                   (makeDrink: MakeDrink)
                   (request: DrinkRequest)
                   =
        findDrink (DrinkId request.DrinkId)
        |> Result.bind (prepareOrder request.Sugar request.Money)
        |> Result.map makeDrink
        |> Result.mapError displayErrorMessage
        |> ignore
