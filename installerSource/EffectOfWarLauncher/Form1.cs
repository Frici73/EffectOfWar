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

namespace EffectOfWarLauncher
{
    public partial class Launcher : Form
    {
        bool needupdate = false;
        string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
        public Launcher()
        {
            InitializeComponent();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            downloadICO();
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
            else Starter.Text = "Játék indítása";
        }

        private async void downloadICO()
        {
            var client = new HttpClient();
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

        private Task<bool> CheckVersion() 
        {
            return Task.Run(() => 
            { 
                return true; 
            } );
        }


        private async void downloadResource()
        {




            Starter.Text = "Játék Indítása";
            needupdate = false;
        }

        private void Starter_Click(object sender, EventArgs e)
        {
            if (!needupdate) 
            {
                Process siker = Process.Start(Path.Combine(exeFolder, "EffectOfWar.exe"));
            }
            else
            {
                downloadResource();
            }
        }
    }
}