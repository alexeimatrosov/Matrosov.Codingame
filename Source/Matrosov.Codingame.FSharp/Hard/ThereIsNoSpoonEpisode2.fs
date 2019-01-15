module Matrosov.Codingame.FSharp.Hard.ThereIsNoSpoonEpisode2

open System
open System.Collections.Generic

type GameState = {
    Grid : char[,]
    Links : Dictionary<(int * int) * (int * int), int>
    NodesOpen : HashSet<int * int>
}

let readString() = Console.ReadLine()
let readInt = readString >> int

let width = readInt()
let height = readInt()

let sortPair (a, b) = if a <= b then (a, b) else (b, a)
let valueAt (x, y) (grid : 'T[,]) = grid.[y,x]
let asInt (c : char) = int c - int '0'
let asIntAt xy = valueAt xy >> asInt

let isNode c = '1' <= c && c <= '8'

let enumerateCells predicate (grid : char[,]) =
    seq {
        for y = 0 to height-1 do
            for x = 0 to width-1 do
                if predicate grid.[y,x] then yield (x, y)
    }

let copy state =
    {
        Grid = state.Grid |> Array2D.copy
        Links = Dictionary<(int * int) * (int * int), int>(state.Links)
        NodesOpen = HashSet<int * int>(state.NodesOpen)
    }

let hasLink xy xy' state = state.Links.ContainsKey(xy, xy') || state.Links.ContainsKey(xy', xy)

let addLinks (xy, links) state =
    let subtractNodeAmount (x, y) amount =
        state.Grid.[y,x] <- char (int state.Grid.[y,x] - amount)

    let drawLink (x1, y1) (x2, y2) =
        if x1 = x2 then
            let (y1, y2) = sortPair (y1, y2)
            for i in y1+1 .. y2-1 do state.Grid.[i,x1] <- '|'
        elif y1 = y2 then
            let (x1, x2) = sortPair (x1, x2)
            for i in x1+1 .. x2-1 do state.Grid.[y1,i] <- '-'

    links
    |> Seq.filter (fun (xy', _) -> state |> hasLink xy xy' |> not)
    |> Seq.iter (fun (xy', amount) -> 
        drawLink xy xy'
        subtractNodeAmount xy amount
        subtractNodeAmount xy' amount
        state.Links.Add((xy, xy'), amount)
        seq {
            yield xy
            yield xy'
        }
        |> Seq.filter (fun (x, y) -> state.Grid.[y,x] = '0')
        |> Seq.iter (state.NodesOpen.Remove >> ignore))
    state

let enumerateNeighbours state (x, y) =
    let rec aux x' y' dx dy =
        seq {
            yield (x', y')
            yield! aux (x'+dx) (y'+dy) dx dy
        }

    let notBlocked c = c <> '-' && c <> '|' && c <> '0'

    let pickNode cells =
        cells
        |> Seq.takeWhile (fun (x', y') -> 0 <= x' && x' < width && 0 <= y' && y' < height && state.Grid.[y',x'] |> notBlocked)
        |> Seq.tryPick (fun (x', y') -> if state.Grid.[y',x'] |> isNode && state |> hasLink (x, y) (x', y') |> not then Some (x', y') else None)

    seq {
        yield aux (x+1) y +1 0 |> pickNode
        yield aux (x-1) y -1 0 |> pickNode
        yield aux x (y+1) 0 +1 |> pickNode
        yield aux x (y-1) 0 -1 |> pickNode
    }
    |> Seq.choose id

let enumeratePossibleMaxLinks state xy =
    let value = state.Grid |> asIntAt xy
    enumerateNeighbours state xy
    |> Seq.map (fun xy' -> (xy', state.Grid |> asIntAt xy' |> min value |> min 2))

let rec playLinks state =
    let tryPutFor (xy, possibleLinks) =
        if (state.Grid |> asIntAt xy) = (possibleLinks |> Seq.map snd |> Seq.sum)
        then Some (xy, possibleLinks)
        else None

    let possibleMaxLinks =
        state.NodesOpen
        |> Seq.map (fun xy -> (xy, xy |> enumeratePossibleMaxLinks state |> Array.ofSeq))
        |> Seq.cache

    let linksOption =
        match possibleMaxLinks |> Seq.tryPick tryPutFor with
        | None when state.NodesOpen.Count > 2 ->
            possibleMaxLinks
            |> Seq.tryPick (fun (xy, possibleLinks) ->
                match state.Grid |> asIntAt xy with
                | 1 -> tryPutFor (xy, possibleLinks |> Seq.filter (fun (xy', _) -> state.Grid |> asIntAt xy' <> 1) |> Array.ofSeq)
                | 2 -> tryPutFor (xy, possibleLinks |> Seq.map (fun (xy', _) -> (xy', if state.Grid |> asIntAt xy' = 2 then 1 else 2)) |> Array.ofSeq)
                | _ -> None)
        | x -> x

    match linksOption with
    | None -> ()
    | Some links -> 
        state
        |> addLinks links
        |> playLinks

let rec solve state =
    playLinks state

    if state.NodesOpen.Count = 0 then Some state
    else
        state.NodesOpen
        |> Seq.sortByDescending (fun xy -> state.Grid |> valueAt xy)
        |> Seq.collect (fun xy ->
            xy
            |> enumeratePossibleMaxLinks state
            |> Seq.collect (fun (xy', maxAmount) -> seq {maxAmount .. -1 .. 1} |> Seq.map (fun i -> (xy, [|xy', i|]))))
        |> Seq.tryPick (fun l -> state |> copy |> addLinks l |> solve)

let gridInput = Array.init height (fun _ -> readString())
let grid = Array2D.init height width (fun i j -> gridInput.[i].[j])

{
    Grid = grid
    Links = Dictionary<(int * int) * (int * int), int>()
    NodesOpen = HashSet<int * int>(grid |> enumerateCells isNode)
}
|> solve
|> Option.get
|> (fun s -> s.Links)
|> Seq.map (fun kvp -> (|KeyValue|) kvp)
|> Seq.iter (fun (((x, y), (x', y')), amount) -> printfn "%d %d %d %d %d" x y x' y' amount)
