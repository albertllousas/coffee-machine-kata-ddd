namespace CoffeeMachine.Domain.Model

open FSharpx.Result

module Types =

    type Error = 
        | NonExistentProduct of productCode: string
        | InvalidQuantityOfSugar
        | NotEnoughMoney of moneyMissing: decimal
        | DrinkMakerError of error: string
        | Shortage of productCode: string

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

    type DrinkId = DrinkId of string

    type Price = Price of decimal

    type DrinkType =
        | Tea
        | Coffee
        | Chocolate
        | OrangeJuice
    
    type Temperature = 
        | Normal
        | ExtraHot
    
    type Drink = { DrinkId : DrinkId; DrinkType: DrinkType; Price: Price}

    type TotalOfMoney = TotalOfMoney of decimal

    type DrinkOrder = private { drink: Drink; sugar: Sugar; stick: Stick; temperature: Temperature; totalOfMoney: TotalOfMoney } with
        member this.Drink = this.drink
        member this.Sugar = this.sugar
        member this.Stick = this.stick
        member this.TotalOfMoney = this.totalOfMoney
        member this.Temperature = this.temperature

    module DrinkOrder =

        let checkEnoughMoney (Price minimumExpected) moneyGiven =  
            if(moneyGiven >= minimumExpected) then Ok moneyGiven else Error (NotEnoughMoney (minimumExpected - moneyGiven))

        let calculateTemperature extraHot drinkType = 
            match drinkType with 
            | OrangeJuice -> Normal
            | _ -> if extraHot then ExtraHot else Normal

        let prepareWith quantityOfSugar moneyGiven extraHot drink = 
            result {
                  let! sugar = Sugar.from quantityOfSugar
                  let! money = checkEnoughMoney drink.Price moneyGiven
                  let stick = Stick.from sugar 
                  let temperature = calculateTemperature extraHot drink.DrinkType
                  return { drink = drink; sugar = sugar; stick = stick; temperature = temperature; totalOfMoney = TotalOfMoney money }
            }
   
        let reconstitute drink sugar stick temperature totalOfMoney =  
            { drink = drink; sugar = sugar; stick = stick; temperature = temperature; totalOfMoney = totalOfMoney }

module Dependencies = 

    module Aliases =
        type Money = decimal
        type QuantityOfSugar = int
        type ExtraHot = bool
    
    open Types
    open Aliases

    type PrepareOrder = QuantityOfSugar -> Money -> ExtraHot -> Drink -> Result<DrinkOrder, Error>

    type FindDrink = DrinkId -> Result<Drink, Error>

    type MakeDrink = DrinkOrder -> Result<DrinkOrder, Error> // catch error

    type UpdateStatistics = DrinkOrder -> unit

    type DisplayMessage = Message -> unit

    type HandleDrinkNotServed = Error -> unit

    type PrintStatistics = unit -> unit

    type NotifyMissingDrink = string -> unit

module DomainServices = 

    open Types

    open Dependencies

    let handleDrinkNotServed (displayMessage:DisplayMessage) (notifyMissingDrink: NotifyMissingDrink): HandleDrinkNotServed = fun (error: Error) -> 
        match error with 
        | NotEnoughMoney missingMoney -> displayMessage (Message $"Not enough money, please add {missingMoney} more")
        | InvalidQuantityOfSugar -> displayMessage (Message "Invalid quantity of sugar")
        | NonExistentProduct code -> displayMessage (Message $"Non existent product with code '{code}'")
        | DrinkMakerError _ -> displayMessage (Message $"Sorry, there was an error making a drink'")
        | Shortage code -> 
            notifyMissingDrink code
            displayMessage (Message $"Sorry, there is no more {code}, a notification to refill it has been sent")
