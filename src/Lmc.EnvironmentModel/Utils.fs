namespace Lmc.EnvironmentModel

[<AutoOpen>]
module private Utils =
    let tee f a =
        f a
        a

    let (=>) key value = key, value

[<AutoOpen>]
module private Regexp =
    open System.Text.RegularExpressions

    // http://www.fssnip.net/29/title/Regular-expression-active-pattern
    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ])
        else None
