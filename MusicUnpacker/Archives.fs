module Archives
open System.IO
open System.IO.Compression
open Types

let createFileStream (x : FileInfo) = 
    try
        if x.FullName.Contains "zip" then
            new FileStream(x.FullName, System.IO.FileMode.Open) |> Success
        else
            sprintf "File %s is not a zip archive" x.FullName |> IncorrectFileType |> Error
    with
    //always downcasting here because we don't care to juggle a dozen different exception types
    | :? System.Exception as e ->
        sprintf "Exception opening file %s stream: %s" x.FullName e.Message |> FileStreamError |> Error

let createZipArchive (x : FileStream) = 
    try 
        new System.IO.Compression.ZipArchive(x) |> Success
    with
    //always downcasting here because we don't care to juggle a dozen different exception types
    | :? System.Exception as e ->
        x.Close()
        sprintf "Exception opening zip archive %s : %s" x.Name e.Message |> ZipArchiveError |> Error

// let archiveContainsMp3s (a : ZipArchive) =
//     let isMp3 (x : ZipArchiveEntry) = x.FullName.Contains(".mp3")
//     match Seq.tryFind isMp3 a.Entries with
//     | Some e ->
//         match generateAlbumInfo e.Name with
//         | Success albumInfo -> Success a
//         | Error e -> Error e
//     | None -> 
//         a.Dispose()
//         "Archive does not contain mp3s" |> ZipArchiveError |> Error

// let filterToSongInfo (a : ZipArchive) =
//     let isMp3 (x : ZipArchiveEntry) = x.FullName.Contains(".mp3")


//     match List.filter isMp3 (Seq.toList a.Entries) with
//     | [] ->
//         a.Dispose()
//         "Archive does not contain mp3s" |> ZipArchiveError |> Error
//     | xs->
//         //at least one entry is an MP3, so convert all to song infos
//         let s = Seq.map (fun (x : ZipArchiveEntry) -> generateSongInfo x.FullName) (xs)
//         (s, xs) |> Success

// let filterToMp3ArchiveEntries (a : ZipArchive) =
//     let entries = Seq.toList a.Entries
//     let isMp3 (x : ZipArchiveEntry) = x.FullName.Contains(".mp3")

//     let createArchiveEntryList (al : ZipArchiveEntry list) =
//         let processEntry (x : ZipArchiveEntry) =
//             match isMp3 x, generateSongInfo x.Name with
//             | true, Success s ->
//                 Mp3 (x, s) |> Success
//             | false, Error e ->
//                 NonMp3File x |> Success
//             | true, Error e ->
//                 Error e
//             | _, _ ->
//                 ZipArchiveError "Unexpected case when processing Zip entry" |> Error                
//         List.map processEntry al

//     match List.tryFind isMp3 entries with
//     | Some _ ->
//         createArchiveEntryList entries
//     | None ->
//         ["Archive does not contain mp3s" |> ZipArchiveError |> Error]

let tryGetBandAndAlbum (l : (string * string * string * ZipArchiveEntry) option list) =
    match List.choose id l with
    | (x, y, _, _) :: _ -> //at least 1 result
        Some (x, y)
    | [] -> None

let tryCreateZipExtractionSet ( a : ZipArchive ) =
    let entries = Seq.toList a.Entries
    let isMp3 (x : ZipArchiveEntry) = x.FullName.Contains(".mp3")    

    match List.tryFind isMp3 entries with 
    | Some _ -> //there is at least 1 mp3
        let splits = List.map (fun (x : ZipArchiveEntry) -> trySplit (x.Name, x)) entries
        match tryGetBandAndAlbum splits with
        | Some (b, a) ->
            let details = List.choose id splits |> List.map (fun (b, a, s, e) -> { entry = e; fileName = s })
            //there may be additional file (such as "cover.jpg") that need to be unzipped
            let additionalFiles = List.filter (fun x -> (isMp3 >> not) x) entries 
                                  |> List.map (fun x -> { entry = x; fileName = x.Name })
            { band = b; album = a; entries = (List.append details additionalFiles)}
            |> Success
        | None ->
            "Archive contains mp3s, but no file names were in the correct format" |> ZipArchiveError |> Error
    | None ->
        a.Dispose()
        "Archive does not contain mp3s" |> ZipArchiveError |> Error


let getMp3ArchivesInDirectory s =
    let processFileInfo (x : FileInfo) = 
        createFileStream x
        >>= createZipArchive
        >>= tryCreateZipExtractionSet 
    let dir = DirectoryInfo(s)
    dir.GetFiles() |> Seq.map processFileInfo
