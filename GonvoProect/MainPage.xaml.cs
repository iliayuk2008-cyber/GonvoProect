using System;
using System.Threading.Tasks;
using GonvoProect.Modles;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace GonvoProect;

public partial class MainPage : ContentPage
{
    const int SIZE = 50;
    const int CELL_SIZE = 40;

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

    void CreateBoard()
    {
        for (int i = 0; i < SIZE; i++)
        {
            Board.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = CELL_SIZE
                });
        }

        for (int i = 0; i < SIZE; i++)
        {
            Board.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = CELL_SIZE
                });
        }

        for (int row = 0; row < SIZE; row++)
        {
            for (int column = 0; column < SIZE; column++)
            {
                Border cell = new Border
                {
                    BackgroundColor = Colors.White,
                    Stroke = Colors.Black,
                    StrokeThickness = 1
                };

                Label symbol = new Label
                {
                    Text = "",
                    FontSize = 28,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };

                symbols[row, column] = symbol;
                cells[row, column] = cell;

                cell.Content = symbol;

                TapGestureRecognizer tap = new TapGestureRecognizer();

                int r = row;
                int c = column;

                tap.Tapped += async (sender, e) =>
                {
                    if (gameOver || botThinking)
                        return;

                    if (desk.Board[r, c] != 0)
                        return;

                    desk.Board[r, c] = 1;
                    desk.AddMoves(r, c);

                    symbols[r, c].Text = "❌";

                    if (desk.IsFive(1))
                    {
                        gameOver = true;

                        HighlightWinningFive(1);

                        await Task.Delay(100);

                        await ShowWinImage();
                        return;
                    }

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

                    if (!desk.InBoard(botRow, botColumn) ||
                        desk.Board[botRow, botColumn] != 0)
                    {
                        botRow = -1;
                        botColumn = -1;

                        for (int i = 0; i < SIZE && botRow == -1; i++)
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

                    if (lastBotRow != -1 && lastBotColumn != -1)
                    {
                        cells[lastBotRow, lastBotColumn].BackgroundColor =
                            Colors.White;
                    }

                    desk.Board[botRow, botColumn] = -1;
                    desk.AddMoves(botRow, botColumn);
                    symbols[botRow, botColumn].Text = "⭕";

                    lastBotRow = botRow;
                    lastBotColumn = botColumn;

                    cells[botRow, botColumn].BackgroundColor =
                        Colors.Yellow;

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

            cells[row, column].BackgroundColor = Colors.LightGreen;
        }
    }

    async Task ShowWinImage()
    {
        var image = new Image
        {
            Source = "win_picture.png",
            Aspect = Aspect.AspectFit,
            HeightRequest = 280,
            WidthRequest = 300
        };

        var okButton = new Button
        {
            Text = "OK",
            HorizontalOptions = LayoutOptions.Fill
        };

        var popup = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#AA000000"),

            Content = new Grid
            {
                Children =
                {
                    new Border
                    {
                        BackgroundColor = Colors.White,
                        Stroke = Colors.White,
                        Padding = 20,
                        WidthRequest = 340,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,

                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = 16
                        },

                        Content = new VerticalStackLayout
                        {
                            Spacing = 15,

                            Children =
                            {
                                new Label
                                {
                                    Text = "Aura +999999 🗿",
                                    FontSize = 32,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Colors.Black,
                                    HorizontalTextAlignment =
                                        TextAlignment.Center
                                },

                                image,
                                okButton
                            }
                        }
                    }
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

    async Task ShowLoseImage()
    {
        var image = new Image
        {
            Source = "lose_meme.jpg",
            Aspect = Aspect.AspectFit,
            HeightRequest = 280,
            WidthRequest = 300
        };

        var okButton = new Button
        {
            Text = "OK",
            HorizontalOptions = LayoutOptions.Fill
        };

        var popup = new ContentPage
        {
            BackgroundColor = Color.FromArgb("#AA000000"),

            Content = new Grid
            {
                Children =
                {
                    new Border
                    {
                        BackgroundColor = Colors.White,
                        Stroke = Colors.White,
                        Padding = 20,
                        WidthRequest = 340,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,

                        StrokeShape = new RoundRectangle
                        {
                            CornerRadius = 16
                        },

                        Content = new VerticalStackLayout
                        {
                            Spacing = 15,

                            Children =
                            {
                                new Label
                                {
                                    Text = "The bot won!",
                                    FontSize = 24,
                                    HorizontalTextAlignment =
                                        TextAlignment.Center
                                },

                                image,
                                okButton
                            }
                        }
                    }
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
                cells[i, j].BackgroundColor = Colors.White;
            }
        }
    }

    async void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
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