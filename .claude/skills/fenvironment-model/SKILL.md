---
name: fenvironment-model
description: Use whenever generating or reviewing F# (or Fable) code that parses, formats, matches, or constructs deployment environments and environment selectors — anything dealing with Tier (dev/devel/deploy/int/prod), EnvironmentNumber, Space (named spaces or AWS accounts), Environment, EnvironmentPattern (with `*` / `X` wildcards), Proxy / EnvironmentDefinition. Trigger on calls to Environment.parse, Environment.toAlias, Environment.toScalar, EnvironmentPattern.parse, Tier.parse, Space.parse, EnvironmentDefinition.resolve, or on mentions of environment aliases like "dev1-services", "prod", "devX-*", AWS-account spaces, business/default space, or resolving `&proxy` references.
---

# F-Environment-Model

Library: [alma-oss/fenvironment-model](https://github.com/alma-oss/fenvironment-model)
NuGet: `Alma.EnvironmentModel`

## Purpose

Provides model types and modules for describing deployment environments and environment selectors. It parses and formats environment aliases (e.g. `dev1-services`, `prod`, an AWS-account form like `exampleaccount-dev1`), models wildcard patterns for selecting groups of environments, and supports symbolic `&proxy` references that resolve to a concrete environment.

## When to Use

- Parsing or formatting an environment alias string to/from a structured value.
- Building or matching environment selectors with wildcards (any tier `*`, any number `X`).
- Distinguishing tiers, numbers, named spaces, and AWS-account spaces in a type-safe way.
- Representing an environment indirectly via a `&proxy` reference and resolving it later.

## When NOT to Use

- General string manipulation unrelated to environment identity.
- Defining business/domain entities or deployment orchestration logic — this library only models environment identity and selection.

## Main Concepts

- **Tier** — deployment tier: `Tier.Dev`, `Tier.Devel`, `Tier.Deploy`, `Tier.Integration` (string `int`), `Tier.Prod`.
- **EnvironmentNumber** — `Number of int` or `Numberless`.
- **Space** — `Space.Space "name"` for a named space (empty string `""` is the business/default space) or `Space.AWSAccount` for an AWS-account space.
- **AWSAccount** — wrapper over a lowercase string that may contain a single internal `-`.
- **FullyQualifiedEnvironment** — record `{ Tier; Number; Space }`.
- **Environment** — either `Environment.FullyQualified` or `Environment.Alias`; the two forms are equal when they denote the same alias (custom equality).
- **Alias** — the canonical string form of an environment (and `Alias.Pattern` for patterns).
- **EnvironmentPattern** — a selector with optional wildcards: `AnyTier`/`ExactTier`, `AnyNumber`/`ExactNumber`, `AnySpace`/`ExactSpace`; `*` matches any tier/space, `X` matches any number.
- **Proxy** — a private, `&`-prefixed symbolic reference (e.g. `&current`); constructed only via `Proxy.create`.
- **EnvironmentDefinition** — `ByEnvironment` or `ByProxy`; resolve a proxy to a concrete environment with `EnvironmentDefinition.resolve`.
- **Errors** — typed errors (`EnvironmentError`, `EnvironmentPatternError`, `SpaceError`, `TierError`, `ProxyError`, …), each module exposing a `format` function; operations return `Result`.

## Related Libraries

- `Feather.ErrorHandling` — `Result` helpers and operators (`<!>`, `<@>`, `=>`) used throughout.
- `Alma.ServiceIdentification` — service identity types, commonly paired when identifying where a service is deployed.

## Keywords for Search

environment, tier, dev, devel, deploy, int, prod, space, AWS account, business space, environment number, numberless, alias, fully qualified environment, environment pattern, wildcard, any tier, any number, proxy, &current, EnvironmentDefinition, resolve, parse, toAlias, toScalar, toTuple, Result, F#, Fable, Alma.EnvironmentModel

## Reference Files

- For composition principles and recommended API usage, read `references/preferred-patterns.md`.
- For known pitfalls and incorrect assumptions, read `references/anti-patterns.md`.
- For worked code examples, read `references/examples.md`.
