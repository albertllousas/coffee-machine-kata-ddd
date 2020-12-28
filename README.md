# Coffee machine kata with DDD

## Introduction

This project implements the [coffee machine kata](https://simcap.github.io/coffeemachine/).

In theory, this kata implementation should be as simple as possible, but in this project besides learning how to do Outside-In TDD I also want to show a way to apply **functional programming, domain-driven design and hexagonal architecture** to solve a "real" problem, hence, the code is **not just a simple coffee machine kata**, architectural patterns and DDD building blocks will be used.

Please, **forgive me in advance**, I am developer mainly focused on JVM languages (kotlin, java, scala) and this is my first contact with dotnet ecosystem, therefore maybe I didn't follow some F# community conventions in terms of files, project, naming and so on ... but I did my best!

## Kata

This [kata](https://simcap.github.io/coffeemachine/) is an iterative kata that consist in 5 steps, the idea is to see if the code that we create is evolvable and able to adapt to changes.

- [First iteration](https://simcap.github.io/coffeemachine/cm-first-iteration.html)
- [Second iteration](https://simcap.github.io/coffeemachine/cm-second-iteration.html)
- [Third iteration](https://simcap.github.io/coffeemachine/cm-third-iteration.html)
- [Fourth iteration](https://simcap.github.io/coffeemachine/cm-fourth-iteration.html)
- [Fifth iteration](https://simcap.github.io/coffeemachine/cm-fifth-iteration.html)

## Important design note!!!

For the sake of simplicity, we have code this kata with the assumption that is not necessary to be reliable in terms of side effects, it means that this **code can not guarantee a safe message display, report updates or a consistent system recovery** if the system goes down or we have a crash in the middle of the flow.

These kinds of problems are typical in distributed systems and are known as [**dual writes**](https://thorben-janssen.com/dual-writes/).

If we wanted to achieve a more reliable and consistent system we would need to introduce more complexity, a solution would be to introduce an async flow, create and publish **domain events**, and solve the dual writes with [**transactional outbox pattern**](https://microservices.io/patterns/data/transactional-outbox.html), for example.

### Idea for a reliable solution:

One async flow with domain events could be to split the process in different asynchronous steps:
 
- Create a drink order:
    - Create `DrinkOrder` applying all the logic, same as it is now
    - Save it 
    - Publish event `DrinkOrderCreated` or `DrinkOrderFailed` ( we could use a simple event bus if we are going to use the same db tx or tx-outbox if want more isolation and scalability)
- Process the drink order, actually, make the drink; 
    - React to `DrinkOrderCreated` event and trigger a use-case (akka command-handler or application-service).
    - Find the `DrinkOrder`
    - Call an **idempotent drink maker** (even though make drink is a side-effect, it is part of the business)
    - Change the state of the drinkOrder aggregate
    - Save the aggregate and publish the events `DrinkOrderServed` or `DrinkOrderFailed`
- To handle non-business side effects with reliable delivery we could subscribe to the events and use transactional outbox for message relay to:
    - Display message 
    - Update report
    - Send email for missing drink

There are a lot of variations, as complex as you want, **but do we really need this right now?** if this was a real production system I would ask the domain experts which kind of reliability we want ... is it important if we miss a report update? Can we deal with eventual consistency? And design the system accordingly ...

## Tests

```shell
dotnet test
```

## Requirements

 Dotnet Version: `5.0.101`
