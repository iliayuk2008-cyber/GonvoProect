using System.Data;
using System.Numerics;

namespace GonvoProect;

public partial class MainPage : ContentPage
{
    const int SIZE = 25;
    const int CELL_SIZE = 25;
    int player = 0;
    Dictionary<(int row, int column), int> board = new();
    public MainPage()
    {
        InitializeComponent();

        CreateBoard();
    }

    public int checkWin() { return 0; }
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
                    FontSize = 20,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };

                cell.Content = symbol;

                TapGestureRecognizer tap = new TapGestureRecognizer();
                int r = row; int c = column;
                tap.Tapped += (sender, e) =>
                {
                    if (board.ContainsKey( (r, c)) ) { return; }
                    if (player == 0) { symbol.Text = "❌"; board[(r, c)] = 0; }
                    else { symbol.Text = "⭕"; board[(r, c)] = 1; }
                    player ^= 1;
                   
                };

                cell.GestureRecognizers.Add(tap);

                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, column);

                Board.Children.Add(cell);
            }
        }
    }
}