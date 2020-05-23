using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Imagon
{
    public class Canvas
    {
        public bool IsActive { get { return _activeTool != null; } }

        public float ScaleX
        {
            get => _scaleX;
            set { _scaleX = value; UpdateMatrix(); }
        }
        private float _scaleX;
        public float ScaleY
        {
            get => _scaleY;
            set { _scaleY = value; UpdateMatrix(); }
        }
        private float _scaleY;
        public decimal Rotation
        {
            get => _rotation;
            set { _rotation = value; UpdateMatrix(); }
        }
        private decimal _rotation;

        public CanvasTools Tools { get; }

        private Size _size;
        private Matrix _matrix;
        private CanvasTool _activeTool;
        private List<CanvasElement> _elements;

        public event EventHandler NeedRepaint;


        public Canvas(Size size)
        {
            _size = size;

            _scaleX = 1;
            _scaleY = 1;
            _rotation = 0;
            _matrix = new Matrix();
            UpdateMatrix();

            Tools = new CanvasTools(this);

            _elements = new List<CanvasElement>();
        }


        public void ActivateTool(CanvasTool tool)
        {
            _activeTool = tool;
        }
        public void DeactivateTool(CanvasTool tool)
        {
            if (_activeTool == tool)
                _activeTool = null;
        }

        public void Draw(Graphics graphics)
        {
            graphics.Transform = _matrix;

            foreach (var element in _elements)
            {
                element.Draw(graphics);
            }
        }
        public void Refresh()
        {
            NeedRepaint?.Invoke(this, null);
        }

        public void SetScale(float scale)
        {
            _scaleX = scale;
            _scaleY = scale;
            UpdateMatrix();
        }
        private void UpdateMatrix()
        {
            _matrix.Reset();
            _matrix.RotateAt((float)_rotation, new PointF(_size.Width / 2f, _size.Height / 2f));
            _matrix.Scale(_scaleX, _scaleY);
        }

        public void OnMouseDown(int x, int y)
        {
            _activeTool.OnMouseDown(x, y);
        }
        public void AddElement(MeasureElement element)
        {
            _elements.Add(element);
        }
    }
}