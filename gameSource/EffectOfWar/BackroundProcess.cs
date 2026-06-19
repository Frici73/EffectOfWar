using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace EffectOfWar
{
    public class Processing
    {
        public List<Character> team1 { get; private set; }
        public List<Character> team2 { get; private set; }
        public byte DeadCharacters;
        public byte LiveCharacters => (byte)(team1.Count+team2.Count);
        internal GameMode gamemode;
        public TextBox Console;

        // frontend & backend
        private Tuple<byte, Skill>?[] useSkills = new Tuple<byte, Skill>?[6];
        private Team activeTeam = Team.first;
        private bool mode = true; // true=selecting | false=deleting
        private TextBox selectedSkills;
        private TextBox CharacterDataInWar;
        private UIEditor editor;
        private MainWindow w;

        public Processing(TextBox tb, TextBox tb2, TextBox tb3, MainWindow w)
        {
            team1 = new List<Character>();
            team2 = new List<Character>();
            DeadCharacters = 0;
            gamemode = GameMode.PvP;
            Console = tb;
            selectedSkills = tb2;
            CharacterDataInWar = tb3;
            this.w = w;
        }
        internal void linkEditor(UIEditor e) => editor = e;
        internal void Change()
        {
            if (gamemode == GameMode.PvP) gamemode = GameMode.BossBattle;
            else gamemode = GameMode.PvP;
            team1.Clear();
            team2.Clear();
        }
        internal void Add(Character c, Team team)
        {
            if (team == Team.first)
            {
                if (team1.Count(d=>d.Name == c.Name) > 0) team1.RemoveAll(d=>d.Name == c.Name);
                else if (((team1.Count < 3 && gamemode == GameMode.PvP) || (team1.Count < 4 && gamemode == GameMode.BossBattle)) && CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) != HType.boss)
                {
                    Character clone = c.Clone();
                    team1.Add(clone);
                    clone.TeamChange(team, this);
                }
            }
            else
            {
                if (team2.Count(d => d.Name == c.Name) > 0) team2.RemoveAll(d => d.Name == c.Name);
                else if ((team2.Count < 3 && gamemode == GameMode.PvP && CharacterInfos.GetCharacterType(c.GetType().Name.ToString())!=HType.boss || (team2.Count < 1 && gamemode == GameMode.BossBattle && CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.boss)))
                {
                    Character clone = c.Clone();
                    team2.Add(clone);
                    clone.TeamChange(team, this);
                }
            }
        }
        internal void Remove(Character c, Team team)
        {
            List<Character> list = team == Team.first ? team1 : team2;
            list.RemoveAll(e => e.Name == c.Name);
            list.ForEach(c2 => c.Slot = (byte)list.IndexOf(c));
        }

        internal void AddClone(Character c, Team team)
        {
            //Character clone = c.Clone();
            if (team == Team.first)
            {
                team1.Add(c);
                c.Slot = (byte)(team1.Count - 1);
            }
            else 
            {
                team2.Add(c);
                c.Slot = (byte)(team1.Count - 1);
            }
            c.TeamChange(team, this);
        }
        
        internal void EditSlot(Team team, Direction dir, byte index)
        {
            List<Character> list = team==Team.first ? team1 : team2;
            int direction = dir==Direction.up?-1:1;
            byte slot = list[index].Slot;
            list[index].Slot = list[index + direction].Slot;
            list[index + direction].Slot = slot;
            list = list.OrderBy(e => e.Slot).ToList();
            team1 = team1.OrderBy(e => e.Slot).ToList();
            team2 = team2.OrderBy(e => e.Slot).ToList();

            string[] paths = new string[list.Count];
            for (int i = 0; i < list.Count; i++) paths[i] = list[i].GetType().Name;
            editor.Offset(list, team, team == Team.second && gamemode == GameMode.BossBattle);
        }
        
        public List<Character> Characters(byte teamID, bool team) // team: true=csapattárs | false=ellenfél
        {
            if (teamID == 1 && team) return team1;
            else if (teamID == 2 && !team) return team1;
            else return team2;
        }

        public object GetPropertyValue(object obj, string fieldName, int? index = null)
        {
            var field = obj.GetType().GetField(fieldName);

            if (field == null)
                return null;

            var value = field.GetValue(obj);

            if (index.HasValue)
            {
                if (value is Array arr)
                {
                    int i = index.Value;

                    if (i < 0 || i >= arr.Length)
                        return null;

                    return arr.GetValue(i);
                }

                return null;
            }

            return value;
        }

        public void SetPropertyValue(object obj, string fieldName, object value, int? index = null)
        {
            var field = obj.GetType().GetField(fieldName);

            if (field == null)
                return;

            if (index.HasValue)
            {
                var arr = field.GetValue(obj) as Array;
                if (arr == null)
                    return;

                int i = index.Value;
                if (i < 0 || i >= arr.Length)
                    return;

                var elemType = arr.GetType().GetElementType();
                var converted = Convert.ChangeType(value, elemType);

                arr.SetValue(converted, i);
                return;
            }

            var targetType = field.FieldType;
            var safeValue = Convert.ChangeType(value, targetType);

            field.SetValue(obj, safeValue);
        }

        public void ChangePropertyValue(object obj, string fieldName, object value, Operator o = Operator.plus, int? index = null)
        {
            var field = obj.GetType().GetField(fieldName);

            if (field == null)
                return;

            var currentValue = field.GetValue(obj);

            if (currentValue == null)
                return;

            // ===== ARRAY =====
            if (index.HasValue && currentValue is Array arr)
            {
                int i = index.Value;
                if (i < 0 || i >= arr.Length)
                    return;

                var elemType = arr.GetType().GetElementType();

                dynamic a = arr.GetValue(i);
                dynamic b = value;

                dynamic rawResult =
                    o == Operator.plus ? a + b :
                    o == Operator.minusz ? a - b :
                    o == Operator.divide ? a / b :
                    a * b;

                var result = Convert.ChangeType(rawResult, elemType);

                arr.SetValue(result, i);
                return;
            }

            // ===== SCALAR =====
            var fieldType = field.FieldType;

            dynamic x = currentValue;
            dynamic y = value;

            dynamic raw =
                o == Operator.plus ? x + y :
                o == Operator.minusz ? x - y :
                o == Operator.divide ? x / y :
                x * y;

            var converted = Convert.ChangeType(raw, fieldType);

            field.SetValue(obj, converted);
        }

        public void InsertText(string text) => Console.Text += text + Environment.NewLine;
    
        public void Dead(Character c)
        {
            if (c.teamID == 1) Remove(c, Team.first);
            else Remove(c, Team.second);
            DeadCharacters++;
        }
    
        public void Reset()
        {
            team1.Clear();
            team2.Clear();
            DeadCharacters = 0;
        }
    
        public Team GetTeam(Character c)
        {
            if (team1.Contains(c)) return Team.first;
            return Team.second;
        }
        public bool Correct()
        {
            return (team1.Count == 3 && gamemode == GameMode.PvP || team1.Count == 4 && gamemode == GameMode.BossBattle) && (team2.Count == 3 && gamemode == GameMode.PvP || team2.Count == 1 && gamemode == GameMode.BossBattle);
        }
    
        public void characterinfosfill()
        {
            // supports
            CharacterInfos.AddCharacter("Joker", HType.support, typeof(Joker));
            CharacterInfos.AddCharacter("Doctor", HType.support, typeof(Doctor));
            CharacterInfos.AddCharacter("Virus", HType.support, typeof(Virus));
            CharacterInfos.AddCharacter("Alchemist", HType.support, typeof(Alchemist));
            CharacterInfos.AddCharacter("Merlin", HType.support, typeof(Merlin));
            CharacterInfos.AddCharacter("Garden", HType.support, typeof(Garden));
            CharacterInfos.AddCharacter("Feather", HType.support, typeof(Feather));
            CharacterInfos.AddCharacter("Connection", HType.support, typeof(Connection));
            CharacterInfos.AddCharacter("Eternal", HType.support, typeof(Eternal));
            CharacterInfos.AddCharacter("Snake", HType.support, typeof(Snake));
            CharacterInfos.AddCharacter("Grandmother", HType.support, typeof(Grandmother));
            CharacterInfos.AddCharacter("Equality", HType.support, typeof(Equality));
            CharacterInfos.AddCharacter("Collect", HType.support, typeof(Collect));
            CharacterInfos.AddCharacter("Further", HType.support, typeof(Further));
            CharacterInfos.AddCharacter("Reverse", HType.support, typeof(Reverse));

            // rangers
            CharacterInfos.AddCharacter("Lightning", HType.ranger, typeof(Lightning));
            CharacterInfos.AddCharacter("Breaker", HType.ranger, typeof(Breaker));
            CharacterInfos.AddCharacter("Reaper", HType.ranger, typeof(Reaper));
            CharacterInfos.AddCharacter("Mage", HType.ranger, typeof(Mage));
            CharacterInfos.AddCharacter("Robin", HType.ranger, typeof(Robin));
            CharacterInfos.AddCharacter("Zoro", HType.ranger, typeof(Zoro));
            CharacterInfos.AddCharacter("Time", HType.ranger, typeof(Time));
            CharacterInfos.AddCharacter("Dynamic", HType.ranger, typeof(Dynamic));
            CharacterInfos.AddCharacter("Gravity", HType.ranger, typeof(Gravity));
            CharacterInfos.AddCharacter("Sacrifice", HType.ranger, typeof(Sacrifice));
            CharacterInfos.AddCharacter("Shard", HType.ranger, typeof(Shard));
            CharacterInfos.AddCharacter("Raven", HType.ranger, typeof(Raven));
            CharacterInfos.AddCharacter("Berserker", HType.ranger, typeof(Berserker));
            CharacterInfos.AddCharacter("Rat", HType.ranger, typeof(Rat));
            CharacterInfos.AddCharacter("Trap", HType.ranger, typeof(Trap));

            // warriors
            CharacterInfos.AddCharacter("Barrier", HType.warrior, typeof(Barrier));
            CharacterInfos.AddCharacter("Guardian", HType.warrior, typeof(Guardian));
            CharacterInfos.AddCharacter("Bulldozer", HType.warrior, typeof(Bulldozer));
            CharacterInfos.AddCharacter("Fulmare", HType.warrior, typeof(Fulmare));
            CharacterInfos.AddCharacter("ArthurKing", HType.warrior, typeof(ArthurKing));
            CharacterInfos.AddCharacter("Trash", HType.warrior, typeof(Trash));
            CharacterInfos.AddCharacter("Afterglow", HType.warrior, typeof(Afterglow));
            CharacterInfos.AddCharacter("Cooldown", HType.warrior, typeof(Cooldown));
            CharacterInfos.AddCharacter("Frame", HType.warrior, typeof(Frame));
            CharacterInfos.AddCharacter("GodOfDeath", HType.warrior, typeof(GodOfDeath));
            CharacterInfos.AddCharacter("Smoke", HType.warrior, typeof(Smoke));
            CharacterInfos.AddCharacter("Fortuneteller", HType.warrior, typeof(Fortuneteller));
            CharacterInfos.AddCharacter("Szunvukung", HType.warrior, typeof(Szunvukung));
            CharacterInfos.AddCharacter("Emerald", HType.warrior, typeof(Emerald));
            CharacterInfos.AddCharacter("Phase", HType.warrior, typeof(Phase));

            // bosses
            CharacterInfos.AddCharacter("Chaos", HType.boss, typeof(Chaos));
            CharacterInfos.AddCharacter("Fate", HType.boss, typeof(Fate));
            CharacterInfos.AddCharacter("Werewolf", HType.boss, typeof(Werewolf));
            CharacterInfos.AddCharacter("Goblins", HType.boss, typeof(Goblins));
            CharacterInfos.AddCharacter("Vampire", HType.boss, typeof(Vampire));
            CharacterInfos.AddCharacter("Moon", HType.boss, typeof(Moon));
            CharacterInfos.AddCharacter("Solmir", HType.boss, typeof(Solmir));
            CharacterInfos.AddCharacter("Tarantula", HType.boss, typeof(Tarantula));
        }
        public void StartOfGame()
        {
            // kaszt nerf
            if (gamemode == GameMode.PvP)
            {
                float[] HealerHealing = new float[3] { 10f, 6.6f, 3.3f };
                float[] TankDMGResistance = new float[3] { 10f, 6.6f, 3.3f };
                float[] RangerDamage = new float[3] { 20f, 16.6f, 13.3f };
                int healers;
                int rangers;
                int warriors;
                
                healers = team1.Count(c => CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.support);
                rangers = team1.Count(c => CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.ranger);
                warriors = team1.Count(c => CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.warrior);
                foreach (Character c in team1)
                    if (CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.support) c.HealDealt = HealerHealing[healers-1];
                    else if (CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.ranger) c.DMGDealt = RangerDamage[rangers-1];
                    else if (CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.warrior) c.DMGResistance = TankDMGResistance[warriors-1];

                healers = team2.Count(c => CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.support);
                rangers = team2.Count(c => CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.ranger);
                warriors = team2.Count(c => CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.warrior);
                foreach (Character c in team2)
                    if (CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.support) c.HealDealt = HealerHealing[healers-1];
                    else if (CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.ranger) c.DMGDealt = RangerDamage[rangers-1];
                    else if (CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.warrior) c.DMGResistance = TankDMGResistance[warriors-1];

            }

            // Start of Game
            for (int i = 0; i<team1.Count; i++)
            {
                team1[i].Slot = (byte)i;
                team1[i].StartOfGame();
            }
            for (int i = 0; i < team2.Count; i++)
            {
                team2[i].Slot = (byte)i;
                team2[i].StartOfGame();
            }
            for (int i = 0; i < team1.Count; i++) team1[i].StartOfTurn();
            editor.Offset(team1, Team.first);
            editor.Offset(team2, Team.second, gamemode == GameMode.BossBattle);
        }
        internal void AddSkill(Team t, byte index, Skill s)
        {
            if (t==activeTeam)
            {
                if (mode)
                {
                    int size = t == Team.first ? team1.Count : team2.Count;
                    bool talent = s == Skill.talent && !useSkills.Any(e => e != null && e.Item1 == index && e.Item2 == Skill.talent);
                    bool skill = s != Skill.talent && (useSkills.Count(d => d!=null && d.Item2 != Skill.talent) < size);
                    if (talent || skill)
                    {
                        int Aindex;
                        try
                        {
                            Aindex = Array.LastIndexOf(useSkills, useSkills.Last(e => e != null)) + 1;
                        }
                        catch { Aindex = 0; }
                        try { useSkills[Aindex] = new Tuple<byte, Skill>(index, s); }
                        catch { }
                    }
                }
                else
                {
                    int removeIndex = Array.FindLastIndex(
                        useSkills,
                        e => e != null &&
                             e.Item1 == index &&
                             e.Item2 == s);
                    if (removeIndex != -1) useSkills[removeIndex] = null;
                }
                selectedSkills.Text = "";
                foreach (var datas in useSkills)
                    if (datas != null) selectedSkills.Text += $"{datas.Item1 + 1} karakter {datas.Item2} képesség\n";
            }
        }
        internal void TurnOff()
        {
            for (int i = 0; i < useSkills.Length; i++) useSkills[i] = null;
            selectedSkills.Text = "";
        }
        private void winner()
        {
            string num = team1.Count == 0 ? "2" : "1";
            MessageBox.Show($"{num}. Játékos nyert");
            w.EndOfBattle();
        }
        private void turnswitch(Team newActive)
        {
            List<Character> end;
            List<Character> start;
            if (newActive == Team.first)
            {
                start = team1;
                end = team2;
            }
            else
            {
                start = team2;
                end = team1;
            }
            for (int i = start.Count - 1; i >= 0; i--) start[i].StartOfTurn();
            for (int i = end.Count - 1; i >= 0; i--) end[i].EndOfTurn();
            if (team1.Count == 0 || team2.Count == 0) winner();
            
        }
        internal void UseSkills()
        {
            for (int i = 0; i < useSkills.Length; i++)
            {
                if (useSkills[i] != null)
                {
                    List<Character> team = activeTeam == Team.first ? team1 : team2;
                    byte index = useSkills[i].Item1;
                    Skill s = useSkills[i].Item2;
                    InsertText($"{team[index].Name} {s} képesség");
                    team[index].UseSkill(s);
                }
                useSkills[i] = null;
            }
            selectedSkills.Text = "";

            if (activeTeam == Team.first)
            {
                activeTeam = Team.second;
                turnswitch(activeTeam);
                
                if (gamemode==GameMode.BossBattle)
                {
                    activeTeam = Team.first;
                    turnswitch(activeTeam);
                }
            }
            else
            {
                activeTeam = Team.first;
                turnswitch(activeTeam);
            }
            editor.Offset(team1, Team.first);
            editor.Offset(team2, Team.second, gamemode == GameMode.BossBattle);
        }
        internal void EditButtonState(object sender)
        {
            Button s = sender as Button;
            if (mode)
            {
                s.Content = "Remove";
                mode = false;
            }  
            else
            {
                s.Content = "Select";
                mode = true;
            }
            
        }
        internal void CharacterDataInWarF(Team team, byte index)
        {
            Character c = team == Team.first ? team1[index] : team2[index];
            CharacterDataInWar.Text = c.State();
        }
        internal void CharacterInfoInWar(Team team, byte index)
        {
            Character c = team == Team.first ? team1[index] : team2[index];
            CharacterDataInWar.Text = c.ToString();
        }
    }

    public static class CharacterInfos
    {
        internal static string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
        internal static string document = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EffectOfWar");
        private static Dictionary<HType, List<string>> Names = new Dictionary<HType, List<string>>() { { HType.ranger, new List<string>() }, { HType.warrior, new List<string>() }, { HType.support, new List<string>() }, { HType.boss, new List<string>() } };
        private static List<Type> Types = new List<Type>();

        public static void AddCharacter(string pathORname, HType htype, Type type)
        {
            if (typeof(Character).IsAssignableFrom(type))
            {
                if (pathORname.EndsWith(".png") || pathORname.EndsWith(".txt")) Names[htype].Add(pathORname.Substring(0, pathORname.Length - 4));
                else Names[htype].Add(pathORname);
                Types.Add(type);
            }
        }

        public static string img(string name, HType? htype = null)
        {
            string keyS = "";
            if (htype.HasValue && Names.ContainsKey(htype.Value) && Names[htype.Value].Contains(name)) keyS = htype.Value.ToString();
            else
                foreach (var key in Names.Keys) if (Names[key].Contains(name)) keyS = key.ToString();
            return System.IO.Path.Combine(exeFolder, "Resources", keyS, name + ".png");
        }

        internal static Tuple<string, string>? GetNameAndIMG(int index, HType htype)
        {
            if (Names.ContainsKey(htype) && index >= 0 && index < Names[htype].Count)
            {
                return new Tuple<string, string>(Names[htype][index], System.IO.Path.Combine(exeFolder, "Resources", htype.ToString(), Names[htype][index] + ".png"));
            }
            return null;
        }

        internal static Character GetCharacter(string name) => (Character)Activator.CreateInstance(Types.First(x => x.Name == name));

        internal static HType GetCharacterType(string name) => Names.First(x => x.Value.Contains(name)).Key;

        internal static string[] GetCharactersFromType(HType htype) => Types.Where(x => Names[htype].Contains(x.Name.ToString())).Select(x => x.Name.ToString()).ToArray();

        internal static int GetCharactersCountFromType(HType htype) => Names.ContainsKey(htype) ? Names[htype].Count : 0;
    }

    public static class Converter
    {
        public static ushort ConvertingToUshort(float f)
        {
            if (f < ushort.MinValue) return ushort.MinValue;
            if (f > ushort.MaxValue) return ushort.MaxValue;
            else return Convert.ToUInt16(f);
        }
        public static short ConvertingToShort(float f)
        {
            if (f < short.MinValue) return short.MinValue;
            if (f > short.MaxValue) return short.MaxValue;
            else return Convert.ToInt16(f);
        }
        public static byte ConvertingToByte(float f) 
        {
            if (f < byte.MinValue) return byte.MinValue;
            if (f > byte.MaxValue) return byte.MaxValue;
            else return Convert.ToByte(f);
        }

        public static sbyte ConvertingToSbyte(float f)
        {
            if (f < sbyte.MinValue) return sbyte.MinValue;
            if (f > sbyte.MaxValue) return sbyte.MaxValue;
            else return Convert.ToSByte(f);
        }
    }

    public static class Rnd
    {
        private static Random r = new Random();
        public static int R(int max)
        {
            return r.Next(max);
        }
        public static int R(int min, int max)
        {
            return r.Next(min, max);
        }
        public static float R(float max)
        {
            return (float)(r.NextDouble() * max);
        }
        public static float R(float min, float max)
        {
            return (float)(r.NextDouble() * (max - min) + min);
        }
    }
}