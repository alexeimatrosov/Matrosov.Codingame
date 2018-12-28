module Matrosov.Codingame.FSharp.Easy.SkynetTheChasm

open System

let readString() = Console.ReadLine();
let readInt = readString >> int

let road = readInt()
let gap = readInt()
let platform = readInt()

let getAction (speed, x) =
    if x < road && speed <= gap then "SPEED"
    elif x = road - 1 then "JUMP"
    elif x > road + gap - 1 || speed > gap + 1 then "SLOW"
    else "WAIT"

while true do
    (readInt(), readInt())
    |> getAction
    |> printfn "%s"
