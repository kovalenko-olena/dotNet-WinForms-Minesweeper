using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Runtime.ConstrainedExecution;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            saluts = new List<Salut>();
        }

        int height = 10;
        int width = 10;
        int offset = 30;
        int bombPercent;
        int indent;
        int countVin = 0;
        List<Salut> saluts;
        Random r = new Random();
        MinerButton[,] field;


        private void Minesweeper_Load(object sender, EventArgs e)
        {
            height = trackBar1.Value;
            width = trackBar1.Value;
            bombPercent = height * width * trackBar2.Value / 100;
            label1.Text = "field size " + height.ToString();
            label2.Text = "bombs " + trackBar2.Value.ToString() + "%";
            indent = (int)1.2 * btnStart.Height;
        }

        public void GenerateField()
        {
            field = new MinerButton[width, height];
            Random rng = new Random();
            int bombCount = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    MinerButton newButton = new MinerButton();
                    newButton.FlatStyle = FlatStyle.Popup;
                    newButton.FlatAppearance.BorderSize = 1;
                    newButton.FlatAppearance.BorderColor = Color.DimGray;
                    newButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12, FontStyle.Bold);
                    newButton.Location = new Point(offset / 10 + x * offset, indent + offset + y * offset);
                    newButton.Size = new Size(offset, offset);
                    newButton.isClickable = true;
                    newButton.wasOpen = false;
                    if (rng.Next(0, height * width) < bombPercent && bombCount < bombPercent)
                    {
                        newButton.isBomb = true;
                        bombCount++;
                    }
                    newButton.xCoord = x;
                    newButton.yCoord = y;
                    Controls.Add(newButton);
                    newButton.MouseUp += new MouseEventHandler(FieldButtonClick);
                    field[x, y] = newButton;
                }
            }
            if (bombPercent > bombCount)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (rng.Next(0, height * width) < bombPercent && bombCount < bombPercent && field[x, y].isBomb == false)
                        {
                            field[x, y].isBomb = true;
                            bombCount++;
                        }
                    }
                }
            }
        }


        void FieldButtonClick(object sender, MouseEventArgs e)
        {
            MinerButton clickedButton = (MinerButton)sender;
            if (e.Button == MouseButtons.Left && clickedButton.isClickable)
            {
                if (clickedButton.isBomb)
                {
                    Explode(clickedButton);
                }
                else
                {
                    OpenRegion(clickedButton);
                }
            }

            if (e.Button == MouseButtons.Right)
            {
                if (!clickedButton.wasOpen || clickedButton.isFlag)
                {
                    clickedButton.isClickable = !clickedButton.isClickable;
                    if (!clickedButton.isClickable)
                    {
                        clickedButton.Image = Properties.Resources.flag;
                        clickedButton.isFlag = true;
                        clickedButton.wasOpen = true;
                    }
                    else
                    {
                        clickedButton.Image = null;
                        clickedButton.wasOpen = false;
                    }
                }
            }
            CheckWin();
        }

        void Explode(MinerButton clickedButton)
        {
            if (clickedButton.Image != Properties.Resources.boom)
            {
                clickedButton.Image = Properties.Resources.boom_fatal;
                clickedButton.Text = "";
                clickedButton.BackColor = Color.DimGray;
            }
            foreach (MinerButton button in field)
            {
                if (button.isBomb && button != clickedButton)
                {
                    button.Image = Properties.Resources.boom;
                    button.wasOpen = true;
                }
                else
                {
                    if (button != clickedButton)
                    {
                        int countBomb = CountBombsAround(button.xCoord, button.yCoord);
                        button.ForeColor = countBomb == 1 ? Color.Blue : countBomb == 2 ? Color.Green : countBomb == 3 ? Color.Red : Color.Purple;
                        button.Text = countBomb > 0 ? "" + countBomb : "";

                    }
                }
                button.Enabled = false;
            }

            btnStart.Image = Properties.Resources.sad;
            countVin = 0;
        }
        void OpenRegion(MinerButton clickedButton)
        {
            Queue<MinerButton> queue = new Queue<MinerButton>();
            queue.Enqueue(clickedButton);
            clickedButton.wasAdded = true;
            while (queue.Count > 0)
            {
                MinerButton currentCell = queue.Dequeue();
                int bombsAround = CountBombsAround(currentCell.xCoord, currentCell.yCoord);
                OpenCell(currentCell);

                if (bombsAround == 0)
                {
                    for (int x = currentCell.xCoord - 1; x <= currentCell.xCoord + 1; x++)
                    {
                        for (int y = currentCell.yCoord - 1; y <= currentCell.yCoord + 1; y++)
                        {
                            if (x == currentCell.xCoord && y == currentCell.yCoord)
                            {
                                continue;
                            }
                            if (x >= 0 && x < width && y < height && y >= 0 && !field[x, y].wasAdded)
                            {
                                queue.Enqueue(field[x, y]);
                                field[x, y].wasAdded = true;
                            }
                        }
                    }
                }
                else
                {
                    if (bombsAround == CountFlagsAround(currentCell.xCoord, currentCell.yCoord))
                    {
                        for (int x = currentCell.xCoord - 1; x <= currentCell.xCoord + 1; x++)
                        {
                            for (int y = currentCell.yCoord - 1; y <= currentCell.yCoord + 1; y++)
                            {
                                if (x == currentCell.xCoord && y == currentCell.yCoord)
                                {
                                    continue;
                                }
                                if (x >= 0 && x < width && y < height && y >= 0 && !field[x, y].wasAdded && !field[x, y].isFlag)
                                {
                                    queue.Enqueue(field[x, y]);
                                    field[x, y].wasAdded = true;
                                }
                            }
                        }

                    }
                }
            }
        }

        void OpenCell(MinerButton clickedButton)
        {
            if (clickedButton.isBomb)
            {
                Explode(clickedButton);
            }
            int bombsAround = CountBombsAround(clickedButton.xCoord, clickedButton.yCoord);
            if (bombsAround != 0)
            {
                clickedButton.ForeColor = bombsAround == 1 ? Color.Blue : bombsAround == 2 ? Color.Green : bombsAround == 3 ? Color.Red : Color.Purple;
                clickedButton.Text = "" + bombsAround;
            }
            clickedButton.BackColor = Color.LightGray;
            clickedButton.wasOpen = true;
        }

        int CountBombsAround(int xCoord, int yCoord)
        {
            int bombsAround = 0;
            for (int x = xCoord - 1; x <= xCoord + 1; x++)
            {
                for (int y = yCoord - 1; y <= yCoord + 1; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        if (field[x, y].isBomb)
                        {
                            bombsAround++;
                        }
                    }
                }
            }
            return bombsAround;
        }

        int CountFlagsAround(int xCoord, int yCoord)
        {
            int flagsAround = 0;
            for (int x = xCoord - 1; x <= xCoord + 1; x++)
            {
                for (int y = yCoord - 1; y <= yCoord + 1; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        if (field[x, y].isFlag)
                        {
                            flagsAround++;
                        }
                    }
                }
            }
            return flagsAround;
        }
        private void DelButtons()
        {
            if (field != null)
            {
                foreach (MinerButton button in field)
                {
                    Controls.Remove(button);
                }
            }
        }

        void CheckWin()
        {
            if (btnStart.Image == Properties.Resources.sad) return;
            int cellsOpened = 0;
           
            foreach(MinerButton button in field)
            {
                if (button.wasOpen)
                {
                    cellsOpened++;
                }
            }
            if (cellsOpened == width * height)
            {
                foreach (MinerButton button in field)
                {
                    button.Hide();
                }
                WinnerSalut();
                //MessageBox.Show("Winner!", "minesweeper");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DelButtons();
            if (customSize.Checked == false)
            {
                height = trackBar1.Value;
                width = trackBar1.Value;
            }
            else
            {
                height = (int)((this.Height - indent - offset*2.7) / offset);
                width = (int)((this.Width- offset * 0.8) / offset);
            }
            bombPercent = height * width * trackBar2.Value / 100;
            btnStart.Image = Properties.Resources.happy;

            GenerateField();
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            bombPercent = height * width * trackBar2.Value / 100;
            label2.Text = "bombs " + trackBar2.Value.ToString() + "%";
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            height = trackBar1.Value;
            width = trackBar1.Value;
            label1.Text = "field size " + height.ToString();
        }

        private void WinnerSalut()
        {
            countVin++;
            Color[] clrs = new Color[] { Color.Red, Color.Blue, Color.Yellow, Color.Magenta, Color.Green, Color.Orange };
            int cnum = r.Next(clrs.GetLength(0));
            Point point;

            for (int i = 0; i < countVin; i++)
            {
                cnum = r.Next(clrs.GetLength(0));
                try
                {
                    point = new Point((offset / 10 + width * offset) / (r.Next(1, 10)), (offset / 10 + height * offset) - ((offset / 10 + height * offset) / (r.Next(1, 10))));
                }
                catch (Exception ex)
                {
                    point = new Point(0, 0);
                }

                ColorSalut salut = new ColorSalut(clrs[cnum], -0.7F, point);

                saluts.Add(salut);
                salut.Start();
            }

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            foreach (Salut s in saluts)
            {
                s.Paint(e.Graphics);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Salut s in saluts)
            {
                s.Update(3.0);
            }
            this.Refresh();
        }

        private void customSize_CheckedChanged(object sender, EventArgs e)
        {
            if (customSize.Checked)
            {
                this.AutoSize = false;
                trackBar1.Enabled = false;
            }
            else
            {
                this.AutoSize = true;
                trackBar1.Enabled = true;
            }
        }
    }
}