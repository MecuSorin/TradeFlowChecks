module ThreeStateCheckboxApp
open Elmish
open Avalonia
open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Avalonia.Media
open Avalonia.FuncUI.Hosts
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Themes.Fluent
open Avalonia.FuncUI.Elmish
open Avalonia.Platform
open System

// Define the checkbox state
type CheckboxState =
    | NotPressed
    | Validated
    | Invalidated

// Define the model
type Phrase = {
    Text: string
    State: CheckboxState
}

type Model = {
    Phrases: Phrase list
}

// Define messages
type Msg =
    | ToggleState of index: int
    | Reset

let mutable phrasesFile = "phrases.txt"
// Initialize the model with some sample phrases
let init () =
    let phrases =
        if System.IO.File.Exists phrasesFile then
            System.IO.File.ReadAllLines phrasesFile
                |> Array.toList
                |> List.filter (System.String.IsNullOrWhiteSpace >> not)
                |> List.map (fun text -> { Text = text.Trim(); State = NotPressed })
        else
            [
                { Text = "HTF POI"; State = NotPressed }
                { Text = "HTF direction bias"; State = NotPressed }
                { Text = "Clear target pro HTF trend"; State = NotPressed }
                { Text = "Clear target against your trade idea"; State = NotPressed }
                { Text = "Price behavior changed"; State = NotPressed }
                { Text = "HTF liquidity was taken"; State = NotPressed }
                { Text = "Entry TF liquidity was taken"; State = NotPressed }
            ]
    { Phrases = phrases }, Cmd.none

// Update function
let update msg model =
    match msg with
    | ToggleState index ->
        let updatedPhrases = 
            model.Phrases 
            |> List.mapi (fun i phrase -> 
                if i = index then
                    let newState = 
                        match phrase.State with
                        | NotPressed -> Invalidated
                        | Validated -> NotPressed
                        | Invalidated -> Validated
                    { phrase with State = newState }
                else
                    phrase
            )
        { model with Phrases = updatedPhrases }, Cmd.none
    | Reset ->
        let resetPhrases = 
            model.Phrases 
            |> List.map (fun phrase -> { phrase with State = NotPressed })
        { model with Phrases = resetPhrases }, Cmd.none

// View function to render a single checkbox
let viewCheckbox dispatch index phrase =
    let foregroundColor, textDecoration, checkSymbol =
        match phrase.State with
        | NotPressed -> Brushes.DarkGray, TextDecorationCollection(), "□"
        | Validated -> Brushes.Lime, TextDecorationCollection(), "■"
        | Invalidated -> 
            let textDecoration = TextDecoration()
            textDecoration.Location <- TextDecorationLocation.Strikethrough
            textDecoration.StrokeThicknessUnit <- TextDecorationUnit.Pixel
            textDecoration.StrokeThickness <- 1.0
            Brushes.Red, TextDecorationCollection [textDecoration], "■"
     
    StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.margin (Thickness(0, 5))
        StackPanel.minWidth 350
        StackPanel.background Brushes.Transparent   // to trigger the pointer event outside the text also
        StackPanel.onPointerPressed (fun _ -> dispatch (ToggleState index))
        StackPanel.children [
            TextBlock.create [
                TextBlock.text checkSymbol
                TextBlock.width 20
                TextBlock.foreground foregroundColor
                TextBlock.fontSize 16
                TextBlock.verticalAlignment VerticalAlignment.Center
            ]
            TextBlock.create [
                TextBlock.text phrase.Text
                TextBlock.foreground foregroundColor
                TextBlock.textDecorations textDecoration
                TextBlock.fontSize 20
                TextBlock.verticalAlignment VerticalAlignment.Center
            ]
        ]
    ]

// Main view function
let view model dispatch =
    DockPanel.create [
        DockPanel.children [
            // Header
            TextBlock.create [
                TextBlock.dock Dock.Top
                TextBlock.text "Stay with the Flow !"
                TextBlock.fontSize 20
                TextBlock.fontWeight FontWeight.Bold
                TextBlock.margin (Thickness 10)
                TextBlock.horizontalAlignment HorizontalAlignment.Center
            ]
            
            // Reset button at the bottom
            Button.create [
                Button.dock Dock.Bottom
                Button.content "Reset"
                Button.fontSize 20
                Button.onClick (fun _ -> dispatch Reset)
                Button.horizontalAlignment HorizontalAlignment.Center
                Button.margin (Thickness(0, 10))
            ]
            
            // Checkboxes in the middle
            ScrollViewer.create [
                ScrollViewer.dock Dock.Top
                ScrollViewer.content (
                    StackPanel.create [
                        StackPanel.margin (Thickness 20)
                        StackPanel.children (
                            model.Phrases 
                            |> List.mapi (fun i phrase -> viewCheckbox dispatch i phrase)
                        )
                    ]
                )
            ]
        ]
    ]

// Define the window
type MainWindow() as this =
    inherit HostWindow()
    do
        base.Title <- "Mecu Trade Flow"
        base.Width <- 400.0
        base.Height <- 450.0    // adjust height based on the number of phrases you are displaying
        // Initialize Elmish program
        Program.mkProgram init update view
            |> Program.withHost this
            |> Program.run
    // Adjust the window position to the left right corner on the seconds screen (overflow the primary screen size)
    // override this.OnLoaded (e: Interactivity.RoutedEventArgs): unit =
    //     base.OnLoaded(e: Interactivity.RoutedEventArgs)
    //     let screen = this.Screens.Primary
    //     let screenSize = screen.WorkingArea.Size
    //     let windowSize = PixelSize.FromSize(this.ClientSize, screen.Scaling)
    //     this.Position <- PixelPoint(screenSize.Width + 5, screenSize.Height - windowSize.Height - 45)


// Entry point
type App() =
    inherit Application()

    override this.Initialize() =
        this.Styles.Add(FluentTheme())
        this.RequestedThemeVariant <- Styling.ThemeVariant.Dark

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            let mainWindow = MainWindow()
            
            // Set the window icon
            let iconUri = Uri("avares://CheckList/Resources/icon.ico")
            let asset = AssetLoader.Open(iconUri)
            if asset <> null then
                mainWindow.Icon <- WindowIcon(asset)
            desktopLifetime.MainWindow <- mainWindow
        | _ -> ()

module Program =
    [<EntryPoint>]
    let main args =
        if args.Length = 1 && System.IO.File.Exists args.[0] 
            then phrasesFile <- args.[0]
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .UseSkia()
            .StartWithClassicDesktopLifetime(args)