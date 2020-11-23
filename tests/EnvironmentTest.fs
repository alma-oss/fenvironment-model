module EnvironmentModel.Test.Environment

open Expecto
open Lmc.EnvironmentModel

let env tier number space: FullyQualifiedEnvironment =
    {
        Tier = tier
        Number = number
        Space = space
    }

let provideEnvironments =
    [
        (env Tier.Prod Numberless Space.Business, Alias "prod")
        (env Tier.Dev (Number 21) Space.Business, Alias "dev21")
        (env Tier.Dev (Number 1) Space.Services, Alias "dev1-services")
        (env Tier.Deploy Numberless Space.Services, Alias "deploy-services")
        (env Tier.Prod Numberless Space.Rad, Alias "prod-rad")
    ]

let envPattern tier number space: FullyQualifiedEnvironmentPattern =
    {
        Tier = tier
        Number = number
        Space = space
    }

let provideEnvironmentPatterns =
    [
        (envPattern (ExactTier Tier.Dev) (ExactNumber (Number 1)) (ExactSpace Space.Business), "dev1")
        (envPattern (ExactTier Tier.Dev) (ExactNumber (Number 1)) (ExactSpace Space.Services), "dev1-services")
        (envPattern (ExactTier Tier.Prod) (ExactNumber Numberless) (ExactSpace Space.Business), "prod")
        (envPattern (ExactTier Tier.Prod) (ExactNumber Numberless) (ExactSpace Space.Services), "prod-services")
        (envPattern (ExactTier Tier.Dev) AnyNumber (ExactSpace Space.Business), "devX")
        (envPattern (ExactTier Tier.Devel) AnyNumber (ExactSpace Space.Rad), "develX-rad")
        (envPattern AnyTier AnyNumber AnySpace, "*")
        (envPattern (ExactTier Tier.Dev) AnyNumber AnySpace, "devX-*")
        (envPattern (ExactTier Tier.Dev) (ExactNumber Numberless) AnySpace, "dev-*")
        (envPattern AnyTier AnyNumber (ExactSpace Space.Business), "*X")
        (envPattern AnyTier (ExactNumber Numberless) AnySpace, "*-*")
        (envPattern AnyTier (ExactNumber (Number 42)) AnySpace, "*42-*")
        (envPattern AnyTier AnyNumber (ExactSpace Space.Business), "*X")
    ]

[<Tests>]
let environmentTests =
    testList "Environment" [
        testCase "transform env to alias" <| fun _ ->
            provideEnvironments
            |> List.iter (fun (env, envAlias) ->
                Expect.equal (Environment.FullyQualified.toAlias env) envAlias (envAlias |> Alias.value)
            )

        testCase "transform alias to env" <| fun _ ->
            provideEnvironments
            |> List.iter (fun (env, envAlias) ->
                Expect.equal (Environment.FullyQualified.parse (envAlias |> Alias.value)) (Ok env) (envAlias |> Alias.value)
            )

        testCase "alias equals the fully qualified" <| fun _ ->
            provideEnvironments
            |> List.iter (fun (env, envAlias) ->
                let environmentByFQ = env |> Environment.FullyQualified
                let environmentByAlias = envAlias |> Environment.Alias

                Expect.isTrue (environmentByFQ = environmentByAlias) (envAlias |> Alias.value)
                Expect.isTrue (environmentByAlias = environmentByFQ) (envAlias |> Alias.value)
            )
    ]

[<Tests>]
let environmentPatternTests =
    testList "Environment pattern" [
        testCase "transform env to alias" <| fun _ ->
            provideEnvironmentPatterns
            |> List.iter (fun (env, envAlias) ->
                Expect.equal (EnvironmentPattern.FullyQualified.value env) envAlias envAlias
            )

        testCase "transform alias to env" <| fun _ ->
            provideEnvironmentPatterns
            |> List.iter (fun (env, envAlias) ->
                Expect.equal (EnvironmentPattern.FullyQualified.parse envAlias) (Ok env) envAlias
            )

        testCase "alias equals the fully qualified" <| fun _ ->
            provideEnvironmentPatterns
            |> List.iter (fun (env, envAlias) ->
                let environmentByFQ = env |> EnvironmentPattern.FullyQualified
                let environmentByAlias = envAlias |> Alias |> Environment |> EnvironmentPattern.Alias

                Expect.isTrue (environmentByFQ = environmentByAlias) envAlias
                Expect.isTrue (environmentByAlias = environmentByFQ) envAlias
            )
    ]
