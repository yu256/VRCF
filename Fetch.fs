module Fetch

open System.Net.Http
open System.Text.Json
open Avalonia.Threading

let inline startAsync action _ =
    Dispatcher.UIThread.Post((fun () -> action |> Async.Start), DispatcherPriority.Background)

let sprintResponse =
    function
    | Ok ok -> ok
    | Error(body, Some statusCode) -> sprintf "statusCode: %A %s" statusCode body
    | Error(body, None) -> body

let postAsync (client: HttpClient) (url: string) body =
    async {
        try
            use content = new StringContent(body)
            let! response = client.PostAsync(url, content) |> Async.AwaitTask
            let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask

            return
                if response.IsSuccessStatusCode then
                    Ok body
                else
                    Error(body, Some response.StatusCode)
        with ex ->
            return Error(ex.Message, None)
    }

type Friend =
    { currentAvatarThumbnailImageUrl: string
      userIcon: string option
      profilePicOverride: string option
      id: string
      status: string
      location: string
      undetermined: bool }

type ResFriend =
    { ``public``: Friend list
      ``private``: Friend list }

type Response<'a> = { Success: 'a }

let deserialize (str: string) =
    try
        JsonSerializer.Deserialize str |> Some
    with _ ->
        // todo bodyをモーダルで表示する
        None
