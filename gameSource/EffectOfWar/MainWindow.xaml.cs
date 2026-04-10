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
        internal string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
        internal string document = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EffectOfWar");
        public MainWindow()
        {
            InitializeComponent();
            processing = new Processing(ConsoleD);

            if (!Directory.Exists(document)) 
            {
                Directory.CreateDirectory(document);
            }
            
            this.Icon = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetParent(Directory.GetParent(exeFolder).ToString()).ToString(), "icon.ico")));
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // menu
                Menu.Width = this.Width;
                Menu.Height = this.Height;
                Menu.Background = Brushes.LightGray;

                BasicStatShower.IsReadOnly = true;
                BasicStatShower.MaxLines = 30;
                BasicStatShower.MinLines = 30;
                BasicStatShower.Width = this.Width/7;
                BasicStatShower.Height = this.Height;
                BasicStatShower.Margin = new Thickness { Left = this.Width - BasicStatShower.Width, Top = this.Height - BasicStatShower.Height, Bottom=0};

                characterListUI[] uIs = new characterListUI[4];
                uIs[0] = new characterListUI(HType.warrior, Menu);
                uIs[1] = new characterListUI(HType.ranger, Menu);
                uIs[2] = new characterListUI(HType.ranger, Menu);
                uIs[3] = new characterListUI(Menu);
            // battleground
                Battleground.Width = this.Width;
                Battleground.Height = this.Height;
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