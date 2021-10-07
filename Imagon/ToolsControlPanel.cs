using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Imagon
{
    public partial class ToolsControlPanel : UserControl
    {
        private Canvas _canvas;


        public ToolsControlPanel()
        {
            InitializeComponent();
        }


        public void ConnectTo(Canvas canvas)
        {
            _canvas = canvas;
        }

        private void pbMeasure_Click(object sender, EventArgs e)
        {
            _canvas.ActivateTool(_canvas.Tools.Measure);
        }

        private void pbRectangle_Click(object sender, EventArgs e)
        {
            _canvas.ActivateTool(_canvas.Tools.Rectangle);
        }
    }
}
