//using MetalPerformanceShadersGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GonvoProect.Modles
{
    internal class Desk
    {
        const int SIZE = 30;
       
        public int[,] Board = new int[SIZE, SIZE];

        public int player; // equal or 1 or -1

        public Desk() { player = 0; }

        public bool InBoard( int x, int y ) // check if the point in the board
        {
            if( x < 0 || y < 0 ) return false;
            if (x >= SIZE || y >= SIZE) return false;
            return true;
        }


        public bool IsFive( int type )
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        bool check = true;

                        for (int k = 0; k < 5; k++)
                        {
                            int x = i + dx[m] * k;
                            int y = j + dy[m] * k;
                            if (!InBoard(x, y)) { check = false; break; }
                            if (Board[x, y] != type) { check = false; break; }
                        }

                        if (check)
                            return true;
                    }
                }
            return false;
        }

        public (int, int, int ) GiveFive( int type )
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        bool check = true;

                        for (int k = 0; k < 5; k++)
                        {
                            int x = i + dx[m] * k;
                            int y = j + dy[m] * k;
                            if (!InBoard(x, y)) { check = false; break; }
                            if (Board[x, y] != type) { check = false; break; }
                        }

                        if (check)
                        {
                            return (i, j, m);
                        }
                    }
                }
            return (-1, -1, -1);
        }
        public bool CanWin( int x, int y, int type )
        {
            if (!InBoard(x, y) || Board[x, y] != 0)
                return false;

            Board[x, y] = type;
            if( IsFive(type) )
            {
                Board[x, y] = 0;
                return true;
            }
            Board[x, y] = 0;
            return false;
        }
        public (int open, int semiopen, int closed) FindSequence(int len, int type) // find the number of sequance if lenght len, of type
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            int open = 0; int semiopen = 0; int closed = 0;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    for (int m = 0; m < 4; m++)
                    {

                        int prevX = i - dx[m];
                        int prevY = j - dy[m];

                        int nextX = i + dx[m] * len;
                        int nextY = j + dy[m] * len;

                        if( InBoard(prevX, prevY) && Board[prevX, prevY] == type) continue;
                        if( InBoard(nextX, nextY) && Board[nextX, nextY] == type) continue;

                        int counter = 0;
                        if( !InBoard(prevX, prevY) ) counter ++;
                        if( !InBoard(nextX, nextY) ) counter ++;
                        if( InBoard(prevX, prevY) && Board[prevX, prevY] == -type) counter ++;
                        if( InBoard(nextX, nextY) && Board[nextX, nextY] == -type) counter ++;

                        bool check = true;

                        for (int k = 0; k < len; k++)
                        {
                            int x = i + dx[m] * k;
                            int y = j + dy[m] * k;
                            if (!InBoard(x, y)) { check = false; break; }
                            if (Board[x, y] != type) { check = false; break; }
                        }

                        if (check)
                        {
                            if (counter == 0) open++;
                            else if (counter == 1) semiopen++;
                            else if (counter == 2) closed++;
                        }
                    }
                }
            return (open, semiopen, closed);
        }

        // it give me three number the sequence of the type: open, semiopen, cloosed

        public (int open, int semiopen, int closed) NumberCreatedSequence( int x, int y, int len, int type ) 
        {
            if( !InBoard(x, y) || Board[x, y] != 0)
                return (0, 0, 0 );

            (int open, int semiopen, int closed) Num = FindSequence( len, type );
            Board[x, y] = type;
            (int open, int semiopen, int closed) newNum = FindSequence(len, type);
            Board[x, y] = 0;

            return (newNum.open - Num.open, newNum.semiopen - Num.semiopen, newNum.closed - Num.closed);
        }



        public int CountSones()
        {
            int counter = 0;
            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    if (Board[i, j] != 0) counter ++;
            return counter;
        }

        public (int x, int y) FindAdjacentMove()
        {
            List<(int x, int y)> moves = new();

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] == 0)
                        continue;

                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0)
                                continue;

                            int x = i + dx;
                            int y = j + dy;

                            if (InBoard(x, y) && Board[x, y] == 0)
                            {
                                if (!moves.Contains((x, y)))
                                    moves.Add((x, y));
                            }
                        }
                }

            if (moves.Count == 0)
                return (-1, -1);

            Random random = new Random();
            return moves[random.Next(moves.Count)];
        }


        public int NumberOfTwoType1( int type )
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            int counter = 0;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if( Board[i, j] != type ) continue;

                    for (int m = 0; m < 4; m++)
                    {

                        int prevX = i - dx[m];
                        int prevY = j - dy[m];

                        int nextX = i + dx[m] * 2;
                        int nextY = j + dy[m] * 2;

                        if (!InBoard(prevX, prevY) || Board[prevX, prevY] != 0) continue;
                        if (!InBoard(nextX, nextY) || Board[nextX, nextY] != 0) continue;

                        int x = i + dx[m];
                        int y = j + dy[m];
                        if (!InBoard(x, y) || Board[x, y] != type) continue;

                        int check = 0;
                        x = i + dx[m]*3;
                        y = j + dy[m]*3;
                        if (!InBoard(x, y) || Board[x, y] != 0) check++;
                        x = i - dx[m] * 2;
                        y = j - dy[m] * 2;
                        if (!InBoard(x, y) || Board[x, y] != 0) check++;

                        if (check < 2) counter++;
                    }
                }
            return counter;
        }

        public int NumberOfTwoType2( int type )
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            int counter = 0;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != type) continue;

                    for (int m = 0; m < 4; m++)
                    {

                        int prevX = i - dx[m];
                        int prevY = j - dy[m];

                        int nextX = i + dx[m] * 3;
                        int nextY = j + dy[m] * 3;

                        if (!InBoard(prevX, prevY) || Board[prevX, prevY] != 0) continue;
                        if (!InBoard(nextX, nextY) || Board[nextX, nextY] != 0) continue;

                        int x = i + dx[m];
                        int y = j + dy[m];
                        if (!InBoard(x, y) || Board[x, y] != 0) continue;

                        x = i + dx[m] * 2;
                        y = j + dy[m] * 2;
                        if (!InBoard(x, y) || Board[x, y] != type) continue;

                        counter++;

                    }
                }
            return counter;
        }

        public int NumberOfSemiThree( int type )
        {
            int[] dx = { 1, 0, 1, 1 };
            int[] dy = { 0, 1, 1, -1 };

            int counter = 0;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    for (int m = 0; m < 4; m++)
                    {
                        int prevX = i - dx[m];
                        int prevY = j - dy[m];

                        int nextX = i + dx[m] * 4;
                        int nextY = j + dy[m] * 4;

                        int check = 0;

                        if (!InBoard(prevX, prevY) || Board[prevX, prevY] != 0) check++;
                        if (!InBoard(nextX, nextY) || Board[nextX, nextY] != 0) check++;

                        if (check != 1) continue;

                        check = 0;

                        for( int k = 0; k < 4; k ++ )
                        {
                            int x = i + dx[m] * k;
                            int y = j + dy[m] * k;
                            if (!InBoard(x, y)) { check = -100; break; }
                            if (Board[x,y] == -type ) { check = -100; break; }
                            if (Board[x, y] == 0) check++;
                        }
                        
                        if( check == 1 ) counter++;

                    }
                }
            return counter;
        }

        public (int, int ) BestMove( int type )
        {

            if (CountSones() == 0) return (SIZE / 2, SIZE / 2);
            if( CountSones() == 1 )
            {
                return FindAdjacentMove();
            }

            for( int i = 0; i < SIZE; i ++ )
                for( int j = 0; j < SIZE; j ++ )
                {
                    if (Board[i, j] != 0) continue;
                    if ( CanWin(i,j, type ) ) return (i,j);                  
                }
            //checking if we can win

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != 0) continue;
                    if (CanWin(i, j, -type)) return (i, j);
                }
            //checing if oppoinent can win


            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != 0) continue;
                    (int open, int semiopen, int closed) n = NumberCreatedSequence(i, j, 4, type);
                    if (n.open > 0) return (i, j);
                }
            //checking if we can do 4 in sequnce 

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != 0) continue;
                    (int open, int semiopen, int closed) n = NumberCreatedSequence(i, j, 4, -type);
                    if (n.open > 0) return (i, j);
                }
            //checking if opponent can do 4 in sequnce 


            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != 0) continue;
                    if ( NumberCreatedSequence(i, j, 4, type).semiopen +
                        NumberCreatedSequence(i, j, 3, type).open >= 2 ) return (i, j);
                }

            // checking if ther are a fork 


            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != 0) continue;
                    if (NumberCreatedSequence(i, j, 4, -type).semiopen +
                        NumberCreatedSequence(i, j, 3, -type).open >= 2) return (i, j);
                }
            // checking if oponent has a fork;


            (int, int) moveProtect = (-1, -1);
            (int, int) moveAttack = (-1, -1);

            int bestPro = -1000;
            int bestAtt = -1000;


            for(int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                {
                    if (Board[i, j] != 0) continue;

                    //if (NumberCreatedSequence(i, j, 3, type).open > 0 ||
                    //    NumberCreatedSequence(i, j, 4, type).semiopen > 0 ) continue;

                    Board[i, j] = type;
                    int MyOpenTwo = NumberOfTwoType1(type) + NumberOfTwoType2(type);
                    int MySemiOpenThree = NumberOfSemiThree(type);
                    Board[i, j] = -type;

                    Board[i, j] = type;
                    int OpOpenTwo = NumberOfTwoType1(-type) + NumberOfTwoType2(-type);
                    int OpSemiOpenThree = NumberOfSemiThree(-type);
                    Board[i, j] = 0;


                    int Mysum = MyOpenTwo + MySemiOpenThree;
                    int Opsum = OpOpenTwo + OpSemiOpenThree;

                    if ( bestPro < Mysum - Opsum )
                    {
                        bestPro = Mysum - Opsum;
                        moveProtect = (i, j);
                    }


                    if (NumberCreatedSequence(i, j, 3, type).open > 0 ||
                        NumberCreatedSequence(i, j, 4, type).semiopen > 0 )
                    {
                        if (bestAtt < Mysum - Opsum)
                        {
                            bestAtt = Mysum - Opsum;
                            moveAttack = (i, j);
                        }
                    }

                }


            if ( moveAttack.Item1 != -1 && moveProtect.Item1 != -1)
            {
                if( Random.Shared.Next(100) < 20 )
                    return moveAttack;
                return moveProtect;
            }

            if (moveProtect.Item1 != -1)
                return moveProtect;

            if (moveAttack.Item1 != -1)
                return moveAttack;

            return FindAdjacentMove();
        }



    }
}
