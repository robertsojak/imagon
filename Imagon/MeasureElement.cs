using System.Drawing;

namespace Imagon
{
    public class MeasureElement : CanvasElement
    {
        public Point? A { get; set; }
        public Point? B { get; set; }

        public override void Draw(Graphics graphics)
        {
            var pen = new Pen(Color.Fuchsia, 1);

            if (A != null)
                graphics.DrawRectangle(pen, A.Value.X - 3, A.Value.Y - 3, 6, 6);
            if (B != null)
                graphics.DrawRectangle(pen, B.Value.X - 3, B.Value.Y - 3, 6, 6);

            if (A != null && B != null)
            {
                graphics.DrawLine(pen, A.Value.X, A.Value.Y, B.Value.X, B.Value.Y);
                graphics.DrawString("??", new Font("Arial", 10, FontStyle.Regular), new SolidBrush(Color.Fuchsia), A.Value.X, A.Value.Y);
            }
        }
    }
}