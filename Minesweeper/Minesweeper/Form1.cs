using System.Runtime.ConstrainedExecution;

namespace Minesweeper
{
    public partial class Minesweeper : Form
    {
        public Minesweeper()
        {
            InitializeComponent();
        }

        int height = 10;
        int width = 10;
        int offset = 40;
        int bombPercent = 15;
        int indent;
        MinerButton[,] field;

        private void Minesweeper_Load(object sender, EventArgs e)
        {
            height = trackBar1.Value;
            width = trackBar1.Value;
            bombPercent = trackBar2.Value;
            label1.Text = "field size " + height.ToString();
            label2.Text = "bombs " + bombPercent.ToString() + "%";
            indent = (int)1.2*btnStart.Height;
        }
        void Generate()
        {
            field = new MinerButton[height, width];

            // AddButton();
            GenerateField();
        }
        public void GenerateField()
        {
            Random rng = new Random();
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
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
                    if (rng.Next(0, height * width) < bombPercent)
                    {
                        newButton.isBomb = true;
                    }
                    newButton.xCoord = x;
                    newButton.yCoord = y;
                    //AddButton();
                    Controls.Add(newButton);
                    newButton.MouseUp += new MouseEventHandler(FieldButtonClick);
                    field[x, y] = newButton;
                }
            }
            /*Thread[] threads = new Thread[height];
            for (int i = 0; i < width; i++)
            {
                threads[i] = new Thread(new ThreadStart(FieldForForm(i)));
            }*/



        }

        /*
        private void AddButton()
        {
            
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(this.AddButton));
            }
            else
            {
                //Thread.Sleep(20);
                Random rng = new Random();
                for (int x = 0; x < height; x++)
                {
                    
                    for (int y = 0; y < width; y++)
                    {
                        FieldButton newButton = new FieldButton();
                        newButton.FlatStyle = FlatStyle.Popup;
                        newButton.FlatAppearance.BorderSize = 1;
                        newButton.FlatAppearance.BorderColor = Color.DimGray;
                        newButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12, FontStyle.Bold);
                        newButton.Location = new Point(offset / 10 + x * offset, indent + offset + y * offset);
                        newButton.Size = new Size(offset, offset);
                        newButton.isClickable = true;
                        newButton.wasOpen = false;
                        if (rng.Next(0, height * width) < bombPercent)
                        {
                            newButton.isBomb = true;
                        }
                        newButton.xCoord = x;
                        newButton.yCoord = y;
                        //AddButton();
                        Controls.Add(newButton);
                        newButton.MouseUp += new MouseEventHandler(FieldButtonClick);
                        field[x, y] = newButton;
                    }
                }
            }
            
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Thread addControlThread =
                new Thread(new ThreadStart(this.AddButton));
            addControlThread.Start();
        }*/

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
            int cellsOpened = 0;
            for (int x = 0; x < height; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (field[x, y].wasOpen)
                    {
                        cellsOpened++;
                    }
                }
            }
            if (cellsOpened == width * height)
            {
                Color c = Color.FromArgb(255);
                
                MessageBox.Show("Winner!", "minesweeper");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DelButtons();
            bombPercent = trackBar2.Value;
            height = trackBar1.Value;
            width = trackBar1.Value;
            btnStart.Image = Properties.Resources.happy;

            Generate();
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            bombPercent = trackBar2.Value;
            label2.Text = "bombs " + bombPercent.ToString() + "%";
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            height = trackBar1.Value;
            width = trackBar1.Value;
            label1.Text = "field size " + height.ToString();
        }
    }
}