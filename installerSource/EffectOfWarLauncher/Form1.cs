using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO.Compression;

namespace EffectOfWarLauncher
{
    public partial class Launcher : Form
    {
        string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
        HttpClient client = new HttpClient();
        string serverVersion;
        public Launcher()
        {
            InitializeComponent();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EffectOfWar/1.0");
        }

        private async void Launcher_Load(object sender, EventArgs e)
        {
            ICO();
            RefreshDatas();
        }
        private void Refresh_Click(object sender, EventArgs e) => RefreshDatas();
        private async void RefreshDatas()
        {
            if (await CheckVersion())
            {
                Starter.Enabled = false;
                ready.Enabled = true;
            }
            else
            {
                Starter.Enabled = true;
                if (File.Exists(Path.Combine(exeFolder, "game", "EffectOfWar.exe"))) ready.Enabled = true;
                else ready.Enabled = false;
            }
        }

        private async void ICO()
        {
            var path = Path.Combine(exeFolder, "icon.ico");
            if (!File.Exists(Path.Combine(exeFolder, "icon.ico")))
            {
                
                var url = $"https://github.com/Frici73/EffectOfWar/releases/latest/download/icon.ico";
                var response = await client.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(path, response);
            }
            using var icon = new Icon(path);
            this.Icon = icon;
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = icon.ToBitmap();
            notesBox.ReadOnly = true;
        }

        private async Task<bool> CheckVersion()
        {
            var url = "https://api.github.com/repos/Frici73/EffectOfWar/releases/latest";
            var response = await client.GetStringAsync(url);
            serverVersion = JsonSerializer.Deserialize<JsonElement>(response).GetProperty("tag_name").GetString();
            if (!File.Exists(Path.Combine(exeFolder, "game", "EffectOfWar.exe")))
            {
                Starter.Text = "Játék telepítése";
                return false;
            }
            else
            {
                Starter.Text = "Játék frissítése";
                var localPath = Path.Combine(exeFolder, "version.json");
                string downloadedVersion;
                try
                {
                    downloadedVersion = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(localPath))["version"];
                }
                catch { return false; }

                return serverVersion == downloadedVersion;
            }

        }
        private async Task downloadResource()
        {
            Starter.Enabled = false;
            Refresh.Enabled = false;
            haladas.Visible = true;
            haladas.Text = "Játék telepítése";
            string gameurl = $"https://github.com/Frici73/EffectOfWar/releases/download/{serverVersion}/game.zip";
            string resourcesurl = $"https://github.com/Frici73/EffectOfWar/releases/download/{serverVersion}/Resources.zip";
            string gameFolder = Path.Combine(exeFolder, "game");
            if (!Directory.Exists(gameFolder)) Directory.CreateDirectory(gameFolder);
            try
            {
                // download the application
                haladas.Text = "Játék telepítése";
                string zip = Path.Combine(exeFolder, "game.zip");
                byte[] gamebytes = await client.GetByteArrayAsync(gameurl);
                File.WriteAllBytes(zip, gamebytes);

                haladas.Text = "Játék kicsomagolása";
                ZipFile.ExtractToDirectory(zip, Path.Combine(exeFolder, "game"), true);

                haladas.Text = "Tisztíttás";
                File.Delete(zip);

                // download resources
                haladas.Text = "Resource letöltése";
                zip = Path.Combine(exeFolder, "Resources.zip");
                byte[] resourcesbytes = await client.GetByteArrayAsync(resourcesurl);
                File.WriteAllBytes(zip, resourcesbytes);

                haladas.Text = "Resources kicsomagolása";
                ZipFile.ExtractToDirectory(zip, Path.Combine(exeFolder, "game"), true);

                haladas.Text = "Tisztíttás";
                File.Delete(zip);

                haladas.Text = "Verzió kezelés";
                StreamWriter w = new StreamWriter(Path.Combine(exeFolder, "version.json"));
                w.WriteLine($"{{\"version\": \"{serverVersion}\"}}");
                w.Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Valószínűleg túl sok kérést ment a szerver felé ezért leállt a letöltés" + ex.Message);
            }

            Refresh.Enabled = true;
            haladas.Visible = false;
            RefreshDatas();
        }

        private async void Starter_Click(object sender, EventArgs e)
        {
            await downloadResource();
        }

        private void ready_Click(object sender, EventArgs e)
        {
            try
            {
                if (Process.GetProcessesByName("EffectOfWar").Length <= 0)
                    Process.Start(Path.Combine(exeFolder, "game", "EffectOfWar.exe"));
                else MessageBox.Show("A játék már fut!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}