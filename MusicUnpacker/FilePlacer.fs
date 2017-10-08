module FilePlacer
    open Types
    open System.IO
    open System.IO.Compression

    let subDirectoryExists (s : string) (d : DirectoryInfo) =
        let equals (x : DirectoryInfo) = x.Name.ToLowerInvariant() = s.ToLowerInvariant()
        d.GetDirectories() |> Array.tryFind equals

    let getOrCreateSubDirectory (s : string) ( d : DirectoryInfo) =
        match subDirectoryExists s d with
        | Some x -> x
        | None -> d.CreateSubdirectory(s)

    // let (|FileIsMp3|FileIsOtherType|) (s : string) =
    //     match s.Contains(".mp3") with
    //     | true -> FileIsMp3
    //     | false -> FileIsOtherType
    
    // let extractZipEntry (targetDir : DirectoryInfo) (files : string List) (entry : ArchiveEntry) =
    //     let fileExists s = List.exists (fun (x : string) -> x.Contains(s)) files
    //     let fileExistsMatch (e : ArchiveEntry) =
    //         match e with 
    //         | Mp3 (zae, si) -> 
    //             fileExists zae.Name
    //         | NonMp3File zae ->
    //             fileExists zae.Name

    //     let checkExistsOrExtract (z : ZipArchiveEntry) (s : string) =
    //         match fileExists z.Name with
    //         | true ->
    //             "File already exists for " + z.Name
    //         | false ->
    //             z.ExtractToFile(targetDir.FullName + "\\" + s)
    //             s + " extracted successfully"

    //     match entry with
    //     | Mp3 (z, s) ->
    //         checkExistsOrExtract z s.song |> Success
    //     | NonMp3File z ->
    //         checkExistsOrExtract z z.Name |> Success


    // let extractZipArchive (a : ArchiveEntry list) (d : DirectoryInfo) =
    //     let existingFiles = d.GetFiles()
    //     let existingFileNames = Array.map (fun (x : FileInfo) -> x.Name) existingFiles |> List.ofArray    
    //     Seq.map (extractZipEntry d existingFileNames) a

    // let unpackArchives (a : ArchiveEntry list) (target : string) =
    //     let dir = DirectoryInfo(s)
    //     let grouped = Seq.groupBy (fun x -> x.band) 

    let processZipExtractionSet (dir : DirectoryInfo) (z : ZipExtractionSet)  =
        let finalDirectory = getOrCreateSubDirectory z.band dir |> getOrCreateSubDirectory z.album
        let extract (e : UnzipDetails)  =
            try
                e.entry.ExtractToFile(finalDirectory.FullName + "\\" + e.fileName)
                sprintf "%s extracted successfully to %s" e.fileName finalDirectory.FullName
                |> Success
            with
            | :? System.Exception as ex ->
                sprintf "Exception unzipping entry %s: %s" e.fileName ex.Message 
                |> UnpackingError |> Error

        Seq.map extract z.entries


    // let processZipExtractionSets (target : string) (sets : ZipExtractionSet list)  =
    //     try 
    //         let dir = DirectoryInfo(target)
    //         Seq.collect (processZipExtractionSet dir) sets
    //     with 
    //     | :? System.IO.DirectoryNotFoundException as e ->
    //         let x = sprintf "Target Directory not found: %s" target |> UnpackingError
    //         [Error x] |> Seq.ofList
    //     | :? System.Exception as e ->
    //         let x = sprintf "Error while unpacking: %s" e.Message |> UnpackingError
    //         [Error x] |> Seq.ofList