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
        bool needupdate = false;
        string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
        HttpClient client = new HttpClient();
        Dictionary<string, string> serverVersion;
        public Launcher()
        {
            InitializeComponent();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EffectOfWar/1.0");
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            ICO();
            RefreshDatas();
        }
        private void Refresh_Click(object sender, EventArgs e) => RefreshDatas();
        private async void RefreshDatas()
        {
            if (!await CheckVersion())
            {
                needupdate = true;
            }
            else
            {
                Starter.Text = "Játék indítása";
                needupdate = false;
            }
        }

        private async void ICO()
        {
            if (!File.Exists(Path.Combine(exeFolder, "icon.ico")))
            {
                var url = "https://raw.githubusercontent.com/Frici73/EffectOfWar/master/build/icon.ico";
                var response = await client.GetByteArrayAsync(url);
                File.WriteAllBytes(Path.Combine(exeFolder, "icon.ico"), response);
            }
            this.Icon = new Icon("icon.ico");
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = (Image)new Bitmap("icon.ico");
            notesBox.ReadOnly = true;
        }

        private async Task<bool> CheckVersion()
        {
            var url = "https://raw.githubusercontent.com/Frici73/EffectOfWar/master/build/version.json";
            var response = await client.GetStringAsync(url);
            serverVersion = JsonSerializer.Deserialize<Dictionary<string, string>>(response);
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

                return serverVersion["version"] == downloadedVersion;
            }

        }
        private async Task downloadResource()
        {
            Starter.Enabled = false;
            Refresh.Enabled = false;
            haladas.Visible = true;
            haladas.Text = "Játék telepítése";
            string urlBase = "https://api.github.com/repos/Frici73/EffectOfWar/contents/build";
            string gameurl = $"https://github.com/Frici73/EffectOfWar/releases/download/{serverVersion["version"]}/game.zip";
            string resourcesurl = urlBase + "/Resources";
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
                ZipFile.ExtractToDirectory(zip, Path.Combine(exeFolder, "game"));

                haladas.Text = "Tisztíttás";
                File.Delete(zip);

                // download resources
                string response = await client.GetStringAsync(resourcesurl);
                List<GitHubContent> dirs = JsonSerializer.Deserialize<List<GitHubContent>>(response);
                dirs.RemoveAll(d => d.download_url != null);
                string resource = Path.Combine(gameFolder, "Resource");
                if (!Directory.Exists(resource)) Directory.CreateDirectory(resource);
                foreach (GitHubContent dir in dirs)
                {
                    haladas.Text = $"{dir.name} letöltése";
                    string dirName = Path.Combine(resource, dir.name);
                    if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                    response = await client.GetStringAsync(dir.url);
                    List<GitHubContent> datas = JsonSerializer.Deserialize<List<GitHubContent>>(response);
                    foreach (GitHubContent file in datas)
                    {
                        if (!Rewrite.Checked && File.Exists(Path.Combine(dirName, file.name))) continue;
                        byte[] bytes = await client.GetByteArrayAsync(file.download_url);
                        File.WriteAllBytes(Path.Combine(dirName, file.name), bytes);
                    }
                }

                haladas.Text = "Verzió kezelés";
                StreamWriter w = new StreamWriter(Path.Combine(exeFolder, "version.json"));
                w.WriteLine("{");
                foreach (var items in serverVersion)
                {
                    w.Write($"\"{items.Key}\": \"{items.Value}\"");
                }
                w.WriteLine("}");
                w.Close();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Valószínűleg túl sok kérést ment a szerver felé ezért leállt a letöltés" + ex.Message);
            }

            Starter.Enabled = true;
            Refresh.Enabled = true;
            haladas.Visible = false;
            RefreshDatas();
        }

        private async void Starter_Click(object sender, EventArgs e)
        {
            if (!needupdate)
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
            else
            {
                await downloadResource();
            }
        }
    }

    public class GitHubContent
    {
        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public long size { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string download_url { get; set; }
        public string type { get; set; }
    }
}