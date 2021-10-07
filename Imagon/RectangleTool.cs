using System.Drawing;

namespace Imagon
{
    public class RectangleTool : CanvasTool
    {
        private Point _from;
        private RectangleElement _element;


        public RectangleTool(Canvas canvas)
            : base(canvas)
        {
        }


        public override void OnMouseDown(int x, int y)
        {
            base.OnMouseDown(x, y);

            _from = new Point(x, y);
            _element = new RectangleElement(_from, new Point(x, y));
            _canvas.AddElement(_element);
            _canvas.Refresh();
        }
        public override void OnMouseMove(int x, int y)
        {
            base.OnMouseMove(x, y);

            if (_element == null)
                return;

            var to = new Point(x, y);
            _element.Change(_from, to);
            _canvas.Refresh();
        }
        public override void OnMouseUp(int x, int y)
        {
            base.OnMouseUp(x, y);

            var to = new Point(x, y);

            if (_from == to)
                _canvas.RemoveElement(_element);
            else
                _element.Change(_from, to);

            _element = null;
            _canvas.Refresh();
        }
    }
}