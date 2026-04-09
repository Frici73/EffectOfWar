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

namespace EffectOfWarLauncher
{
    public partial class Launcher : Form
    {
        bool needupdate = false;
        public Launcher()
        {
            InitializeComponent();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            downloadICO();

        }

        private async void downloadICO()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("EffectOfWar", "1.0"));
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
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

        private async void downloadResource()
        {

        }

        private void Starter_Click(object sender, EventArgs e)
        {
            if (needupdate) 
            {
                // run EffectOfWar.exe
            }
            else
            {
                downloadResource();
            }
        }
    }
}