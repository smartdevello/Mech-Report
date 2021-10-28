using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Text;
using System.IO;

namespace Mech_Report
{
    public class MechRenderer
    {
        private int width = 0, height = 0;
        private double totHeight = 1050;
        private Bitmap bmp = null;
        private Graphics gfx = null;
        private List<MechData> data = null;
        private Dictionary<string, int> printers = null;

        List<MechData> NF = null;
        List<MechData> MF = null;
        List<MechData> DF = null;
        List<MechData> HF = null;
        List<Calibration> OoC = null;
        Random rnd = new Random();
        Image crossImg = Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "assets", "cross.png"));
        Image logoImg = Image.FromFile(Path.Combine(Directory.GetCurrentDirectory(), "assets", "logo.png"));
        public MechRenderer(int width, int height)
        {
            this.width = width;
            this.height = height;

            NF = new List<MechData>();
            MF = new List<MechData>();
            DF = new List<MechData>();
            HF = new List<MechData>();
            OoC = new List<Calibration>();
        }
        public void setRenderSize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        public Dictionary<string, int> getPrinters()
        {
            return this.printers;
        }
        public int getPrintersCount()
        {
            if (this.printers == null) return 0;
            return this.printers.Count();
        }
        public List<MechData> getData()
        {
            return this.data;
        }
        public void setChatData(List<MechData> data, Dictionary<string, int> printers)
        {
            this.data = data;
            this.printers = printers;
            MF = data.Where(e => e.result == "MF").ToList();
            DF = data.Where(e => e.result == "DF").ToList();
            HF = data.Where(e => e.result == "HF").ToList();
            NF = data.Where(e => e.result != "MF" && e.result != "DF" && e.result != "HF").ToList();

        }

        public Point convertCoord(Point a)
        {
            double px = height / totHeight;

            Point res = new Point();
            res.X = (int)((a.X + 20) * px);
            res.Y = (int)((1000 - a.Y) * px);
            return res;
        }
        public PointF convertCoord(PointF p)
        {
            double px = height / totHeight;
            PointF res = new PointF();
            res.X = (int)((p.X + 20) * px);
            res.Y = (int)((1000 - p.Y) * px);
            return res;
        }
        public Bitmap getBmp()
        {
            return this.bmp;
        }

        public void draw(int currentChartIndex)
        {
            if (bmp == null)
                bmp = new Bitmap(width, height);
            else {
                if (bmp.Width != width || bmp.Height != height)
                {
                    bmp.Dispose();
                    bmp = new Bitmap(width, height);

                    gfx.Dispose();
                    gfx = Graphics.FromImage(bmp);
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                }
            }

            if (gfx == null)
            {
                gfx = Graphics.FromImage(bmp);
                gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            } else
            {
                gfx.Clear(Color.Transparent);
            }
                
            drawRectangle(new Pen(Color.Black, 4), new Rectangle(0, 920, 300, 900));
            drawsubRectangle();


            drawImg(logoImg, new Point(20, 18), new Size(150, 70));
            if (data == null) return;
            //drawGuideLine(0, 40, 210);
            parseOoC();

            drawGuideLine(0, 150, 200, 10000);

            List<Calibration> bf = OoC.Where(o => o.calibration_type == "BF").ToList();
            int maxVal = 0;
            if (bf.Count > 0)
                maxVal = bf.Max(o => o.deviation);
            else maxVal = 0;

            maxVal = drawGuideLine(150, 250, 200, maxVal);
            foreach(var item in bf)
            {
                drawCross(item.deviation, 150, 250, 200, maxVal, Color.Red);
            }

            List<Calibration> mf = OoC.Where(o => o.calibration_type == "MF").ToList();
            if (mf.Count > 0)
                maxVal = mf.Max(o => o.deviation);
            else maxVal = 0;

            maxVal = drawGuideLine(400, 250, 200, maxVal);
            foreach (var item in mf) drawCross(item.deviation, 400, 250, 200, maxVal, Color.Red);

            List<Calibration> tf = OoC.Where(o => o.calibration_type == "TF").ToList();
            if (tf.Count > 0)
                maxVal = tf.Max(o => o.deviation);
            else maxVal = 0;

            maxVal = drawGuideLine(650, 250, 200, maxVal);
            foreach (var item in tf) drawCross(item.deviation, 650, 250, 200, maxVal, Color.Red);


            //Draw Pie Chart
            float nextAngle = 270.0f;
            float angle = NF.Count / (float)data.Count * 360.0f;
            float percentage;

            drawPie(Color.Gray, new Point(320, 900), new Size(200, 200), nextAngle, angle, "  NF");
            fillRectangle(Color.Gray, new Rectangle(320, 640, 15, 15));
            percentage = NF.Count * 100.0f / (float)data.Count;            
            drawString(new Point(340, 640), "NF " + Math.Round(percentage, 2).ToString() + "%", 11);

            nextAngle += angle;
            angle = HF.Count / (float)data.Count * 360.0f;
            drawPie(Color.Red, new Point(320, 900), new Size(200, 200), nextAngle, angle, "  HF");
            fillRectangle(Color.Red, new Rectangle(450, 640, 15, 15));
            percentage = HF.Count * 100.0f / (float)data.Count;
            drawString(new Point(470, 640), "HF " + Math.Round(percentage, 2).ToString() + "%", 11);


            nextAngle += angle;
            angle = MF.Count / (float)data.Count * 360.0f;
            drawPie(Color.Green, new Point(320, 900), new Size(200, 200), nextAngle, angle, "  MF");

            fillRectangle(Color.Green, new Rectangle(320, 600, 15, 15));
            percentage = MF.Count * 100.0f / (float)data.Count;
            drawString(new Point(340, 600), "MF " + Math.Round(percentage, 2).ToString() + "%", 11);



            nextAngle += angle;
            angle = DF.Count / (float)data.Count * 360.0f;
            drawPie(Color.Pink, new Point(320, 900), new Size(200, 200), nextAngle, angle, "  DF");
            fillRectangle(Color.Pink, new Rectangle(450, 600, 15, 15));
            percentage = DF.Count * 100.0f / (float)data.Count;
            drawString(new Point(470, 600), "DF " + Math.Round(percentage, 2).ToString() + "%", 11);

            drawCenteredString("ACTUAL", new Rectangle(320, 930, 250, 30), Brushes.Black);
            //drawString(new Point(480, 930), "ACTUAL");


            //Draw Bar Chart            
            drawCenteredString("BASE LINE", new Rectangle(320, 530, 250, 30), Brushes.Black);

            drawLine(new Point(350, 500), new Point(350, 300), Color.Gray);
            drawString(new Point(345, 295), "0", 8);

            drawLine(new Point(420, 500), new Point(420, 300), Color.Gray);
            drawString(new Point(400, 295), "5000%", 8);


            drawLine(new Point(490, 500), new Point(490, 300), Color.Gray);
            drawString(new Point(470, 295), "10000%", 8);

            drawLine(new Point(560, 500), new Point(560, 300), Color.Gray);
            drawString(new Point(530, 295), "15000%(+)", 8);


            //70px per 25 percent
            //drawString(new Point(320, 590), "NF", 10);

            //drawString(new Point(320, 540), "MF", 10);

            List<Calibration> temp1 = new List<Calibration>();
            List<Calibration> temp2 = new List<Calibration>();

            double avg1 = 0, avg2 = 0;
            drawString(new Point(320, 460), "HF", 10);
            temp1 = OoC.Where(o => o.result == "HF").ToList();
            if (temp1.Count > 0)
                avg1 = temp1.Average(o => o.deviation);
            else avg1 = 0;

            if (avg1 > 15000) avg1 = 15000;
            fillRectangle(Color.Red, new Rectangle(350, 470, (int)(70 / 5000.0 * avg1), 40));

            drawString(new Point(320, 370), "DF", 10);
            temp2 = OoC.Where(o => o.result == "DF").ToList();
            if (temp2.Count > 0)
                avg2 = temp2.Average(o => o.deviation);
            else avg2 = 0;
            if (avg2 > 15000) avg2 = 15000;
            fillRectangle(Color.Pink, new Rectangle(350, 380, (int)(70 / 5000.0 * avg2), 40));



            if (OoC.Count > 0)
            {
                //Draw OoC
                //drawString(new Point(390, 270), "OoC");
                drawCenteredString("OoC", new Rectangle(320, 220, 200, 30), Brushes.Black);
                drawOoC();
            }




            //Draw Printers

            var sorted_printers = printers.OrderByDescending(o => o.Value);
            int height_gap = 40;
            int width_gap = 270;
            int gapCount = printers.Count() / 20;
            if (printers.Count() % 20 != 0) gapCount++;


            drawCenteredString("REPRESENTATION", new Rectangle(550, 950, 270 * Math.Min(gapCount, 5), 30), Brushes.Black);
            string msg = sorted_printers.Count().ToString();
            if (gapCount > 5)
            {
                int currentPrinterCnt = Math.Min(sorted_printers.Count(), currentChartIndex * 100 + 100) - currentChartIndex * 100;
                msg = string.Format("{0} of {1}", currentPrinterCnt, sorted_printers.Count());
            }
            drawCenteredString(msg, new Rectangle(550, 920, 270 * Math.Min(gapCount, 5), 100), Brushes.Red, 40);
            drawCenteredString("HF ALERT-Precint", new Rectangle(600, 840, 200, 40), Brushes.Red, 13);

            int len = sorted_printers.Count();
            for (int i = currentChartIndex * 100; i < Math.Min(sorted_printers.Count(), currentChartIndex * 100 + 100); i++)
            {
                int j = i % 100;
                drawString(new Point(600 + width_gap * (j / 20), 800 - height_gap * (j % 20)), sorted_printers.ElementAt(i).Key, 8);
                percentage = sorted_printers.ElementAt(i).Value * 100 / (float)data.Count;
                drawPercentageLine(percentage, 750 + width_gap * (j / 20), 800 - height_gap * (j % 20));
            }
            drawCenteredString(data.Count().ToString(), new Rectangle(800 + width_gap * (Math.Min(gapCount, 5) - 1), 100, width_gap, 100), Brushes.Black, 40);

        }
        public void drawCenteredString(string content, Rectangle rect, Brush brush , int fontSize = 15)
        {

            using (Font font1 = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.Point))
            {

                // Create a StringFormat object with the each line of text, and the block
                // of text centered on the page.
                double px = height / totHeight;
                rect.Location = convertCoord(rect.Location);
                rect.Width = (int)(px * rect.Width);
                rect.Height = (int)(px * rect.Height);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                // Draw the text and the surrounding rectangle.
                gfx.DrawString(content, font1, brush, rect, stringFormat);
                //gfx.DrawRectangle(Pens.Black, rect);
            }
        }
        public void drawOoC()
        {

            Brush brush = new SolidBrush(Color.Red);
            PointF[] pyramid = new PointF[4];
            pyramid[0] = new PointF(320, 190);
            pyramid[1] = new PointF(520, 190);
            pyramid[2] = new PointF(489, 140);
            pyramid[3] = new PointF(351, 140);
            fillPolygon(brush, pyramid);
            if (OoC.Count == 0) return;
            int maxOoC = OoC.Max(o => o.deviation);
            drawCenteredString(string.Format("{0}%", maxOoC), new Rectangle(351, 180, 138, 30), Brushes.White);
            drawString(new Point(520, 180), "Highest", 10);

            brush = new SolidBrush(Color.FromArgb(255, 87, 87));
            pyramid[0] = new PointF(354, 135);
            pyramid[1] = new PointF(486, 135);
            pyramid[2] = new PointF(454, 85);
            pyramid[3] = new PointF(386, 85);
            fillPolygon(brush, pyramid);
            int avgOoC = (int)OoC.Average(o => o.deviation);
            drawCenteredString(string.Format("{0}%", avgOoC), new Rectangle(380, 125, 80, 30), Brushes.White, 10);
            drawString(new Point(520, 125), "Average", 10);


            brush = new SolidBrush(Color.FromArgb(255, 102, 196));
            pyramid[0] = new PointF(389, 80);
            pyramid[1] = new PointF(451, 80);
            pyramid[2] = new PointF(420, 30);
            pyramid[3] = new PointF(420, 30);
            fillPolygon(brush, pyramid);
            int minOoC = (int)OoC.Min(o => o.deviation);
            drawCenteredString(string.Format("{0}%", minOoC), new Rectangle(389, 80, 62, 20), Brushes.White, 10);
            drawString(new Point(520, 70), "Lowest", 10);


            brush.Dispose();
        }

        private void fillPolygon(Brush brush ,PointF[] points)
        {
            for (int i = 0; i< points.Length; i++)
            {
                points[i] = convertCoord(points[i]);
            }
            gfx.FillPolygon(brush, points);
        }
        private void drawPercentageLine(float percent, int X, int Y)
        {
            fillRectangle(Color.Black, new Rectangle(X, Y, 20, 20));
            fillRectangle(Color.Black, new Rectangle(X + 80, Y, 20, 20));
            drawLine(new Point(X, Y - 10), new Point(X + 80, Y - 10), Color.Black, 4);
            if (percent != 0.0F)
            {
                drawString(new Point(X + 30, Y + 10), string.Format("{0:F}%", percent), 8);
            }            
            
        }

        public void drawsubRectangle()
        {
            Pen dotpen = new Pen(Color.Black, 2);
            Brush brush = new SolidBrush(Color.FromArgb(100, Color.Black));

            Point p1 = new Point(0, 170);
            Point p2 = new Point(300, p1.Y);
            p1 = convertCoord(p1);
            p2 = convertCoord(p2);
            gfx.DrawLine(dotpen, p1, p2);
            drawCenteredString("BF", new Rectangle(0, 420, 300, 250), brush, 40);

            p1 = new Point(0, 420);
            p2 = new Point(300, p1.Y);
            p1 = convertCoord(p1);
            p2 = convertCoord(p2);
            gfx.DrawLine(dotpen, p1, p2);
            drawCenteredString("MF", new Rectangle(0, 670, 300, 250), brush, 40);

            p1 = new Point(0, 670);
            p2 = new Point(300, p1.Y);
            p1 = convertCoord(p1);
            p2 = convertCoord(p2);
            gfx.DrawLine(dotpen, p1, p2);
            drawCenteredString("TF", new Rectangle(0, 920, 300, 250), brush, 40);

            brush.Dispose();
            dotpen.Dispose();
        }

        private void parseOoC()
        {
            OoC.Clear();
            foreach (var item in data)
            {
                // "RTF:N_1100 RMF:N_1400 RBF:N_104300"
                item.deviation = new int[] { };
                item.calibration_type = new string[] { };
                item.direction = new string[] { };

                if (string.IsNullOrEmpty(item._deviation)) continue;
                string[] subs = item._deviation.Split(' ');
                for (int i = 0; i < subs.Length; i++)
                {
                    // "RBF:N_104300"
                    var parts = subs[i].Split('_');
                    int deviation = Convert.ToInt32(parts[1]);
                    string direction = parts[0].Split(':')[1];
                    string calibration_type = parts[0].Split(':')[0].Substring(1);

                    OoC.Add(new Calibration
                    {
                        deviation = deviation,
                        direction = direction,
                        calibration_type = calibration_type,
                        image_id = item.image_id,
                        result = item.result
                    });
                }

            }
        }

        public void drawCrossBar(Point p1, Point p2, Color color)
        {
            drawLine(p1, p2, color, 4);
        }
        public void drawCross(int Y, int baseHeight, int regionHeight, int minVal, int maxVal, Color color)
        {
            double scale = regionHeight / (Math.Log(maxVal) - Math.Log(minVal));
            Point o = new Point(rnd.Next(20, 250), (int)(baseHeight + (Math.Log(Y) - Math.Log(minVal)) * scale));

            o.X = o.X - 5;
            o.Y = o.Y + 5;
            drawLine(new Point(o.X - 5, o.Y + 5), new Point(o.X + 5, o.Y - 5), color, 4);
            drawLine(new Point(o.X - 5, o.Y - 5), new Point(o.X + 5, o.Y + 5), color, 4);

        }

        public int drawGuideLine(int baseHeight, int regionHeight, int minVal, int maxVal)
        {

            foreach(int h in Helper.guide_lines)
            {
                if (h >= maxVal) {
                    maxVal = h; 
                    break;
                }
            }
            double scale = regionHeight / ( Math.Log(maxVal)  - Math.Log(minVal));
            Pen pen = new Pen(Color.Black);
            pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

            foreach (int h in Helper.guide_lines)
            {
                
                Point p1 = new Point(20, (int)(baseHeight + (Math.Log(h) - Math.Log(minVal)) * scale));
                Point p2 = new Point(250, p1.Y);
                drawString(new Point(250, p1.Y + 5), string.Format("{0}%", h), 7);
                p1 = convertCoord(p1);
                p2 = convertCoord(p2);
                gfx.DrawLine(pen, p1, p2);
                if (h >= maxVal) break;
            }
            pen.Dispose();

            return maxVal;
        }
        public void drawLine(Point p1, Point p2, Color color, int linethickness = 1)
        {
            if (color == null)
                color = Color.Gray;

            p1 = convertCoord(p1);
            p2 = convertCoord(p2);
            gfx.DrawLine(new Pen(color, linethickness), p1, p2);

        }
        public void drawString(Point o, string content, int font = 15)
        {

            double px = height / totHeight;
            o = convertCoord(o);

            // Create font and brush.
            Font drawFont = new Font("Arial", font);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            gfx.DrawString(content, drawFont, drawBrush, o.X, o.Y);
            
        }
        public void drawString(Color color, Point o, string content, int font = 15)
        {

            double px = height / totHeight;
            o = convertCoord(o);

            // Create font and brush.
            Font drawFont = new Font("Arial", font);
            SolidBrush drawBrush = new SolidBrush(color);

            gfx.DrawString(content, drawFont, drawBrush, o.X, o.Y);

            drawFont.Dispose();
            drawBrush.Dispose();

        }
        public void fillRectangle(Color color, Rectangle rect)
        {
            rect.Location = convertCoord(rect.Location);
            double px = height / totHeight;
            rect.Width = (int)(rect.Width * px);
            rect.Height = (int)(rect.Height * px);

            Brush brush = new SolidBrush(color);
            gfx.FillRectangle(brush, rect);
            brush.Dispose();

        }
        public void drawRectangle(Pen pen, Rectangle rect)
        {
            rect.Location = convertCoord(rect.Location);
            double px = height / totHeight;
            rect.Width = (int)(rect.Width * px);
            rect.Height = (int)(rect.Height * px);
            gfx.DrawRectangle(pen, rect);
        }

        public void drawImg(Image img, Point o, Size size)
        {
            double px = height / totHeight;
            o = convertCoord(o);
            Rectangle rect = new Rectangle(o, new Size((int)(size.Width * px), (int)(size.Height * px)));
            gfx.DrawImage(img, rect);
            
        }

        public void drawPie(Color color, Point o, Size size, float startAngle, float sweepAngle, string content = "")
        {
            // Create location and size of ellipse.
            double px = height / totHeight;
            Rectangle rect = new Rectangle(convertCoord(o), size);
            // Draw pie to screen.            
            Brush grayBrush = new SolidBrush(color);
            gfx.FillPie(grayBrush, rect, startAngle, sweepAngle);

            //o.X += size.Width / 2;
            //o.Y -= size.Height / 2;
            //float radius = size.Width * 0.3f;
            //o.X += (int)(radius * Math.Cos(Helper.DegreesToRadians(startAngle + sweepAngle / 2)));
            //o.Y -= (int)(radius * Math.Sin(Helper.DegreesToRadians(startAngle + sweepAngle / 2)));
            //content += "\n" + string.Format("{0:F}%", sweepAngle * 100.0f / 360.0f);
            //drawString(o, content, 9);
        }





    }
}
