module Matrosov.Codingame.Hard.Surface

open System

let readString() = Console.ReadLine()
let readInt = readString >> int

let width = readInt()
let height = readInt()

let grid = Array.init height (fun _ -> readString())

let surface (x, y) =
    let queued = Array2D.create height width false
    queued.[y,x] <- true

    let neighbours x y =
        seq {yield (x+1,y); yield (x-1,y); yield (x,y+1); yield (x,y-1)}
        |> Seq.filter (fun (x, y) -> x >= 0 && x < width && y >= 0 && y < height && not queued.[y,x])
        |> Seq.map (fun (x, y) -> queued.[y,x] <- true; (x, y))
        |> List.ofSeq

    let rec visit queue surface =
        match queue with
        | [] -> surface
        | (x, y) :: tail ->
            match grid.[y].[x] with
            | 'O' -> visit ((neighbours x y) @ tail) (surface+1) 
            | _ -> visit tail surface

    visit [(x, y)] 0

for _ in 1 .. (readInt()) do
    let coordinates = readString().Split [|' '|] |> Array.map int |> (fun a -> (a.[0], a.[1]))
    printfn "%d" (surface coordinates)
