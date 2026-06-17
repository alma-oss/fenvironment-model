# Anti-Patterns

Format below: **mistake → why → fix**.

## Proxies and EnvironmentDefinition

- **Calling `EnvironmentDefinition.environment` on a `ByProxy` value** → it throws (`failwithf`) because a proxy is not yet a concrete environment → resolve first with `EnvironmentDefinition.resolve (key => environment)`, then read the environment, or pattern-match `ByProxy` / `ByEnvironment` yourself.
- **Trying to build a `Proxy` by calling its constructor** → the `Proxy` case is `private`, so direct construction does not compile → create it via `Proxy.create "&name"` (returns `Result`), or use the predefined `EnvironmentDefinition.Current` for `&current`.
- **Passing a proxy string without the leading `&`** → `Proxy.create` returns `Error (ProxyError.InvalidFormat …)` because a proxy must start with `&` and be non-empty → always prefix with `&` (e.g. `&current`).
- **Resolving a proxy with the wrong key** → `Proxy.resolve` returns `Error (ResolveDifferentProxy …)` when the key proxy differs from the target → resolve each proxy with its own matching key.

## Spaces

- **Treating the business space as a missing/null space** → the business/default space is `Space.Space ""`, a real value → construct it explicitly and remember `Space.format` renders it as `"business"` while `Space.value` renders it as `""`.
- **Assuming any lowercase word parses as a named space** → only the empty space and a fixed set (`services`, `rad`, `bi`, `ict`, `internal`) are named spaces; everything else lowercase becomes an AWS-account space, and tier-like tokens are rejected → check the `Result` from `Space.parse` instead of assuming success.
- **Expecting a tier name (e.g. `dev`) to parse as a space** → `Space.parse "dev"` returns `Error (UnknownSpace "dev")` by design → do not use tier words as spaces.

## Environment vs Alias representation

- **Assuming `Environment.toScalar` / `toTuple` always succeed** → for an `Environment.Alias` they parse the alias and may fail, so they return `Result` → handle the `Error` branch rather than force-unwrapping.
- **Comparing environments by their union case instead of value** → `Environment` and `EnvironmentPattern` define custom equality based on the alias, so a `FullyQualified` and an equivalent `Alias` are equal → rely on `=` / equality, do not branch on the case to decide identity.

## Patterns

- **Expecting to match all AWS environments without naming the space** → there is no pattern for "any AWS account"; an AWS-account space must be given explicitly → specify the space (e.g. `exampleaccount-*`) when targeting AWS environments.
- **Writing the redundant pattern `*X-*`** → `EnvironmentPattern.parse "*X-*"` returns `Error (UnnecessarilyComplex …)` because it is equivalent to `*` → use `*` for "all environments".
- **Confusing the two wildcards** → `*` is any tier or any space, `X` is any number → use `X` only in the number position and `*` only in tier/space positions.

## Error handling

- **Throwing or ignoring on parse failure** → all parse/resolve APIs return `Result` and the only function that throws is the proxy-misuse guard above → branch on `Ok` / `Error` and render messages with the matching `*.format` function.

## Fable compatibility

- **Introducing .NET-only APIs in code shared with Fable** → this library is Fable-compatible and consumers may transpile to JavaScript → keep usage to the library's own API and portable F# so it works in both targets.
