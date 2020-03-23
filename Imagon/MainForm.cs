using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Imagon.Properties;

namespace Imagon
{
    public partial class MainForm : Form
    {
        private static readonly string[] SUPPORTED_OPEN_EXTENSIONS = { "bmp", "gif", "jpg", "jpeg", "jpe", "jif", "jfif", "jfi", "png", "tiff", "tif" };
        private static readonly string SUPPORTED_OPEN_EXTENSIONS_FILTER = $"*.{string.Join(";*.", SUPPORTED_OPEN_EXTENSIONS)}";
        private static readonly string[] SUPPORTED_SAVE_EXTENSIONS = { "bmp", "gif", "jpg", "jpeg", "png", "tiff", "tif" };
        private static readonly string SUPPORTED_SAVE_EXTENSIONS_FILTER = $"*.{string.Join(";*.", SUPPORTED_SAVE_EXTENSIONS)}";
        private static readonly Dictionary<string, ImageFormat> EXT_FORMAT_MAPPINGS = new Dictionary<string, ImageFormat> {
            { "bmp", ImageFormat.Bmp },
            { "gif", ImageFormat.Gif },
            { "jpg", ImageFormat.Jpeg },
            { "jpeg", ImageFormat.Jpeg },
            { "png", ImageFormat.Png },
            { "tif", ImageFormat.Tiff },
            { "tiff", ImageFormat.Tiff }
        };
        private const PixelFormat PIXEL_FORMAT = PixelFormat.Format32bppArgb;

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
                        Filter = $"Supported formats|{SUPPORTED_OPEN_EXTENSIONS_FILTER}|All files|*.*"
                    };
                }
                return _openFileDialog;
            }
        }
        private OpenFileDialog _openFileDialog;

        private SaveFileDialog SaveFileDialog
        {
            get
            {
                if (_saveFileDialog == null)
                {
                    _saveFileDialog = new SaveFileDialog()
                    {
                        OverwritePrompt = true,
                        AddExtension = false,
                        CheckPathExists = false,
                        Filter = $"Supported formats|{SUPPORTED_SAVE_EXTENSIONS_FILTER}"
                    };
                }
                return _saveFileDialog;
            }
        }
        private SaveFileDialog _saveFileDialog;


        private Image Image
        {
            get { return pbImageView.BackgroundImage; }
            set { pbImageView.BackgroundImage = value; }
        }
        private Bitmap OverlayImage { get; set; }

        private int _imageXOffset;
        private int _imageYOffset;
        private int _imageWidthToFullWidth;
        private int _imageHeightToFullHeight;

        private Dictionary<string, ToolStripMenuItem> _menuItems;
        private ToolStripMenuItem _currentTransparencyMenuItem;
        private ToolStripMenuItem _currentZoomMenuItem;
        private double _customTransparencyValue = 0;
        private double _currentTransparency;
        private double _customZoomValue = 1;
        private double _currentZoom;

        private bool _isLocked;
        private bool _resizeInX;
        private bool _resizeInY;


        public MainForm()
        {
            InitializeComponent();

            var imageScreenRect = this.RectangleToScreen(pbImageView.Bounds);
            _imageXOffset = (imageScreenRect.Left - this.Left);
            _imageYOffset = (imageScreenRect.Top - this.Top);
            _imageWidthToFullWidth = (Width - ClientSize.Width);
            _imageHeightToFullHeight = (Height - ClientSize.Height + msMainMenu.Height);

            InitMainMenu();
            InitContextMenu();

            if (Clipboard.ContainsImage())
                GetFromClipboard();
            else
                LoadImage(GetStartupImage());

            SetTransparency(0);
            SetZoom(100);
        }


        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if (MenuItem("Clickthrough").Checked)
                        DisableClickthrough();
                    break;
                case Keys.Pause:
                    ToggleClickthrough();
                    break;

                case Keys.Left:
                    AdjustFormPosition(e.Shift ? new Size(-10, 0) : new Size(-1, 0));
                    break;
                case Keys.Right:
                    AdjustFormPosition(e.Shift ? new Size(10, 0) : new Size(1, 0));
                    break;
                case Keys.Up:
                    AdjustFormPosition(e.Shift ? new Size(0, -10) : new Size(0, -1));
                    break;
                case Keys.Down:
                    AdjustFormPosition(e.Shift ? new Size(0, 10) : new Size(0, 1));
                    break;

                case Keys.Add:
                    if (e.Control)
                        SetZoom(_currentZoom + 10);
                    else
                        SetTransparency(_currentTransparency - 10);
                    break;
                case Keys.Subtract:
                    if (e.Control)
                        SetZoom(_currentZoom - 10);
                    else
                        SetTransparency(_currentTransparency + 10);
                    break;
            }
        }
        private void MainForm_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Bitmap))
                    e.Effect = DragDropEffects.Copy;
            }
            catch { }
        }
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Any())
                        LoadImage(files.First());
                }
                else if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    var bitmap = e.Data.GetData(DataFormats.Bitmap) as Bitmap;
                    LoadImage(bitmap);
                }
            }
            catch { }
        }
        private void pbImageView_MouseDown(object sender, MouseEventArgs e)
        {
            WinApi.InvokeUserMoveWindow(this);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SIZING = 0x214;
            const int WM_EXITSIZEMOVE = 0x232;
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MOVE = 0xF010;

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

                case WM_SYSCOMMAND:
                    int command = m.WParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE && _isLocked)
                        return;

                    base.WndProc(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        private void InitMainMenu()
        {
            _menuItems = new Dictionary<string, ToolStripMenuItem>();
            msMainMenu.Items.AddRange(new[]
            {
                CreateMenuItem(
                    "Image",
                    "&Image",
                    CreateMenuItem("Open","&Open...", Shortcut.CtrlO, OpenFile),
                    CreateMenuItem("Download","&Download...", Shortcut.CtrlD, Download),
                    CreateMenuItem("FromClipboard","From &Clipboard", Shortcut.CtrlV, GetFromClipboard),
                    CreateMenuItem("FromUnderWindow", "From &Under Window", Shortcut.CtrlU, GetScreenshotUnderImage),
                    CreateMenuItem("ToClipboard","To C&lipboard", Shortcut.CtrlC, PasteToClipboard),
                    CreateSeparator(),
                    CreateMenuItem("SaveAs","&Save as...", Shortcut.CtrlS, SaveImage),
                    CreateSeparator(),
                    CreateMenuItem("Exit","&Exit", null, Exit)
                    ),
                CreateMenuItem(
                    "Display",
                    "&Display",
                    CreateMenuItem("Lock","&Lock", Shortcut.CtrlL, ToggleLock),
                    CreateMenuItem("Topmost","Top&most", Shortcut.CtrlT, ToggleTopmost),
                    CreateMenuItem("Borderless","&Borderless", Shortcut.CtrlB, ToggleBorderless),
                    CreateMenuItem("Clickthrough","&Clickthrough", Shortcut.CtrlI, ToggleClickthrough),
                    CreateMenuItem("Transparency","&Transparency"),
                    CreateMenuItem("Zoom","&Zoom"),
                    CreateMenuItem("Overlay", "&Overlay")
                    ),
                //CreateMenuItem(
                //    "Tools",
                //    "&Tools",
                //    CreateMenuItem("Diff","Diff",null,Diff)
                //    )
            });

            MenuItem("Transparency").DropDownItems.AddRange(new[]
            {
                CreateMenuItem("Transparency0", "0 % (Opaque)", null, () => SetTransparency(0)),
                CreateMenuItem("Transparency25", "25 %", null, () => SetTransparency(25)),
                CreateMenuItem("Transparency50", "50 %", null, () => SetTransparency(50)),
                CreateMenuItem("Transparency75", "75 %", null, () => SetTransparency(75))
            });
            var customTransparencyMenuItem = CreateMenuItem("TransparencyCustom", "Custom", null, SetCustomTransparency);
            MenuItem("Transparency").DropDownItems.Add(customTransparencyMenuItem);

            var zoomMenuItem = MenuItem("Zoom");
            int[] zoomValues = { 5, 10, 25, 50, 75, 100, 125, 150, 175, 200, 300, 500 };
            foreach (int zoom in zoomValues)
            {
                var menuItem = CreateMenuItem($"Zoom{zoom}", $"{zoom} %", null, () => SetZoom(zoom));
                zoomMenuItem.DropDownItems.Add(menuItem);
            }
            var customZoomMenuItem = CreateMenuItem("ZoomCustom", "Custom", null, SetCustomZoom_dialog);
            zoomMenuItem.DropDownItems.Add(customZoomMenuItem);

            var overlayMenuItem = MenuItem("Overlay");
            overlayMenuItem.DropDownItems.AddRange(new[]
            {
                CreateMenuItem("ShowOverlay", "&Show", Shortcut.CtrlA, ToggleOverlayVisible)
            });
            overlayMenuItem.Enabled = false;

            MenuItem("Zoom100").ShortcutKeys = (Keys)Shortcut.Ctrl0;
        }
        private void InitContextMenu()
        {
            cmsContextMenu.Items.AddRange(CreateMenuItemSubItemsShadowCopies("Display"));
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
        private void Download()
        {
            try
            {
                bool retry;
                do
                {
                    retry = false;
                    string url = Interaction.InputBox("Url of the image", "Download Image");
                    if (!string.IsNullOrEmpty(url))
                    {
                        if (!IsSupportedUrl(url))
                        {
                            ShowWarning("Url format is invalid or not supported");
                            retry = true;
                        }
                        else
                        {
                            LoadImageFromUrl(url);
                        }
                    }
                } while (retry);
            }
            catch { }
        }
        private void GetFromClipboard()
        {
            try
            {
                if (_isLocked && !PromptUnlockImage())
                    return;

                bool hasSupportedContent = false;
                if (Clipboard.ContainsImage())
                {
                    hasSupportedContent = true;
                    var image = Clipboard.GetImage();
                    LoadImage(image);
                }
                else if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText(TextDataFormat.UnicodeText);
                    text = text.Trim();
                    if (IsImageFilePath(text))
                    {
                        hasSupportedContent = true;
                        LoadImage(text);
                    }
                    else if (IsSupportedUrl(text))
                    {
                        hasSupportedContent = true;
                        LoadImageFromUrl(text);
                    }
                }

                if (!hasSupportedContent)
                    MessageBox.Show("Current clipboard doesn't contain image data, file path or URL");
            }
            catch
            {
                ShowError("Loading from Clipboard failed");
            }
        }
        private void PasteToClipboard()
        {
            try
            {
                Clipboard.SetImage(Image);
            }
            catch
            {
                ShowError("Pasting to Clipboard failed");
            }
        }
        private void GetScreenshotUnderImage()
        {
            if (_isLocked && !PromptUnlockImage())
                return;

            LoadImage(CreateScreenshotUnderImage());
        }
        private void SaveImage()
        {
            try
            {
                bool retry;
                do
                {
                    retry = false;
                    var dialogResult = SaveFileDialog.ShowDialog();
                    if (dialogResult != DialogResult.OK)
                        return;

                    string filePath = Path.GetFullPath(SaveFileDialog.FileName);
                    string path = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    string ext = Path.GetExtension(filePath).TrimStart('.');
                    if (EXT_FORMAT_MAPPINGS.TryGetValue(ext, out var imageFormat))
                    {
                        Image.Save(filePath, imageFormat);
                    }
                    else
                    {
                        ShowWarning("Unsupported image format");
                        retry = true;
                    }
                } while (retry);
            }
            catch { }
        }
        private void Exit()
        {
            this.Close();
        }

        private void HandleWindowResizing(ref Message m)
        {
            UncheckCurrentZoom();

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

            double ratioX = (windowRect.Width - _imageWidthToFullWidth) / (double)Image.Width;
            double ratioY = (windowRect.Height - _imageHeightToFullHeight) / (double)Image.Height;

            double ratio = 1;
            if (_resizeInX && _resizeInY)
                ratio = ratioX < ratioY ? ratioY : ratioX;
            else if (_resizeInX)
                ratio = ratioX;
            else if (_resizeInY)
                ratio = ratioY;

            windowRect.Width = (int)(Image.Width * ratio + _imageWidthToFullWidth);
            windowRect.Height = (int)(Image.Height * ratio + _imageHeightToFullHeight);

            Marshal.StructureToPtr(windowRect, m.LParam, false);
            m.Result = (IntPtr)1;
        }
        private void AdjustFormPosition(Size delta)
        {
            if (_isLocked)
                return;

            this.DesktopLocation += delta;
        }

        private void LoadImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    ShowError($"File '{filePath}' doesn't exist");
                    return;
                }

                var image = System.Drawing.Image.FromFile(filePath);
                LoadImage(image);
            }
            catch { }
        }
        private void LoadImage(Image image)
        {
            if (image == null)
                return;

            DisposeOverlayImage();

            if (image.PixelFormat != PIXEL_FORMAT)
            {
                var img = new Bitmap(image.Width, image.Height, PIXEL_FORMAT);
                using (var g = Graphics.FromImage(img))
                    g.DrawImageUnscaled(image, 0, 0);
                image = img;
            }

            Image = image;
            ResetSize();
        }
        private void LoadImageFromUrl(string url)
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead(url))
                    {
                        var image = new Bitmap(stream);
                        LoadImage(image);
                    }
                }
            }
            catch { }
        }
        private void ResetSize()
        {
            SetZoom(100);
        }
        private void SetImageSizeMode(bool keepAspectRatio)
        {
            pbImageView.SizeMode = (keepAspectRatio ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.StretchImage);
        }


        private void ToggleLock()
        {
            var menuItem = MenuItem("Lock");
            menuItem.Checked = !menuItem.Checked;
            _isLocked = menuItem.Checked;
            this.SizeGripStyle = _isLocked ? SizeGripStyle.Hide : SizeGripStyle.Auto;
            this.MaximizeBox = !_isLocked;
            if (_isLocked)
            {
                this.MinimumSize = this.Size;
                this.MaximumSize = this.Size;
            }
        }
        private void ToggleTopmost()
        {
            var menuItem = MenuItem("Topmost");
            menuItem.Checked = !menuItem.Checked;
            this.TopMost = menuItem.Checked;
        }
        private void ToggleBorderless()
        {
            this.SuspendLayout();
            try
            {
                int imageWidth = pbImageView.Width;
                int imageHeight = pbImageView.Height;

                var menuItem = MenuItem("Borderless");
                menuItem.Checked = !menuItem.Checked;
                this.FormBorderStyle = menuItem.Checked ? FormBorderStyle.None : FormBorderStyle.Sizable;
                msMainMenu.Visible = !menuItem.Checked;

                bool borderVisible = !menuItem.Checked;
                if (borderVisible)
                {
                    this.Location = new Point(this.Location.X - _imageXOffset, this.Location.Y - _imageYOffset);
                    this.Width = (imageWidth + _imageWidthToFullWidth);
                    this.Height = (imageHeight + _imageHeightToFullHeight);
                }
                else
                {
                    this.Location = new Point(this.Location.X + _imageXOffset, this.Location.Y + _imageYOffset);
                    this.Width = imageWidth;
                    this.Height = imageHeight;
                }
            }
            finally
            {
                this.ResumeLayout();
            }
        }
        private void ToggleClickthrough()
        {
            var menuItem = MenuItem("Clickthrough");
            menuItem.Checked = !menuItem.Checked;
            if (menuItem.Checked)
                WinApi.EnableClickThrough(this);
            else
                WinApi.DisableClickThrough(this);
        }
        private void DisableClickthrough()
        {
            MenuItem("Clickthrough").Checked = true;
            ToggleClickthrough();
        }
        private void UncheckCurrentTransparency()
        {
            if (_currentTransparencyMenuItem != null)
                _currentTransparencyMenuItem.Checked = false;
        }
        private void SetTransparency(double transparencyValue)
        {
            if (transparencyValue < 0 || 99 <= transparencyValue)
                return;

            UncheckCurrentTransparency();

            this.Opacity = (double)(100 - transparencyValue) / 100d;

            _currentTransparency = transparencyValue;

            _currentTransparencyMenuItem = MenuItem($"Transparency{transparencyValue}");
            if (_currentTransparencyMenuItem == null)
            {
                _currentTransparencyMenuItem = MenuItem("TransparencyCustom");
                _customTransparencyValue = (transparencyValue / 100d);
            }
            _currentTransparencyMenuItem.Checked = true;
        }
        private void SetCustomTransparency()
        {
            try
            {
                string enteredTransparency = Interaction.InputBox(
                    "Custom Transparency value\r\n(0 % opaque, 100 % fully transparent)",
                    "Set Custom Transparency",
                    (_customTransparencyValue * 100).ToString("0.00")
                    );
                double transparency = double.Parse(enteredTransparency);
                SetTransparency(transparency);
            }
            catch (Exception) { }
        }
        private void UncheckCurrentZoom()
        {
            if (_currentZoomMenuItem != null)
                _currentZoomMenuItem.Checked = false;
        }
        private void SetZoom(double zoom)
        {
            UncheckCurrentZoom();

            double scale = (zoom / 100d);
            this.ClientSize = new Size(
                (int)(Image.Width * scale),
                (int)(Image.Height * scale) + msMainMenu.Height
                );

            _currentZoom = zoom;

            _currentZoomMenuItem = MenuItem($"Zoom{zoom}");
            if (_currentZoomMenuItem == null)
            {
                _currentZoomMenuItem = MenuItem("ZoomCustom");
                _customZoomValue = scale;
            }
            _currentZoomMenuItem.Checked = true;
        }
        private void SetCustomZoom_dialog()
        {
            try
            {
                string enteredZoom = Interaction.InputBox("Custom Zoom value (%)", "Set Custom Zoom", (_customZoomValue * 100).ToString("#.00"));
                double zoom = double.Parse(enteredZoom);
                SetZoom(zoom);
            }
            catch (Exception) { }
        }
        private void ToggleOverlayVisible()
        {
            var menuItem = MenuItem("ShowOverlay");
            menuItem.Checked = !menuItem.Checked;

            if (menuItem.Checked)
                ShowOverlayImage(OverlayImage);
            else
                HideOverlayImage();
        }

        private void ShowOverlayImage(Bitmap image)
        {
            OverlayImage = image;
            pbImageView.Image = OverlayImage;

            MenuItem("Overlay").Enabled = true;
            MenuItem("ShowOverlay").Checked = true;
        }
        private void HideOverlayImage()
        {
            MenuItem("ShowOverlay").Checked = false;
        }
        private void DisposeOverlayImage()
        {
            if (OverlayImage == null)
                return;

            MenuItem("Overlay").Enabled = false;
            pbImageView.Image = null;
            OverlayImage.Dispose();
            OverlayImage = null;
        }

        private bool PromptUnlockImage()
        {
            return false;
        }

        private void Diff()
        {
            var size = pbImageView.ClientSize;
            int width = pbImageView.ClientSize.Width;
            int height = pbImageView.ClientSize.Height;
            var rect = new Rectangle(0, 0, width, height);

            var image = new Bitmap(Image, size);
            var screen = CreateScreenshotUnderImage();
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            result.MakeTransparent();

            var imageData = image.LockBits(rect, ImageLockMode.ReadOnly, image.PixelFormat);
            var screenData = screen.LockBits(rect, ImageLockMode.ReadOnly, screen.PixelFormat);
            var resultData = result.LockBits(rect, ImageLockMode.WriteOnly, result.PixelFormat);
            unsafe
            {
                for (int y = 0; y < height; y++)
                {
                    byte* imageRow = (byte*)imageData.Scan0 + (y * imageData.Stride);
                    byte* screenRow = (byte*)screenData.Scan0 + (y * screenData.Stride);
                    byte* resultRow = (byte*)resultData.Scan0 + (y * resultData.Stride);
                    for (int x = 0; x < width; x++)
                    {
                        //if (imagePx != screenPx)
                        //    result.SetPixel(x, y, Color.Red);
                    }
                }
            }

            ShowOverlayImage(result);
        }

        private Bitmap CreateScreenshotWithoutForm(Rectangle screenSource)
        {
            try
            {
                //var screen = Screen.FromControl(this);
                this.SendToBack();
                return CreateScreenshot(screenSource);
            }
            catch
            {
                ShowError("Unable to create a screenshot");
                return null;
            }
            finally
            {
                this.BringToFront();
            }
        }
        private Bitmap CreateScreenshotUnderImage()
        {
            var screenSource = this.RectangleToScreen(pbImageView.Bounds);
            return CreateScreenshotWithoutForm(screenSource);
        }
        private Bitmap CreateScreenshot(Rectangle screenSource)
        {
            var result = new Bitmap(screenSource.Width, screenSource.Height, PixelFormat.Format32bppRgb);
            using (var graphics = Graphics.FromImage(result))
                graphics.CopyFromScreen(screenSource.X, screenSource.Y, 0, 0, screenSource.Size, CopyPixelOperation.SourceCopy);

            return result;
        }

        private void ShowMessage(string title, string message, MessageBoxIcon type = MessageBoxIcon.Information)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, type);
        }
        private void ShowWarning(string message)
        {
            ShowMessage("Warning", message, MessageBoxIcon.Warning);
        }
        private void ShowError(string message)
        {
            ShowMessage("Error", message, MessageBoxIcon.Error);
        }

        private ToolStripMenuItem MenuItem(string name)
        {
            _menuItems.TryGetValue(name, out var result);
            return result;
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

        private ToolStripMenuItem[] CreateMenuItemSubItemsShadowCopies(string name)
        {
            var menuItem = MenuItem(name);
            var result = new ToolStripMenuItem[menuItem.DropDownItems.Count];
            for (int i = 0; i < menuItem.DropDownItems.Count; i++)
            {
                var original = menuItem.DropDownItems[i] as ToolStripMenuItem;
                var clone = CreateMenuItemShadowCopy(original);
                result[i] = clone;
                if (clone.DropDownItems.Count == 0)
                    original.CheckedChanged += (s, e) => clone.Checked = ((ToolStripMenuItem)s).Checked;
            }
            return result;
        }
        private ToolStripMenuItem CreateMenuItemShadowCopy(ToolStripMenuItem menuItem)
        {
            if (menuItem.DropDownItems.Count == 0)
            {
                var clone = new ToolStripMenuItem(menuItem.Text, null, (s, e) => menuItem.PerformClick());
                menuItem.CheckedChanged += (s, e) => clone.Checked = ((ToolStripMenuItem)s).Checked;
                return clone;
            }
            else
            {
                var subItems = new ToolStripMenuItem[menuItem.DropDownItems.Count];
                for (int i = 0; i < menuItem.DropDownItems.Count; i++)
                {
                    var subItem = CreateMenuItemShadowCopy(menuItem.DropDownItems[i] as ToolStripMenuItem);
                    subItems[i] = subItem;
                }
                var clone = new ToolStripMenuItem(menuItem.Text, null, subItems);
                return clone;
            }
        }

        private bool IsImageFilePath(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (text.Length < 4)
                return false;

            if (text.Substring(1, 2) != @":\")
                return false;

            bool containsInvalidPathChar = (text.IndexOfAny(Path.GetInvalidPathChars()) != -1);
            if (containsInvalidPathChar)
                return false;

            string extension = Path.GetExtension(text).TrimStart('.');
            bool hasImageSupportedExtension = SUPPORTED_OPEN_EXTENSIONS.Any(e => e == extension);
            if (!hasImageSupportedExtension)
                return false;

            return true;
        }
        private bool IsSupportedUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            bool isHttp = (text.StartsWith("http://") || text.StartsWith("https://"));
            if (!isHttp)
                return false;

            return Uri.IsWellFormedUriString(text, UriKind.Absolute);
        }


        private static Image GetStartupImage()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string name = assembly.GetManifestResourceNames().First(n => n.Contains("BackgroundImage"));
                var stream = assembly.GetManifestResourceStream(name);
                var image = Bitmap.FromStream(stream);
                return image;
            }
            catch
            {
                return new Bitmap(1, 1);
            }
        }
    }
}
