//using Android.Renderscripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GonvoProect.Modles
{
    internal class Desk3D
    {
        const int SIZE = 4;

        public int[,,] Board = new int[SIZE, SIZE, SIZE];

        public int player;
        public Desk3D() { player = 1; }

        public bool InBoard(int x, int y, int z)
        {
            if (x < 0 || x >= SIZE) return false;
            if (y < 0 || y >= SIZE) return false;
            if (z < 0 || z >= SIZE) return false;
            return true;
        }


        public (int two, int three, int four) NumberOfEveryThing(int type)
        {
            int[] dx = { 1, 0, 0, 0, 1, 1, 0, -1, -1, 1, -1, 1, 1 };
            int[] dy = { 0, 1, 0, 1, 1, 0, -1, 1, 0, 1, 1, -1, 1 };
            int[] dz = { 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, -1 };

            int two = 0, three = 0, four = 0;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                        for (int m = 0; m < 13; m++)
                        {
                            bool vaild = true;
                            int num = 0;
                            for (int l = 0; l < SIZE; l++)
                            {
                                int x = i + dx[m] * l;
                                int y = j + dy[m] * l;
                                int z = k + dz[m] * l;

                                if (!InBoard(x, y, z)) { vaild = false; break; }

                                if (Board[x, y, z] == -type) { vaild = false; break; }

                                if (Board[x, y, z] == type) num++;
                            }

                            if (vaild == false) continue;

                            if (num == 2) two++;
                            if (num == 3) three++;
                            if (num == 4) four++;
                        }
            return (two, three, four);
        }


        public bool IsFour(int type)
        {
            var v = NumberOfEveryThing(type);
            if (v.four > 0) return true;
            return false;
        }

        public (int x, int y, int z, int derection) FindFour(int type)
        {
            if (!IsFour(type)) return (-1, -1, -1, -1);

            int[] dx = { 1, 0, 0, 0, 1, 1, 0, -1, -1, 1, -1, 1, 1 };
            int[] dy = { 0, 1, 0, 1, 1, 0, -1, 1, 0, 1, 1, -1, 1 };
            int[] dz = { 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, -1 };

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                        for (int m = 0; m < 13; m++)
                        {
                            bool vaild = true;
                            for (int l = 0; l < SIZE; l++)
                            {
                                int x = i + dx[m] * l;
                                int y = j + dy[m] * l;
                                int z = k + dz[m] * l;

                                if (!InBoard(x, y, z)) { vaild = false; break; }
                                if (Board[x, y, z] != type) { vaild = false; break; }
                            }

                            if (vaild == true)
                            {
                                return (i, j, k, m);
                            }
                        }

            return (-1, -1, -1, -1);
        }

        public bool IsBoardFull()
        {
            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                        if (Board[i, j, k] == 0) return false;
            return true;
        }




        public (int x, int y, int z) BestMove(int type)
        {

            if (IsBoardFull())
                return (-1, -1, -1);

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                    {
                        if (Board[i, j, k] != 0) continue;

                        Board[i, j, k] = type;
                        var v = NumberOfEveryThing(type);
                        Board[i, j, k] = 0;
                        if (v.four > 0) return (i, j, k);
                    }
            //check if i can win;

            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                    {
                        if (Board[i, j, k] != 0) continue;

                        Board[i, j, k] = -type;
                        var v = NumberOfEveryThing(-type);
                        Board[i, j, k] = 0;
                        if (v.four > 0) return (i, j, k);
                    }

            //check if oponent can win


            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                    {
                        if (Board[i, j, k] != 0) continue;

                        Board[i, j, k] = type;
                        var v = NumberOfEveryThing(type);
                        Board[i, j, k] = 0;
                        if (v.three >= 2) return (i, j, k);
                    }

            //check if we can do fork



            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                    {
                        if (Board[i, j, k] != 0) continue;

                        Board[i, j, k] = -type;
                        var v = NumberOfEveryThing(-type);
                        Board[i, j, k] = 0;
                        if (v.three >= 2) return (i, j, k);
                    }
            // check if opponent can do fork, and prevent it




            int best = -1000;

            int x = -1, y = -1, z = -1;


            for (int i = 0; i < SIZE; i++)
                for (int j = 0; j < SIZE; j++)
                    for (int k = 0; k < SIZE; k++)
                    {
                        if (Board[i, j, k] != 0) continue;

                        Board[i, j, k] = type;
                        var we = NumberOfEveryThing(type);
                        var op = NumberOfEveryThing(-type);
                        Board[i, j, k] = 0;
                        if (best < we.two - op.two)
                        {
                            x = i; y = j; z = k;
                            best = we.two - op.two;
                        }
                    }
            return (x, y, z);
        }
    }
}
