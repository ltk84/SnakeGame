using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WPFTEST
{
    public partial class MainWindow : Window
    {
        // DEFINITION
        public class PartOfSnake
        {
            public Point Pos;
            public Rectangle Skin = new Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Green
            };
            public PartOfSnake()
            {
                Pos.X = 150;
                Pos.Y = 150;
            }
            public PartOfSnake(double x, double y)
            {
                Pos.X = x;
                Pos.Y = y;
            }
        }
        public class Food
        {
            public Point Pos;
            public Ellipse Skin = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.OrangeRed
            };
        }

        public class Rock
        {
            public Point Pos;
            public Rectangle Skin = new Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.DarkGray
            };
        }

        public enum Directions
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        };

        // INITIALIZATION
        List<PartOfSnake> Snake = new List<PartOfSnake>(); // Khởi tạo danh sách các phần của rắn.
        Food FoodForSnake = new Food(); // Khởi tạo thức ăn.
        List<Rock> RockList = new List<Rock>();
        DispatcherTimer Time;
        PartOfSnake OldTailOfSnake; // Biến để lưu vị trí trước đó của phần đuôi rắn, để mỗi khi ăn thức ăn, phần đuôi của rắn sẽ mọc dài ra.

        int FirstFoodInit = 0;  // 0: Khởi tạo thức ăn ban đầu cho rắn.
                                // 1: Thức ăn đã được khởi, lúc này thức ăn mới sẽ xuất hiện mỗi khi rắn ăn được thức ăn.

        int KeyPivot = 0;   // 0: Chưa xử lý key Input (cho phép rắn di chuyển - MoveSnake).
                            // 1: Đã xử lý key Input trước đó.

        int ButtonPivot = 0;    // Chốt này được dùng cho Button "Pause - Continue".
                                // 0: Button được click sẽ là "Pause".
                                // 1: Button được click sẽ là "Continue".

        int RockPivot = 0;  // 0: Từ chối tạo Rock.
                            // 1: Được phép tạo Rock.

        //  DEFAULT SETTINGS
        public double speed = 100;  // Tốc độ mặc định ban đầu là 100 (milliseconds).
        Directions currentDirection = Directions.DOWN;  // Hướng di chuyển mặc định ban đầu là hướng xuống.
        int Score = 0;  // Điểm ban đầu là 0.

        // FUNCTIONS
        public MainWindow()
        {
            InitializeComponent();
        }

        public void StartGame(object sender, EventArgs e)
        {
            MoveSnake();
            if (FirstFoodInit == 0)
            {
                DrawFood();
                FirstFoodInit = 1;
            }
            Handling();
            Thread CreateRock = new Thread(DrawRock);
            CreateRock.Start();
            DrawSnake();
        }

        public void CreateSnake()
        {
            PartOfSnake SnakePart = new PartOfSnake(150, 150);
            Snake.Add(SnakePart);
            PartOfSnake SnakePart2 = new PartOfSnake(150, 140);
            Snake.Add(SnakePart2);
            PartOfSnake SnakePart3 = new PartOfSnake(150, 130);
            Snake.Add(SnakePart3);
            PartOfSnake SnakePart4 = new PartOfSnake(150, 120);
            Snake.Add(SnakePart4);
        }

        public void DrawSnake()
        {
            if (Time.IsEnabled)
            {
                foreach (var SnakePart in Snake)
                {
                    if (SnakeColor.Text != "Default")
                    {
                        switch (SnakeColor.Text)
                        {
                            case "Black":   
                                SnakePart.Skin.Fill = Brushes.Black;
                                break;
                            case "Blue":   
                                SnakePart.Skin.Fill = Brushes.Blue;
                                break;
                            case "Brown":   
                                SnakePart.Skin.Fill = Brushes.Brown;
                                break;
                            case "Gray":   
                                SnakePart.Skin.Fill = Brushes.Gray;
                                break;
                            case "Orange":
                                SnakePart.Skin.Fill = Brushes.Orange;
                                break;
                            case "Pink":
                                SnakePart.Skin.Fill = Brushes.Pink;
                                break;
                            case "Purple":
                                SnakePart.Skin.Fill = Brushes.Purple;
                                break;
                            case "Red":
                                SnakePart.Skin.Fill = Brushes.Red;
                                break;
                            case "Violet":
                                SnakePart.Skin.Fill = Brushes.Violet;
                                break;
                            case "White":
                                SnakePart.Skin.Fill = Brushes.White;
                                break;
                            case "Yellow":
                                SnakePart.Skin.Fill = Brushes.Yellow;
                                break;
                        }
                    }
                    if (!Area.Children.Contains(SnakePart.Skin))
                    {
                        Area.Children.Add(SnakePart.Skin);
                    }
                    Canvas.SetLeft(SnakePart.Skin, SnakePart.Pos.X);
                    Canvas.SetTop(SnakePart.Skin, SnakePart.Pos.Y);
                }
            }
        }

        public void MoveSnake()
        {
            if (Time.IsEnabled)
            {
                if (KeyPivot != 1)
                {
                    PartOfSnake SnakePart = Snake.Last();
                    SnakePart.Pos.X = Snake.First().Pos.X;
                    SnakePart.Pos.Y = Snake.First().Pos.Y;
                    OldTailOfSnake = new PartOfSnake(Snake.Last().Pos.X, Snake.Last().Pos.Y);
                    switch (currentDirection)
                    {
                        case Directions.UP:
                            SnakePart.Pos.Y -= 10;
                            break;
                        case Directions.DOWN:
                            SnakePart.Pos.Y += 10;
                            break;
                        case Directions.LEFT:
                            SnakePart.Pos.X -= 10;
                            break;
                        case Directions.RIGHT:
                            SnakePart.Pos.X += 10;
                            break;
                    }
                    Snake.RemoveAt(Snake.Count - 1);
                    Snake.Insert(0, SnakePart);
                }
                else if (KeyPivot == 1)
                {
                    KeyPivot = 0;
                }
            }
        }

        //public bool IsFoodValid(Food RealFood)
        //{
        //    foreach (Rock Stone in RockList)
        //    {
        //        if (RealFood.Pos.X == Stone.Pos.X && RealFood.Pos.Y == Stone.Pos.Y)
        //        {
        //            return false;
        //        }
        //    }

        //    foreach (PartOfSnake SnakePart in Snake)
        //    {
        //        if (RealFood.Pos.X == SnakePart.Pos.X && RealFood.Pos.Y == SnakePart.Pos.Y)
        //        {
        //            return false;
        //        }
        //    }
        //    return true; 
        //}

        public bool IsRockValid(Rock RealRock)
        {
            if (RealRock.Pos.X == FoodForSnake.Pos.X && RealRock.Pos.Y == FoodForSnake.Pos.Y)
            {
                return false;
            }

            foreach (PartOfSnake SnakePart in Snake)
            {
                if (RealRock.Pos.X == SnakePart.Pos.X && RealRock.Pos.Y == SnakePart.Pos.Y)
                {
                    return false;
                }
            }
            return true;
        }

        public void DrawFood()
        {
            //do
            //{
                Random RandPos = new Random(DateTime.Now.Second);
                FoodForSnake.Pos.X = (int)RandPos.Next(0, 30) * 10;
                FoodForSnake.Pos.Y = (int)RandPos.Next(0, 40) * 10;
            //} while (!IsFoodValid(FoodForSnake));

            if (!Area.Children.Contains(FoodForSnake.Skin))
            {
                Area.Children.Add(FoodForSnake.Skin);
            }
            Canvas.SetLeft(FoodForSnake.Skin, FoodForSnake.Pos.X);
            Canvas.SetTop(FoodForSnake.Skin, FoodForSnake.Pos.Y);
        }

        public void DrawRock()
        {
            if (RockPivot == 1)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    Rock Stone = new Rock();
                    do
                    {
                        Random RandPos = new Random(DateTime.Now.Second);
                        Stone.Pos.X = (int)RandPos.Next(0, 30) * 10;
                        Stone.Pos.Y = (int)RandPos.Next(0, 40) * 10;
                    } while (!IsRockValid(Stone));

                    RockList.Add(Stone);
                    if (!Area.Children.Contains(Stone.Skin))
                    {
                        Area.Children.Add(Stone.Skin);
                    }
                    Canvas.SetLeft(Stone.Skin, Stone.Pos.X);
                    Canvas.SetTop(Stone.Skin, Stone.Pos.Y);
                }));
                RockPivot = 0;
            }
        }
        public void Handling()
        {
            if (Time.IsEnabled)
            {
                if (Snake.First().Pos.X > 300 || Snake.First().Pos.Y > 400 || Snake.First().Pos.X < 0 || Snake.First().Pos.Y < 0)
                {
                    GameOver();
                    return;
                }

                foreach (Rock Stone in RockList)
                {
                    if (Snake.First().Pos.X == Stone.Pos.X && Snake.First().Pos.Y == Stone.Pos.Y)
                    {
                        GameOver();
                        return;
                    }
                }

                for (int u = 1; u < Snake.Count; u++)
                {
                    if (Snake[0].Pos.X == Snake[u].Pos.X && Snake[0].Pos.Y == Snake[u].Pos.Y)
                    {
                        GameOver();
                        return;
                    }
                }

                if (Snake[0].Pos.X == FoodForSnake.Pos.X && Snake[0].Pos.Y == FoodForSnake.Pos.Y)
                {
                    Snake.Add(OldTailOfSnake);
                    Score++;
                    ScoreValue.Content = Score.ToString();

                    // Điều kiện cho phép tạo Đá.
                    if (Score >= 10 && Score % 5 == 0)
                        RockPivot = 1;

                    // Điều kiện giảm tốc.
                    if (speed > 40) speed -= 2;
                    Time.Interval = TimeSpan.FromMilliseconds(speed);
                   
                    DrawFood();
                    return;
                }
            }
        }

        // Events
        public void GameOver()
        {
            Time.Stop();
            gameBtn.IsEnabled = true;
            gamePauseBtn.IsEnabled = false;
            gameStopBtn.IsEnabled = false;
            SnakeColor.IsEnabled = true;
            SpeedHolder.IsEnabled = true;
            MessageBox.Show("GAME OVER!");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Đặt lại các giá trị ban đầu.
            Area.Children.Clear();
            Snake.Clear();
            CreateSnake();
            FirstFoodInit = 0;
            Score = 0;
            ScoreValue.Content = "0";
            currentDirection = Directions.DOWN;
            KeyPivot = 0;
            RockPivot = 0;
            speed = double.Parse(SpeedHolder.Text);
            
            // Khởi tạo Timer.
            Time = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(speed)
            };
            Time.Tick += StartGame;
            Time.Start();

            // Giá của các Button.
            gameBtn.IsEnabled = false;
            gamePauseBtn.IsEnabled = true;
            gamePauseBtn.Content = "Pause";
            ButtonPivot = 0;
            gameStopBtn.IsEnabled = true;
            SnakeColor.IsEnabled = false;
            SpeedHolder.IsEnabled = false;

            // Rock Initialization.

        }

        private void GamePause_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonPivot == 0)
            {
                Time.Stop();
                gamePauseBtn.Content = "Continue";
                ButtonPivot = 1;
            }
            else
            {
                Time.Start();
                gamePauseBtn.Content = "Pause";
                ButtonPivot = 0;
            }
        }
        private void GameStop_Button_Click(object sender, RoutedEventArgs e)
        {
            Time.Stop();
            gamePauseBtn.IsEnabled = false;
            gameBtn.IsEnabled = true;
            SnakeColor.IsEnabled = true;
            SpeedHolder.IsEnabled = true;
        }
        private void KeyDownCheck(object sender, KeyEventArgs e)
        {   
            if (Area.Children.Count != 0 && Time.IsEnabled)
            {
                switch (e.Key)
                {
                    case Key.W:
                        if (currentDirection != Directions.DOWN && currentDirection != Directions.UP)
                        {
                            currentDirection = Directions.UP;
                        }
                        break;
                    case Key.S:
                        if (currentDirection != Directions.UP && currentDirection != Directions.DOWN)
                        {
                            currentDirection = Directions.DOWN;
                        }
                        break;
                    case Key.A:
                        if (currentDirection != Directions.RIGHT && currentDirection != Directions.LEFT)
                        {
                            currentDirection = Directions.LEFT;
                        }
                        break;
                    case Key.D:
                        if (currentDirection != Directions.LEFT && currentDirection != Directions.RIGHT) 
                        {
                            currentDirection = Directions.RIGHT;
                        }
                        break;
                }
                MoveSnake();
                Handling();
                DrawSnake();
                KeyPivot = 1;
            }
        }
    }
}
