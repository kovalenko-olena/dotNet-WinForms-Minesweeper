using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    class Spark
    {
        protected float alpha=1.0F;
        public Spark(Color c, PointF position, PointF speed)
        {
            Clr = c;
            Position = position;
            Speed = speed;
            Size = 8F;
        }

        public Color Clr { get; set; }
        public float Size { get; set; }
        public PointF Position { get; set; }
        public PointF Speed { get; set; }
        public float Gravity { get; set; }
        public float OffSpeed { get; set; } = 0.1F;
        

        public bool IsAlive
        {
            get { return alpha > 0; }
        }

        public virtual void Update(double time)
        {
            Position = new PointF(
                Position.X + Speed.X * (float)time,
                Position.Y + Speed.Y * (float)time);

            Speed = new PointF(
                Speed.X,
                Speed.Y - Gravity * (float)time
                );

            alpha = alpha - OffSpeed;
        }

        public virtual void Paint(Graphics g)
        {
            if (!IsAlive) return;

            Color c = Color.FromArgb(Convert.ToInt32(Math.Round(255 * alpha)), Clr);
            using (Brush b = new SolidBrush(c))
            {
                g.FillEllipse(b, Position.X - Size / 2, Position.Y - Size / 2, Size, Size);
            }
        }
    }
}
