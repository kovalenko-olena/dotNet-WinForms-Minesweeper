using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    class SparkWithTrace : Spark
    {
        public int MiniSparksCount { get; set; }
        public float SparkingTime { get; private set; }
        const int SPARKS_PER_TIME = 1;

        public SparkWithTrace(Color c, PointF position, PointF speed, int miniSparksCount, float sparkingTime)
            : base(c, position, speed)
        {
            MiniSparksCount = miniSparksCount;
            SparkingTime = sparkingTime;
            minisparks = new List<Spark>(MiniSparksCount);
        }

        Random r = new Random();
        List<Spark> minisparks;

        private void CreateMiniSpark()
        {
            PointF nPosition = new PointF(
                this.Position.X,
                this.Position.Y
                );

            PointF nSpeed = new PointF(
                -this.Speed.X * 1.2F + (float)r.NextDouble() * 6F - 3F,
                -this.Speed.Y * 1.2F + (float)r.NextDouble() * 6F - 3F
                );
            Spark s = new Spark(Color.Black, nPosition, nSpeed);
            s.Size = 2F;
            s.OffSpeed = 0.2F;

            minisparks.Add(s);
        }

        public override void Update(double time)
        {
            base.Update(time);
            foreach (Spark s in minisparks)
            {
                s.Update(time);
            }

            if (this.alpha >= 0.4)
            {
                for (int i = 0; i < SPARKS_PER_TIME; i++)
                {
                    CreateMiniSpark();
                }
            }
        }

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            foreach (Spark s in minisparks)
            {
                s.Paint(g);
            }
        }


    }
}
