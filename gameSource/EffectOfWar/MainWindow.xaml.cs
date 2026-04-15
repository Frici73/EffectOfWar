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
        internal UIEditor editor;
        public MainWindow()
        {
            InitializeComponent();
            processing = new Processing(ConsoleD);
            editor = new UIEditor(team1UI, team2UI, processing);

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
                editor.Create(processing.team1, processing.team2, processing.gamemode);
                processing.StartOfGame();
            }
        }
    }
    public class ListItem
    {
        public string Name { get; set; }
        public HType type { get; set; }
        public string ImagePath { get; set; }
    }

    internal class CharacterUI
    {
        private Image img;
        private Button imgBtn;
        private Button up;
        private Button down;
        private Button talent;
        private Button s1;
        private Button s2;
        private Dictionary<string, bool> show;
        byte slot;
        internal CharacterUI(byte slot, Grid ui, Team team, Processing proc)
        {
            this.slot = slot;
            // Dict alkotás
            show = new Dictionary<string, bool>();
            show.Add("s1", true);
            show.Add("s2", true);
            show.Add("talent", true);
            show.Add("up", true);
            show.Add("down", true);

            // kép
            img = new Image() { Stretch = Stretch.Uniform };
            imgBtn = new Button() { Content=img };
            Grid.SetRowSpan(imgBtn, 2);
            Grid.SetRow(imgBtn, slot*2);
            ui.Children.Add(imgBtn);
            Panel.SetZIndex(imgBtn, 3);

            // up
            up = new Button() { Content = "↑", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(up, slot*2);
            ui.Children.Add(up);
            Panel.SetZIndex(up, 2);
            //up.MouseLeftButtonDown += new MouseEventHandler(proc.EditSlot(team, Direction.up, slot));
                


            // down
            down = new Button() { Content = "↓", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(down, slot*2+1);
            ui.Children.Add(down);
            Panel.SetZIndex(down, 2);
            proc.EditSlot(team, Direction.down, slot);

            // s1
            s1 = new Button() { Content = "S1", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(s1, slot * 2);
            ui.Children.Add(s1);
            Panel.SetZIndex(s1, 1);
            proc.UseSkill(team, slot, Skill.first);

            // s2
            s2 = new Button() { Content = "S2", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(s2, slot*2+1);
            ui.Children.Add(s2);
            Panel.SetZIndex(s2, 1);
            proc.UseSkill(team, slot, Skill.second);

            // talent
            var text = new TextBlock
            {
                TextAlignment = TextAlignment.Center
            };
            text.Inlines.Add("T");
            text.Inlines.Add(new LineBreak());
            text.Inlines.Add("A");
            text.Inlines.Add(new LineBreak());
            text.Inlines.Add("L");
            text.Inlines.Add(new LineBreak());
            text.Inlines.Add("E");
            text.Inlines.Add(new LineBreak());
            text.Inlines.Add("N");
            text.Inlines.Add(new LineBreak());
            text.Inlines.Add("T");
            talent = new Button() { Content = text, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRowSpan(talent, 2);
            Grid.SetRow(talent, slot * 2);
            Panel.SetZIndex(talent, 0);
            ui.Children.Add(talent);
            proc.UseSkill(team, slot, Skill.talent);
        }
    
        internal void EditLayout(Team team, bool value, params string[] keys)
        {
            foreach (string key in keys) show[key] = value;
            ModifyFrontend(team);
        }

        internal void EditLayout(Team team, bool[] values, params string[] keys)
        {
            if (values.Length != keys.Length) throw new Exception($"Parameters length is not equal: bool {values.Length} | strings {keys.Length}");
            for (int i = 0; i < values.Length; i++) show[keys[i]] = values[i];
            ModifyFrontend(team);
        }

        internal void ChangeIMG(string path)
        {
            if (File.Exists(path)) img.Source = new BitmapImage(new Uri(path));
        }

        private void ModifyFrontend(Team team)
        {
            byte span = 1;
            byte col = team == Team.first ? (byte)3 : (byte)0;
            void Edit(bool inc=false)
            {
                if (inc)
                {
                    span++;
                    if (team == Team.first) col--;
                }
                else
                {
                    if (span != 1 && team == Team.second) col++;
                    span = 1;
                    if (team == Team.first) col--;
                    else col++;
                }
            }

            if (show["talent"]) { Grid.SetColumn(talent, col); Edit(); }
            else Edit(true);

            if (show["s1"] && show["s2"])
            {
                Grid.SetColumn(s1, col);
                Grid.SetColumn(s2, col);
                Grid.SetRow(s1, slot*2);
                Grid.SetRow(s2, slot * 2 + 1);
                Grid.SetColumnSpan(s1, span);
                Grid.SetColumnSpan(s2, span);
                Grid.SetRowSpan(s1, 1);
                Grid.SetRowSpan(s2, 1);
                Edit();
            }
            else if (show["s1"])
            {
                Grid.SetColumn(s1, col);
                Grid.SetRow(s1, slot * 2);
                Grid.SetColumnSpan(s1, span);
                Grid.SetRowSpan(s1, 2);
                Edit();
            }
            else if (show["s2"])
            {
                Grid.SetColumn(s2, col);
                Grid.SetRow(s2, slot * 2);
                Grid.SetColumnSpan(s2, span);
                Grid.SetRowSpan(s2, 2);
                Edit();
            }
            else Edit(true);

            if (show["up"] && show["down"])
            {
                Grid.SetColumn(up, col);
                Grid.SetColumn(down, col);
                Grid.SetRow(up, slot * 2);
                Grid.SetRow(down, slot * 2 + 1);
                Grid.SetColumnSpan(up, span);
                Grid.SetColumnSpan(down, span);
                Grid.SetRowSpan(up, 1);
                Grid.SetRowSpan(down, 1);
                up.Visibility = Visibility.Visible;
                down.Visibility = Visibility.Visible;
                Edit();
            }
            else if (show["up"])
            {
                Grid.SetColumn(up, col);
                Grid.SetRow(up, slot * 2);
                Grid.SetColumnSpan(up, span);
                Grid.SetRowSpan(up, 2);
                up.Visibility = Visibility.Visible;
                down.Visibility = Visibility.Collapsed;
                Edit();
            }
            else if (show["down"])
            {
                Grid.SetColumn(down, col);
                Grid.SetRow(down, slot * 2);
                Grid.SetColumnSpan(down, span);
                Grid.SetRowSpan(down, 2);
                up.Visibility = Visibility.Collapsed;
                down.Visibility = Visibility.Visible;
                Edit();
            }
            else Edit(true);

            Grid.SetColumn(imgBtn, team==Team.first?0:col);
            Grid.SetColumnSpan(imgBtn, span);
        }
    }

    internal class UIEditor
    {
        private CharacterUI?[] team1UI = new CharacterUI?[5];
        private CharacterUI?[] team2UI = new CharacterUI?[6];
        private Grid team1G;
        private Grid team2G;
        private Processing link;
        public UIEditor(Grid team1, Grid team2, Processing proc) 
        {
            team1G = team1;
            team2G = team2;
            link = proc;
        }

        public void Config(Grid ui, byte characterCount)
        {
            int rowsExist = ui.RowDefinitions.Count;
            int rows = characterCount * 2;
            while (rowsExist != rows)
            {
                if (rowsExist > rows) ui.RowDefinitions.RemoveAt(rowsExist - 1);
                else 
                {
                    RowDefinition NewRow = new RowDefinition();
                    NewRow.Height = new GridLength(1, GridUnitType.Star);
                    ui.RowDefinitions.Add(NewRow); 
                }
                rowsExist = ui.RowDefinitions.Count;
            }
        }

        public void Offset(List<Character> chars, Team team, bool hide=false)
        {
            CharacterUI[] ui = team == Team.first ? team1UI : team2UI;
            Grid grid = team == Team.first ? team1G : team2G;
            Config(grid, (byte)chars.Count);
            for (int i = 0; i < ui.Length; i++) 
            {
                if (i >= chars.Count) ui[i] = null;
                else if (i < chars.Count && ui[i] == null) ui[i] = new CharacterUI((byte)i, grid, team, link);
                if (ui[i] != null)
                {
                    if (hide) ui[i].EditLayout(team, false, "talent", "s1", "s2", "up", "down");
                    else 
                        ui[i].EditLayout(team,
                            new bool[] { chars[i].TalentT != "", chars[i].S1T != "", chars[i].S2T != "", i != 0, i != chars.Count-1 },
                            "talent", "s1", "s2", "up", "down"
                            );
                    ui[i].ChangeIMG(chars[i].img);
                }
            }
        }
    
        public void Create(List<Character> team1, List<Character> team2, GameMode gm)
        {
            Offset(team1, Team.first);
            Offset(team2, Team.second, gm==GameMode.BossBattle?true:false);
        }
    }
}