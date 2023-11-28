module Cache

open System.Net.Http
open System.IO
open System.Security.Cryptography
open System

[<Literal>]
let CACHE_DIR = "ImageCache"

let toBitMap (imageData: string) =
    new Avalonia.Media.Imaging.Bitmap(imageData)

let downloadImage (client: HttpClient) (url: string) =
    async {
        let hash = MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes url)

        Directory.CreateDirectory CACHE_DIR |> ignore

        let fileName = BitConverter.ToString(hash).Replace("-", "")
        let filePath = Path.Combine(CACHE_DIR, fileName)

        let toBitMap = toBitMap >> Some

        if File.Exists filePath then
            return filePath |> toBitMap
        else
            let! res = client.GetAsync url |> Async.AwaitTask
            let! stream = res.Content.ReadAsStreamAsync() |> Async.AwaitTask

            stream.CopyTo(File.Create filePath)

            return filePath |> toBitMap
    }
