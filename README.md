Environment Model
=================

> Library which contains a Model types and basic modules for Environment Definition and Deployment.

## Install

Add following into `paket.dependencies`
```
source https://nuget.pkg.github.com/almacareer/index.json username: "%PRIVATE_FEED_USER%" password: "%PRIVATE_FEED_PASS%"
# LMC Nuget dependencies:
nuget Alma.EnvironmentModel
```

NOTE: For local development, you have to create ENV variables with your github personal access token.
```sh
export PRIVATE_FEED_USER='{GITHUB USERNANME}'
export PRIVATE_FEED_PASS='{TOKEN}'	# with permissions: read:packages
```

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
