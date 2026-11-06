using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using GonvoProect.Modles;

namespace GonvoProect.Views
{
    public partial class GamePageSuper3by3 : ContentPage
    {
        private const int SIZE = 3;

        private const int HUMAN = 1;
        private const int BOT = -1;

        private readonly DeskSuper3by3 desk = new DeskSuper3by3();

        private readonly Button[,,,] buttons =
            new Button[SIZE, SIZE, SIZE, SIZE];

        private readonly Grid[,] miniGrids =
            new Grid[SIZE, SIZE];

        private readonly Label[,] overlays =
            new Label[SIZE, SIZE];

        private bool gameOver = false;
        private bool botThinking = false;

        public GamePageSuper3by3()
        {
            InitializeComponent();

            BuildBoard();
            RefreshBoard();
        }

        // Build the 3 x 3 main board.
        // Each square contains another 3 x 3 board.

        private void BuildBoard()
        {
            for (int squareX = 0; squareX < SIZE; squareX++)
            {
                for (int squareY = 0; squareY < SIZE; squareY++)
                {
                    Grid miniGrid = new Grid
                    {
                        RowSpacing = 1,
                        ColumnSpacing = 1,
                        BackgroundColor = Color.FromArgb("#46566E")
                    };

                    for (int i = 0; i < SIZE; i++)
                    {
                        miniGrid.RowDefinitions.Add(
                            new RowDefinition
                            {
                                Height = GridLength.Star
                            });

                        miniGrid.ColumnDefinitions.Add(
                            new ColumnDefinition
                            {
                                Width = GridLength.Star
                            });
                    }

                    // Create the nine cells of this mini-board.

                    for (int cellX = 0; cellX < SIZE; cellX++)
                    {
                        for (int cellY = 0; cellY < SIZE; cellY++)
                        {
                            Button cellButton = new Button
                            {
                                Text = "",
                                FontSize = 19,
                                FontAttributes = FontAttributes.Bold,
                                Padding = 0,
                                Margin = 0,
                                CornerRadius = 3,
                                MinimumHeightRequest = 0,
                                MinimumWidthRequest = 0,
                                HorizontalOptions = LayoutOptions.Fill,
                                VerticalOptions = LayoutOptions.Fill,
                                BackgroundColor =
                                    Color.FromArgb("#1B2940"),
                                TextColor = Colors.White
                            };

                            int sx = squareX;
                            int sy = squareY;
                            int cx = cellX;
                            int cy = cellY;

                            cellButton.Clicked += async (sender, e) =>
                            {
                                await HandleCellClickedAsync(
                                    sx, sy, cx, cy);
                            };

                            buttons[squareX, squareY, cellX, cellY]
                                = cellButton;

                            miniGrid.Children.Add(cellButton);

                            Grid.SetRow(cellButton, cellX);
                            Grid.SetColumn(cellButton, cellY);
                        }
                    }

                    // Overlay that displays the result of a mini-board.

                    Label overlay = new Label
                    {
                        Text = "",
                        FontSize = 46,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        BackgroundColor = Color.FromArgb("#E6111929"),
                        IsVisible = false,
                        InputTransparent = true
                    };

                    Grid.SetRowSpan(overlay, SIZE);
                    Grid.SetColumnSpan(overlay, SIZE);

                    miniGrid.Children.Add(overlay);

                    overlays[squareX, squareY] = overlay;
                    miniGrids[squareX, squareY] = miniGrid;

                    BoardGrid.Children.Add(miniGrid);

                    Grid.SetRow(miniGrid, squareX);
                    Grid.SetColumn(miniGrid, squareY);
                }
            }
        }

        // Handle a human move.

        private async Task HandleCellClickedAsync(
            int squareX,
            int squareY,
            int cellX,
            int cellY)
        {
            if (gameOver || botThinking)
                return;

            // The selected mini-board must be playable.

            if (!desk.CanPlayInSquare(squareX, squareY))
                return;

            // If the required mini-board is available,
            // the player must play there.

            bool targetAvailable =
                desk.CanPlayInSquare(desk.curX, desk.curY);

            if (targetAvailable &&
                (squareX != desk.curX || squareY != desk.curY))
            {
                return;
            }

            // The cell must be empty.

            if (desk.Board[squareX, squareY, cellX, cellY] != 0)
                return;

            // Place X.

            desk.Board[squareX, squareY, cellX, cellY] = HUMAN;

            desk.MainBoard[squareX, squareY] =
                desk.GetSquareResult(squareX, squareY);

            // The cell coordinates determine the next target board.

            desk.curX = cellX;
            desk.curY = cellY;

            // Check whether the game has ended.

            if (TryFinishGame())
            {
                RefreshBoard();
                return;
            }

            // Bot's turn.

            botThinking = true;

            StatusLabel.Text = "Bot is thinking...";
            RefreshBoard();

            var move = await Task.Run(() => desk.BestMove(BOT));

            botThinking = false;

            // No move available.

            if (move.Item1 < 0 ||
                move.Item2 < 0 ||
                move.Item3 < 0 ||
                move.Item4 < 0)
            {
                gameOver = true;
                StatusLabel.Text = "Draw!";
                HelpLabel.Text = "No legal moves remain.";

                RefreshBoard();
                return;
            }

            // BestMove returns:
            // Item1 = cellX
            // Item2 = cellY
            // Item3 = squareX
            // Item4 = squareY

            int botCellX = move.Item1;
            int botCellY = move.Item2;
            int botSquareX = move.Item3;
            int botSquareY = move.Item4;

            // Safety check before applying the bot's move.

            if (!desk.CanPlayInSquare(botSquareX, botSquareY) ||
                desk.Board[
                    botSquareX, botSquareY,
                    botCellX, botCellY] != 0)
            {
                gameOver = true;
                StatusLabel.Text = "Bot move error.";
                HelpLabel.Text = "Please start a new game.";

                RefreshBoard();
                return;
            }

            // Place O.

            desk.Board[
                botSquareX, botSquareY,
                botCellX, botCellY] = BOT;

            desk.MainBoard[botSquareX, botSquareY] =
                desk.GetSquareResult(botSquareX, botSquareY);

            // Determine where the human must play next.

            desk.curX = botCellX;
            desk.curY = botCellY;

            TryFinishGame();

            RefreshBoard();
        }

        // Check for a winner or a draw.

        private bool TryFinishGame()
        {
            int winner = desk.SquareCheckWin(desk.MainBoard);

            if (winner == HUMAN)
            {
                gameOver = true;
                StatusLabel.Text = "You win!";
                HelpLabel.Text = "Congratulations!";
                return true;
            }

            if (winner == BOT)
            {
                gameOver = true;
                StatusLabel.Text = "Bot wins!";
                HelpLabel.Text = "Press NEW GAME to try again.";
                return true;
            }

            if (desk.FullBoard())
            {
                gameOver = true;
                StatusLabel.Text = "Draw!";
                HelpLabel.Text = "No legal moves remain.";
                return true;
            }

            return false;
        }

        // Refresh cell text, colors, overlays and available moves.

        private void RefreshBoard()
        {
            bool targetAvailable =
                desk.CanPlayInSquare(desk.curX, desk.curY);

            for (int squareX = 0; squareX < SIZE; squareX++)
            {
                for (int squareY = 0; squareY < SIZE; squareY++)
                {
                    bool squareAvailable =
                        desk.CanPlayInSquare(squareX, squareY);

                    bool isTarget =
                        targetAvailable &&
                        squareX == desk.curX &&
                        squareY == desk.curY;

                    // Highlight the required mini-board.
                    // When there is no required board,
                    // all available boards are highlighted normally.

                    if (isTarget)
                    {
                        miniGrids[squareX, squareY].BackgroundColor =
                            Color.FromArgb("#F5C65D");
                    }
                    else if (squareAvailable)
                    {
                        miniGrids[squareX, squareY].BackgroundColor =
                            Color.FromArgb("#50617A");
                    }
                    else
                    {
                        miniGrids[squareX, squareY].BackgroundColor =
                            Color.FromArgb("#263347");
                    }

                    // Show the result of a completed mini-board.

                    int result = desk.MainBoard[squareX, squareY];

                    Label overlay = overlays[squareX, squareY];

                    overlay.IsVisible = result != 0;

                    if (result == HUMAN)
                    {
                        overlay.Text = "X";
                        overlay.TextColor = Color.FromArgb("#FF6B6B");
                    }
                    else if (result == BOT)
                    {
                        overlay.Text = "O";
                        overlay.TextColor = Color.FromArgb("#6CB8FF");
                    }
                    else if (result == 2)
                    {
                        overlay.Text = "DRAW";
                        overlay.FontSize = 17;
                        overlay.TextColor = Colors.White;
                    }
                    else
                    {
                        overlay.Text = "";
                    }

                    if (result != 2)
                        overlay.FontSize = 46;

                    // Refresh individual cells.

                    for (int cellX = 0; cellX < SIZE; cellX++)
                    {
                        for (int cellY = 0; cellY < SIZE; cellY++)
                        {
                            Button button = buttons[
                                squareX, squareY, cellX, cellY];

                            int value = desk.Board[
                                squareX, squareY, cellX, cellY];

                            if (value == HUMAN)
                            {
                                button.Text = "X";
                                button.TextColor =
                                    Color.FromArgb("#FF6B6B");
                            }
                            else if (value == BOT)
                            {
                                button.Text = "O";
                                button.TextColor =
                                    Color.FromArgb("#6CB8FF");
                            }
                            else
                            {
                                button.Text = "";
                                button.TextColor = Colors.White;
                            }

                            button.BackgroundColor = isTarget
                                ? Color.FromArgb("#2D425E")
                                : Color.FromArgb("#1B2940");

                            bool allowedBoard =
                                squareAvailable &&
                                (!targetAvailable || isTarget);

                            button.IsEnabled =
                                !gameOver &&
                                !botThinking &&
                                allowedBoard &&
                                value == 0;
                        }
                    }
                }
            }

            // Update information for the player.

            if (botThinking)
            {
                StatusLabel.Text = "Bot is thinking...";
                HelpLabel.Text = "Please wait.";
            }
            else if (!gameOver)
            {
                StatusLabel.Text = "Your turn (X)";

                if (targetAvailable)
                {
                    HelpLabel.Text =
                        $"Play in the highlighted mini-board " +
                        $"({desk.curX + 1}, {desk.curY + 1}).";
                }
                else
                {
                    HelpLabel.Text =
                        "Choose any available mini-board.";
                }
            }
            else
            {
                HelpLabel.Text = "Press NEW GAME to play again.";
            }

            NewGameButton.IsEnabled = !botThinking;
        }

        // Reset the game.

        private void OnNewGameClicked(object sender, EventArgs e)
        {
            if (botThinking)
                return;

            Array.Clear(desk.Board, 0, desk.Board.Length);
            Array.Clear(desk.MainBoard, 0, desk.MainBoard.Length);

            desk.player = HUMAN;
            desk.curX = -1;
            desk.curY = -1;

            gameOver = false;
            botThinking = false;

            StatusLabel.Text = "Your turn (X)";
            HelpLabel.Text = "Choose any available mini-board.";

            RefreshBoard();
        }
    }
}