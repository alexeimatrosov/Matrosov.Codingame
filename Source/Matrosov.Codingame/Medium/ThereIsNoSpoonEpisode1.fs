module Matrosov.Codingame.Medium.ThereIsNoSpoonEpisode1

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let width = readInt()
let height = readInt()

let readRow() = readString()
                |> Seq.map (fun c -> if c = '0' then 1 else 0)
                |> Seq.toArray

let field =
    seq { for _ in 1 .. height do yield readRow() }
    |> Seq.toArray

let rec findNeighbour dx dy x y =
    let xn = x + dx
    let yn = y + dy
    if xn = width || yn = height
    then (-1, -1)
    else
        match field.[yn].[xn] with
        | 0 -> findNeighbour dx dy xn yn
        | _ -> (xn, yn)

let findRightNeighbour = findNeighbour 1 0
let findBottomNeighbour = findNeighbour 0 1

for y = 0 to height-1 do
    for x = 0 to width-1 do
        match field.[y].[x] with
        | 0 -> ()
        | _ -> 
            let (xRight, yRight) = findRightNeighbour x y 
            let (xBottom, yBottom) = findBottomNeighbour x y
            printfn "%d %d %d %d %d %d" x y xRight yRight xBottom yBottom
