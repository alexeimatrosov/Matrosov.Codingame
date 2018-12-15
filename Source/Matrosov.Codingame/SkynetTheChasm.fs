module Matrosov.Codingame.SkynetTheChasm

open System

let readString() = Console.In.ReadLine();
let readInt = readString >> int

let road, gap, platform = readInt(), readInt(), readInt()

let getAction speed coordX =
    if coordX < road && speed <= gap then
        "SPEED"
    elif coordX = road - 1 then
        "JUMP"
    elif coordX > road + gap - 1 || speed > gap + 1 then
        "SLOW"
    else
        "WAIT"

while true do
    printfn "%s" (getAction (readInt()) (readInt()))