# AGENTS.md — Alma.EnvironmentModel

## Project Purpose

F# library containing model types and modules for environment definition and deployment. Defines environment tiers (dev, devel, deploy, int, prod), numbering, spaces (including AWS accounts), and pattern matching for environment selectors. Also usable in Fable (F#→JavaScript) projects. Published as NuGet package `Alma.EnvironmentModel`.

## Tech Stack

- **Language:** F# (.NET 10)
- **Framework:** .NET SDK library, Fable-compatible
- **Package management:** Paket
- **Build system:** FAKE (F# Make) via `build.sh`
- **Testing:** Expecto (via `YoloDev.Expecto.TestSdk`)
- **Linting:** fsharplint
- **CI/CD:** GitHub Actions
- **Key dependencies:** `FSharp.Core ~> 10.0`, `Feather.ErrorHandling ~> 2.0`, `Alma.ServiceIdentification ~> 11.0`

## Commands

```bash
# Install dependencies
dotnet tool restore && dotnet paket install

# Build
./build.sh build

# Run tests
./build.sh -t tests

# Lint
dotnet fsharplint lint src/Alma.EnvironmentModel/EnvironmentModel.fsproj

# Full pipeline (clean → build → lint → tests)
./build.sh -t tests
```

## Project Structure

```
fenvironment-model/
├── src/
│   └── Alma.EnvironmentModel/
│       ├── EnvironmentModel.fsproj   # Main project (PackageId: Alma.EnvironmentModel, v10.0.0)
│       ├── AssemblyInfo.fs           # Auto-generated
│       ├── Utils.fs                  # Internal utilities
│       ├── Proxy.fs                  # Proxy types
│       ├── EnvironmentDefinition.fs  # Core environment model (tiers, numbers, spaces, patterns)
│       └── paket.references          # FSharp.Core, Feather.ErrorHandling, Alma.ServiceIdentification
├── tests/
│   └── tests.fsproj                  # Expecto test project
├── build/
│   ├── build.fsproj                  # FAKE build project
│   ├── Build.fs                      # Build entry point
│   └── AssemblyInfo.fs               # Build assembly info
├── build.sh                          # Build entry script
├── paket.dependencies                # Top-level dependencies
├── fsharplint.json                   # Lint configuration
├── CHANGELOG.md                      # Release history
└── .github/workflows/
    ├── tests.yaml                    # Tests on PRs and nightly
    ├── pr-check.yaml                 # Fixup commit blocker, ShellCheck
    └── publish.yaml                  # NuGet publish on tags
```

## Architecture

Pure library with these domain concepts:

- **Tier** — `dev`, `devel`, `deploy`, `int`, `prod`
- **Number** — exact number or none
- **Space** — lowercase string (e.g., `"services"`), AWS account, or empty (business/default)
- **Environment patterns** — wildcard matching with `*` (any space/tier) and `X` (any number)
  - Example: `devX-*` matches all dev environments across all spaces

**Fable compatibility** — the `.fsproj` includes `<Content Include="*.fsproj; *.fs;" PackagePath="fable\" />` for Fable consumption.

## Build System (FAKE)

Standard library target chain: `Clean → AssemblyInfo → Build → Lint → Tests → Release → Publish`

## CI/CD

- **tests.yaml** — runs on PRs and nightly
- **pr-check.yaml** — blocks fixup commits, runs ShellCheck
- **publish.yaml** — publishes to NuGet on semver tags

## Release Process

1. Increment `<Version>` in `src/Alma.EnvironmentModel/EnvironmentModel.fsproj`
2. Update `CHANGELOG.md`
3. Commit, tag with version, push

## Conventions

- Source code lives under `src/Alma.EnvironmentModel/` (not project root)
- Fable-compatible: avoid .NET-only APIs that don't transpile
- `Feather.ErrorHandling` for Result patterns
- `Alma.ServiceIdentification` for service identity types
- Compile order in `.fsproj` matters

## Pitfalls

- **No Docker** — pure library
- **Fable compatibility** — code must work both in .NET and Fable (JavaScript); avoid .NET-only APIs
- **Source location** — source is in `src/Alma.EnvironmentModel/`, not the project root
- **Paket, not NuGet CLI** — use `dotnet paket install`
- **AssemblyInfo.fs** — auto-generated, do not edit manually
