namespace CoffeeMachine.Infrastructure

[<AbstractClass>]
type DrinkMakerDriver() =
    abstract member ReceiveCommand: string -> unit 

module DrinkMakerWithStringProtocolAdapter =

    open CoffeeMachine.Domain.Model.Types

    open CoffeeMachine.Domain.Model.Dependencies

    let private translateDrink (drink: Drink) = 
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
        | WithStick -> "O" 

    let makeDrink (drinkMaker: DrinkMakerDriver): MakeDrink = fun (drinkOrder: DrinkOrder) -> 
        $"{translateDrink drinkOrder.Drink}:{translateSugar drinkOrder.Sugar}:{translateStick drinkOrder.Stick}"
        |> drinkMaker.ReceiveCommand

    let displayMessage (drinkMaker: DrinkMakerDriver): DisplayMessage = fun (Message message) ->
        drinkMaker.ReceiveCommand $"M:{message}"
