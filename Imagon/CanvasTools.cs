namespace Imagon
{
    public class CanvasTools
    {
        public MeasureTool Measure { get; }
        public CanvasTool Rectangle { get;  }


        private Canvas _canvas;


        public CanvasTools(Canvas parent)
        {
            _canvas = parent;

            Rectangle = new RectangleTool(_canvas);
            Measure = new MeasureTool(_canvas);
        }
    }
}