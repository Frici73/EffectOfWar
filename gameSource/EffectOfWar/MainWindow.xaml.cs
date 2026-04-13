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
            GamemodeB.Content = $"Gamemode: {processing.gamemode.ToString()}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // menu
                Menu.Width = this.Width;
                Menu.Height = this.Height;
                Menu.Background = Brushes.LightGray;
                BasicStatShower.IsReadOnly = true;
                

                foreach (Character w in AllCharacter.warriors) if (File.Exists(w.img)) warriors.Add(new ListItem { Name = w.Name, type = w.type, ImagePath = w.img });
                foreach (Character s in AllCharacter.supports) if (File.Exists(s.img)) supports.Add(new ListItem { Name = s.Name, type = s.type, ImagePath = s.img });
                foreach (Character r in AllCharacter.rangers) if (File.Exists(r.img)) rangers.Add(new ListItem { Name = r.Name, type = r.type, ImagePath = r.img });
                foreach (Character b in AllCharacter.bosses) if (File.Exists(b.img)) bosses.Add(new ListItem { Name = b.Name, type = b.type, ImagePath = b.img });
                WarriorsList.ItemsSource = warriors;
                SupportsList.ItemsSource = supports;
                RangersList.ItemsSource = rangers;
                BossesList.ItemsSource = bosses;
                WarriorsList.SelectionChanged += ItemSelect;
                RangersList.SelectionChanged += ItemSelect;
                SupportsList.SelectionChanged += ItemSelect;
                BossesList.SelectionChanged += ItemSelect;

            // battleground
                Battleground.Width = this.Width;
                Battleground.Height = this.Height;
        }

        public async void ItemSelect(object sender, RoutedEventArgs e)
        {
            ListBox listBox = sender as ListBox;


            if (listBox != null && listBox.SelectedItem is ListItem selectedItem)
            {
                string name = selectedItem.Name;
                List<Character>? list;
                switch (selectedItem.type)
                {
                    case HType.ranger: list = AllCharacter.rangers; break;
                    case HType.warrior: list = AllCharacter.warriors; break;
                    case HType.support: list = AllCharacter.supports; break;
                    default: list = null; break;
                }
                Character selected;
                Team? team;
                if (noTeam.IsChecked == true) team = null;
                else if (firstTeam.IsChecked == true) team = Team.first;
                else team = Team.second;
                try
                {
                    selected = list.First(b => b.Name == selectedItem.Name);
                }
                catch 
                {
                    selected = AllCharacter.bosses.First(b => b.Name == selectedItem.Name);
                }
                if (team != null) processing.Add(selected, (Team)team);
                BasicStatShower.Text = selected.ToString();

                firstShower.Content = "Csapat 1: ";
                foreach (Character c in processing.team1) firstShower.Content += c.Name+"; ";
                secondShower.Content = "Csapat 2: ";
                foreach (Character c in processing.team2) secondShower.Content += c.Name+"; ";
                listBox.UnselectAll();
            }
        }
        public async void ChangeGamemode(object sender, RoutedEventArgs e)
        {
            processing.Change();
            GamemodeB.Content = $"Gamemode: {processing.gamemode.ToString()}";
            firstShower.Content = "Csapat 1: ";
            secondShower.Content = "Csapat 2: ";
        }
        public async void StartGame(object sender, RoutedEventArgs e)
        {
            if (processing.Correct())
            {
                Menu.Visibility = Visibility.Collapsed;
                Battleground.Visibility = Visibility.Visible;
            }
        }
    }
    public class ListItem
    {
        public string Name { get; set; }
        public HType type { get; set; }
        public string ImagePath { get; set; }
    }
}