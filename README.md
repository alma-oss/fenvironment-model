Environment Model
=================

[![NuGet](https://img.shields.io/nuget/v/Alma.EnvironmentModel.svg)](https://www.nuget.org/packages/Alma.EnvironmentModel)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Alma.EnvironmentModel.svg)](https://www.nuget.org/packages/Alma.EnvironmentModel)
[![Tests](https://github.com/alma-oss/fenvironment-model/actions/workflows/tests.yaml/badge.svg)](https://github.com/alma-oss/fenvironment-model/actions/workflows/tests.yaml)

> Library which contains a Model types and basic modules for Environment Definition and Deployment.

## Install

Add following into `paket.references`
```
Alma.EnvironmentModel
```

**Note**: You can also use this library in a Fable project.

## Environment

### Tier
- dev
- devel
- deploy
- int (*integration*)
- prod

### Number
- exact number
- without number

### Space
- lowercased string, such as "services" (empty "" is considered a business/default space)
- AWS Account (a string - lowercased and could contain a - in the middle)

---

## Environment Patterns

### Tier
- dev
- devel
- deploy
- int (*integration*)
- prod
- `*` (_any_)

### Number
- exact number
- without number
- `X` (_any_)

### Space
- lowercased string, such as "services" (empty "" is considered a business/default space)
- AWS Account (a string - lowercased and could contain a - in the middle)
- `*` (_any_)

### Examples
Lets have only these environments:

`dev3`, `devel6`, `deploy`, `prod`
`dev1-services`, `devel1-services`, `deploy-services`, `prod-services`, `dev4-rad`
`privacy-components-int1`, `dex-dev`

| Pattern    | Description                      | Matched environments |
| ---        | ---                              | ---                  |
| `*`        | All environments                 | _all environments ..._ |
| `devX`     | All business devs                | `dev3` |
| `devX-*`   | All devs (excluding AWS)         | `dev3`, `dev1-services`, `dev4-rad` |
| `deploy`   | Business deploy without number   | `deploy` |
| `deploy-*` | All deploys without number       | `deploy`, `deploy-services` |
| `dex-*`    | All devs from AWS dex space      | `dex-dev` |
| `privacy-components-intX`    | All devs from AWS dex space      | `privacy-components-int1` |
| `*1-*`     | All environments with number 1   | `dev1-services`, `devel1-services` |

NOTE: There currently is not a pattern for matching AWS environments without specifying an exact space.

## Software Component
A software component is an `Instance` (from `Alma.ServiceIdentification`) deployed in an `Environment`, written `instance@environment`, for example `alma-userManager-account-v1@dev1-services`. It may carry its concept type as a prefix: `AC:` (application component), `SS:` (system software), `ST:` (storage); without a prefix it is `Untyped`.

```fsharp
open Alma.EnvironmentModel

let component = SoftwareComponent.parse "-" "AC:alma-userManager-account-v1@dev1-services"
// Ok (ApplicationComponent (instance, environment))

component |> Result.map SoftwareComponent.value             // Ok "alma-userManager-account-v1@dev1-services"
component |> Result.map (SoftwareComponent.serialize "-")   // Ok "AC:alma-userManager-account-v1@dev1-services"
```

`parse` returns a `SoftwareComponentError` (`InvalidFormat`, `InstanceError`, `EnvironmentError`), formatted by `SoftwareComponentError.format`.

## Release
1. Increment version in `EnvironmentModel.fsproj`
2. Update `CHANGELOG.md`
3. Commit new version and tag it

## Development
### Requirements
- [dotnet core](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial)

### Build
```bash
./build.sh build
```

### Tests
```bash
./build.sh -t tests
```
