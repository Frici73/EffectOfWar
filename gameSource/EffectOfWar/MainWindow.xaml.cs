using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        internal UIEditor editor;
        internal Tuple<RowDefinition, Button, string>?[]? txts;
        internal TextBox? ruleBox;
        internal Grid?[] ListboxsPlace = new Grid?[4];
        public MainWindow()
        {
            InitializeComponent();
            processing = new Processing(ConsoleD, SelectedSkills, CharacterDataInWar);
            editor = new UIEditor(team1UI, team2UI, processing);
            processing.linkEditor(editor);

            if (!Directory.Exists(CharacterInfos.document)) Directory.CreateDirectory(CharacterInfos.document);

            this.Icon = new BitmapImage(new Uri(System.IO.Path.Combine(Directory.GetParent(Directory.GetParent(CharacterInfos.exeFolder).ToString()).ToString(), "icon.ico")));
            this.Loaded += Window_Loaded;
            GamemodeB.Content = $"Gamemode: {processing.gamemode.ToString()}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            processing.characterinfosfill();
            // menu
            Menu.Width = this.Width;
            Menu.Height = this.Height;
            Menu.Background = Brushes.LightGray;
            BasicStatShower.IsReadOnly = true;
            Menu.IsVisibleChanged += (s, e) => ListboxsEdit();
            ListboxsEdit();
            Menu.RowDefinitions[0].Height = new GridLength(Menu.Height * 0.7);

            // battleground
            Battleground.Width = this.Width;
            Battleground.Height = this.Height;
            FireB.Click += new RoutedEventHandler((e, s) => processing.UseSkills());
            SkillUseFuncB.Click += new RoutedEventHandler((e, s) => processing.EditButtonState(SkillUseFuncB));

            // InfosG
            InfosG.ColumnDefinitions.Add(new ColumnDefinition());
            InfosG.ColumnDefinitions.Add(new ColumnDefinition());
        }
        private void ListboxsEdit()
        {
            if (Menu.Visibility != Visibility.Visible)
            {
                for (int i = 0; i < ListboxsPlace.Length; i++)
                {
                    if (ListboxsPlace[i] != null)
                    {
                        Menu.Children.Remove(ListboxsPlace[i]);
                        ListboxsPlace[i].Children.Clear();
                        ListboxsPlace[i] = null;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    HType CharType = (HType)i;
                    ListboxsPlace[i] = new Grid()
                    {
                        ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() { Width = new GridLength(0.1, GridUnitType.Star) } },
                        RowDefinitions = { new RowDefinition() { Height = new GridLength(0.1, GridUnitType.Star) }, new RowDefinition() },
                        HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    Menu.Children.Add(ListboxsPlace[i]);
                    Grid.SetRow(ListboxsPlace[i], 0);
                    Grid.SetColumn(ListboxsPlace[i], i);

                    TextBlock ListName = new TextBlock() { Text = CharType == HType.boss ? "Bosses" : CharType.ToString() + "s", TextAlignment = TextAlignment.Center };
                    ListboxsPlace[i].Children.Add(ListName);
                    Grid.SetRow(ListName, 0);
                    Grid.SetColumn(ListName, 0);
                    Grid.SetColumnSpan(ListName, 2);
                    ListBox listBox = new ListBox() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                    ListboxsPlace[i].Children.Add(listBox);
                    listBox.SelectionChanged += new SelectionChangedEventHandler((s, e) => ItemSelect(s, e));
                    Grid.SetRow(listBox, 1);
                    Grid.SetColumn(listBox, 0);
                    ScrollBar scrollBar = new ScrollBar()
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Minimum = 0,
                        Maximum = CharacterInfos.GetCharactersCountFromType(CharType)-4,
                        SmallChange = 1,
                        LargeChange = 1
                    };
                    ListboxsPlace[i].Children.Add(scrollBar);
                    Grid.SetRow(scrollBar, 1);
                    Grid.SetColumn(scrollBar, 1);
                    scrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>((s, e) => ScrollList(s));
                    ScrollList(scrollBar);
                }
            }
        }

        public void ScrollList(object sender)
        {
            ScrollBar bar = (ScrollBar)sender;
            ListBox? box = ListboxsPlace?.FirstOrDefault(g => g.Children.Contains(bar))?.Children.OfType<ListBox>().FirstOrDefault();
            TextBlock? title = ListboxsPlace?.FirstOrDefault(g => g.Children.Contains(bar))?.Children.OfType<TextBlock>().FirstOrDefault();
            if (box == null || title == null) return;
            box.Items.Clear();
            Tuple<string, string>?[] datas = new Tuple<string, string>[4];
            HType ht = (HType)Array.IndexOf(ListboxsPlace, ListboxsPlace.FirstOrDefault(g => g.Children.Contains(bar)));
            for (int i = 0; i < datas.Length; i++)
            {
                datas[i] = CharacterInfos.GetNameAndIMG((int)bar.Value + i, ht);
            }
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i] != null && System.IO.Path.Exists(datas[i].Item2))
                {
                    StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
                    BitmapImage bmp = new BitmapImage();

                    bmp.BeginInit();
                    bmp.UriSource = new Uri(datas[i].Item2, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();

                    Image img = new Image()
                    {
                        Source = bmp,
                        Width = 100,
                        Height = 100,
                        Margin = new Thickness(5)
                    };
                    TextBlock txt = new TextBlock() { Text = datas[i].Item1, VerticalAlignment = VerticalAlignment.Center };
                    switch (ht)
                    {
                        case HType.warrior: box.Background = Brushes.Orange; txt.Background = Brushes.Orange; txt.Foreground = Brushes.White; title.Background = Brushes.Orange; title.Foreground = Brushes.White; break;
                        case HType.ranger: box.Background = Brushes.DarkRed; txt.Background = Brushes.DarkRed; txt.Foreground = Brushes.White; title.Background = Brushes.DarkRed; title.Foreground = Brushes.White; break;
                        case HType.support: box.Background = Brushes.Green; txt.Background = Brushes.Green; txt.Foreground = Brushes.White; title.Background = Brushes.Green; title.Foreground = Brushes.White; break;
                        case HType.boss: box.Background = Brushes.Purple; txt.Background = Brushes.Purple; txt.Foreground = Brushes.White; title.Background = Brushes.Purple; title.Foreground = Brushes.White; break;
                    }
                    panel.Children.Add(img);
                    panel.Children.Add(txt);
                    box.Items.Add(new ListBoxItem() { Content = panel });
                }
                else break;
            }
        }
        public async void ItemSelect(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;


            if (listBox != null && listBox.SelectedItem is ListBoxItem Item)
            {
                StackPanel selectedItem = (StackPanel)Item.Content;
                string name = selectedItem.Children.OfType<TextBlock>().FirstOrDefault().Text.ToString();
                Character selected = CharacterInfos.GetCharacter(name);
                Team? team;
                if (noTeam.IsChecked == true) team = null;
                else if (firstTeam.IsChecked == true) team = Team.first;
                else team = Team.second;

                if (team != null) processing.Add(selected, (Team)team);
                BasicStatShower.Text = selected.ToString();

                firstShower.Content = "Csapat 1: ";
                foreach (Character c in processing.team1) firstShower.Content += c.Name + "; ";
                secondShower.Content = "Csapat 2: ";
                foreach (Character c in processing.team2) secondShower.Content += c.Name + "; ";
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
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void ConsoleVis(object sender, RoutedEventArgs e)
        {
            if (ConsoleD.Visibility == Visibility.Visible) ConsoleD.Visibility = Visibility.Collapsed;
            else ConsoleD.Visibility = Visibility.Visible;
        }
        private void ShowDatas(object sender, RoutedEventArgs e)
        {
            if (InfosG.Visibility == Visibility.Visible)
            {
                InfosG.Visibility = Visibility.Collapsed;
                for (int i = txts.Length - 1; i >= 0; i--)
                {
                    InfosG.Children.Remove(txts[i].Item2);
                    txts[i] = null;
                }
                InfosG.RowDefinitions.Clear();
                txts = null;
                ruleBox = null;
            }
            else
            {
                string[] files = Directory.GetFiles(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "rules"));
                InfosG.Visibility = Visibility.Visible;
                txts = new Tuple<RowDefinition, Button, string>[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    txts[i] = new Tuple<RowDefinition, Button, string>(new RowDefinition(), new Button() { Content = System.IO.Path.GetFileNameWithoutExtension(files[i]).ToUpper(), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center }, files[i]);
                    InfosG.RowDefinitions.Add(txts[i].Item1);
                    InfosG.Children.Add(txts[i].Item2);
                    txts[i].Item2.Click += new RoutedEventHandler(ShowRule);
                    Grid.SetRow(txts[i].Item2, i);
                }
                ruleBox = new TextBox() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap };
                InfosG.Children.Add(ruleBox);
                Grid.SetRow(ruleBox, 0);
                Grid.SetRowSpan(ruleBox, files.Length);
                Grid.SetColumn(ruleBox, 1);
            }
        }
        private void ShowRule(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Tuple<RowDefinition, Button, string> DataDuo = txts.First(d => d.Item2 == btn);
            ruleBox.Text = File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "rules", DataDuo.Item3));
        }
    }

    internal class CharacterUI
    {
        private Image img;
        private Label imgBtn;
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
            imgBtn = new Label()
            {
                Content = img,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = Brushes.LightGray,
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.DarkGray
            };

            Grid.SetRowSpan(imgBtn, 2);
            Grid.SetRow(imgBtn, slot * 2);
            ui.Children.Add(imgBtn);
            Panel.SetZIndex(imgBtn, 3);
            imgBtn.MouseEnter += new MouseEventHandler((s, e) => proc.CharacterInfoInWar(team, slot));
            imgBtn.MouseLeave += new MouseEventHandler((s, e) => proc.CharacterDataInWarF(team, slot));

            // up
            up = new Button() { Content = "↑", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(up, slot * 2);
            ui.Children.Add(up);
            Panel.SetZIndex(up, 2);
            up.Click += new RoutedEventHandler((s, e) => proc.EditSlot(team, Direction.up, slot));

            // down
            down = new Button() { Content = "↓", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(down, slot * 2 + 1);
            ui.Children.Add(down);
            Panel.SetZIndex(down, 2);
            down.Click += new RoutedEventHandler((s, e) => proc.EditSlot(team, Direction.down, slot));

            // s1
            s1 = new Button() { Content = "S1", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(s1, slot * 2);
            ui.Children.Add(s1);
            Panel.SetZIndex(s1, 1);
            s1.Click += new RoutedEventHandler((s, e) => proc.AddSkill(team, slot, Skill.first));

            // s2
            s2 = new Button() { Content = "S2", HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            Grid.SetRow(s2, slot * 2 + 1);
            ui.Children.Add(s2);
            Panel.SetZIndex(s2, 1);
            s2.Click += new RoutedEventHandler((s, e) => proc.AddSkill(team, slot, Skill.second));

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
            talent.Click += new RoutedEventHandler((s, e) => proc.AddSkill(team, slot, Skill.talent));
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
            void Edit(bool inc = false)
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

            if (show["talent"]) { Grid.SetColumn(talent, col); Edit(); talent.Visibility = Visibility.Visible; }
            else
            {
                Edit(true);
                talent.Visibility = Visibility.Collapsed;
            }

            if (show["s1"] && show["s2"])
            {
                Grid.SetColumn(s1, col);
                Grid.SetColumn(s2, col);
                Grid.SetRow(s1, slot * 2);
                Grid.SetRow(s2, slot * 2 + 1);
                Grid.SetColumnSpan(s1, span);
                Grid.SetColumnSpan(s2, span);
                Grid.SetRowSpan(s1, 1);
                Grid.SetRowSpan(s2, 1);
                Edit();
                s1.Visibility = Visibility.Visible;
                s2.Visibility = Visibility.Visible;
            }
            else if (show["s1"])
            {
                Grid.SetColumn(s1, col);
                Grid.SetRow(s1, slot * 2);
                Grid.SetColumnSpan(s1, span);
                Grid.SetRowSpan(s1, 2);
                Edit();
                s1.Visibility = Visibility.Visible;
                s2.Visibility = Visibility.Collapsed;
            }
            else if (show["s2"])
            {
                Grid.SetColumn(s2, col);
                Grid.SetRow(s2, slot * 2);
                Grid.SetColumnSpan(s2, span);
                Grid.SetRowSpan(s2, 2);
                Edit();
                s2.Visibility = Visibility.Visible;
                s1.Visibility = Visibility.Collapsed;
            }
            else
            {
                Edit(true);
                s1.Visibility = Visibility.Collapsed;
                s2.Visibility = Visibility.Collapsed;
            }
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
            else
            {
                Edit(true);
                up.Visibility = Visibility.Collapsed;
                down.Visibility = Visibility.Collapsed;
            }
            Grid.SetColumn(imgBtn, team == Team.first ? 0 : col);
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

        public void Offset(List<Character> chars, Team team, bool hide = false)
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
                            new bool[] { chars[i].TalentT != "", chars[i].S1T != "", chars[i].S2T != "", i != 0, i != chars.Count - 1 },
                            "talent", "s1", "s2", "up", "down"
                            );
                    ui[i].ChangeIMG(CharacterInfos.img(chars[i].GetType().Name.ToString()));
                }
            }
        }

        public void Create(List<Character> team1, List<Character> team2, GameMode gm)
        {
            Offset(team1, Team.first);
            Offset(team2, Team.second, gm == GameMode.BossBattle);
        }
    }

}