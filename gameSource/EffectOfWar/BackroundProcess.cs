using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EffectOfWar
{
    public enum Team
    {
        first, second
    }
    public enum GameMode
    {
        BossBattle, PvP
    }
    enum Direction
    {
        up, down
    }
    public class Processing
    {
        public List<Character> team1 { get; private set; }
        public List<Character> team2 { get; private set; }
        public byte DeadCharacters;
        public byte LiveCharacters => (byte)(team1.Count+team2.Count);
        internal GameMode gamemode;
        public TextBox tb;
        public Processing(TextBox tb)
        {
            team1 = new List<Character>();
            team2 = new List<Character>();
            DeadCharacters = 0;
            gamemode = GameMode.PvP;
            this.tb = tb;
        }
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
                else if (((team1.Count < 3 && gamemode == GameMode.PvP) || (team1.Count < 4 && gamemode == GameMode.BossBattle)) && !team1.Any(e => e.Name == c.Name) && CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) != HType.boss)
                {
                    Character clone = c.Clone();
                    team1.Add(clone);
                    clone.TeamChange(team, this);
                }
            }
            else
            {
                if (team2.Count(d => d.Name == c.Name) > 0) team2.RemoveAll(d => d.Name == c.Name);
                else if ((team2.Count < 3 && gamemode == GameMode.PvP && CharacterInfos.GetCharacterType(c.GetType().Name.ToString())!=HType.boss || (team2.Count < 1 && gamemode == GameMode.BossBattle && CharacterInfos.GetCharacterType(c.GetType().Name.ToString()) == HType.boss)) && !team2.Any(e => e.Name == c.Name))
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
        
        internal void EditSlot(Team team, Direction dir, byte index)
        {
            List<Character> list = team==Team.first ? team1 : team2;
            if (dir == Direction.up)
            {
                list[index].Slot -= 1;
                list[index - 1].Slot += 1;
            }
            else 
            {
                list[index].Slot += 1;
                list[index + 1].Slot -= 1;
            }
            list.OrderBy(chars => chars.Slot);
        }
        
        public List<Character> Characters(byte teamID, bool team) // team: true=csapattárs | false=ellenfél
        {
            if (teamID == 1 && team) return team1;
            else if (teamID == 2 && !team) return team1;
            else return team2;
        }

        public void InsertText(string text)
        {
            tb.Text += text + Environment.NewLine;
        }
    
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

            // rangers
            CharacterInfos.AddCharacter("Lightning", HType.ranger, typeof(Lightning));
            CharacterInfos.AddCharacter("Breaker", HType.ranger, typeof(Breaker));
            CharacterInfos.AddCharacter("Reaper", HType.ranger, typeof(Reaper));

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
            CharacterInfos.AddCharacter("Fortune_teller", HType.warrior, typeof(Fortune_teller));

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
            /*if (gamemode == GameMode.PvP)
            {
                float[] hPERdd = new float[3] { 1f, 0.66f, 0.33f };
                float[] dt = new float[3] { 1f, 1.33f, 1.66f };
                int healers = team1.Count(h=>h.type == HType.support)-1;
                int rangers = team1.Count(h => h.type == HType.ranger)-1;
                int warriors = team1.Count(h => h.type == HType.warrior)-1;
                foreach (Character c in team1)
                {
                    if (c.type == HType.support) c.HealDealt = hPERdd[healers];
                    else if (c.type == HType.ranger) c.DMGDealt = hPERdd[rangers];
                    else c.DMGTaken = dt[warriors];
                }

                healers = team2.Count(h => h.type == HType.support) - 1;
                rangers = team2.Count(h => h.type == HType.ranger) - 1;
                warriors = team2.Count(h => h.type == HType.warrior) - 1;
                foreach (Character c in team2)
                {
                    if (c.type == HType.support) c.HealDealt = hPERdd[healers];
                    else if (c.type == HType.ranger) c.DMGDealt = hPERdd[rangers];
                    else c.DMGTaken = dt[warriors];
                }
            }*/

            // Start of Game
            foreach (Character c in team1)
            {
                c.StartOfGame();
            }
            foreach (Character c in team2)
            {
                c.StartOfGame();
            }
        }
        internal void UseSkill(Team t, byte index, Skill s)
        {
            List<Character> list = t==Team.first ? team1 : team2;
            list[index].UseSkill(s);
        }
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
    }
}