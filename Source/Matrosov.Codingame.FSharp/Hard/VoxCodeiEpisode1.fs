module Matrosov.Codingame.FSharp.Hard.VoxCodeiEpisode1

open System

type GameState = {
    Grid : char[,]
    Turns : (int * int) option list
    NodesOpen : Set<int * int>
    NodesClosed : Set<int * int>
    Bombs : Set<int * int>
    RoundsLeft : int
    BombsLeft : int
}

let readString() = Console.ReadLine()
let readInt = readString >> int
let readIntPair() = readString().Split [|' '|] |> Array.map int |> (fun a -> (a.[0], a.[1]))

let (width, height) = readIntPair()
let gridInput = Array.init height (fun _ -> readString())
let grid = Array2D.init height width (fun i j -> gridInput.[i].[j])

let (roundsLimit, bombsLimit) = readIntPair()

let inline isEmpty c = c = '.'
let inline isNode c = c = '@'
let inline isBlock c = c = '#'
let inline isTriggeredBomb c = c = '0'

let enumerateCells predicate (grid : char[,]) =
    seq {
        for y = 0 to height-1 do
            for x = 0 to width-1 do
                if predicate grid.[y,x] then yield (x, y)
    }

let enumerateBlastingCells (x, y) (grid : char[,]) =
    let rec aux x y dx dy xLimit yLimit =
        seq {
            if 0 <= x && x < width && 0 <= y && y < height && grid.[y,x] |> isBlock |> not
            then
                yield (x, y)
                if x <> xLimit || y <> yLimit then yield! aux (x+dx) (y+dy) dx dy xLimit yLimit
        }
    seq {
        yield! aux (x+1) y +1 0 (x+3) y
        yield! aux (x-1) y -1 0 (x-3) y
        yield! aux x (y+1) 0 +1 x (y+3)
        yield! aux x (y-1) 0 -1 x (y-3)
    }

let blastingCellsCache =
    grid
    |> enumerateCells (fun _ -> true)
    |> Seq.map (fun xy -> (xy, grid |> enumerateBlastingCells xy |> Array.ofSeq))
    |> Map.ofSeq

let play turn state =
    let inline cell (x, y) = state.Grid.[y,x]

    let inline tick c = match c with
                        | '3' -> '2'
                        | '2' -> '1'
                        | '1' -> '0'
                        | x -> x

    let inline blast c = match c with
                         | '3' | '2' | '1' -> '0'
                         | '@' -> '.'
                         | x -> x

    let putBomb (grid : char[,]) bombs nodesOpen nodesClosed =
        match turn with
        | None -> (bombs, nodesOpen, nodesClosed)
        | Some (x, y) -> 
            grid.[y,x] <- '3'
            let bombs = bombs |> Set.add (x, y)
            let nodesClosing = blastingCellsCache.[(x, y)] |> Seq.filter (cell >> isNode) |> Set.ofSeq
            let nodesOpen = Set.difference nodesOpen nodesClosing
            let nodesClosed = Set.union nodesClosed nodesClosing
            (bombs, nodesOpen, nodesClosed)

    let tickBombs (grid : char[,]) bombs =
        bombs |> Set.iter (fun (x, y) -> grid.[y,x] <- grid.[y,x] |> tick)

    let rec explodeBombs (grid : char[,]) bombs nodesClosed =
        let triggeredBombs = bombs |> Seq.filter (cell >> isTriggeredBomb) |> Set.ofSeq
        if triggeredBombs.Count = 0 then (bombs, nodesClosed)
        else
            let blastedCells =
                triggeredBombs
                |> Seq.map (fun xy -> blastingCellsCache.[xy])
                |> Seq.concat
                |> Seq.cache

            let blastedNodes =
                blastedCells
                |> Seq.filter (cell >> isNode)
                |> Set.ofSeq

            triggeredBombs |> Seq.iter (fun (x, y) -> grid.[y,x] <- '.')
            blastedCells |> Seq.iter (fun (x, y) -> grid.[y,x] <- grid.[y,x] |> blast)

            explodeBombs grid (Set.difference bombs triggeredBombs) (Set.difference nodesClosed blastedNodes)

    let gridCopy = state.Grid |> Array2D.copy
    let (bombs, nodesOpen, nodesClosed) = putBomb gridCopy state.Bombs state.NodesOpen state.NodesClosed
    tickBombs gridCopy bombs
    let (bombs, nodesClosed) = explodeBombs gridCopy bombs nodesClosed

    {
        Grid = gridCopy
        Turns = turn :: state.Turns
        NodesOpen = nodesOpen
        NodesClosed = nodesClosed
        Bombs = bombs
        RoundsLeft = state.RoundsLeft - 1
        BombsLeft = state.BombsLeft - (if turn |> Option.isSome then 1 else 0)
    }

let rec solve state =
    if state.NodesOpen.Count = 0 && state.NodesClosed.Count = 0 then Some state.Turns
    elif state.RoundsLeft = 0 then None
    else
        seq {
            if state.BombsLeft <> 0 && state.NodesOpen.Count > 0
            then
                let emptyCellsClosingNodes =
                    state.NodesOpen
                    |> Seq.map (fun xy -> blastingCellsCache.[xy] |> Seq.filter (fun (x, y) -> state.Grid.[y,x] |> isEmpty))
                    |> Seq.concat
                    |> Seq.distinct

                let emptyCellsWithUniqueNodesClosing =
                    emptyCellsClosingNodes
                    |> Seq.map (fun xy -> (xy, blastingCellsCache.[xy] |> Seq.filter (fun xy -> state.NodesOpen |> Set.contains xy) |> Set.ofSeq))
                    |> Seq.distinctBy snd
                    |> Seq.sortByDescending (snd >> Set.count)
                    |> Seq.map fst

                yield! emptyCellsWithUniqueNodesClosing
                       |> Seq.map (fun xy -> Some xy)

            if state.NodesClosed.Count > 0 then yield None
        }
        |> Seq.tryPick (fun t -> state |> play t |> solve)

{
    Grid = grid
    Turns = []
    NodesOpen = grid |> enumerateCells isNode |> Set.ofSeq
    NodesClosed = Set.empty
    Bombs = Set.empty
    RoundsLeft = roundsLimit
    BombsLeft = bombsLimit
}
|> solve
|> Option.get
|> Seq.append [|None|]
|> Seq.rev
|> Seq.iter (fun t ->
    match t with
    | Some (x, y) -> printfn "%d %d" x y
    | None -> printfn "WAIT")
