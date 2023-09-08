namespace Alma.EnvironmentModel

[<AutoOpen>]
module ProxyModule =
    [<RequireQualifiedAccess>]
    type ProxyError =
        | InvalidFormat of given: string * requires: string

    [<RequireQualifiedAccess>]
    module ProxyError =
        let format = function
            | ProxyError.InvalidFormat (given, requires) -> sprintf "Proxy %A must start with %A and must not be empty." given requires

    type Proxy = private Proxy of string

    [<RequireQualifiedAccess>]
    type ProxyResolveError =
        | ResolveDifferentProxy of key: Proxy * target: Proxy

    [<RequireQualifiedAccess>]
    module ProxyResolveError =
        let format = function
            | ProxyResolveError.ResolveDifferentProxy (Proxy key, Proxy target) ->
                sprintf "You can not resolve a proxy %A by proxy %A." target key

    [<RequireQualifiedAccess>]
    module Proxy =
        let isValid (value: string) =
            value.StartsWith("&") && value.Length > 1

        let create: string -> Result<Proxy, ProxyError> = function
            | proxy when proxy |> isValid -> Ok (Proxy proxy)
            | invalid -> Error (ProxyError.InvalidFormat (invalid, "&"))

        let value (Proxy value) = value.TrimStart('&')
        let format (Proxy value) = value

        let resolve (key, target) proxy =
            if key = proxy then Ok target
            else Error (ProxyResolveError.ResolveDifferentProxy (key, proxy))
