Environment Model
=================

> Library which contains a Model types and basic modules for Environment Definition and Deployment.

## Environment

### Tier
- dev
- devel
- deploy
- prod

### Number
- exact number
- without number

### Space
- business (same as empty)
- rad
- bi
- services

---

## Environment Patterns

### Tier
- dev
- devel
- deploy
- prod
- `*` (_any_)

### Number
- exact number
- without number
- `X` (_any_)

### Space
- business (same as empty)
- rad
- bi
- services
- `*` (_any_)

### Examples
Lets have only these environments:

`dev3`, `devel6`, `deploy`, `prod`
`dev1-services`, `devel1-services`, `deploy-services`, `prod-services`, `dev4-rad`

| Pattern    | Description                      | Matched environments |
| ---        | ---                              | ---                  |
| `*`        | All environments                 | _all environments ..._ |
| `devX`     | All business devs                | `dev3` |
| `devX-*`   | All devs                         | `dev3`, `dev1-services`, `dev4-rad` |
| `deploy`   | Business deploy without number   | `deploy` |
| `deploy-*` | All deploys without number       | `deploy`, `deploy-services` |
| `*1-*`     | All environments with number 1   | `dev1-services`, `devel1-services` |
