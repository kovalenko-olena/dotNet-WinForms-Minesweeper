using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    internal class MinerButton : Button
    {
        public bool isBomb;
        public bool isFlag;
        public bool isClickable;
        public bool wasAdded;
        public bool wasOpen;
        public int xCoord;
        public int yCoord;
    }
}
