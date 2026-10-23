namespace Alma.EnvironmentModel

open Alma.ServiceIdentification
open Feather.ErrorHandling

[<RequireQualifiedAccess>]
type SoftwareComponentError =
    | InvalidFormat of given: string
    | InstanceError of given: string * InstanceError
    | EnvironmentError of EnvironmentError

[<RequireQualifiedAccess>]
module SoftwareComponentError =
    let format = function
        | SoftwareComponentError.InvalidFormat given ->
            sprintf "Software component %A could not be parsed. Make sure it has a format of \"instance@environment\"." given
        | SoftwareComponentError.InstanceError (given, _) ->
            sprintf "Instance %A could not be parsed. Make sure it has a format of \"domain-context-purpose-version\"." given
        | SoftwareComponentError.EnvironmentError error -> EnvironmentError.format error

/// A deployed instance in an environment (`instance@environment`), optionally typed by its concept.
type SoftwareComponent =
    | ApplicationComponent of Instance * Environment
    | SystemSoftware of Instance * Environment
    | Storage of Instance * Environment
    | Untyped of Instance * Environment

[<RequireQualifiedAccess>]
module SoftwareComponent =
    let instance = function
        | ApplicationComponent (instance, _)
        | SystemSoftware (instance, _)
        | Storage (instance, _)
        | Untyped (instance, _) -> instance

    let environment = function
        | ApplicationComponent (_, environment)
        | SystemSoftware (_, environment)
        | Storage (_, environment)
        | Untyped (_, environment) -> environment

    let value softwareComponent =
        sprintf "%s@%s"
            (softwareComponent |> instance |> Instance.concat "-")
            (softwareComponent |> environment |> Environment.value)

    let format formatInstance formatEnvironment softwareComponent =
        sprintf "%s@%s"
            (softwareComponent |> instance |> formatInstance)
            (softwareComponent |> environment |> formatEnvironment)

    let private typePrefix = function
        | ApplicationComponent _ -> "AC:"
        | SystemSoftware _ -> "SS:"
        | Storage _ -> "ST:"
        | Untyped _ -> ""

    /// Renders the component with its type prefix (`AC:`, `SS:`, `ST:`, none for untyped), the inverse of `parse`.
    let serialize serviceSeparator softwareComponent =
        sprintf "%s%s@%s"
            (softwareComponent |> typePrefix)
            (softwareComponent |> instance |> Instance.concat serviceSeparator)
            (softwareComponent |> environment |> Environment.value)

    let parseWithType serviceSeparator (softwareComponent: string, softwareComponentType: string): Result<SoftwareComponent, SoftwareComponentError> =
        match softwareComponent.Split('@') with
        | [| instance; environment |] ->
            result {
                let! instance =
                    instance
                    |> Instance.parseStrict serviceSeparator
                    |> Result.mapError (fun error -> SoftwareComponentError.InstanceError (instance, error))

                let! environment =
                    environment
                    |> Environment.parse
                    |> Result.mapError SoftwareComponentError.EnvironmentError

                return
                    match softwareComponentType with
                    | "AC" | "ApplicationComponent" -> ApplicationComponent (instance, environment)
                    | "SS" | "SystemSoftware" -> SystemSoftware (instance, environment)
                    | "ST" | "Storage" -> Storage (instance, environment)
                    | _ -> Untyped (instance, environment)
            }
        | _ -> Error (SoftwareComponentError.InvalidFormat softwareComponent)

    /// Parses `[type:]instance@environment`; without a type prefix the component is `Untyped`.
    let parse serviceSeparator (softwareComponent: string) =
        match softwareComponent.Split(':') with
        | [| softwareComponentType; untypedComponent |] -> parseWithType serviceSeparator (untypedComponent, softwareComponentType)
        | [| untypedComponent |] -> parseWithType serviceSeparator (untypedComponent, "")
        | _ -> Error (SoftwareComponentError.InvalidFormat softwareComponent)
