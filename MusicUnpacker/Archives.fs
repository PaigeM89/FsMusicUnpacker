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

let archiveContainsMp3s (a : ZipArchive) =
    let isMp3 (x : ZipArchiveEntry) = x.FullName.Contains(".mp3")
    match Seq.exists isMp3 a.Entries with
    | true -> 
        Success a
    | false -> 
        a.Dispose()
        "Archive does not contain mp3s" |> ZipArchiveError |> Error

//binding. note the change to the "contains MP3s" function for this to work
let getMp3ArchivesInDirectory s =
    let processFileInfo (x : FileInfo) = 
        createFileStream x
        >>= createZipArchive
        >>= archiveContainsMp3s
    let dir = DirectoryInfo(s)
    dir.GetFiles() |> Seq.map processFileInfo
