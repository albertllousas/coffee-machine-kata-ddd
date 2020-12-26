# Coffee machine kata with DDD

## Introduction

This project implements the [coffee machine kata](https://simcap.github.io/coffeemachine/).

In theory, this kata implementation should be as simple as possible, but in this project besides learning how to do Outside-In TDD I also want to show a way to apply **functional programming, domain-driven design and hexagonal architecture** to solve a "real" problem, hence, the code is **not just a simple coffee machine kata**, architectural patterns and DDD building blocks will be used.

Please, **forgive me in advance**, I am developer mainly focused on JVM languages (kotlin, java, scala) and this is my first contact with dotnet ecosystem, therefore maybe I didn't follow some F# community conventions in terms of files, project, naming and so on ... but I did my best!

## Kata

This [kata](https://simcap.github.io/coffeemachine/) is an iterative kata that consist in 5 steps, the idea is to see is yout code is evolvable and able to adapt to changes.

- First iteration
- Second iteration
- Third iteration
- Fourth iteration
- Fifth iteration

## Important design note

For the sake of simplicity, we have code this kata with the assumption that is not necessary to be reliable in terms of side effects, it means that this code can not guarantee a safe message display, reporting updates, message displaying or system recovery if the system goes down or we have a failure in the middle of the flow.

These kind of problems are typical in a distributed systems and are know as [**dual writes**](https://thorben-janssen.com/dual-writes/).

If we would want to achive a more reliable and consistent system we would need to introduce more complexity, a solution would be to introduce an async flow, **domain events** and solve the dual writes with [**transactional outbox pattern**](https://microservices.io/patterns/data/transactional-outbox.html), for example.


## Tests

```shell
dotnet test
```

## Requirements

 Dotnet Version: `5.0.101`

## Fake App