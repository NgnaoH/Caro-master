using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace Caro
{
    public partial class Form1 : Form
    {
        #region Properties
        ChessBoardManager ChessBoard;

        SocketManager socket;
        #endregion
        public Form1()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            ChessBoard = new ChessBoardManager(pnlChessBoard, txbName, pctbMark);
            ChessBoard.PlayerMarked += ChessBoard_PlayerMarked;
            ChessBoard.EndedGame += ChessBoard_EndedGame;

            ChessBoard.DrawChessBoard();

            socket = new SocketManager();
        }

        void EndGame()
        {
            pnlChessBoard.Enabled = false;
        }
        void NewGame()
        {
            ChessBoard.DrawChessBoard();
        }

        void ChessBoard_EndedGame(object sender, ButtonClickEvent e)
        {
            EndGame();

            socket.Send(new SocketData((int)SocketCommand.EndGame, e.Current, new Point()));
        }

        void ChessBoard_PlayerMarked(object sender, ButtonClickEvent e)
        {
            pnlChessBoard.Enabled = false;

            socket.Send(new SocketData((int)SocketCommand.SendPoint, "", e.ClickedPoint));

            Listen();
        }

        void QuitGame()
        {
            Application.Exit();
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("New Game ???", "Notification", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                NewGame();
                socket.Send(new SocketData((int)SocketCommand.NewGame, "", new Point()));
                pnlChessBoard.Enabled = true;
            }             
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QuitGame();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Quit Game ???", "Notification", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.Quit, "No connect", new Point()));
                }
                catch { }
            }
               
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("New Game ???", "Notification", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                NewGame();
                socket.Send(new SocketData((int)SocketCommand.NewGame, "", new Point()));
                pnlChessBoard.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            QuitGame();
        }

        private void btnLAN_Click(object sender, EventArgs e)
        {
            socket.IP = txbIP.Text;

            if (!socket.ConnectServer())
            {
                txbStatus.Text = "Connecting ...";
                socket.isServer = true;
                pnlChessBoard.Enabled = true;
                btnLAN.Enabled = false;
                socket.CreateServer();
            }
            else
            {
                txbStatus.Text = "Connected";
                socket.isServer = false;
                pnlChessBoard.Enabled = false;
                btnLAN.Enabled = false;
                socket.Send(new SocketData((int)SocketCommand.Notify, "Connected", new Point()));
                Listen();
            }

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txbIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (String.IsNullOrEmpty(txbIP.Text))
                txbIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
        }

        void Listen()
        {
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();

                    ProcesssData(data);
                }
                catch               {

                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void ProcesssData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.Notify:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        btnLAN.Enabled = false;
                        txbStatus.Text = data.Message;
                    }));
                    break;
                case (int)SocketCommand.SendPoint:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pnlChessBoard.Enabled = true;
                        ChessBoard.OtherPlayerMark(data.Point);
                    }));
                    break;
                case (int)SocketCommand.NewGame:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pnlChessBoard.Enabled = false;
                        NewGame();
                    }));
                    break;
                case (int)SocketCommand.EndGame:
                    MessageBox.Show("Player" + data.Message + " wonˆˆ");
                    break;
                case (int)SocketCommand.Quit:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        socket.isServer = true;
                        ChessBoard.DrawChessBoard();
                        btnLAN.Enabled = true;
                        txbStatus.Text = data.Message;
                        MessageBox.Show("Player has quit");
                    }));
                    break;
                default:
                    break;
            }
            Listen();
        }
    }
}
