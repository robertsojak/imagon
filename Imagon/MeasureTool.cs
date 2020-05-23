using System.Drawing;

namespace Imagon
{
    public class MeasureTool : CanvasTool
    {
        MeasureElement Element { get; set; }

        private States _state;

        public MeasureTool(Canvas canvas) : base(canvas)
        {
            _state = States.PuttingA;
        }


        public override void OnMouseDown(int x, int y)
        {
            if (_state == States.PuttingA)
            {
                Element = new MeasureElement();
                _canvas.AddElement(Element);
                Element.A = new Point(x, y);
                _state = States.PuttingB;
            }
            else if (_state == States.PuttingB)
            {
                Element.B = new Point(x, y);
                _state = States.PuttingA;
            }

            _canvas.Refresh();
        }


        private enum States
        {
            PuttingA,
            PuttingB
        }
    }
}