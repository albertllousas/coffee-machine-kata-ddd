namespace CoffeeMachine

open CoffeeMachine.Domain.Model.Types

open CoffeeMachine.Domain.Model.Dependencies

module UseCases =

    type DrinkRequest = {Drink: Drink; Sugar: Sugar}

    let serveDrink (prepareOrder: PrepareOrder) (makeDrink: MakeDrink) (request: DrinkRequest) =
        prepareOrder request.Drink request.Sugar 
        |> makeDrink

    let forwardMessageToCustomer (displayMessage: DisplayMessage) message = displayMessage message

