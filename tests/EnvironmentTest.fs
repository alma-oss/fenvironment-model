module EnvironmentModel.Test.Environment

open Expecto
open Alma.EnvironmentModel

let okOrFail = function
    | Ok value -> value
    | Error error -> failwithf "%A" error

let env tier number space: FullyQualifiedEnvironment =
    {
        Tier = tier
        Number = number
        Space = space
    }

let awsSpace = AWSAccount >> Space.AWSAccount

let provideAWSSpaces =
    [
        "privacycomponents", Ok (Space.AWSAccount (AWSAccount "privacycomponents"))
        "privacy-components", Ok (Space.AWSAccount (AWSAccount "privacy-components"))
        "devX", Error (UnknownSpace "devX")
        "dev", Error (UnknownSpace "dev")
        "dev*", Error (UnknownSpace "dev*")
    ]

let provideEnvironments =
    [
        // Lmc environments
        (env Tier.Prod Numberless Space.Business, Alias "prod")
        (env Tier.Dev (Number 21) Space.Business, Alias "dev21")
        (env Tier.Dev (Number 1) Space.Services, Alias "dev1-services")
        (env Tier.Deploy Numberless Space.Services, Alias "deploy-services")
        (env Tier.Prod Numberless Space.Rad, Alias "prod-rad")
        // AWS environments
        (env Tier.Dev (Number 1) (awsSpace "privacycomponents"), Alias "privacycomponents-dev1")
        (env Tier.Internal Numberless (awsSpace "privacycomponents"), Alias "privacycomponents-int")
        (env Tier.Prod Numberless (awsSpace "privacy-components"), Alias "privacy-components-prod")
    ]

let envPattern tier number space: FullyQualifiedEnvironmentPattern =
    {
        Tier = tier
        Number = number
        Space = space
    }

let provideEnvironmentPatterns =
    [
        // Lmc environments
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
        // AWS environments
        (envPattern (ExactTier Tier.Dev) (ExactNumber (Number 1)) (ExactSpace (awsSpace "privacycomponents")), "privacycomponents-dev1")
        (envPattern (ExactTier Tier.Internal) AnyNumber (ExactSpace (awsSpace "privacycomponents")), "privacycomponents-intX")
        (envPattern (ExactTier Tier.Prod) AnyNumber (ExactSpace (awsSpace "privacy-components")), "privacy-components-prodX")
        (envPattern AnyTier (ExactNumber Numberless) (ExactSpace (awsSpace "privacycomponents")), "privacycomponents-*")
        (envPattern AnyTier AnyNumber (ExactSpace (awsSpace "privacycomponents")), "privacycomponents-*X")
    ]

[<Tests>]
let spacesTests =
    testList "Space" [
        testCase "parse AWS Space" <| fun _ ->
            provideAWSSpaces
            |> List.iter (fun (space, expected) ->
                Expect.equal (Space.parse space) expected space
            )

        testCase "match AWS Space" <| fun _ ->
            provideAWSSpaces
            |> List.iter (fun (spaceString, expected) ->
                let expectedSpace = expected |> Result.toOption

                let space =
                    match spaceString with
                    | Space.IsAWSAccount account -> Some (Space.AWSAccount account)
                    | _ -> None

                Expect.equal space expectedSpace spaceString
            )
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
