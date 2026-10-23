module EnvironmentModel.Test.SoftwareComponent

open Expecto
open Alma.ServiceIdentification
open Alma.EnvironmentModel

let private okOrFail = function
    | Ok value -> value
    | Error error -> failwithf "%A" error

let private instance = Instance.parseStrict "-" "alma-userManager-account-v1" |> okOrFail
let private environment = Environment.parse "dev1-services" |> okOrFail

let private provideSerialized =
    [
        ApplicationComponent (instance, environment), "AC:alma-userManager-account-v1@dev1-services"
        SystemSoftware (instance, environment), "SS:alma-userManager-account-v1@dev1-services"
        Storage (instance, environment), "ST:alma-userManager-account-v1@dev1-services"
        Untyped (instance, environment), "alma-userManager-account-v1@dev1-services"
    ]

[<Tests>]
let softwareComponentTests =
    testList "SoftwareComponent" [
        testCase "should render value as instance@environment for every type" <| fun _ ->
            for softwareComponent, _ in provideSerialized do
                Expect.equal (softwareComponent |> SoftwareComponent.value) "alma-userManager-account-v1@dev1-services" (sprintf "%A" softwareComponent)

        testCase "should expose its instance and environment" <| fun _ ->
            Expect.equal (Untyped (instance, environment) |> SoftwareComponent.instance) instance "instance"
            Expect.equal (Untyped (instance, environment) |> SoftwareComponent.environment) environment "environment"

        testCase "should serialize with the type prefix" <| fun _ ->
            for softwareComponent, expected in provideSerialized do
                Expect.equal (softwareComponent |> SoftwareComponent.serialize "-") expected expected

        testCase "should parse what it serialized" <| fun _ ->
            for softwareComponent, serialized in provideSerialized do
                Expect.equal (serialized |> SoftwareComponent.parse "-") (Ok softwareComponent) serialized

        testCase "should parse the long type names" <| fun _ ->
            Expect.equal
                ("ApplicationComponent:alma-userManager-account-v1@dev1-services" |> SoftwareComponent.parse "-")
                (Ok (ApplicationComponent (instance, environment)))
                "ApplicationComponent"

            Expect.equal
                ("Storage:alma-userManager-account-v1@dev1-services" |> SoftwareComponent.parse "-")
                (Ok (Storage (instance, environment)))
                "Storage"

        testCase "should parse with a custom service separator" <| fun _ ->
            Expect.equal
                ("alma.userManager.account.v1@dev1-services" |> SoftwareComponent.parse ".")
                (Ok (Untyped (instance, environment)))
                "dot separator"

        testCase "should format with custom formatters" <| fun _ ->
            Expect.equal
                (Untyped (instance, environment) |> SoftwareComponent.format (Instance.concat ".") Environment.format)
                "alma.userManager.account.v1@dev1-services (FullyQualified)"
                "custom format"

        testCase "should fail without the @ separator" <| fun _ ->
            Expect.equal
                ("alma-userManager-account-v1" |> SoftwareComponent.parse "-")
                (Error (SoftwareComponentError.InvalidFormat "alma-userManager-account-v1"))
                "no environment"

        testCase "should fail with more than one @" <| fun _ ->
            Expect.equal
                ("alma-userManager-account-v1@dev1-services@x" |> SoftwareComponent.parse "-")
                (Error (SoftwareComponentError.InvalidFormat "alma-userManager-account-v1@dev1-services@x"))
                "two separators"

        testCase "should fail on an invalid instance with a typed error" <| fun _ ->
            match "alma-userManager@dev1-services" |> SoftwareComponent.parse "-" with
            | Error (SoftwareComponentError.InstanceError ("alma-userManager", InstanceError.InvalidFormat _)) -> ()
            | other -> failtestf "expected InstanceError, got %A" other

        testCase "should fail on an invalid environment with a typed error" <| fun _ ->
            match "alma-userManager-account-v1@nowhere-x-y" |> SoftwareComponent.parse "-" with
            | Error (SoftwareComponentError.EnvironmentError _) -> ()
            | other -> failtestf "expected EnvironmentError, got %A" other

        testCase "should format every error" <| fun _ ->
            for error in [
                SoftwareComponentError.InvalidFormat "x"
                SoftwareComponentError.InstanceError ("x", InstanceError.InvalidFormat "x")
                SoftwareComponentError.EnvironmentError EnvironmentError.InvalidFormat
            ] do
                Expect.isNotEmpty (SoftwareComponentError.format error) (sprintf "%A" error)
    ]
