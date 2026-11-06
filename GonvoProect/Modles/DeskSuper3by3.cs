//using Android.AdServices.CustomAudiences;
//using Java.Sql;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GonvoProect.Modles
{
    internal class DeskSuper3by3
    {
        const int SIZE = 3;
        public int[,,,] Board = new int[SIZE, SIZE, SIZE, SIZE];
        public int[,] MainBoard = new int[SIZE, SIZE];

        public int player;
        public int curX;
        public int curY;
        public DeskSuper3by3() { player = 1;curX = -1; curY = -1;}


        public bool InBoard( int x, int y)
        {
            if (x < 0 || x >= SIZE) return false;
            if (y < 0 || y >= SIZE) return false;
            return true;
        }

        public int NumberOfTwo( int[,] board , int type)
        {
            int[] dx = { 0, 1, 1, -1 };
            int[] dy = { 1, 0, 1, 1 };

            int counter = 0;

            for( int i = 0; i < SIZE; i ++ )
                for( int j = 0; j < SIZE; j ++ )
                    for( int m = 0; m < 4; m ++ )
                    {
                        bool vaild = true;
                        int num = 0;
                        for( int l = 0; l < SIZE; l ++ )
                        {
                            int x = i + dx[m] * l;
                            int y = j + dy[m] * l;

                            if( !InBoard(x, y) ) {  vaild = false; break; }
                            if (board[x, y] != 0 && board[x, y] != type) { vaild = false; break; }
                            if (board[x, y] == 0) num++;
                        }

                        if (num == 1 && vaild) counter++;
                    }
            return counter;
        }
        public int SquareScore(int[,] board, int type )
        {
            int score = 10*NumberOfTwo(board, type) - 10*NumberOfTwo(board,-type);
            if (board[1,1] == type) score++;
            if (board[1,1] == -type ) score--;
            return score;

        }


        public int[,] GetSquare(int x, int y)
        {
            int[,] square = new int[SIZE, SIZE];

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    square[i, j] = Board[x, y, i, j];

            return square;
        }
        public int SquareCheckWin(int[,] board)
        {
            int[] dx = { 0, 1, 1, -1 };
            int[] dy = { 1, 0, 1, 1 };

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int m = 0; m < 4; m++)
                    {
                        int stoneType = 0;
                        bool vaild = true;
                        for (int l = 0; l < SIZE; l++)
                        {
                            int x = i + dx[m] * l;
                            int y = j + dy[m] * l;

                            if (!InBoard(x, y)) { vaild = false; break; }
                            if (board[x, y] != 1 && board[x, y] != -1) { vaild = false; break; }

                            if (stoneType == 0) stoneType = board[x, y];
                            else if (stoneType != board[x, y]) { vaild = false; break; }
                        }

                        if (vaild) return stoneType;
                    }
            return 0;
        }

        public int GetSquareResult(int x, int y)
        {
            int[,] square = GetSquare(x, y);

            int winner = SquareCheckWin(square);

            if (winner != 0)
                return winner;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    if (square[i, j] == 0)
                        return 0;

            return 2;
        }

        public int BoardScore( int type )
        {
            int score = 0;

            int winner = SquareCheckWin(MainBoard);

            if (winner != 0)
                return winner == type ? 1000000 : -1000000; 
            

            for( int i = 0; i < SIZE; i++ ) 
                for( int j = 0; j < SIZE; j++ )
                {
                    if( i == 1 && j == 1 )
                    {
                        if (MainBoard[i, j] == type) score += 400;
                        if (MainBoard[i, j] == -type) score -= 400;
                    }

                    else
                    {
                        if (MainBoard[i, j] == type) score += 300;
                        if (MainBoard[i, j] == -type) score -= 300;
                    }

                }

            score += SquareScore(MainBoard, type)*1000;

            for( int i = 0; i < SIZE; i ++ )
                for( int j = 0; j < SIZE; j ++ )
                {
                    if (MainBoard[i, j] != 0) continue;

                    if( i == 1 && j == 1 ) 
                        score += 5*SquareScore( GetSquare(i, j), type);
                    else score += 4 * SquareScore(GetSquare(i, j), type);
                }

            return score;
        }

        public bool CanPlayInSquare(int x, int y)
        {
            if (!InBoard(x, y))
                return false;

            if (MainBoard[x, y] != 0)
                return false;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    if (Board[x, y, i, j] == 0)
                        return true;

            return false;
        }

        public bool FullBoard()
        {
            for (int x = 0; x < SIZE; x++)
                for (int y = 0; y < SIZE; y++)
                    if (CanPlayInSquare(x, y))
                        return false;

            return true;
        }

        public int MaxScore(int curX, int curY, int type, int iter, ref int bestX, ref int bestY, ref int bestSquareX, ref int bestSquareY)
        {

            int winner = SquareCheckWin(MainBoard);

            if (winner != 0)
                return winner == type ? 1000000 : -1000000;

            if (FullBoard())
                return 0;




            if (iter == 4) // WHY C# SO SLOW!!!!!!!!!!
            {
                int BS = BoardScore(type);
                return BS;
            } 



            if (iter == 0)
            {

                int best = int.MinValue;

                if ( !CanPlayInSquare(curX, curY) )
                {

                    for (int x = 0; x < SIZE; x++)
                        for (int y = 0; y < SIZE; y++)
                        {
                            if (!CanPlayInSquare(x, y)) continue;

                            for (int i = 0; i < SIZE; i++)
                                for (int j = 0; j < SIZE; j++)
                                {
                                    if (Board[x, y, i, j] != 0) continue;

                                    Board[x, y, i, j] = type;
                                    int MB = MainBoard[x, y];

                                    if (MB == 0)
                                        MainBoard[x, y] = GetSquareResult(x, y);
                                    int MS = MaxScore(i, j, type, iter + 1, ref bestX, ref bestY, ref bestSquareX, ref bestSquareY );

                                    if (best < MS)
                                    {
                                        best = MS;
                                        bestX = i;
                                        bestY = j;
                                        bestSquareX = x;
                                        bestSquareY = y;
                                    }

                                    MainBoard[x, y] = MB;
                                    Board[x,y, i, j] = 0;

                                }
                        }
                    return best;
                }  



                for (int i = 0; i < SIZE; i++)
                    for (int j = 0; j < SIZE; j++)
                    {
                        if (Board[curX, curY, i, j] != 0) continue;

                        Board[curX, curY, i, j] = type;
                        int MB = MainBoard[curX, curY];

                        if (MB == 0)
                            MainBoard[curX, curY] = GetSquareResult(curX, curY);
                        int MS = MaxScore(i, j, type, iter + 1, ref bestX, ref bestY, ref bestSquareX, ref bestSquareY);
                        
                        if( best < MS )
                        {
                            best = MS;
                            bestX = i;
                            bestY = j;
                            bestSquareX = curX;
                            bestSquareY = curY;
                        }

                        MainBoard[curX, curY] = MB;
                        Board[curX, curY, i, j] = 0;

                    }
                return best;
            }

            if (iter % 2 == 0)
            {
                int best = int.MinValue;
                if (!CanPlayInSquare(curX, curY))
                {

                    for (int x = 0; x < SIZE; x++)
                        for (int y = 0; y < SIZE; y++)
                        {
                            if (!CanPlayInSquare(x, y)) continue;

                            for (int i = 0; i < SIZE; i++)
                                for (int j = 0; j < SIZE; j++)
                                {
                                    if (Board[x, y, i, j] != 0) continue;

                                    Board[x, y, i, j] = type;
                                    int MB = MainBoard[x, y];

                                    if (MB == 0)
                                        MainBoard[x, y] = GetSquareResult(x, y);
                                    int MS = MaxScore(i, j, type, iter + 1, ref bestX, ref bestY, ref bestSquareX, ref bestSquareY);
                                    best = Math.Max(best, MS);

                                    MainBoard[x, y] = MB;
                                    Board[x, y, i, j] = 0;

                                }
                        }
                    return best;
                }

                else
                {
                    for (int i = 0; i < SIZE; i++)
                        for (int j = 0; j < SIZE; j++)
                        {
                            if (Board[curX, curY, i, j] != 0) continue;

                            Board[curX, curY, i, j] = type;
                            int MB = MainBoard[curX, curY];

                            if (MB == 0)
                                MainBoard[curX, curY] = GetSquareResult(curX, curY);
                            int MS = MaxScore(i, j, type, iter + 1, ref bestX, ref bestY, ref bestSquareX, ref bestSquareY);
                            best = Math.Max(best, MS);


                            MainBoard[curX, curY] = MB;
                            Board[curX, curY, i, j] = 0;

                        }
                }
                return best;
            }

            else
            {

                int best = int.MaxValue;
                if (!CanPlayInSquare(curX, curY))
                {
                    for (int x = 0; x < SIZE; x++)
                        for (int y = 0; y < SIZE; y++)
                        {
                            if (!CanPlayInSquare(x, y)) continue;

                            for (int i = 0; i < SIZE; i++)
                                for (int j = 0; j < SIZE; j++)
                                {
                                    if (Board[x, y, i, j] != 0) continue;

                                    Board[x, y, i, j] = -type;
                                    int MB = MainBoard[x, y];

                                    if (MB == 0)
                                        MainBoard[x, y] = GetSquareResult(x, y);
                                    int MS = MaxScore(i, j, type, iter + 1, ref bestX, ref bestY, ref bestSquareX, ref bestSquareY);
                                    best = Math.Min(best, MS);

                                    MainBoard[x, y] = MB;
                                    Board[x, y, i, j] = 0;

                                }
                        }
                    return best;
                }

                for (int i = 0; i < SIZE; i++)
                    for (int j = 0; j < SIZE; j++)
                    {
                        if (Board[curX, curY, i, j] != 0) continue;

                        Board[curX, curY, i, j] = -type;
                        int MB = MainBoard[curX, curY];

                        if (MB == 0)
                            MainBoard[curX, curY] = GetSquareResult(curX, curY);
                        int MS = MaxScore(i, j, type, iter + 1, ref bestX, ref bestY, ref bestSquareX, ref bestSquareY);
                        best = Math.Min(best, MS);

                        MainBoard[curX, curY] = MB;
                        Board[curX, curY, i, j] = 0;

                    }
                return best;
            }
        }
 
        public (int, int, int,int ) BestMove( int type )
        {
            int bestX = -1; 
            int bestY = -1;
            int bestSquareX = -1;
            int bestSquareY = -1;

            if(FullBoard())
                return (-1, -1, -1, -1);

            MaxScore( curX, curY, type, 0, ref bestX, ref bestY, ref bestSquareX,ref bestSquareY );
            return (  bestX, bestY, bestSquareX, bestSquareY );
        }
    }

}
