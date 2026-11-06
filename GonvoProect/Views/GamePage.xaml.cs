using System;
using System.Threading.Tasks;
using GonvoProect.Modles;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace GonvoProect.Views
{
    public partial class GamePage : ContentPage
    {
        private const int SIZE = 4;
        private const int HUMAN = 1;
        private const int COMPUTER = -1;

        private Desk3D _game = new Desk3D();

        private Button[,,] _cells = new Button[SIZE, SIZE, SIZE];

        private bool _gameOver;
        private bool _computerThinking;

        // The computer's most recent move.
        private (int x, int y, int z) _lastComputerMove = (-1, -1, -1);

        // Prevent an old computer turn from continuing after NEW GAME.
        private int _gameSession;

        // Directions matching Desk3D.FindFour().
        private static readonly int[] Dx =
            { 1, 0, 0, 0, 1, 1, 0, -1, -1, 1, -1, 1, 1 };

        private static readonly int[] Dy =
            { 0, 1, 0, 1, 1, 0, -1, 1, 0, 1, 1, -1, 1 };

        private static readonly int[] Dz =
            { 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, -1 };

        public GamePage()
        {
            InitializeComponent();
            StartNewGame();
        }

        // --------------------------------------------------
        // NEW GAME
        // --------------------------------------------------

        private void StartNewGame()
        {
            _gameSession++;

            _game = new Desk3D();
            _gameOver = false;
            _computerThinking = false;

            _lastComputerMove = (-1, -1, -1);

            StatusLabel.Text = "Your turn — place X";

            BuildLayers();
            RefreshButtonStates();
        }

        // --------------------------------------------------
        // BUILD THE FOUR LAYERS
        // --------------------------------------------------

        private void BuildLayers()
        {
            LayersContainer.Children.Clear();

            _cells = new Button[SIZE, SIZE, SIZE];

            for (int z = 0; z < SIZE; z++)
            {
                Grid boardGrid = new Grid
                {
                    RowSpacing = 5,
                    ColumnSpacing = 5,
                    WidthRequest = 212,
                    HeightRequest = 212
                };

                for (int i = 0; i < SIZE; i++)
                {
                    boardGrid.RowDefinitions.Add(
                        new RowDefinition
                        {
                            Height = GridLength.Star
                        });

                    boardGrid.ColumnDefinitions.Add(
                        new ColumnDefinition
                        {
                            Width = GridLength.Star
                        });
                }

                for (int x = 0; x < SIZE; x++)
                {
                    for (int y = 0; y < SIZE; y++)
                    {
                        Button cell = new Button
                        {
                            Text = "",
                            FontSize = 23,
                            FontAttributes = FontAttributes.Bold,
                            Padding = 0,
                            Margin = 0,
                            CornerRadius = 9,
                            BackgroundColor =
                                Color.FromArgb("#1C2942"),
                            TextColor = Colors.White,
                            BorderColor =
                                Color.FromArgb("#344666"),
                            BorderWidth = 1
                        };

                        int cellX = x;
                        int cellY = y;
                        int cellZ = z;

                        cell.Clicked += (sender, args) =>
                            OnCellClicked(cellX, cellY, cellZ);

                        _cells[x, y, z] = cell;

                        Grid.SetRow(cell, x);
                        Grid.SetColumn(cell, y);

                        boardGrid.Children.Add(cell);
                    }
                }

                Label layerTitle = new Label
                {
                    Text = $"Layer {z + 1}",
                    FontSize = 19,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#F5F7FF"),
                    HorizontalTextAlignment = TextAlignment.Center
                };

                Label layerSubtitle = new Label
                {
                    Text = $"Z = {z}",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#8F9DB8"),
                    HorizontalTextAlignment = TextAlignment.Center
                };

                VerticalStackLayout cardContent =
                    new VerticalStackLayout
                    {
                        Spacing = 5,
                        HorizontalOptions = LayoutOptions.Center
                    };

                cardContent.Children.Add(layerTitle);
                cardContent.Children.Add(layerSubtitle);

                cardContent.Children.Add(new BoxView
                {
                    HeightRequest = 3,
                    BackgroundColor = Color.FromArgb("#697CFF"),
                    Margin = new Thickness(35, 3, 35, 5)
                });

                cardContent.Children.Add(boardGrid);

                Border layerCard = new Border
                {
                    WidthRequest = 244,
                    Padding = 12,
                    BackgroundColor = Color.FromArgb("#121B2E"),
                    Stroke = Color.FromArgb("#2D3A55"),
                    StrokeThickness = 1,
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(18)
                    },
                    Content = cardContent,
                    VerticalOptions = LayoutOptions.Start
                };

                LayersContainer.Children.Add(layerCard);
            }
        }

        // --------------------------------------------------
        // PLAYER MOVE
        // --------------------------------------------------

        private async void OnCellClicked(int x, int y, int z)
        {
            if (_gameOver || _computerThinking)
                return;

            if (_game.Board[x, y, z] != 0)
                return;

            int currentSession = _gameSession;

            // Place the player's X.
            _game.Board[x, y, z] = HUMAN;
            PaintCell(x, y, z, HUMAN);

            if (_game.IsFour(HUMAN))
            {
                FinishGame("You win! Four Xs in a row.", HUMAN);
                return;
            }

            if (_game.IsBoardFull())
            {
                FinishDraw();
                return;
            }

            // Start the computer's turn.
            _computerThinking = true;
            StatusLabel.Text = "Computer is thinking...";

            RefreshButtonStates();

            await Task.Delay(250);

            // A new game may have started during the delay.
            if (currentSession != _gameSession)
                return;

            var move = _game.BestMove(COMPUTER);

            // Fallback if BestMove returns invalid coordinates.
            if (move.x < 0 || move.x >= SIZE ||
                move.y < 0 || move.y >= SIZE ||
                move.z < 0 || move.z >= SIZE ||
                _game.Board[move.x, move.y, move.z] != 0)
            {
                move = FindFirstEmptyCell();
            }

            if (move.x < 0)
            {
                FinishDraw();
                return;
            }

            // Place the computer's O.
            _game.Board[move.x, move.y, move.z] = COMPUTER;

            PaintCell(move.x, move.y, move.z, COMPUTER);

            // Highlight the computer's latest move.
            HighlightLastComputerMove(move.x, move.y, move.z);

            _computerThinking = false;

            if (_game.IsFour(COMPUTER))
            {
                FinishGame(
                    "Computer wins! Four Os in a row.",
                    COMPUTER);

                return;
            }

            if (_game.IsBoardFull())
            {
                FinishDraw();
                return;
            }

            StatusLabel.Text = "Your turn — place X";

            RefreshButtonStates();
        }

        // --------------------------------------------------
        // PAINT X AND O
        // --------------------------------------------------

        private void PaintCell(int x, int y, int z, int type)
        {
            Button cell = _cells[x, y, z];

            if (type == HUMAN)
            {
                cell.Text = "X";
                cell.TextColor = Color.FromArgb("#AFC5FF");
                cell.BackgroundColor = Color.FromArgb("#263C67");
            }
            else
            {
                cell.Text = "O";
                cell.TextColor = Color.FromArgb("#F1B8FF");
                cell.BackgroundColor = Color.FromArgb("#432C52");
            }

            cell.IsEnabled = false;
        }

        // --------------------------------------------------
        // HIGHLIGHT THE LAST COMPUTER MOVE
        // --------------------------------------------------

        private void HighlightLastComputerMove(int x, int y, int z)
        {
            // Restore the previous computer move to its normal style.
            if (_lastComputerMove.x >= 0)
            {
                Button previous = _cells[
                    _lastComputerMove.x,
                    _lastComputerMove.y,
                    _lastComputerMove.z];

                previous.BackgroundColor = Color.FromArgb("#432C52");
                previous.TextColor = Color.FromArgb("#F1B8FF");
                previous.BorderColor = Color.FromArgb("#344666");
                previous.BorderWidth = 1;
            }

            // Remember the new computer move.
            _lastComputerMove = (x, y, z);

            // Give the newest O a bright background and golden border.
            Button current = _cells[x, y, z];

            current.BackgroundColor = Color.FromArgb("#654276");
            current.BorderColor = Color.FromArgb("#F6C85F");
            current.BorderWidth = 3;
        }

        // --------------------------------------------------
        // FINISH THE GAME
        // --------------------------------------------------

        private void FinishGame(string message, int winner)
        {
            _gameOver = true;
            _computerThinking = false;

            StatusLabel.Text = message;

            // Find and highlight the winning four cells.
            var line = _game.FindFour(winner);

            if (line.derection >= 0 && line.derection < 13)
            {
                for (int l = 0; l < SIZE; l++)
                {
                    int x = line.x + Dx[line.derection] * l;
                    int y = line.y + Dy[line.derection] * l;
                    int z = line.z + Dz[line.derection] * l;

                    Button cell = _cells[x, y, z];

                    cell.BackgroundColor = Color.FromArgb("#F6C85F");
                    cell.TextColor = Color.FromArgb("#101827");
                    cell.BorderColor = Color.FromArgb("#FFE29A");
                    cell.BorderWidth = 1;
                }
            }

            // Keep the latest computer move identifiable,
            // even if it belongs to the winning line.
            if (_lastComputerMove.x >= 0)
            {
                Button last = _cells[
                    _lastComputerMove.x,
                    _lastComputerMove.y,
                    _lastComputerMove.z];

                last.BorderColor = Colors.White;
                last.BorderWidth = 4;
            }

            RefreshButtonStates();
        }

        private void FinishDraw()
        {
            _gameOver = true;
            _computerThinking = false;

            StatusLabel.Text = "It's a draw. The board is full.";

            RefreshButtonStates();
        }

        // --------------------------------------------------
        // FIND AN EMPTY CELL IF NEEDED
        // --------------------------------------------------

        private (int x, int y, int z) FindFirstEmptyCell()
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    for (int z = 0; z < SIZE; z++)
                    {
                        if (_game.Board[x, y, z] == 0)
                            return (x, y, z);
                    }
                }
            }

            return (-1, -1, -1);
        }

        // --------------------------------------------------
        // ENABLE OR DISABLE CELLS
        // --------------------------------------------------

        private void RefreshButtonStates()
        {
            for (int x = 0; x < SIZE; x++)
            {
                for (int y = 0; y < SIZE; y++)
                {
                    for (int z = 0; z < SIZE; z++)
                    {
                        _cells[x, y, z].IsEnabled =
                            !_gameOver &&
                            !_computerThinking &&
                            _game.Board[x, y, z] == 0;
                    }
                }
            }
        }

        // --------------------------------------------------
        // NEW GAME BUTTON
        // --------------------------------------------------

        private void OnNewGameClicked(object sender, EventArgs e)
        {
            StartNewGame();
        }
    }
}