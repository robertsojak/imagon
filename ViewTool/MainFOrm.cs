using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ViewTool.Properties;

namespace ViewTool
{
    public partial class MainForm : Form
    {
        private const string SUPPORTED_EXTENSIONS = "*.bmp;*.gif;*.jpg;*.jpeg;*.jpe;*.jif;*.jfif;*.jfi;*.png;*.tiff;*.tif";

        private OpenFileDialog OpenFileDialog
        {
            get
            {
                if (_openFileDialog == null)
                {
                    _openFileDialog = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        AddExtension = false,
                        Filter = $"Supported formats|{SUPPORTED_EXTENSIONS}|All files|*.*"
                    };
                }
                return _openFileDialog;
            }
        }
        private OpenFileDialog _openFileDialog;

        private double ImageAspectRatio => pbImageView.Image.Width / (double)pbImageView.Image.Height;
        private double FormAspectRatio => (pbImageView.Image.Width + (Width - ClientSize.Width)) / (double)(pbImageView.Image.Height + msMainMenu.Height + (Height - ClientSize.Height));

        private int _imageWidthToFullWidth;
        private int _imageHeightToFullHeight;

        private Dictionary<string, ToolStripMenuItem> _menuItems;
        private ToolStripMenuItem _currentZoomMenuItem;

        private uint _initialWindowStyle;

        private bool _resizeInX;
        private bool _resizeInY;


        public MainForm()
        {
            InitializeComponent();

            InitMainMenu();
            InitContextMenu();

            _imageWidthToFullWidth = (Width - ClientSize.Width);
            _imageHeightToFullHeight = (Height - ClientSize.Height + msMainMenu.Height);

            if (Clipboard.ContainsImage())
                GetFromClipboard();
            else
                LoadImage(GetBackgroundImage());
        }
        private static Image GetBackgroundImage()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string name = assembly.GetManifestResourceNames().Where(n => n.Contains("BackgroundImage")).First();
            var stream = assembly.GetManifestResourceStream(name);
            var image = Bitmap.FromStream(stream);
            return image;
        }


        protected override void WndProc(ref Message m)
        {
            const int WM_SIZING = 0x214;
            const int WM_EXITSIZEMOVE = 0x232;

            switch (m.Msg)
            {
                case WM_SIZING:
                    HandleWindowResizing(ref m);
                    break;

                case WM_EXITSIZEMOVE:
                    _resizeInX = false;
                    _resizeInY = false;
                    base.WndProc(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        private void HandleWindowResizing(ref Message m)
        {
            if (!Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                SetImageSizeMode(keepAspectRatio: false);
                base.WndProc(ref m);
                return;
            }

            SetImageSizeMode(keepAspectRatio: true);

            var windowRect = (WinApi.RECT)Marshal.PtrToStructure(m.LParam, typeof(WinApi.RECT));

            int widthDiff = Math.Abs(Width - windowRect.Width);
            if (widthDiff > 0)
                _resizeInX = true;
            int heightDiff = Math.Abs(Height - windowRect.Height);
            if (heightDiff > 0)
                _resizeInY = true;

            double ratioX = (windowRect.Width - _imageWidthToFullWidth) / (double)pbImageView.Image.Width;
            double ratioY = (windowRect.Height - _imageHeightToFullHeight) / (double)pbImageView.Image.Height;

            double ratio = 1;
            if (_resizeInX && _resizeInY)
                ratio = ratioX < ratioY ? ratioY : ratioX;
            else if (_resizeInX)
                ratio = ratioX;
            else if (_resizeInY)
                ratio = ratioY;

            windowRect.Width = (int)(pbImageView.Image.Width * ratio + _imageWidthToFullWidth);
            windowRect.Height = (int)(pbImageView.Image.Height * ratio + _imageHeightToFullHeight);

            Marshal.StructureToPtr(windowRect, m.LParam, false);
            m.Result = (IntPtr)1;
        }

        private void SetImageSizeMode(bool keepAspectRatio)
        {
            pbImageView.SizeMode = keepAspectRatio ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.StretchImage;
        }


        private void InitMainMenu()
        {
            _menuItems = new Dictionary<string, ToolStripMenuItem>();
            msMainMenu.Items.AddRange(new[]
            {
                CreateMenuItem(
                    "Image",
                    "&Image",
                    CreateMenuItem("Open","&Open", Shortcut.CtrlO, OpenFile),
                    CreateMenuItem("FromClipboard","From &Clipboard", Shortcut.CtrlL, GetFromClipboard),
                    CreateSeparator(),
                    CreateMenuItem("Exit","&Exit", null, Exit)
                    ),
                CreateMenuItem(
                    "Display",
                    "&Display",
                    CreateMenuItem("Topmost","Top&most", Shortcut.CtrlM, ToggleTopmost),
                    CreateMenuItem("Borderless","&Borderless", Shortcut.CtrlB, ToggleBorderless),
                    CreateMenuItem("Clickthrough","&Clickthrough", Shortcut.CtrlI, ToggleClickthrough),
                    CreateMenuItem("Transparency","&Transparency"),
                    CreateMenuItem("Zoom","&Zoom")
                    )
            });

            MenuItem("Transparency").DropDownItems.AddRange(new[]
            {
                CreateMenuItem("Transparency0", "0 % (Opaque)", null, () => SetTransparency(0)),
                CreateMenuItem("Transparency25", "25 %", null, () => SetTransparency(25)),
                CreateMenuItem("Transparency50", "50 %", null, () => SetTransparency(50)),
                CreateMenuItem("Transparency75", "75 %", null, () => SetTransparency(75))
            });

            var zoomMenuItem = MenuItem("Zoom");
            int[] zoomValues = { 5, 10, 25, 50, 75, 100, 125, 150, 175, 200, 300, 500 };
            foreach (int zoom in zoomValues)
            {
                var menuItem = CreateMenuItem($"Zoom{zoom}", $"{zoom} %", null, () => SetZoom(zoom));
                zoomMenuItem.DropDownItems.Add(menuItem);
            }
            var customZoomMenuItem = CreateMenuItem("ZoomCustom", "Custom", null, SetCustomZoom);
            zoomMenuItem.DropDownItems.Add(customZoomMenuItem);

            MenuItem("Zoom100").ShortcutKeys = (Keys)Shortcut.Ctrl0;
        }
        private void InitContextMenu()
        {
        }


        private void OpenFile()
        {
            var dialogResult = OpenFileDialog.ShowDialog();
            if (dialogResult != DialogResult.OK)
                return;

            string filePath = Path.GetFullPath(OpenFileDialog.FileName);
            OpenFileDialog.FileName = string.Empty;

            LoadImage(filePath);
        }
        private void GetFromClipboard()
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("Current clipboard doesn't contain image data");
                return;
            }

            var image = Clipboard.GetImage();
            LoadImage(image);
        }
        private void Exit()
        {
        }

        private void LoadImage(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowError("File doesn't exist");
                return;
            }

            var image = Image.FromFile(filePath);
            LoadImage(image);
        }
        private void LoadImage(Image image)
        {
            pbImageView.Image = image;
            ResetSize();
        }
        private void ResetSize()
        {
            SetZoom(100);
        }


        private void ToggleTopmost()
        {
            var menuItem = MenuItem("Topmost");
            menuItem.Checked = !menuItem.Checked;
            this.TopMost = menuItem.Checked;
        }
        private void ToggleBorderless()
        {
            var menuItem = MenuItem("Borderless");
            menuItem.Checked = !menuItem.Checked;
            this.FormBorderStyle = menuItem.Checked ? FormBorderStyle.None : FormBorderStyle.SizableToolWindow;
            msMainMenu.Visible = !menuItem.Checked;
        }
        private void ToggleClickthrough()
        {
            var menuItem = MenuItem("Clickthrough");
            menuItem.Checked = !menuItem.Checked;
            if (menuItem.Checked)
                _initialWindowStyle = WinApi.EnableClickThrough(this);
            else
                WinApi.DisableClickThrough(this, _initialWindowStyle);
        }
        private void SetTransparency(int transparencyValue)
        {
            this.Opacity = (double)(100 - transparencyValue) / 100d;
        }
        private void SetZoom(int zoom)
        {
            if (_currentZoomMenuItem != null)
                _currentZoomMenuItem.Checked = false;

            double percent = (zoom / 100d);
            this.ClientSize = new Size(
                (int)(pbImageView.Image.Width * percent),
                (int)(pbImageView.Image.Height * percent) + msMainMenu.Height
                );

            _currentZoomMenuItem = (MenuItem($"Zoom{zoom}") ?? MenuItem("ZoomCustom"));
            _currentZoomMenuItem.Checked = true;
        }
        private void SetCustomZoom()
        {
        }


        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private ToolStripMenuItem MenuItem(string name)
        {
            return _menuItems[name];
        }
        private ToolStripMenuItem CreateMenuItem(string name, string text, params ToolStripItem[] subItems)
        {
            var result = new ToolStripMenuItem(text, null, subItems);
            _menuItems.Add(name, result);
            return result;
        }
        private ToolStripMenuItem CreateMenuItem(string name, string text, Shortcut? shortcut, Action onClickAction)
        {
            var result = new ToolStripMenuItem(text, null, (s, e) => onClickAction());
            if (shortcut != null)
                result.ShortcutKeys = (Keys)shortcut.Value;
            _menuItems.Add(name, result);
            return result;
        }
        private ToolStripSeparator CreateSeparator()
        {
            return new ToolStripSeparator();
        }
    }
}
