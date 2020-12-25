namespace CoffeeMachine.Domain.Model

open FSharpx.Result

module Types =

    type Error = 
            | NonExistentProduct of productCode: string
            | InvalidQuantityOfSugar
            | NotEnoughMoney of moneyMissing: decimal

    type Sugar =
        | NoSugar
        | WithOneSugar
        | WithTwoSugars

    module Sugar =
        let from quantity =
            match quantity with
            | 0 -> Ok NoSugar
            | 1 -> Ok WithOneSugar
            | 2 -> Ok WithTwoSugars
            | _ -> Error InvalidQuantityOfSugar

    type Stick =
        | NoStick
        | AddStick

    module Stick =
        let from sugar =
            match sugar with
            | NoSugar -> NoStick
            | _ -> AddStick
    
    type Message = Message of string

    type Money = decimal

    type QuantityOfSugar = int

    type DrinkId = DrinkId of string

    type Price = Price of decimal

    type DrinkType =
        | Tea
        | Coffee
        | Chocolate
    
    type Drink = { DrinkId : DrinkId; DrinkType: DrinkType; Price: Price}

    type TotalOfMoney = TotalOfMoney of decimal

    type DrinkOrder = private { drinkType: DrinkType; sugar: Sugar; stick: Stick; totalOfMoney: TotalOfMoney } with
        member this.DrinkType = this.drinkType
        member this.Sugar = this.sugar
        member this.Stick = this.stick
        member this.TotalOfMoney = this.totalOfMoney

    module DrinkOrder =

        let checkEnoughMoney (Price minimumExpected) moneyGiven =  
            if(moneyGiven >= minimumExpected) then Ok moneyGiven else Error (NotEnoughMoney (minimumExpected - moneyGiven))

        let prepareWith (quantityOfSugar:QuantityOfSugar) (moneyGiven: Money) drink = 
            result {
                  let! sugar = Sugar.from quantityOfSugar
                  let! money = checkEnoughMoney drink.Price moneyGiven
                  let stick = Stick.from sugar 
                  return { drinkType = drink.DrinkType; sugar = sugar; stick = stick; totalOfMoney = TotalOfMoney money }
            }
   
        let reconstitute drinkType sugar stick totalOfMoney =  { drinkType = drinkType; sugar = sugar; stick = stick; totalOfMoney = totalOfMoney }
        
        // active pattern    
        let (|DrinkOrder|) {drinkType = drinkType; sugar = sugar; stick = stick} =  (drinkType, sugar, stick)

module Dependencies =

    open Types

    type FindDrink = DrinkId -> Result<Drink, Error>

    type PrepareOrder = QuantityOfSugar -> Money -> Drink -> Result<DrinkOrder, Error>

    type MakeDrink = DrinkOrder -> unit

    type DisplayMessage = Message -> unit

    type DisplayErrorMessage = Error -> unit

    // type PublishEvents = DomainEvent list -> unit
    
module DomainServices = 

    open Types

    open Dependencies

    let displayErrorMessage (displayMessage:DisplayMessage) : DisplayErrorMessage = fun (error: Error) -> 
        match error with 
        | NotEnoughMoney missingMoney -> displayMessage (Message $"Not enough money, please add {missingMoney} more")
        | InvalidQuantityOfSugar -> displayMessage (Message "Invalid quantity of sugar")
        | NonExistentProduct code -> displayMessage (Message $"Non existent product with code '{code}'")
       