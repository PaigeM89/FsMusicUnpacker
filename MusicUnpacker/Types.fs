module Types

type ErrorType =
| FileStreamError of string
| ZipArchiveError of string
| FileNameError of string
| IncorrectFileType of string
| UnpackingError of string
| Other of string

type Result<'a> = 
    | Success of 'a
    | Error of ErrorType

    static member Bind f m = 
        match m with
        | Success a -> f a
        | Error e -> Error e
        
    static member (>>=) (m, f) = Result<_>.Bind f m

    static member BindList f m =
        match m with 
        | Success a -> f a
        | Error e -> [Error e]

    static member (>>=) (m, f) = Result<_>.BindList f m
    static member Return x = Success x

// type SongInfo = { band : string; album : string; song : string }

// //type annotation is needed for the String class calls in function
// let generateSongInfo (s : string) =
//     let splitChars =['-'] |> Array.ofList
//     let afterSplit = s.Split(splitChars, 3) |> Array.map (fun x -> x.Trim())
//     match afterSplit with
//     | x when Array.length x = 3 ->
//         { band = (Array.item 0 afterSplit); album = (Array.item 1 afterSplit); song = (Array.item 2 afterSplit)} 
//         |> Success
//     | _ -> 
//         "Unable to generate names from file " + s |> FileNameError |> Error


// type AlbumInfo = { band : string; album : string }

// let generateAlbumInfo (s : string) =
//     let splitChars =['-'] |> Array.ofList
//     let afterSplit = s.Split(splitChars, 3) |> Array.map (fun x -> x.Trim())
//     match afterSplit with
//     | x when Array.length x = 3 ->
//         { band = (Array.item 0 afterSplit); album = (Array.item 1 afterSplit)} 
//         |> Success
//     | _ -> 
//         "Unable to generate names from file " + s |> FileNameError |> Error

open System.IO.Compression

// type ArchiveEntry = 
// | Mp3 of (ZipArchiveEntry * SongInfo)
// | NonMp3File of (ZipArchiveEntry)

type UnzipDetails = { entry : ZipArchiveEntry; fileName : string }

//this is it br0
type ZipExtractionSet = { band : string; album : string; entries : UnzipDetails list }

let trySplit (s : string, e : ZipArchiveEntry) =
    let splitChars =['-'] |> Array.ofList
    let afterSplit = s.Split(splitChars, 3) |> Array.map (fun x -> x.Trim())
    match afterSplit with
    | x when Array.length x = 3 ->
        Some (Array.item 0 afterSplit, Array.item 1 afterSplit, Array.item 2 afterSplit, e)
    | _ -> 
        None
