# Examples

All example code for this skill lives here. Examples are ordered from basic to complete.

## Basic Parsing

Parse an alias string into a structured environment and read its parts.

```fsharp
open Alma.EnvironmentModel

// Named-space environment
match Environment.parse "dev1-services" with
| Ok env ->
    // env is Environment.FullyQualified { Tier = Tier.Dev; Number = Number 1; Space = Space.Space "services" }
    printfn "alias = %s" (env |> Environment.value)   // "dev1-services"
| Error e ->
    eprintfn "%s" (EnvironmentError.format e)

// Business/default space (empty space) and a numberless tier
let prod = Environment.parse "prod"                   // Ok (FullyQualified { Tier = Prod; Number = Numberless; Space = Space.Space "" })

// AWS-account space alias
let aws = Environment.parse "exampleaccount-dev1"     // Ok (... Space = Space.AWSAccount (AWSAccount "exampleaccount"))
```

## Constructing and Formatting

Build a fully qualified environment directly and render it.

```fsharp
open Alma.EnvironmentModel

let env: FullyQualifiedEnvironment =
    { Tier = Tier.Deploy; Number = Numberless; Space = Space.Space "services" }

let alias = env |> Environment.FullyQualified.toAlias   // Alias "deploy-services"
let text  = alias |> Alias.value                        // "deploy-services"

// Decompose to plain strings
let tier, number, space = env |> Environment.FullyQualified.toScalar
// ("deploy", "", "services")

// Space rendering differs for the business space:
Space.value  (Space.Space "")   // ""
Space.format (Space.Space "")   // "business"
```

## Pattern Parsing

Read environment selectors with wildcards (`*` = any tier/space, `X` = any number).

```fsharp
open Alma.EnvironmentModel

EnvironmentPattern.parse "*"        // Ok (... AnyTier, AnyNumber, AnySpace)
EnvironmentPattern.parse "devX"     // Ok (ExactTier Dev, AnyNumber, ExactSpace (Space.Space ""))
EnvironmentPattern.parse "devX-*"   // Ok (ExactTier Dev, AnyNumber, AnySpace)
EnvironmentPattern.parse "exampleaccount-intX"  // Ok (ExactTier Integration, AnyNumber, ExactSpace (AWSAccount ...))

// Render a pattern back to its alias form
let pattern: FullyQualifiedEnvironmentPattern =
    { Tier = ExactTier Tier.Dev; Number = AnyNumber; Space = AnySpace }

let selector = EnvironmentPattern.FullyQualified.value pattern   // "devX-*"
```

## Working with Proxies

Create a proxy reference and resolve an `EnvironmentDefinition`.

```fsharp
open Alma.EnvironmentModel

// Build a proxy (constructor is private — must go through create)
let current = EnvironmentDefinition.Current            // the predefined &current proxy

// A definition may be a concrete environment or a proxy
let definition = ByProxy current

// Resolve the proxy to a concrete environment by providing its matching key + target
let target = Environment.parse "dev1-services" |> Result.toOption |> Option.get

match definition with
| ByEnvironment env -> Ok env
| ByProxy _         -> EnvironmentDefinition.resolve (current, target) definition
```

## Round-trip Test

Table-driven test asserting alias ↔ structured value in both directions (Expecto).

```fsharp
open Expecto
open Alma.EnvironmentModel

let cases =
    [
        ({ Tier = Tier.Prod;  Number = Numberless; Space = Space.Space "" },        Alias "prod")
        ({ Tier = Tier.Dev;   Number = Number 1;   Space = Space.Space "services" }, Alias "dev1-services")
        ({ Tier = Tier.Dev;   Number = Number 1;   Space = Space.AWSAccount (AWSAccount "exampleaccount") }, Alias "exampleaccount-dev1")
    ]

[<Tests>]
let roundTrip =
    testList "environment round-trip" [
        testCase "structured -> alias" <| fun _ ->
            for env, expected in cases do
                Expect.equal (Environment.FullyQualified.toAlias env) expected (expected |> Alias.value)

        testCase "alias -> structured" <| fun _ ->
            for env, alias in cases do
                Expect.equal (Environment.FullyQualified.parse (alias |> Alias.value)) (Ok env) (alias |> Alias.value)

        testCase "alias form equals fully qualified form" <| fun _ ->
            for env, alias in cases do
                Expect.isTrue (Environment.FullyQualified env = Environment.Alias alias) (alias |> Alias.value)
    ]
```

## Full Workflow

Parse input, branch on the space kind, and surface a formatted error on failure.

```fsharp
open Alma.EnvironmentModel
open Feather.ErrorHandling.Result.Operators

let describe (input: string) : Result<string, string> =
    input
    |> Environment.parse
    |> Result.map (fun env ->
        match env |> Environment.toTuple with
        | Ok (tier, number, Space.AWSAccount account) ->
            sprintf "AWS env: account=%s tier=%s number=%s"
                (account |> AWSAccount.value) (tier |> Tier.value) (number |> EnvironmentNumber.value)
        | Ok (tier, _, Space.Space "") ->
            sprintf "business env: tier=%s" (tier |> Tier.value)
        | Ok (tier, _, space) ->
            sprintf "named env: tier=%s space=%s" (tier |> Tier.value) (space |> Space.value)
        | Error _ ->
            "unresolved alias")
    |> Result.mapError EnvironmentError.format
```
