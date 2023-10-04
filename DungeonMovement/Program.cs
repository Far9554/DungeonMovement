using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DungeonMovement
{
    public class Vector2
    {
        public int x;
        public int y;

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public void Addition(Vector2 other)
        { 
            this.x += other.x;
            this.y += other.y;
        }

        public double Distance(Vector2 other)
        {
            double d = 0;

            d = Math.Sqrt(Math.Pow(x- other.x, 2) + Math.Pow(y-other.y,2));

            return d;
        }
    }
    public class Room
    {
        public Vector2 pos;
        public Vector2 size;
        public Room(Vector2 xpos, Vector2 xsize) 
        { 
            pos = xpos;
            size = xsize;
        }
    }
    public class Enemy
    {
        public Vector2 pos;
        public int live;

        public Enemy(Vector2 pos, int live)
        {
            this.pos = pos;
            this.live = live;
        }
    }
    public class Bonus
    {
        public Vector2 pos;
        public int live;

        public Bonus(Vector2 pos, int live)
        {
            this.pos = pos;
            this.live = live;
        }
    }

    internal class Program
    {
        public static Vector2 posPlayer;
        public static int livePlayer;
        public static List<Enemy> Enemys = new List<Enemy>();
        public static List<Room> Rooms = new List<Room>();

        public static int[,] grid = new int[213,50];
        // 0 -> Vacio
        // 1 -> Wall
        // 2 -> Aire
        // 3 -> HallWay

        //public static int[,] gridEntities = new int[213, 50];
        // 0 -> Vacio
        // 1 -> Player
        // 2 -> Enemy
        // 3 -> Coins

        string test = "┐ ┌ └ ┘ ─ │ ┼";

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);

        static void Main(string[] args)
        {
            StartWindow();
            StartWorld();

            StartPlayer();

            while (true)
            {
                //DrawMap();
                Console.SetCursorPosition(posPlayer.x, posPlayer.y);
                MovePlayer();
                for(int e = 0; e < Enemys.Count; e++) { MoveEnemy(Enemys[e]); }
                
            }
        }
        
        static void GenerateEnemy()
        {
            bool ok=false;

            while (!ok)
            {
                Random r = new Random();
                int x = r.Next(20, Console.WindowWidth - 20);
                int y = r.Next(5, Console.WindowHeight - 10);

                if (grid[x,y] == 2)
                {
                    if(Enemys.Count != 0)
                    {
                        for(int e = 0; e < Enemys.Count;e++)
                            if(x != Enemys[e].pos.x && y != Enemys[e].pos.y)
                            {
                                Enemy newEnemy = new Enemy(new Vector2(x, y), r.Next(1,6));
                                Enemys.Add(newEnemy);
                                ok = true;

                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.SetCursorPosition(x, y);
                                Console.Write(newEnemy.live.ToString());
                            }
                    }
                    else
                    {
                        Enemy newEnemy = new Enemy(new Vector2(x, y), r.Next(1, 6));
                        Enemys.Add(newEnemy);
                        ok = true;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.SetCursorPosition(x, y);
                        Console.Write(newEnemy.live.ToString());
                    }
                }
            }
        }
        static Vector2 Dir(int i)
        {
            if (i==0)
                return new Vector2(-1, 0);
            else if (i == 1)
                return new Vector2(1, 0);
            else if (i == 2)
                return new Vector2(0, -1);
            else if (i == 3)
                return new Vector2(0, 1);
            else
                return new Vector2(0, 0);
        }
        static void MoveEnemy(Enemy enemy)
        {
            Random r = new Random();
            Vector2 LastPos = new Vector2(enemy.pos.x, enemy.pos.y);
            Console.SetCursorPosition(enemy.pos.x, enemy.pos.y);

            int Randomdir = r.Next(0, 4);
            Vector2 dir = Dir(Randomdir);

            if (enemy.pos.Distance(posPlayer) < 10)
            {
                Vector2 dif = new Vector2(posPlayer.x - enemy.pos.x, posPlayer.y - enemy.pos.y);
                Vector2 newDir;

                if (Math.Abs(dif.x) > Math.Abs(dif.y))
                {
                    if (dif.x < 0) newDir = Dir(0);
                    else newDir = Dir(1);
                }
                else
                {
                    if (dif.y < 0) newDir = Dir(2);
                    else newDir = Dir(3);
                }

                if (CanMove(new Vector2(enemy.pos.x + newDir.x, enemy.pos.y + newDir.y))) dir = newDir;
            }

            bool enemyOnWay = false;

            for (int e = 0; e < Enemys.Count; e++)
                if (enemy.pos.x + dir.x == Enemys[e].pos.x && enemy.pos.y + dir.y == Enemys[e].pos.y)
                    enemyOnWay = true;

            if (!enemyOnWay && CanMove(new Vector2(enemy.pos.x + dir.x, enemy.pos.y + dir.y)))
            {
                enemy.pos.Addition(dir);
            }

            Console.SetCursorPosition(LastPos.x, LastPos.y);
            DrawMapSpecific(LastPos.x, LastPos.y);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(enemy.pos.x, enemy.pos.y);
            Console.Write(enemy.live.ToString());
        }

        static void StartPlayer()
        {
            posPlayer = new Vector2(Rooms[0].pos.x + (Rooms[0].size.x / 2), Rooms[0].pos.y + (Rooms[0].size.y / 2));
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(posPlayer.x, posPlayer.y);
            Console.Write(livePlayer);
        }
        static Vector2 MovInput()
        {
            ConsoleKey key = Console.ReadKey().Key;

            if (key == ConsoleKey.LeftArrow || key == ConsoleKey.A) 
                return new Vector2(-1, 0);
            else if (key == ConsoleKey.RightArrow || key == ConsoleKey.D)
                return new Vector2(1, 0);
            else if (key == ConsoleKey.UpArrow || key == ConsoleKey.W)
                return new Vector2(0, -1);
            else if (key == ConsoleKey.DownArrow || key == ConsoleKey.S)
                return new Vector2(0, 1);
            else
                return new Vector2(0, 0);
        }
        static bool CanMove(Vector2 newPos)
        {
            if (newPos.x < 0 || newPos.y < 0) { return false; }
            if (newPos.x > Console.WindowWidth - 1 || newPos.y > Console.WindowHeight - 1) { return false; }

            if (grid[newPos.x, newPos.y] == 1 || grid[newPos.x, newPos.y] == 0) { return false; }

            return true;
        }
        static void MovePlayer()
        {
            Vector2 LastPos = new Vector2(posPlayer.x, posPlayer.y);
            Vector2 dir = MovInput();
            if (CanMove(new Vector2(dir.x + posPlayer.x, dir.y + posPlayer.y)))
                posPlayer.Addition(dir);

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(LastPos.x, LastPos.y);
            DrawMapSpecific(LastPos.x, LastPos.y);
            Console.SetCursorPosition(posPlayer.x, posPlayer.y);
            Console.Write(livePlayer);
        }

        static void StartWindow()
        {
            Console.CursorVisible = false;
            Console.SetWindowPosition(0, 0);
            Console.WindowWidth = 213;
            Console.WindowHeight = 50;
            Console.SetBufferSize(213, 50);

            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }
        static void StartWorld()
        {
            DrawUI();

            for (int i = 0; i < 8; i++) GeneretaRoom();
            for (int i = 0; i < Rooms.Count - 1; i++) DrawLine(Rooms[i], Rooms[i + 1]);

            for (int i = 0; i < 5; i++) GenerateEnemy();

            Console.ForegroundColor = ConsoleColor.Gray;
            DrawMap();
        }
        static void DrawUI()
        {
            Console.WriteLine("Live: 3      Level 1-1");

            Console.SetCursorPosition(0, 1);
            for (int i = 0; i < Console.WindowWidth; i++)
            {
                Console.Write("-");
            }
        }

        static void GeneretaRoom()
        {
            Random random = new Random();
            random.Next();

            bool b = true;
            while (b)
            {
                int posx = random.Next(20, Console.WindowWidth - 50);
                int posy = random.Next(10, Console.WindowHeight - 20);
                Vector2 pos = new Vector2(posx, posy);

                int scalex = random.Next(15, 26);
                int scaley = random.Next(8, 16);
                Vector2 scale = new Vector2(scalex, scaley);

                if (Rooms.Count == 0)
                {
                    Room newRoom = new Room(pos, scale);
                    Rooms.Add(newRoom);
                    PlaceRoom(newRoom);
                    b = false;
                }
                else
                {
                    bool CanPlace = true;

                    for (int x = pos.x-2; x < pos.x+scale.x+2; x++)
                    {
                        for (int y = pos.y-2; y < pos.y+scale.y+2; y++)
                        {
                            if (grid[x, y] != 0)
                                CanPlace = false;
                        }
                    }

                    if (CanPlace)
                    {
                        Room newRoom = new Room(pos, scale);
                        Rooms.Add(newRoom);
                        PlaceRoom(newRoom);
                        b = false;
                    }
                }
            }
        }
        static void PlaceRoom(Room newRoom)
        {
            for(int x = 0; x <= newRoom.size.x; x++) {
                for (int y = 0; y <= newRoom.size.y; y++) {
                    if (x == 0 || y == 0 || x == newRoom.size.x || y == newRoom.size.y)
                        grid[newRoom.pos.x + x, newRoom.pos.y + y] = 1;
                    else
                        grid[newRoom.pos.x + x, newRoom.pos.y + y] = 2;
                }
            }
        }
        //GENERAR PASILLOS
        static void DrawLine(Room roomA, Room roomB)
        {
            Vector2 posA = new Vector2(roomA.pos.x + (roomA.size.x /2), roomA.pos.y + (roomA.size.y / 2));
            Vector2 posB = new Vector2(roomB.pos.x + (roomB.size.x / 2), roomB.pos.y + (roomB.size.y / 2));

            while (posA.x != posB.x || posA.y != posB.y)
            {
                Console.SetCursorPosition(posA.x, posA.y);
                if(posA.x != posB.x)
                {
                    if (posA.x < posB.x) posA.x++;
                    else if (posA.x > posB.x) posA.x--;

                    if (grid[posA.x, posA.y] != 2)
                        grid[posA.x, posA.y] = 3;
                }
                else
                {
                    if (grid[posA.x, posA.y] != 2)
                        grid[posA.x, posA.y] = 3;

                    if (posA.y < posB.y) posA.y++;
                    else if (posA.y > posB.y) posA.y--;
                }
            }
        }
        static void DrawMap()
        {
            for (int x = 0; x < 213; x++)
            {
                for(int y = 0; y < 50; y++)
                {
                    Console.SetCursorPosition(x, y);
                    if (grid[x, y] == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        if (grid[x, y - 1] == 1 && grid[x, y + 1] == 1)
                        {
                            if(grid[x - 1, y] == 1 || grid[x + 1, y] == 1) Console.WriteLine("┼");
                            else Console.WriteLine("│");
                        }
                        else if (grid[x - 1, y] == 1 && grid[x + 1, y] == 1) 
                        {
                            if (grid[x, y - 1] == 1 || grid[x, y + 1] == 1) Console.WriteLine("┼");
                            else Console.WriteLine("─");
                        }
                        else Console.WriteLine("┼");
                    }
                    else if (grid[x, y] == 3)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("#");
                    }
                    else if (grid[x, y] == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("·");
                    }
                }
            }
        }

        static void DrawMapSpecific(int x, int y)
        {
            Console.SetCursorPosition(x, y);
            if (grid[x, y] == 1)
            {
                Console.ForegroundColor = ConsoleColor.White;
                if (grid[x, y - 1] == 1 && grid[x, y + 1] == 1)
                {
                    if (grid[x - 1, y] == 1 || grid[x + 1, y] == 1) Console.WriteLine("┼");
                    else Console.WriteLine("│");
                }
                else if (grid[x - 1, y] == 1 && grid[x + 1, y] == 1)
                {
                    if (grid[x, y - 1] == 1 || grid[x, y + 1] == 1) Console.WriteLine("┼");
                    else Console.WriteLine("─");
                }
                else Console.WriteLine("┼");
            }
            else if (grid[x, y] == 3)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("#");
            }
            else if (grid[x, y] == 2)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("·");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
