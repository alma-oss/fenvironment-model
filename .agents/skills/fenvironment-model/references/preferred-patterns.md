# Preferred Patterns

## Core Principles

- The string alias is the canonical interchange format; structured types (`FullyQualifiedEnvironment`, `FullyQualifiedEnvironmentPattern`) are the safe in-memory representation. Parse at the boundary, work with structured values, format on the way out.
- Treat `Space.Space ""` as the business/default space — it is a real, valid value, not a missing one.
- An `Environment` may already be fully qualified or still an alias. Use the module functions that accept `Environment` (they parse the alias when needed) rather than assuming one representation.
- Patterns are a superset of environments: every concrete environment is also a valid pattern (all parts `Exact*`).

## Recommended API Usage

- **Parsing an alias string** → `Environment.parse` (returns `Result<Environment, EnvironmentError>`) or `Environment.FullyQualified.parse` when you specifically need the record. See `examples.md` → Basic Parsing.
- **Formatting back to a string** → `Environment.value` (alias string) or `Environment.toAlias`. For display with the representation tag, use `Environment.format`.
- **Decomposing** → `Environment.toScalar` yields a `(tier, number, space)` tuple of plain strings; `Environment.toTuple` yields the typed tuple. Both return `Result` because an `Alias`-form environment must be parsed first.
- **Tiers and spaces** → `Tier.parse` / `Tier.value` and `Space.parse` / `Space.value` / `Space.format`. `Space.format` renders the empty space as `"business"`; `Space.value` renders it as `""`.
- **Patterns** → `EnvironmentPattern.parse` to read a selector, `EnvironmentPattern.value` to render it, `EnvironmentPattern.toScalar` / `toTuple` to decompose. See `examples.md` → Pattern Parsing.
- **Proxies** → build with `Proxy.create` (never construct directly; the case is private). The predefined `EnvironmentDefinition.Current` corresponds to `&current`.

## Valid Spaces

`Space.parse` accepts the empty/business space (`""`, `business`, `-business`) and a fixed set of named spaces (`services`, `rad`, `bi`, `ict`, `internal`), each optionally prefixed with `-`. Any other lowercase token (optionally containing one internal `-`) that is not a tier is treated as an AWS-account space. Tokens that look like a tier are rejected with `UnknownSpace`.

## Error Handling

- Every parse/resolve returns a `Result`; branch on `Ok` / `Error` rather than throwing.
- Each error type has a paired `format` function (e.g. `EnvironmentError.format`, `EnvironmentPatternError.format`, `SpaceError.format`) producing a human-readable message — use it for surfacing failures.
- Compose with `Feather.ErrorHandling` operators: `<!>` maps the `Ok` value, `<@>` maps the `Error` value (used to lift `SpaceError`/`TierError` into the wider `EnvironmentError`).

## Composition

- Map a successful parse straight into your own type with `<!>` instead of unwrapping and re-wrapping.
- When you need both a tier and a space from one alias, prefer a single `Environment.FullyQualified.parse` over parsing the substrings separately — the regex matchers already separate AWS-account aliases from named-space aliases.

## Integration with Other Libraries

- Pair with `Alma.ServiceIdentification` when an environment identifies where a given service runs.
- Lean on `Feather.ErrorHandling` for the `Result` plumbing rather than reimplementing it.

## Naming Conventions

- Modules mirror their types and are `[<RequireQualifiedAccess>]`: call `Tier.parse`, `Space.value`, `Environment.toAlias`, `EnvironmentPattern.parse` with the qualifier.
- Pattern parts use the `Exact*` / `Any*` prefixes (`ExactTier`, `AnyNumber`, `AnySpace`).

## Testing Recommendations

- Drive tests from a table of `(structured value, alias string)` pairs and assert both directions: structured → alias and alias → structured. See `examples.md` → Round-trip Test.
- Assert the custom equality between the `FullyQualified` and `Alias` forms of the same environment.
- Cover the business/default space, a named space, and an AWS-account space, plus at least one wildcard pattern.
