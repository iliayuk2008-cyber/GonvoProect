using System;
using System.Threading.Tasks;
using GonvoProect.Modles;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace GonvoProect.Views;

public partial class MainPage : ContentPage
{
    const int SIZE = 50;
    const int CELL_SIZE = 44;

    // The symbols nearly fill their cells.
    const double SYMBOL_FONT_SIZE = 37;

    static readonly Color BackgroundCell =
        Color.FromArgb("#131D35");

    static readonly Color NormalCellBorder =
        Color.FromArgb("#35466F");

    // Player: red crosses.
    static readonly Color CrossColor =
        Color.FromArgb("#FF416C");

    // Bot: blue circles.
    static readonly Color CircleColor =
        Color.FromArgb("#36A3FF");

    // Last bot move.
    static readonly Color LastMoveBackground =
        Color.FromArgb("#34245C");

    static readonly Color LastMoveBorder =
        Color.FromArgb("#A78BFA");

    // Winning five.
    static readonly Color WinningBackground =
        Color.FromArgb("#164E63");

    static readonly Color WinningBorder =
        Color.FromArgb("#67E8F9");

    Desk desk = new Desk();

    Label[,] symbols = new Label[SIZE, SIZE];
    Border[,] cells = new Border[SIZE, SIZE];

    bool botThinking = false;
    bool gameOver = false;

    int lastBotRow = -1;
    int lastBotColumn = -1;

    double startScrollX;
    double startScrollY;

    public MainPage()
    {
        InitializeComponent();

        CreateBoard();

        PanGestureRecognizer pan = new PanGestureRecognizer();
        pan.PanUpdated += OnPanUpdated;

        Board.GestureRecognizers.Add(pan);

        Loaded += OnPageLoaded;
    }

    // =============================================
    // INITIAL PAGE POSITION
    // =============================================

    async void OnPageLoaded(object? sender, EventArgs e)
    {
        await Task.Delay(100);

        double x = (Board.Width - Scroll.Width) / 2;
        double y = (Board.Height - Scroll.Height) / 2;

        if (x < 0)
            x = 0;

        if (y < 0)
            y = 0;

        await Scroll.ScrollToAsync(x, y, false);
    }

    // =============================================
    // CREATE THE BOARD
    // =============================================

    void CreateBoard()
    {
        // Create rows.
        for (int i = 0; i < SIZE; i++)
        {
            Board.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = CELL_SIZE
                });
        }

        // Create columns.
        for (int i = 0; i < SIZE; i++)
        {
            Board.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = CELL_SIZE
                });
        }

        // Create cells.
        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                Border cell = new Border
                {
                    BackgroundColor = BackgroundCell,
                    Stroke = NormalCellBorder,
                    StrokeThickness = 1,
                    Padding = 0,

                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = 5
                    }
                };

                Label symbol = new Label
                {
                    Text = "",
                    FontSize = SYMBOL_FONT_SIZE,
                    FontAttributes = FontAttributes.Bold,

                    HorizontalTextAlignment =
                        TextAlignment.Center,

                    VerticalTextAlignment =
                        TextAlignment.Center,

                    HorizontalOptions =
                        LayoutOptions.Fill,

                    VerticalOptions =
                        LayoutOptions.Fill,

                    TextColor = Colors.White,

                    Padding = 0,
                    Margin = 0,

                    LineBreakMode = LineBreakMode.NoWrap
                };

                symbols[row, column] = symbol;
                cells[row, column] = cell;

                cell.Content = symbol;

                TapGestureRecognizer tap =
                    new TapGestureRecognizer();

                int r = row;
                int c = column;

                tap.Tapped += async (sender, e) =>
                {
                    // Ignore clicks while the game is over
                    // or the bot is thinking.
                    if (gameOver || botThinking)
                        return;

                    // Do not allow occupied cells.
                    if (desk.Board[r, c] != 0)
                        return;

                    // =================================
                    // PLAYER'S MOVE
                    // =================================

                    desk.Board[r, c] = 1;
                    desk.AddMoves(r, c);

                    // Red cross.
                    symbols[r, c].Text = "✕";
                    symbols[r, c].TextColor = CrossColor;

                    // Check whether the player won.
                    if (desk.IsFive(1))
                    {
                        gameOver = true;

                        HighlightWinningFive(1);

                        await Task.Delay(100);

                        await ShowWinImage();
                        return;
                    }

                    // =================================
                    // BOT'S MOVE
                    // =================================

                    botThinking = true;

                    (int, int) move;

                    try
                    {
                        move = await Task.Run(
                            () => desk.BestMove(-1));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);

                        botThinking = false;
                        gameOver = true;

                        await DisplayAlert(
                            "Error",
                            "The bot encountered an error. The game will restart.",
                            "OK");

                        RestartGame();
                        return;
                    }

                    int botRow = move.Item1;
                    int botColumn = move.Item2;

                    // Validate the bot's move.
                    if (!desk.InBoard(botRow, botColumn) ||
                        desk.Board[botRow, botColumn] != 0)
                    {
                        botRow = -1;
                        botColumn = -1;

                        // Find an empty cell if the move is invalid.
                        for (int i = 0;
                             i < SIZE && botRow == -1;
                             i++)
                        {
                            for (int j = 0; j < SIZE; j++)
                            {
                                if (desk.Board[i, j] == 0)
                                {
                                    botRow = i;
                                    botColumn = j;
                                    break;
                                }
                            }
                        }
                    }

                    // Draw if there are no empty cells.
                    if (botRow == -1)
                    {
                        gameOver = true;
                        botThinking = false;

                        await DisplayAlert(
                            "Draw",
                            "There are no empty cells left.",
                            "OK");

                        RestartGame();
                        return;
                    }

                    // =================================
                    // REMOVE OLD BOT HIGHLIGHT
                    // =================================

                    if (lastBotRow != -1 &&
                        lastBotColumn != -1)
                    {
                        cells[lastBotRow, lastBotColumn]
                            .BackgroundColor = BackgroundCell;

                        cells[lastBotRow, lastBotColumn]
                            .Stroke = NormalCellBorder;

                        cells[lastBotRow, lastBotColumn]
                            .StrokeThickness = 1;
                    }

                    // =================================
                    // PLACE BOT'S CIRCLE
                    // =================================

                    desk.Board[botRow, botColumn] = -1;
                    desk.AddMoves(botRow, botColumn);

                    // Blue circle.
                    symbols[botRow, botColumn].Text = "◯";

                    symbols[botRow, botColumn].TextColor =
                        CircleColor;

                    // Remember the last bot move.
                    lastBotRow = botRow;
                    lastBotColumn = botColumn;

                    // Highlight the last move.
                    cells[botRow, botColumn]
                        .BackgroundColor = LastMoveBackground;

                    cells[botRow, botColumn]
                        .Stroke = LastMoveBorder;

                    cells[botRow, botColumn]
                        .StrokeThickness = 2;

                    // Check whether the bot won.
                    if (desk.IsFive(-1))
                    {
                        gameOver = true;
                        botThinking = false;

                        HighlightWinningFive(-1);

                        await Task.Delay(100);

                        await ShowLoseImage();
                        return;
                    }

                    botThinking = false;
                };

                cell.GestureRecognizers.Add(tap);

                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);

                Board.Children.Add(cell);
            }
        }
    }

    // =============================================
    // HIGHLIGHT THE WINNING FIVE
    // =============================================

    void HighlightWinningFive(int type)
    {
        var five = desk.GiveFive(type);

        if (five.Item1 < 0)
            return;

        int[] dx = { 1, 0, 1, 1 };
        int[] dy = { 0, 1, 1, -1 };

        int startRow = five.Item1;
        int startColumn = five.Item2;
        int direction = five.Item3;

        for (int k = 0; k < 5; k++)
        {
            int row = startRow + dx[direction] * k;
            int column = startColumn + dy[direction] * k;

            if (!desk.InBoard(row, column))
                continue;

            cells[row, column].BackgroundColor =
                WinningBackground;

            cells[row, column].Stroke =
                WinningBorder;

            cells[row, column].StrokeThickness = 2;
        }
    }

    // =============================================
    // PLAYER WINS
    // =============================================

    async Task ShowWinImage()
    {
        await ShowResultPopup(
            "AURA +999999 🗿",
            "win_picture.png",
            Color.FromArgb("#A78BFA"));
    }

    // =============================================
    // BOT WINS
    // =============================================

    async Task ShowLoseImage()
    {
        await ShowResultPopup(
            "The bot won!",
            "lose_meme.jpg",
            Color.FromArgb("#36A3FF"));
    }

    // =============================================
    // RESULT POPUP
    // =============================================

    async Task ShowResultPopup(
        string title,
        string imageSource,
        Color accentColor)
    {
        Image image = new Image
        {
            Source = imageSource,
            Aspect = Aspect.AspectFit,
            HeightRequest = 260,
            WidthRequest = 280
        };

        Button okButton = new Button
        {
            Text = "PLAY AGAIN",

            HorizontalOptions = LayoutOptions.Fill,

            BackgroundColor = accentColor,
            TextColor = Colors.White,

            FontSize = 16,
            FontAttributes = FontAttributes.Bold,

            CornerRadius = 12,
            HeightRequest = 48
        };

        Border popupCard = new Border
        {
            BackgroundColor = Color.FromArgb("#121B33"),
            Stroke = accentColor,
            StrokeThickness = 1,

            Padding = 20,
            WidthRequest = 340,

            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,

            StrokeShape = new RoundRectangle
            {
                CornerRadius = 22
            },

            Content = new VerticalStackLayout
            {
                Spacing = 16,

                Children =
                {
                    new Label
                    {
                        Text = title,

                        FontSize = 27,
                        FontAttributes = FontAttributes.Bold,

                        TextColor = accentColor,

                        HorizontalTextAlignment =
                            TextAlignment.Center
                    },

                    image,
                    okButton
                }
            }
        };

        ContentPage popup = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#E6080D1C"),

            Content = new Grid
            {
                Padding = 12,

                Children =
                {
                    popupCard
                }
            }
        };

        okButton.Clicked += async (sender, e) =>
        {
            await Navigation.PopModalAsync();

            RestartGame();
        };

        await Navigation.PushModalAsync(popup);
    }

    // =============================================
    // RESTART GAME
    // =============================================

    void RestartGame()
    {
        desk = new Desk();

        botThinking = false;
        gameOver = false;

        lastBotRow = -1;
        lastBotColumn = -1;

        for (int i = 0; i < SIZE; i++)
        {
            for (int j = 0; j < SIZE; j++)
            {
                symbols[i, j].Text = "";
                symbols[i, j].TextColor = Colors.White;

                cells[i, j].BackgroundColor =
                    BackgroundCell;

                cells[i, j].Stroke =
                    NormalCellBorder;

                cells[i, j].StrokeThickness = 1;
            }
        }
    }

    // =============================================
    // BOARD NAVIGATION
    // =============================================

    async void OnPanUpdated(
        object? sender,
        PanUpdatedEventArgs e)
    {
        if (e.StatusType == GestureStatus.Started)
        {
            startScrollX = Scroll.ScrollX;
            startScrollY = Scroll.ScrollY;
        }

        if (e.StatusType == GestureStatus.Running)
        {
            double newX = startScrollX - e.TotalX;
            double newY = startScrollY - e.TotalY;

            if (newX < 0)
                newX = 0;

            if (newY < 0)
                newY = 0;

            await Scroll.ScrollToAsync(
                newX,
                newY,
                false);
        }
    }
}