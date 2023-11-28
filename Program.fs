#nowarn "3570" "25"
namespace VRCF

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Hosts
open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open Avalonia.Layout

module Main =
    let fallbackImage = new Media.Imaging.Bitmap("")

    let view () =
        Component(fun ctx ->
            let httpClient = new System.Net.Http.HttpClient()
            let client =  Fetch.postAsync httpClient ""
            let token = ctx.useState ""
            let state = ctx.useState None

            let image ({ Success = { ``public`` = pub; ``private`` = pri } }: Fetch.Response<Fetch.ResFriend>) =
                Component.create ("friends", fun ctx ->
                    let imageComponent link =
                        let image = ctx.useState None
                        ctx.useEffect (
                            handler = (
                                async {
                                    let! img = Cache.downloadImage httpClient link
                                    img |> image.Set
                                }
                                |> Fetch.startAsync
                            ),
                            triggers = [ EffectTrigger.AfterInit ]
                        )
                        Image.source (image.Current |> Option.defaultValue fallbackImage)

                    let [pub; pri] =
                        [pub; pri]
                        |> List.map (List.map _.currentAvatarThumbnailImageUrl >> List.map imageComponent)

                    DockPanel.create [
                        DockPanel.children [
                            Image.create pub
                            Image.create pri
                        ]
                    ]
                )

            DockPanel.create [
                DockPanel.children [
                    Button.create [
                        Button.dock Dock.Bottom
                        Button.onClick (async {
                            let! res = client token.Current
                            Fetch.sprintResponse >> Fetch.deserialize >> state.Set <| res
                        }
                        |> Fetch.startAsync)
                        Button.content "fetch"
                        Button.horizontalAlignment HorizontalAlignment.Stretch
                        Button.horizontalContentAlignment HorizontalAlignment.Center
                    ]
                    match state.Current with | Some a -> image a | None -> ()
                    TextBox.create [
                        TextBox.text token.Current
                        TextBox.onTextChanged token.Set
                        TextBox.name "set token"
                    ]
                ]
            ]
        )

type MainWindow() =
    inherit HostWindow()
    do
        base.Title <- "VRCF"
        base.Content <- Main.view ()

type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add (FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Dark

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow()
        | _ -> ()

module Program =

    [<EntryPoint>]
    let main args =
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)
