module MusicUnpacker

[<EntryPoint>]
let main argv =
    printfn "%A" argv
    printfn "hello world!"

    let r = Archives.getMp3ArchivesInDirectory "C:/Test"

    let printResult r = printfn "%A" r
    Seq.iter printResult r

    FilePlacer.test()

    0 // return an integer exit code
