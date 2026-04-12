using System.Collections.ObjectModel;
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
        internal ObservableCollection<ListItem> warriors = new ObservableCollection<ListItem>();
        internal ObservableCollection<ListItem> rangers = new ObservableCollection<ListItem>();
        internal ObservableCollection<ListItem> supports = new ObservableCollection<ListItem>();
        internal ObservableCollection<ListItem> bosses = new ObservableCollection<ListItem>();
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
                

                foreach (Character w in AllCharacter.warriors) if (File.Exists(w.img)) warriors.Add(new ListItem { Name = w.Name, ImagePath = w.img });
                foreach (Character s in AllCharacter.supports) if (File.Exists(s.img)) supports.Add(new ListItem { Name = s.Name, ImagePath = s.img });
                foreach (Character r in AllCharacter.rangers) if (File.Exists(r.img)) rangers.Add(new ListItem { Name = r.Name, ImagePath = r.img });
                foreach (Character b in AllCharacter.bosses) if (File.Exists(b.img)) bosses.Add(new ListItem { Name = b.Name, ImagePath = b.img });
                WarriorsList.ItemsSource = warriors;
                SupportsList.ItemsSource = supports;
                RangersList.ItemsSource = rangers;
                BossesList.ItemsSource = bosses;
            // battleground
                Battleground.Width = this.Width;
                Battleground.Height = this.Height;
        }
    }
    public class ListItem
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
    }
}