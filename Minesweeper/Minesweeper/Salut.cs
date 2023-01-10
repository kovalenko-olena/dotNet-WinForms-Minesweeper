using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    abstract class Salut
    {
        public abstract void Start();
        public abstract void Update(double time);
        public abstract void Paint(Graphics g);
        public virtual float Gravity { get; set; }
    }
}
