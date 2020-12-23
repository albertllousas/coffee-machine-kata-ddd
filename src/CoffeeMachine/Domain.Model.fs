namespace CoffeeMachine.Domain.Model

module Types =

    type Sugar =
        | NoSugar
        | WithOneSugar
        | WithTwoSugars

    type Stick =
        | NoStick
        | WithStick

    type Drink =
        | Tea
        | Coffee
        | Chocolate

    type Message = Message of string

    type DrinkOrder = private { drink: Drink; sugar: Sugar; stick: Stick } with
        member this.Drink = this.drink
        member this.Sugar = this.sugar
        member this.Stick = this.stick

    module DrinkOrder =

        // smart constructor, ddd factory
        let prepareWith drink sugar =
            match sugar with
            | NoSugar -> { drink = drink; sugar = sugar; stick = NoStick }
            | _ -> { drink = drink; sugar = sugar; stick = WithStick }

        let reconstitute drink sugar stick =  { drink = drink; sugar = sugar; stick = stick }
        
        // active pattern    
        let (|DrinkOrder|) {drink = drink; sugar = sugar; stick = stick} =  (drink, sugar, stick)
    

    // type DrinkOrdered = {a: string}

    // type DomainEvent =
    //     DrinkOrdered of DrinkOrdered 
     
    // let d = DrinkOrdered {a = ""}

    // type DomainEvent =
        // DrinkOrdered of string 

    // let createEvents 
    // publishEvents then check there 


module Dependencies =

    open Types

    type PrepareOrder = Drink -> Sugar -> DrinkOrder

    type MakeDrink = DrinkOrder -> unit

    type DisplayMessage = Message -> unit

    // type PublishEvents = DomainEvent list -> unit
    
