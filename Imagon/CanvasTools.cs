namespace Imagon
{
    public class CanvasTools
    {
        public MeasureTool Measure { get; }


        private Canvas _canvas;


        public CanvasTools(Canvas parent)
        {
            _canvas = parent;

            Measure = new MeasureTool(_canvas);
        }
    }
}