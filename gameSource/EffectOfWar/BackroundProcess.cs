using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EffectOfWar
{
    enum Team
    {
        first, second
    }
    enum GameMode
    {
        BossBattle, PvP
    }
    enum Direction
    {
        up, down
    }
    internal class Processing
    {
        internal List<Character> team1 { get; private set; }
        internal List<Character> team2 { get; private set; }
        internal byte DeadCharacters;
        internal byte LiveCharacters => (byte)(team1.Count+team2.Count);
        internal GameMode gamemode;
        internal TextBox tb;
        internal Processing(TextBox tb)
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
                else if (((team1.Count < 3 && gamemode == GameMode.PvP) || (team1.Count < 4 && gamemode == GameMode.BossBattle)) && !team1.Any(e => e.Name == c.Name) && !AllCharacter.bosses.Contains(c))
                {
                    team1.Add(c.Clone());
                    c.TeamChange(team, this);
                }
            }
            else
            {
                if (team2.Count(d => d.Name == c.Name) > 0) team2.RemoveAll(d => d.Name == c.Name);
                else if ((team2.Count < 3 && gamemode == GameMode.PvP && !AllCharacter.bosses.Contains(c) || (team2.Count < 1 && gamemode == GameMode.BossBattle && AllCharacter.bosses.Contains(c))) && !team2.Any(e => e.Name == c.Name))
                {
                    team2.Add(c.Clone());
                    c.TeamChange(team, this);
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
        
        internal List<Character> Characters(byte teamID, bool team) // team: true=csapattárs | false=ellenfél
        {
            if (teamID == 1 && team) return team1;
            else if (teamID == 2 && !team) return team1;
            else return team2;
        }

        internal void InsertText(string text)
        {
            tb.Text += text + Environment.NewLine;
        }
    
        internal void Dead(Character c)
        {
            if (c.teamID == 1) Remove(c, Team.first);
            else Remove(c, Team.second);
            DeadCharacters++;
        }
    
        internal void Reset()
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
        internal bool Correct()
        {
            return (team1.Count == 3 && gamemode == GameMode.PvP || team1.Count == 4 && gamemode == GameMode.BossBattle) && (team2.Count == 3 && gamemode == GameMode.PvP || team2.Count == 1 && gamemode == GameMode.BossBattle);
        }
    }
}
