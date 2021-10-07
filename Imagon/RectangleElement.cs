using System;
using System.Drawing;

namespace Imagon
{
    public class RectangleElement : CanvasElement
    {
        public Pen Pen { get; }

        private Rectangle _rect;


        public RectangleElement(Point from, Point to)
        {
            Pen = new Pen(Color.Fuchsia);
            Change(from, to);
        }


        public override void Draw(Graphics graphics)
        {
            graphics.DrawRectangle(Pen, _rect);
        }

        public void Change(Point from, Point to)
        {
            int x = Math.Min(from.X, to.X);
            int y = Math.Min(from.Y, to.Y);
            int w = Math.Abs(from.X - to.X);
            int h = Math.Abs(from.Y - to.Y);
            _rect = new Rectangle(x, y, w, h);
        }
    }
}