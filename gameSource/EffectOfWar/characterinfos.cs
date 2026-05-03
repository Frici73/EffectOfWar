using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Collections.Specialized;

namespace EffectOfWar
{
    /*public static class AllCharacter
    {
        internal static List<Character> supports = new List<Character>() { new Joker() };
        internal static List<Character> rangers = new List<Character>() { new Lightning(), new Breaker(), new Reaper() };
        internal static List<Character> warriors = new List<Character>() { new Barrier(), new Guardian(), new Bulldozer(), new Fulmare(), new ArthurKing(), new Trash(), new Afterglow(), new Cooldown(), new Frame(), new GodOfDeath(), new Smoke(), new Fortune_teller() };
        internal static List<Boss> bosses = new List<Boss>() { new Chaos(), new Fate(), new Werewolf(), new Goblins(), new Vampire(), new Moon(), new Solmir(), new Tarantula() };
    }*/
    public enum HType
    {
        ranger, warrior, support, boss
    }
    public enum ShiftMode
    {
        circle, line
    }
    public enum Skill
    {
        first, second, third, talent
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
    
        public static string img(string name, HType? htype=null)
        {
            string keyS = "";
            if (htype.HasValue && Names.ContainsKey(htype.Value) && Names[htype.Value].Contains(name)) keyS = htype.Value.ToString();
            foreach (var key in Names.Keys) if (Names[key].Contains(name)) keyS = key.ToString();
            return System.IO.Path.Combine(exeFolder, "Resources", keyS, name + ".png");
        }

        internal static Character GetCharacter(string name) => (Character)Activator.CreateInstance(Types.First(x => x.Name == name));

        internal static HType GetCharacterType(string name) => Names.First(x => x.Value.Contains(name)).Key;

        internal static string[] GetTypeList(HType htype) => Types.Where(x => Names[htype].Contains(x.Name.ToString())).Select(x => x.Name.ToString()).ToArray();
    }
}
