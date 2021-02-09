namespace Lmc.EnvironmentModel

open Lmc.ErrorHandling

//
// Parse Errors
//

type SpaceError =
    | UnknownSpace of string

type TierError =
    | UnknownTier of string

type AliasError =
    | InvalidAlias of string

[<RequireQualifiedAccess>]
module SpaceError =
    let format = function
        | UnknownSpace space -> sprintf "Unknown space %A." space

[<RequireQualifiedAccess>]
module TierError =
    let format = function
        | UnknownTier tier -> sprintf "Unknown tier %A." tier

[<RequireQualifiedAccess>]
module AliasError =
    let format = function
        | InvalidAlias alias -> sprintf "Alias %A is not valid." alias

type EnvironmentError =
    | SpaceError of SpaceError
    | TierError of TierError
    | AliasError of AliasError
    | ProxyError of ProxyError
    | InvalidFormat
    | UnknownProxy of uknown: Proxy * available: string list

[<RequireQualifiedAccess>]
module EnvironmentError =
    let format = function
        | SpaceError error -> SpaceError.format error
        | TierError error -> TierError.format error
        | AliasError error -> AliasError.format error
        | ProxyError error -> ProxyError.format error
        | InvalidFormat -> "Environment format is invalid (you have to define either `alias`, `tier*number*space`, or `&proxy`)"
        | UnknownProxy (unknown, available) ->
            available
            |> List.map (sprintf "  - %s")
            |> String.concat "\n"
            |> sprintf "Given proxy %A is not defined.\n\nUse one of following:\n%s" (unknown |> Proxy.format)

type AliasPatternError =
    | InvalidPattern of string
    | UnnecessarilyComplex of alias: string * simplier: string

[<RequireQualifiedAccess>]
module AliasPatternError =
    let format = function
        | InvalidPattern pattern -> sprintf "Alias pattern %A is not valid." pattern
        | UnnecessarilyComplex (alias, simplier) -> sprintf "Alias pattern %A is unnecessarily complex, simply use %A instead." alias simplier

type EnvironmentPatternError =
    | SpaceError of SpaceError
    | TierError of TierError
    | AliasPatternError of AliasPatternError

[<RequireQualifiedAccess>]
module EnvironmentPatternError =
    let format = function
        | SpaceError error -> SpaceError.format error
        | TierError error -> TierError.format error
        | AliasPatternError error -> AliasPatternError.format error

//
// Types
//

// Exact

[<RequireQualifiedAccess>]
type Space =
    | Business
    | Services
    | Rad
    | Bi
    | Ict
    | Internal

[<RequireQualifiedAccess>]
module Space =
    let value = function
        | Space.Business -> ""
        | Space.Services -> "services"
        | Space.Rad -> "rad"
        | Space.Bi -> "bi"
        | Space.Ict -> "ict"
        | Space.Internal -> "internal"

    let format = function
        | Space.Business -> "business"
        | Space.Services -> "services"
        | Space.Rad -> "rad"
        | Space.Bi -> "bi"
        | Space.Ict -> "ict"
        | Space.Internal -> "internal"

    let parse = function
        | "" | "business" | "-business" -> Ok Space.Business
        | "services" | "-services" -> Ok Space.Services
        | "rad" | "-rad" -> Ok Space.Rad
        | "bi" | "-bi" -> Ok Space.Bi
        | "ict" | "-ict" -> Ok Space.Ict
        | "internal" | "-internal" -> Ok Space.Internal
        | space -> Error (UnknownSpace space)

[<RequireQualifiedAccess>]
type Tier =
    | Dev
    | Devel
    | Deploy
    | Prod

[<RequireQualifiedAccess>]
module Tier =
    let value = function
        | Tier.Dev -> "dev"
        | Tier.Devel -> "devel"
        | Tier.Deploy -> "deploy"
        | Tier.Prod -> "prod"

    let parse = function
        | "dev" -> Ok Tier.Dev
        | "devel" -> Ok Tier.Devel
        | "deploy" -> Ok Tier.Deploy
        | "prod" -> Ok Tier.Prod
        | tier -> Error (UnknownTier tier)

type EnvironmentNumber =
    | Number of int
    | Numberless

[<RequireQualifiedAccess>]
module EnvironmentNumber =
    let value = function
        | Number number -> string number
        | Numberless -> ""

// Pattern

type EnvironmentPatternNumber =
    | ExactNumber of EnvironmentNumber
    | AnyNumber

type EnvironmentPatternSpace =
    | ExactSpace of Space
    | AnySpace

type EnvironmentPatternTier =
    | ExactTier of Tier
    | AnyTier

[<RequireQualifiedAccess>]
module EnvironmentPatternNumber =
    let value = function
        | ExactNumber n -> n |> EnvironmentNumber.value
        | AnyNumber -> "X"

[<RequireQualifiedAccess>]
module EnvironmentPatternTier =
    let value = function
        | ExactTier t -> t |> Tier.value
        | AnyTier -> "*"

[<RequireQualifiedAccess>]
module EnvironmentPatternSpace =
    let value = function
        | ExactSpace s -> s |> Space.value
        | AnySpace -> "*"

// ==== Env ====

type Alias = Alias of string

[<RequireQualifiedAccess>]
module Alias =
    let private aliasForBusinessEnvironment tier number =
        sprintf "%s%s" (tier |> Tier.value) (number |> EnvironmentNumber.value)

    let private aliasForEnvironment tier number space =
        sprintf "%s%s-%s" (tier |> Tier.value) (number |> EnvironmentNumber.value) (space |> Space.value)

    let format = function
        | tier, number, Space.Business -> aliasForBusinessEnvironment tier number
        | tier, number, space -> aliasForEnvironment tier number space

    let value (Alias alias) = alias

    [<RequireQualifiedAccess>]
    module Pattern =
        let private aliasForBusinessEnvironmentPattern tier number =
            sprintf "%s%s" (tier |> EnvironmentPatternTier.value) (number |> EnvironmentPatternNumber.value)

        let private aliasForEnvironmentPattern tier number space =
            sprintf "%s%s-%s" (tier |> EnvironmentPatternTier.value) (number |> EnvironmentPatternNumber.value) (space |> EnvironmentPatternSpace.value)

        let create = function
            | AnyTier, AnyNumber, AnySpace -> Alias "*"
            | ExactTier tier, ExactNumber number, ExactSpace space -> format (tier, number, space) |> Alias
            | tier, number, ExactSpace Space.Business -> aliasForBusinessEnvironmentPattern tier number |> Alias
            | tier, number, space -> aliasForEnvironmentPattern tier number space |> Alias

        let format = function
            | AnyTier, AnyNumber, AnySpace -> "*"
            | ExactTier tier, ExactNumber number, ExactSpace space -> format (tier, number, space)
            | tier, number, ExactSpace Space.Business -> aliasForBusinessEnvironmentPattern tier number
            | tier, number, space -> aliasForEnvironmentPattern tier number space

[<RequireQualifiedAccess>]
module private EnvironmentAlias =
    let value = Alias.value

[<RequireQualifiedAccess>]
[<CustomEquality; NoComparison>]
type Environment =
    | FullyQualified of FullyQualifiedEnvironment
    | Alias of Alias

    member private this.ToAlias () =
        match this with
        | FullyQualified { Tier = tier; Number = number; Space = space } -> Alias.format (tier, number, space)
        | Alias alias -> alias |> Alias.value

    override this.GetHashCode() =
        this.ToAlias() |> hash

    override this.Equals (b) =
        match b with
        | :? Environment as environmentB -> this.ToAlias() = environmentB.ToAlias()
        | _ -> false

and FullyQualifiedEnvironment = {
    Number: EnvironmentNumber
    Space: Space
    Tier: Tier
}

[<AutoOpen>]
module private MatchingHelp =
    // todo - what about this? -> convert to active patterns?

    // matching helpers
    type MatchTier =
        | MatchExactTier
        | MatchAnyTier

    type MatchNumber =
        | MatchExactNumber
        | MatchWithoutNumber
        | MatchAnyNumber

    type MatchSpace =
        | MatchExactSpace
        | MatchAnySpace

    let matchEnvWith tier number space =
        let tierPart =
            match tier with
            | MatchExactTier -> @"([a-z]+){1}"
            | MatchAnyTier -> @"\*"
        let numberPart =
            match number with
            | MatchExactNumber -> @"(\d+){1}"
            | MatchWithoutNumber -> @"(?!\d+)"
            | MatchAnyNumber -> @"X"
        let spacePart =
            match space with
            | MatchExactSpace -> @"(-[a-z]*)?"
            | MatchAnySpace -> @"(?>-\*)?"  // match but not capture

        sprintf @"^%s%s%s$" tierPart numberPart spacePart

[<RequireQualifiedAccess>]
module Environment =
    open Lmc.ErrorHandling.Result.Operators

    [<RequireQualifiedAccess>]
    module FullyQualified =
        let toTuple (environment: FullyQualifiedEnvironment) =
            (environment.Tier, environment.Number, environment.Space)

        let toAlias = toTuple >> Alias.format >> Alias

        let parse: string -> Result<FullyQualifiedEnvironment, EnvironmentError> =
            fun alias -> result {
                match alias with
                | Regex (matchEnvWith MatchExactTier MatchExactNumber MatchExactSpace) [ t; n; s ] ->
                    let! space = Space.parse s <@> EnvironmentError.SpaceError
                    let! tier = Tier.parse t <@> EnvironmentError.TierError

                    return {
                        Number = Number (n |> int)
                        Tier = tier
                        Space = space
                    }
                | Regex (matchEnvWith MatchExactTier MatchWithoutNumber MatchExactSpace) [ t; s ] ->
                    let! space = Space.parse s <@> EnvironmentError.SpaceError
                    let! tier = Tier.parse t <@> EnvironmentError.TierError

                    return {
                        Number = Numberless
                        Tier = tier
                        Space = space
                    }
                | _ ->
                    return! Error <| AliasError (InvalidAlias alias)
            }

        let toScalar (environment: FullyQualifiedEnvironment) =
            (
                environment.Tier |> Tier.value,
                environment.Number |> EnvironmentNumber.value,
                environment.Space |> Space.value
            )

    let toAlias = function
        | Environment.FullyQualified environment -> environment |> FullyQualified.toAlias
        | Environment.Alias alias -> alias

    let toScalar = function
        | Environment.FullyQualified environment -> Ok (environment |> FullyQualified.toScalar)
        | Environment.Alias alias -> alias |> Alias.value |> FullyQualified.parse <!> FullyQualified.toScalar

    let toTuple = function
        | Environment.FullyQualified environment -> Ok (environment |> FullyQualified.toTuple)
        | Environment.Alias alias -> alias |> Alias.value |> FullyQualified.parse <!> FullyQualified.toTuple

    let value = toAlias >> Alias.value

    let format = function
        | Environment.FullyQualified environment -> environment |> FullyQualified.toAlias |> Alias.value |> sprintf "%s (FullyQualified)"
        | Environment.Alias alias -> alias |> Alias.value |> sprintf "%s (Alias)"

    let parse environmentValue = environmentValue |> FullyQualified.parse <!> Environment.FullyQualified

type EnvironmentDefinition =
    | ByProxy of Proxy
    | ByEnvironment of Environment

[<RequireQualifiedAccess>]
module EnvironmentDefinition =
    let Current = "&current" |> Proxy.create |> Result.orFail

    let environment = function
        | ByProxy proxy -> failwithf "Environment definition is set as proxy %A.\nYou have to resolve the proxy first." (proxy |> Proxy.format)
        | ByEnvironment environment -> environment

    let format = function
        | ByProxy proxy -> proxy |> Proxy.format
        | ByEnvironment environment -> environment |> Environment.toAlias |> Alias.value

    let resolve (key, (environment: Environment)) = function
        | ByEnvironment environment -> Ok environment
        | ByProxy proxy -> proxy |> Proxy.resolve (key => environment)

// ==== /Env ====

// ==== Env pattern ====

[<RequireQualifiedAccess>]
[<CustomEquality; NoComparison>]
type EnvironmentPattern =
    | FullyQualified of FullyQualifiedEnvironmentPattern
    | Alias of EnvironmentPatternAlias

    member private this.ToAlias () =
        match this with
        | FullyQualified { Tier = tier; Number = number; Space = space } -> Alias.Pattern.create (tier, number, space) |> Environment
        | Alias alias -> alias

    override this.GetHashCode() =
        this.ToAlias() |> hash

    override this.Equals (b) =
        match b with
        | :? EnvironmentPattern as environmentB -> this.ToAlias() = environmentB.ToAlias()
        | _ -> false

and EnvironmentPatternAlias =
    | Environment of Alias
    | Any

and FullyQualifiedEnvironmentPattern = {
    Number: EnvironmentPatternNumber
    Space: EnvironmentPatternSpace
    Tier: EnvironmentPatternTier
}

[<RequireQualifiedAccess>]
module EnvironmentPattern =
    open Lmc.ErrorHandling.Result.Operators

    [<RequireQualifiedAccess>]
    module FullyQualified =
        let toTuple (env: FullyQualifiedEnvironmentPattern) =
            (env.Tier, env.Number, env.Space)

        let toScalar (environment: FullyQualifiedEnvironmentPattern) =
            (
                environment.Tier |> EnvironmentPatternTier.value,
                environment.Number |> EnvironmentPatternNumber.value,
                environment.Space |> EnvironmentPatternSpace.value
            )

        let parse: string -> Result<FullyQualifiedEnvironmentPattern, EnvironmentPatternError> =
            fun alias ->
                match alias |> Environment.FullyQualified.parse with
                | Ok env ->
                    Ok {
                        Tier = ExactTier env.Tier
                        Number = ExactNumber env.Number
                        Space = ExactSpace env.Space
                    }
                | _ ->
                    result {
                        match alias with
                        | "*" ->
                            return {
                                Tier = AnyTier
                                Number = AnyNumber
                                Space = AnySpace
                            }
                        | Regex (matchEnvWith MatchExactTier MatchAnyNumber MatchExactSpace) [ t; s ] -> // devX-services, devX
                            let! space = Space.parse s <@> EnvironmentPatternError.SpaceError
                            let! tier = Tier.parse t <@> EnvironmentPatternError.TierError

                            return {
                                Tier = ExactTier tier
                                Number = AnyNumber
                                Space = ExactSpace space
                            }
                        | Regex (matchEnvWith MatchAnyTier MatchAnyNumber MatchExactSpace) [ s ] -> // *X-services, *X
                            let! space = Space.parse s <@> EnvironmentPatternError.SpaceError

                            return {
                                Tier = AnyTier
                                Number = AnyNumber
                                Space = ExactSpace space
                            }
                        | Regex (matchEnvWith MatchExactTier MatchAnyNumber MatchAnySpace) [ t ] -> // devX-*
                            let! tier = Tier.parse t <@> EnvironmentPatternError.TierError

                            return {
                                Tier = ExactTier tier
                                Number = AnyNumber
                                Space = AnySpace
                            }
                        | Regex (matchEnvWith MatchAnyTier MatchExactNumber MatchExactSpace) [ n; s ] ->  // *41-services
                            let! space = Space.parse s <@> EnvironmentPatternError.SpaceError

                            return {
                                Tier = AnyTier
                                Number = ExactNumber (Number (n |> int))
                                Space = ExactSpace space
                            }
                        | Regex (matchEnvWith MatchExactTier MatchExactNumber MatchAnySpace) [ t; n ] ->  // dev42-*
                            let! tier = Tier.parse t <@> EnvironmentPatternError.TierError

                            return {
                                Tier = ExactTier tier
                                Number = ExactNumber (Number (n |> int))
                                Space = AnySpace
                            }
                        | Regex (matchEnvWith MatchAnyTier MatchExactNumber MatchAnySpace) [ n ] ->  // *42-*
                            return {
                                Tier = AnyTier
                                Number = ExactNumber (Number (n |> int))
                                Space = AnySpace
                            }
                        | Regex (matchEnvWith MatchExactTier MatchWithoutNumber MatchAnySpace) [ t ] ->  // dev-*
                            let! tier = Tier.parse t <@> EnvironmentPatternError.TierError

                            return {
                                Tier = ExactTier tier
                                Number = ExactNumber (Numberless)
                                Space = AnySpace
                            }
                        | Regex (matchEnvWith MatchAnyTier MatchWithoutNumber MatchExactSpace) [ s ] ->  // *-services
                            let! space = Space.parse s <@> EnvironmentPatternError.SpaceError

                            return {
                                Tier = AnyTier
                                Number = ExactNumber (Numberless)
                                Space = ExactSpace space
                            }
                        | Regex (matchEnvWith MatchAnyTier MatchWithoutNumber MatchAnySpace) [] ->  // *-*
                            return {
                                Tier = AnyTier
                                Number = ExactNumber (Numberless)
                                Space = AnySpace
                            }
                        | "*X-*" ->
                            return! Error <| EnvironmentPatternError.AliasPatternError (UnnecessarilyComplex (alias, "*"))
                        | _ ->
                            return! Error <| EnvironmentPatternError.AliasPatternError (InvalidPattern alias)
                    }

        let value = toTuple >> Alias.Pattern.format

        let toAlias = toTuple >> Alias.Pattern.format >> (function
            | "*" -> EnvironmentPatternAlias.Any
            | alias -> EnvironmentPatternAlias.Environment (Alias alias)
        )

    [<RequireQualifiedAccess>]
    module Alias =
        let value = function
            | EnvironmentPatternAlias.Environment alias -> alias |> Alias.value
            | EnvironmentPatternAlias.Any -> "*"

    let parse environmentPatternValue = environmentPatternValue |> FullyQualified.parse <!> EnvironmentPattern.FullyQualified

    let toAlias = function
        | EnvironmentPattern.FullyQualified e -> e |> FullyQualified.toAlias
        | EnvironmentPattern.Alias a -> a

    let toScalar = function
        | EnvironmentPattern.FullyQualified e -> Ok (e |> FullyQualified.toScalar)
        | EnvironmentPattern.Alias aliasPattern ->
            match aliasPattern with
            | Environment alias -> alias |> EnvironmentAlias.value |> FullyQualified.parse <!> FullyQualified.toScalar
            | Any -> Ok ("*", "X", "*")

    let toTuple = function
        | EnvironmentPattern.FullyQualified e -> Ok (e |> FullyQualified.toTuple)
        | EnvironmentPattern.Alias aliasPattern ->
            match aliasPattern with
            | Environment alias -> alias |> EnvironmentAlias.value |> FullyQualified.parse <!> FullyQualified.toTuple
            | Any -> Ok (AnyTier, AnyNumber, AnySpace)

    let value = toAlias >> Alias.value

// ==== /Env pattern ====
