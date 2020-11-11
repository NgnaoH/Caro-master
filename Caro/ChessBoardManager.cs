using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro
{
    public class ChessBoardManager
    {
        #region Properties
        private Panel chessBoard;
        private List<Player> player;
        private int currentPlayer;
        private TextBox playerName;
        private PictureBox playerMark;
        private List<List<Button>> matrix;
        private event EventHandler<ButtonClickEvent> playerMarked;
        private event EventHandler<ButtonClickEvent> endedGame;

        public Panel ChessBoard { get => chessBoard; set => chessBoard = value; }
        public List<Player> Player { get => player; set => player = value; }
        public int CurrentPlayer { get => currentPlayer; set => currentPlayer = value; }
        public TextBox PlayerName { get => playerName; set => playerName = value; }
        public PictureBox PlayerMark { get => playerMark; set => playerMark = value; }
        public List<List<Button>> Matrix { get => matrix; set => matrix = value; }
        public event EventHandler<ButtonClickEvent> PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }
        public event EventHandler<ButtonClickEvent> EndedGame
        {
            add
            {
                endedGame += value;
            }
            remove
            {
                endedGame -= value;
            }
        }

        #endregion
        #region Initialize
        public ChessBoardManager(Panel chessBoard, TextBox playerName, PictureBox playerMark)
        {
            this.ChessBoard = chessBoard;
            this.PlayerName = playerName;
            this.PlayerMark = playerMark;

            this.Player = new List<Player>()
            {
                new Player("Player1", Image.FromFile(Application.StartupPath + "\\Resources\\x.jpg")),
                new Player("Player2", Image.FromFile(Application.StartupPath + "\\Resources\\o.jpg")),
            };
        }
        #endregion
        #region Methods
        public void DrawChessBoard()
        {
            ChessBoard.Enabled = false;
            ChessBoard.Controls.Clear();
            CurrentPlayer = 0;
            ChangePlayer();

            Matrix = new List<List<Button>>();

            Button oldbtn = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < Cons.Chess_Board_Height; i++)
            {
                Matrix.Add(new List<Button>());
                for (int j = 0; j < Cons.Chess_Board_Width; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Cons.Chess_Width,
                        Height = Cons.Chess_Height,
                        Location = new Point(oldbtn.Location.X + oldbtn.Width, oldbtn.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i.ToString()
                    };

                    btn.Click += btn_Click;
                    ChessBoard.Controls.Add(btn);
                    Matrix[i].Add(btn);
                    oldbtn = btn;
                }
                oldbtn.Location = new Point(0, oldbtn.Location.Y + Cons.Chess_Height);
                oldbtn.Width = 0;
                oldbtn.Height = 0;
            }
        }

        #region Play

        private Point GetChessPoint(Button btn)
        {
            int vertical = Convert.ToInt32(btn.Tag);
            int horizonal = Matrix[vertical].IndexOf(btn);

            Point point = new Point(horizonal, vertical);

            return point;
        }

        void btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            ChangePlayer();

            if (playerMarked != null)
                playerMarked(this, new ButtonClickEvent(GetChessPoint(btn), ""));

            if (isEndGame(btn))
            {
                EndGame();
            }
        }

        public void OtherPlayerMark(Point point)
        {
            Button btn = Matrix[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            ChangePlayer();

            if (isEndGame(btn))
            {
                EndGame();
            }
        }

        private void Mark(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;

            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
        }

        private void ChangePlayer()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;

            PlayerMark.Image = Player[CurrentPlayer].Mark;
        }
        #endregion       

        #region CheckEndGame
        public void EndGame()
        {
            if (endedGame != null)
                endedGame(this, new ButtonClickEvent(new Point(), (Math.Abs(CurrentPlayer - 1) + 1).ToString()));
        }

        private bool isEndGame(Button btn)
        {
            return isEndHorizontal(btn) || isEndVertical(btn) || isEndPrimaryDiagonal(btn) || isEndSubDiagonal(btn);
        }

        private bool isEndHorizontal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                    break;
            }

            int countRight = 0;
            for (int i = point.X + 1; i < Cons.Chess_Board_Width; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }

            return countRight + countLeft == 5;
        }

        private bool isEndVertical(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countUp = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countUp++;
                }
                else
                    break;
            }

            int countDown = 0;
            for (int i = point.Y + 1; i < Cons.Chess_Board_Height; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countDown++;
                }
                else
                    break;
            }

            return countUp + countDown == 5;
        }

        private bool isEndPrimaryDiagonal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countUp = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countUp++;
                }
                else
                    break;
            }

            int countDown = 0;
            for (int i = 1; i < Cons.Chess_Board_Width - point.X; i++)
            {
                if (point.X + i >= Cons.Chess_Board_Width || point.Y + i >= Cons.Chess_Board_Height)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countDown++;
                }
                else
                    break;
            }

            return countUp + countDown == 5;
        }

        private bool isEndSubDiagonal(Button btn)
        {
            Point point = GetChessPoint(btn);

            int countUp = 0;
            for (int i = 0; i < Cons.Chess_Board_Width; i++)
            {
                if (point.X + i >= Cons.Chess_Board_Width || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countUp++;
                }
                else
                    break;
            }

            int countDown = 0;
            for (int i = 1; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y + i >= Cons.Chess_Board_Height)
                    break;

                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countDown++;
                }
                else
                    break;
            }

            return countUp + countDown == 5;
        }
        #endregion               
        #endregion
    }

    public class ButtonClickEvent : EventArgs
    {
        private Point clickedPoint;
        private string current;

        public Point ClickedPoint { get => clickedPoint; set => clickedPoint = value; }
        public string Current { get => current; set => current = value; }

        public ButtonClickEvent(Point point, string current)
        {
            this.ClickedPoint = point;
            this.Current = current;
        }
    }
}
