module Matrosov.Codingame.Hard.CgxFormatter

open System
open System.Text.RegularExpressions

let readString() = Console.ReadLine()
let readInt = readString >> int

let makeTabs n = String.replicate n "    "

let rec format previousToken depth tokens =
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

            yield! format t (depth + depthInc) tail
    }

Array.init (readInt()) (fun _ -> readString())
|> String.concat ""
|> (fun i -> Regex.Matches(i, "\(|\)|=|[0-9]+|true|false|'[^']*'|;"))
|> Seq.cast
|> Seq.map string
|> List.ofSeq
|> format "" 0
|> String.concat ""
|> printfn "%s"
