namespace Imagon
{
    public abstract class CanvasTool
    {
        protected Canvas _canvas;

        public CanvasTool(Canvas canvas)
        {
            _canvas = canvas;
        }

        public virtual void OnMouseMove(int x, int y) { }
        public virtual void OnMouseDown(int x, int y) { }
        public virtual void OnMouseUp(int x, int y) { }
    }
}