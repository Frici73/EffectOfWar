using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EffectOfWar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal Processing processing;
        internal string exeFolder;
        internal string document;
        public MainWindow()
        {
            InitializeComponent();
            processing = new Processing(Console);

            exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            document = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(System.IO.Path.Combine(document, "EffectOfWar"))) 
            {
                Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.Combine(document, "EffectOfWar")));
            }
            document = System.IO.Path.Combine(System.IO.Path.Combine(document, "EffectOfWar"));
            window.Icon = new BitmapImage(new Uri(System.IO.Path.Combine(exeFolder, "Resources", "icon.ico")));

            window.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // menu
                Menu.Width = window.Width;
                Menu.Height = window.Height;
                Menu.Background = Brushes.LightGray;

                BasicStatShower.IsReadOnly = true;
                BasicStatShower.MaxLines = 30;
                BasicStatShower.MinLines = 30;
                BasicStatShower.Width = window.Width/7;
                BasicStatShower.Height = window.Height;
                BasicStatShower.Margin = new Thickness { Left = window.Width - BasicStatShower.Width, Top = window.Height - BasicStatShower.Height, Bottom=0};

                characterListUI[] uIs = new characterListUI[4];
                uIs[0] = new characterListUI(HType.warrior, Menu);
                uIs[1] = new characterListUI(HType.ranger, Menu);
                uIs[2] = new characterListUI(HType.ranger, Menu);
                uIs[3] = new characterListUI(Menu);
            // battleground
                Battleground.Width = window.Width;
                Battleground.Height = window.Height;
                Battleground.Visibility = Visibility.Collapsed;
        }
    }
    internal class characterListUI
    {
        private Border border;
        private Label title;
        private List<Image> images;
        public characterListUI (HType type, Grid p) : this(type.ToString() + 's', p) { }
        public characterListUI(Grid p) : this("bosses", p) { }
        private characterListUI(string title, Grid parent)
        {   
            this.title = new Label();
            parent.Children.Add(this.title);
            this.title.Content = title;
            this.title.Width = 100;
            this.title.Height = 100;
            this.title.Visibility = Visibility.Visible;

            images = new List<Image>();
            for (int i = 0; i < 20; i++)
            {
                images.Add(new Image());
                parent.Children.Add(images[i]);
                images[i].Source = null;
            }
            //images[i].Source = new BitmapImage(new Uri(System.IO.Path.Combine(exeFolder, "Resources", title, )));
        }
    }
}