module Matrosov.Codingame.CgxFormatter

open System
open System.Text.RegularExpressions

let readString() = Console.ReadLine()
let readInt = readString >> int

let makeTabs n = String.replicate n "    "

let format tokens =
    let rec formatTokens tokens previousToken depth =
        seq {
            match tokens with
            | [] -> ()
            | t :: tail ->
                match t with
                | ";" -> yield ";\n"
                | "=" -> yield "="
                | "(" ->
                    if previousToken = "=" then yield "\n"
                    yield makeTabs depth
                    yield "(\n"
                | ")" ->
                    if previousToken <> "(" then yield "\n"
                    yield makeTabs (depth - 1)
                    yield ")"
                | _ ->
                    if previousToken <> "=" then yield makeTabs depth
                    yield t

                let depthInc = 
                    match t with
                    | "(" -> 1
                    | ")" -> -1
                    | _ -> 0

                yield! formatTokens tail t (depth + depthInc)
        }
    formatTokens tokens "" 0

let input =
    Array.init (readInt()) (fun _ -> readString())
    |> String.concat ""

let tokenMatches = Regex.Matches(input, "\(|\)|=|[0-9]+|true|false|'[^']*'|;")

let answer =
    format [for t in tokenMatches -> t.ToString()]
    |> String.concat ""

printfn "%s" answer