using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;

namespace Imagon
{
    public partial class OcrForm : Form
    {
        private const string TESSERACT_DATA_DIR = "TesseractData";

        private readonly MainForm MainForm;
        private List<Language> _availableLanguages;

        public OcrForm(MainForm parent)
        {
            InitializeComponent();

            MainForm = parent;

            try
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
                var languagesDirInfo = new DirectoryInfo(TESSERACT_DATA_DIR);
                _availableLanguages = languagesDirInfo.GetFiles()
                    .Select(f => Path.GetFileNameWithoutExtension(f.Name))
                    .Select(f => new Language() { Name = cultures.FirstOrDefault(c => c.ThreeLetterISOLanguageName == f)?.NativeName, ThreeLetterName = f })
                    .ToList();

                cboLanguage.Items.Clear();
                cboLanguage.Items.AddRange(_availableLanguages.Cast<object>().ToArray());

                var currentCulture = CultureInfo.CurrentCulture;
                while (!currentCulture.IsNeutralCulture)
                    currentCulture = currentCulture.Parent;

                var selectedLanguage = _availableLanguages.FirstOrDefault(l => l.ThreeLetterName == currentCulture.ThreeLetterISOLanguageName);
                if (selectedLanguage == null)
                    selectedLanguage = _availableLanguages.FirstOrDefault(l => l.ThreeLetterName == "eng"); ;
                cboLanguage.SelectedItem = selectedLanguage;
            }
            catch { }
        }

        private void lnkTesseractOcr_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/tesseract-ocr/tesseract");
        }
        private void lnkTesseractDotNet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/charlesw/tesseract");
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            await RunAsync();
        }
        private void btnRemoveLineBreaks_Click(object sender, EventArgs e)
        {
            txtResult.Text = txtResult.Text.Replace(Environment.NewLine, " ");
        }


        private async Task RunAsync()
        {
            btnRun.Enabled = false;
            try
            {
                using (var bitmap = new Bitmap(MainForm.Image))
                    txtResult.Text = await RecognizeTextAsync(bitmap);
            }
            finally
            {
                btnRun.Enabled = true;
            }
        }
        private async Task<string> RecognizeTextAsync(Bitmap image)
        {
            return await Task.Run(() =>
            {
                using (var tesseract = new TesseractEngine("TesseractData", "eng", EngineMode.Default))
                {
                    var page = tesseract.Process(image);
                    string text = page.GetText();
                    text = text.Replace("\n", Environment.NewLine);
                    return text;
                }
            });
        }

        private class Language
        {
            public string Name { get; set; }
            public string ThreeLetterName { get; set; }
            public override string ToString() => Name;
        }
    }
}
