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

namespace EffectOfWarLauncher
{
    public partial class Launcher : Form
    {
        bool needupdate = false;
        string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
        HttpClient client = new HttpClient();
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
        private void RefreshDatas()
        {
            if (!File.Exists(Path.Combine(exeFolder, "EffectOfWar.exe")))
            {
                needupdate = true;
                Starter.Text = "Játék telepítése";
            }
            else if (!CheckVersion().Result)
            {
                needupdate = true;
                Starter.Text = "Játék frissítése";
            }
            else
            {
                Starter.Text = "Játék indítása";
                needupdate = false;
            }
        }

        private async void ICO()
        {
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EffectOfWar", "1.0"));
            if (!File.Exists(Path.Combine(exeFolder, "icon.ico")))
            {
                var url = "https://raw.githubusercontent.com/Frici73/EffectOfWar/master/build/Resources/icon.ico";
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

            var serverVersion = JsonSerializer.Deserialize<Dictionary<string, string>>(response)["version"];

            var localPath = Path.Combine(exeFolder, "version.json");
            var downloadedVersion =
                JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(localPath))["version"];

            return serverVersion == downloadedVersion;
        }
        private async Task downloadResource()
        {
            string urlBase = "https://api.github.com/repos/Frici73/EffectOfWar/contents/build";
            string gameurl = urlBase + "/game";
            string resourcesurl = urlBase + "/Resources";
            string gameFolder = Path.Combine(exeFolder, "game");
            if (!Directory.Exists(gameFolder)) Directory.CreateDirectory(gameFolder);

            // download the application
            string response = await client.GetStringAsync(gameurl);
            var appDatas = JsonSerializer.Deserialize<List<GitHubContent>>(response);
            foreach (var appData in appDatas)
            {
                if (appData.download_url == null) continue;
                byte[] bytes = await client.GetByteArrayAsync(appData.download_url);
                File.WriteAllBytes(Path.Combine(gameFolder, appData.name), bytes);
            }

            // download resources
            response = await client.GetStringAsync(resourcesurl);
            List<GitHubContent> dirs = JsonSerializer.Deserialize<List<GitHubContent>>(response);
            dirs.RemoveAll(d=>d.download_url==null || d.download_url.Length == 0);
            string resource = Path.Combine(gameFolder, "Resource");
            if (!Directory.Exists(resource)) Directory.CreateDirectory(resource);
            foreach (GitHubContent dir in dirs)
            {
                string dirName = Path.Combine(resource, dir.name);
                if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
                response = await client.GetStringAsync(dir.url);
                List<GitHubContent> datas = JsonSerializer.Deserialize<List<GitHubContent>>(response);
                foreach (GitHubContent file in datas)
                {
                    byte[] bytes = await client.GetByteArrayAsync(file.download_url);
                    File.WriteAllBytes(Path.Combine(dirName, file.name), bytes );
                }
            }

            byte[] data = await client.GetByteArrayAsync(urlBase+"version.json");
            File.WriteAllBytes(Path.Combine(exeFolder, "/version.json"), data);

            RefreshDatas();
        }

        private async void Starter_Click(object sender, EventArgs e)
        {
            if (!needupdate)
            {
                try
                {
                    if (Process.GetProcessesByName("EffectOfWar").Length <= 0)
                        Process.Start(Path.Combine(exeFolder, "EffectOfWar.exe"));
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