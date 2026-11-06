
using GonvoProect.Services;
using System;
using System.Threading.Tasks;
using GonvoProect.Modles;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace GonvoProect.Views;

public partial class MainPage : ContentPage
{
    SoundService sound = new SoundService();

    const int SIZE = 50;
    const int CELL_SIZE = 44;
    const int CELL_GAP = 2;
    const int BOARD_PADDING = 4;

    const double SYMBOL_FONT_SIZE = 37;
    const double DRAG_THRESHOLD = 5;

    const int STEP = CELL_SIZE + CELL_GAP;

    const int BOARD_PIXEL_SIZE =
        BOARD_PADDING * 2 +
        SIZE * CELL_SIZE +
        (SIZE - 1) * CELL_GAP;

    // =============================================
    // COLORS
    // =============================================

    static readonly Color BackgroundCell =
        Color.FromArgb("#131D35");

    static readonly Color NormalCellBorder =
        Color.FromArgb("#35466F");

    static readonly Color CrossColor =
        Color.FromArgb("#FF416C");

    static readonly Color CircleColor =
        Color.FromArgb("#36A3FF");

    static readonly Color LastMoveBackground =
        Color.FromArgb("#34245C");

    static readonly Color LastMoveBorder =
        Color.FromArgb("#A78BFA");

    static readonly Color WinningBackground =
        Color.FromArgb("#164E63");

    static readonly Color WinningBorder =
        Color.FromArgb("#67E8F9");

    static readonly Color BoardBackground =
        Color.FromArgb("#080D1C");

    // =============================================
    // GAME
    // =============================================

    Desk desk = new Desk();

    readonly BoardDrawable boardDrawable;

    bool botThinking = false;
    bool gameOver = false;

    // =============================================
    // VIEWPORT POSITION
    // =============================================

    double offsetX = 0;
    double offsetY = 0;

    bool boardCentered = false;

    // =============================================
    // INTERACTION
    // =============================================

    bool interactionActive = false;
    bool interactionDragged = false;

    PointF interactionStart;

    double startOffsetX;
    double startOffsetY;

    // =============================================
    // CONSTRUCTOR
    // =============================================

    public MainPage()
    {
        InitializeComponent();

        boardDrawable = new BoardDrawable();

        Board.Drawable = boardDrawable;

        // GRAPHICSVIEW INTERACTIONS

        Board.StartInteraction += OnBoardStartInteraction;

        Board.DragInteraction += OnBoardDragInteraction;

        Board.EndInteraction += OnBoardEndInteraction;

        Board.CancelInteraction += OnBoardCancelInteraction;

        // BOARD SIZE

        Board.SizeChanged += OnBoardSizeChanged;
    }

    // =============================================
    // INITIAL BOARD POSITION
    // =============================================

    void OnBoardSizeChanged(object? sender, EventArgs e)
    {
        if (Board.Width <= 0 || Board.Height <= 0)
            return;

        if (!boardCentered)
        {
            offsetX = Math.Max(
                0,
                (BOARD_PIXEL_SIZE - Board.Width) / 2);

            offsetY = Math.Max(
                0,
                (BOARD_PIXEL_SIZE - Board.Height) / 2);

            boardCentered = true;
        }

        ClampOffsets();

        UpdateDrawable();
    }

    // =============================================
    // CLAMP VIEWPORT POSITION
    // =============================================

    void ClampOffsets()
    {
        double maxX = Math.Max(
            0,
            BOARD_PIXEL_SIZE - Board.Width);

        double maxY = Math.Max(
            0,
            BOARD_PIXEL_SIZE - Board.Height);

        offsetX = Math.Clamp(offsetX, 0, maxX);
        offsetY = Math.Clamp(offsetY, 0, maxY);
    }

    // =============================================
    // UPDATE DRAWABLE
    // =============================================

    void UpdateDrawable()
    {
        boardDrawable.SetViewport(
            offsetX,
            offsetY);

        Board.Invalidate();
    }

    // =============================================
    // START INTERACTION
    // =============================================

    void OnBoardStartInteraction(
        object? sender,
        TouchEventArgs e)
    {
        if (e.Touches.Length == 0)
            return;

        interactionActive = true;
        interactionDragged = false;

        interactionStart = e.Touches[0];

        startOffsetX = offsetX;
        startOffsetY = offsetY;
    }

    // =============================================
    // DRAG INTERACTION
    // =============================================

    void OnBoardDragInteraction(
        object? sender,
        TouchEventArgs e)
    {
        if (!interactionActive ||
            e.Touches.Length == 0)
        {
            return;
        }

        PointF current = e.Touches[0];

        double deltaX =
            current.X - interactionStart.X;

        double deltaY =
            current.Y - interactionStart.Y;

        if (!interactionDragged &&
            deltaX * deltaX + deltaY * deltaY <
            DRAG_THRESHOLD * DRAG_THRESHOLD)
        {
            return;
        }

        interactionDragged = true;

        offsetX = startOffsetX - deltaX;
        offsetY = startOffsetY - deltaY;

        ClampOffsets();

        UpdateDrawable();
    }

    // =============================================
    // END INTERACTION
    // =============================================

    async void OnBoardEndInteraction(
        object? sender,
        TouchEventArgs e)
    {
        if (!interactionActive)
            return;

        interactionActive = false;

        if (e.Touches.Length == 0)
        {
            interactionDragged = false;
            return;
        }

        PointF position = e.Touches[0];

        double deltaX =
            position.X - interactionStart.X;

        double deltaY =
            position.Y - interactionStart.Y;

        if (deltaX * deltaX + deltaY * deltaY >=
            DRAG_THRESHOLD * DRAG_THRESHOLD)
        {
            interactionDragged = true;
        }

        if (interactionDragged)
        {
            interactionDragged = false;
            return;
        }

        interactionDragged = false;

        if (!e.IsInsideBounds)
            return;

        if (gameOver || botThinking)
            return;

        await HandleBoardTapAsync(position);
    }

    // =============================================
    // CANCEL INTERACTION
    // =============================================

    void OnBoardCancelInteraction(
        object? sender,
        EventArgs e)
    {
        interactionActive = false;
        interactionDragged = false;
    }

    // =============================================
    // HANDLE PLAYER MOVE
    // =============================================

    async Task HandleBoardTapAsync(PointF position)
    {
        if (gameOver || botThinking)
            return;

        // SCREEN -> BOARD COORDINATES

        double x = position.X + offsetX - BOARD_PADDING;
        double y = position.Y + offsetY - BOARD_PADDING;

        if (x < 0 || y < 0)
            return;

        int column = (int)(x / STEP);
        int row = (int)(y / STEP);

        if (row < 0 || row >= SIZE ||
            column < 0 || column >= SIZE)
        {
            return;
        }

        // Ignore gaps between cells.

        if (x % STEP >= CELL_SIZE ||
            y % STEP >= CELL_SIZE)
        {
            return;
        }

        if (desk.Board[row, column] != 0)
            return;

        // =========================================
        // PLAYER'S MOVE
        // =========================================

        desk.Board[row, column] = 1;

        desk.AddMoves(row, column);

        boardDrawable.SetCell(row, column, 1);

        Board.Invalidate();

        if (desk.IsFive(1))
        {
            gameOver = true;

            HighlightWinningFive(1);

            await Task.Delay(100);

            await ShowWinImage();

            return;
        }

        // =========================================
        // BOT'S MOVE
        // =========================================

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

        // =========================================
        // VALIDATE BOT MOVE
        // =========================================

        if (!desk.InBoard(botRow, botColumn) ||
            desk.Board[botRow, botColumn] != 0)
        {
            botRow = -1;
            botColumn = -1;

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

        // =========================================
        // DRAW
        // =========================================

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

        // =========================================
        // BOT'S MOVE
        // =========================================

        desk.Board[botRow, botColumn] = -1;

        desk.AddMoves(botRow, botColumn);

        boardDrawable.SetCell(
            botRow,
            botColumn,
            -1);

        boardDrawable.SetLastBotMove(
            botRow,
            botColumn);

        Board.Invalidate();

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
    }

    // =============================================
    // HIGHLIGHT WINNING FIVE
    // =============================================

    void HighlightWinningFive(int type)
    {
        var five = desk.GiveFive(type);

        if (five.Item1 < 0)
            return;

        boardDrawable.HighlightWinningFive(
            five.Item1,
            five.Item2,
            five.Item3);

        Board.Invalidate();
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
        await sound.PlaySound("LoseSound.mp3");

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
            BackgroundColor =
                Color.FromArgb("#121B33"),

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
            BackgroundColor =
                Color.FromArgb("#E6080D1C"),

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

        boardDrawable.ClearBoard();

        Board.Invalidate();
    }

    // =============================================
    // GRAPHICS DRAWABLE
    // =============================================

    sealed class BoardDrawable : IDrawable
    {
        readonly int[,] renderedBoard =
            new int[SIZE, SIZE];

        readonly bool[,] winningCells =
            new bool[SIZE, SIZE];

        int lastBotRow = -1;
        int lastBotColumn = -1;

        double offsetX = 0;
        double offsetY = 0;

        // =========================================
        // SET VIEWPORT
        // =========================================

        public void SetViewport(
            double x,
            double y)
        {
            offsetX = x;
            offsetY = y;
        }

        // =========================================
        // SET CELL
        // =========================================

        public void SetCell(
            int row,
            int column,
            int value)
        {
            if (row < 0 || row >= SIZE ||
                column < 0 || column >= SIZE)
            {
                return;
            }

            renderedBoard[row, column] = value;
        }

        // =========================================
        // LAST BOT MOVE
        // =========================================

        public void SetLastBotMove(
            int row,
            int column)
        {
            lastBotRow = row;
            lastBotColumn = column;
        }

        // =========================================
        // WINNING FIVE
        // =========================================

        public void HighlightWinningFive(
            int startRow,
            int startColumn,
            int direction)
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            if (direction < 0 || direction >= 4)
                return;

            for (int k = 0; k < 5; k++)
            {
                int row =
                    startRow + dx[direction] * k;

                int column =
                    startColumn + dy[direction] * k;

                if (row < 0 || row >= SIZE ||
                    column < 0 || column >= SIZE)
                {
                    continue;
                }

                winningCells[row, column] = true;
            }
        }

        // =========================================
        // CLEAR BOARD
        // =========================================

        public void ClearBoard()
        {
            Array.Clear(
                renderedBoard,
                0,
                renderedBoard.Length);

            Array.Clear(
                winningCells,
                0,
                winningCells.Length);

            lastBotRow = -1;
            lastBotColumn = -1;
        }

        // =========================================
        // DRAW
        // =========================================

        public void Draw(
            ICanvas canvas,
            RectF dirtyRect)
        {
            canvas.FillColor = BoardBackground;

            canvas.FillRectangle(dirtyRect);

            canvas.Font =
                Microsoft.Maui.Graphics.Font.DefaultBold;

            canvas.FontSize =
                (float)SYMBOL_FONT_SIZE;

            // Determine visible cells.

            int firstColumn = Math.Max(
                0,
                (int)Math.Floor(
                    (offsetX - BOARD_PADDING - CELL_SIZE) / STEP) + 1);

            int firstRow = Math.Max(
                0,
                (int)Math.Floor(
                    (offsetY - BOARD_PADDING - CELL_SIZE) / STEP) + 1);

            int lastColumn = Math.Min(
                SIZE - 1,
                (int)Math.Ceiling(
                    (offsetX + dirtyRect.Width - BOARD_PADDING) / STEP) - 1);

            int lastRow = Math.Min(
                SIZE - 1,
                (int)Math.Ceiling(
                    (offsetY + dirtyRect.Height - BOARD_PADDING) / STEP) - 1);

            if (firstColumn > lastColumn ||
                firstRow > lastRow)
            {
                return;
            }

            // Draw only visible cells.

            for (int row = firstRow;
                 row <= lastRow;
                 row++)
            {
                for (int column = firstColumn;
                     column <= lastColumn;
                     column++)
                {
                    float x = (float)(
                        BOARD_PADDING +
                        column * STEP -
                        offsetX);

                    float y = (float)(
                        BOARD_PADDING +
                        row * STEP -
                        offsetY);

                    RectF cellRect = new RectF(
                        x,
                        y,
                        CELL_SIZE,
                        CELL_SIZE);

                    Color background = BackgroundCell;
                    Color border = NormalCellBorder;

                    float borderThickness = 1;

                    // Last bot move.

                    if (row == lastBotRow &&
                        column == lastBotColumn)
                    {
                        background = LastMoveBackground;
                        border = LastMoveBorder;

                        borderThickness = 2;
                    }

                    // Winning five.

                    if (winningCells[row, column])
                    {
                        background = WinningBackground;
                        border = WinningBorder;

                        borderThickness = 2;
                    }

                    // Cell background.

                    canvas.FillColor = background;

                    canvas.FillRoundedRectangle(
                        cellRect,
                        5);

                    // Cell border.

                    canvas.StrokeColor = border;
                    canvas.StrokeSize = borderThickness;

                    canvas.DrawRoundedRectangle(
                        cellRect,
                        5);

                    // Draw symbol.

                    int value =
                        renderedBoard[row, column];

                    if (value == 0)
                        continue;

                    canvas.FontColor =
                        value == 1
                            ? CrossColor
                            : CircleColor;

                    string symbol =
                        value == 1 ? "✕" : "◯";

                    canvas.DrawString(
                        symbol,
                        cellRect,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center);
                }
            }
        }
    }
}
