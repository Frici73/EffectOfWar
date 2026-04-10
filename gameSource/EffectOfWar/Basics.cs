using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace EffectOfWar
{
    public static class AllCharacter
    {
        internal static List<Character> healers = new List<Character> () { new Joker() };
        internal static List<Character> rangers = new List<Character>() { new Lightning(), new Breaker(), new Reaper() };
        internal static List<Character> warriors = new List<Character>() { new Barrier(), new Guardian(), new Bulldozer(), new Fulmare(), new ArthurKing(), new Trash(), new Afterglow(), new Cooldown(), new Frame(), new GodOfDeath(), new Smoke(), new Fortune_teller() };
        internal static List<Boss> bosses = new List<Boss>() { new Chaos(), new Fate(), new Werewolf(), new Goblins(), new Vampire(), new Moon(), new Solmir(), new Tarantula() };
    }
    enum HType
    {
        ranger, warrior, support
    }
    enum ShiftMode
    {
        circle, line
    }
    enum Skill
    {
        first, second, third, talent
    }
    enum Subclass
    {
        Automated, Burst, Medic, Buffer, Debuffer, Shielder, Tank, ShapeShifter, Charger, Sustain, Avanger, Resist, Unknown, Stealer, TankKiller
    }

    internal abstract class Character
    {
        // leírások
        internal string Name = "";
        internal string S1T = "";
        internal string S2T = "";
        internal string TalentT = "";
        internal string SpecialT = "";
        internal HType type;
        internal Subclass[] subclass;
        internal string img = "";

        // általános adatok
        internal Processing link;
        internal byte teamID = 0;
        internal byte Slot = 0;  // 0, 1, 2, 3
        internal float DMGDealt = 0;
        internal float DMGTaken = 0;
        internal float regeneration = 0.01f;
        internal List<short> LostedHitpointsInRounds = new List<short>();
        internal short LastLostedHP = 0;

        // HP stats
        internal ushort[] shield = new ushort[] { 0, 0 }; // from previous round, from current round
        internal short[] MaxHitpoints = new short[] { 0, 0 }; // real, init
        internal short[] Hitpoints = new short[] { 0, 0 }; // real, init

        // magic
        internal byte[] MagicalAttack = new byte[] { 0, 0 }; // real, init
        internal byte[] MagicalDefense = new byte[] { 0, 0 }; // real, init
        internal float[] ManaSensitivity = new float[] { 0, 0 }; // real, init
        internal float[] MagicalKnowledge = new float[] { 0, 0 }; // real, init

        // physic
        internal byte[] PhysicalAttack = new byte[] { 0, 0 }; // real, init
        internal byte[] PhysicalDefense = new byte[] { 0, 0 }; // real, init
        internal float[] Punctual = new float[] { 0, 0 }; // real, init
        internal float[] Immun = new float[] { 0, 0 }; // real, init

        // immunity
        internal float[] BuffImmunity = new float[] { 0, 0 }; // real, init
        internal float[] DebuffImmunity = new float[] { 0, 0 }; // real, init
        internal float[] DoTImmunity = new float[] { 0, 0 }; // real, init
        internal float[] HoTImmunity = new float[] { 0, 0 }; // real, init

        // kiegészítő adatok
        internal Shift shift;
        internal Talent talent;
        internal Charge charge;

        internal Counter counter;
        internal Reflect reflect;
        internal Marker[] OwnMarker;

        // effects
        internal List<EffectGroup> effects = new List<EffectGroup>();
        internal List<OverTime> DoTs = new List<OverTime>();
        internal List<OverTime> HoTs = new List<OverTime>();
        internal List<Marker> Markers = new List<Marker>();

        // Harci statisztika
        internal ushort TotalBuffing = 0;
        internal ushort TotalDebuffing = 0;

        internal ushort TotalShielding = 0;
        internal ushort TotalHealing = 0;

        internal ushort TotalDamageDealt = 0;
        internal ushort TotalDamageTaken = 0;

        internal ushort TotalRegeneration = 0;
        internal byte KillCount = 0;

        // functions
        internal virtual void TeamChange(Team team, Processing process)
        {
            if (team == Team.first) teamID = 1;
            else teamID = 2;
            link = process;
        }
        internal virtual void init(short hp, byte pa, byte pd, byte ma, byte md, float sens, float know, float immune, float punct)
        {
            MaxHitpoints[0] = hp;
            MaxHitpoints[1] = hp;
            Hitpoints[0] = hp;
            Hitpoints[1] = hp;
            PhysicalAttack[0] = pa;
            PhysicalAttack[1] = pa;
            PhysicalDefense[0] = pd;
            PhysicalDefense[1] = pd;
            MagicalAttack[0] = ma;
            MagicalAttack[1] = ma;
            MagicalDefense[0] = md;
            MagicalDefense[1] = md;
            ManaSensitivity[0] = sens;
            ManaSensitivity[1] = sens;
            Punctual[0] = punct;
            Punctual[1] = punct;
            Immun[0] = immune;
            Immun[1] = immune;
            MagicalKnowledge[0] = know;
            MagicalKnowledge[1] = know;
            img = Name.Replace(" ", "").ToLower() + ".png";
            counter = new Counter(this);
            reflect = new Reflect(this);
        }
        internal virtual void SpecialTechnique() { }
        public virtual Character Clone()
        {
            return (Character)this.MemberwiseClone();
        }

        // skilluse
        internal virtual void BeforeSkillUse(Skill used) { }
        internal virtual void AfterSkillUse(Skill used) { }

        internal virtual void BeforeTeammateUseSkill(Skill used, Character user) { }
        internal virtual void AfterTeammateUseSkill(Skill used, Character user) { }
        internal virtual void BeforeEnemyUseSkill(Skill used, Character user) { }
        internal virtual void AfterEnemyUseSkill(Skill used, Character user) { }
        internal virtual void SkillOne() { }
        internal virtual void SkillTwo() { }
        internal virtual void UseSkill(Skill used)
        {
            if (effects.Any(e => e.Have(Effect.sleep))) return;
            switch (used)
            {
                case Skill.first: this.SkillOne(); break;
                case Skill.second: this.SkillTwo(); break;
                case Skill.talent: this.talent.Activate(); break;
                default: break;
            }
        }

        // talent
        internal virtual void Talent() { }

        // charge
        internal virtual bool OnChargeLoaded() { return false; }

        // shift
        internal virtual void OnShifting() { }

        // before dmgtaken
        internal virtual void BeforeTeammateGetDMG(Character attacker, Character teammate, DMG dmg) { }
        internal virtual void BeforeEnemyGetDMG(Character attacker, Character enemy, DMG dmg) { }
        internal virtual void BeforeSelfGetDMG(Character attacker, DMG dmg) { }

        // after dmgtaken
        internal virtual void AfterSelfGetDMG(Character attacker, DMG dmg, short taked) { }
        internal virtual void AfterTeammateGetDMG(Character attacker, Character teammate, DMG dmg, short taked) { }
        internal virtual void AfterEnemyGetDMG(Character attacker, Character enemy, DMG dmg, short taked) { }

        // dmgtaken hp, pajzs, összes
        internal virtual ushort[] Defense(Character attacker, DMG dmg)
        {
            if (effects.Any(e => e.Have(Effect.Untouchable))) return new ushort[] { 0, 0, 0 };
            BeforeSelfGetDMG(attacker, dmg);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.BeforeTeammateGetDMG(attacker, this, dmg);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.BeforeEnemyGetDMG(attacker, this, dmg);
            }

            ushort val = 0;
            if (dmg.atktype != AttackType.Reflect)
            {
                val += Convert.ToUInt16(dmg.physical * dmg.punctual - PhysicalAttack[0] * Punctual[0]);
                val += Convert.ToUInt16(dmg.magical * dmg.magicalknowledge - MagicalDefense[0] * MagicalKnowledge[0]);
                val = Convert.ToUInt16(val * (dmg.dmgD - DMGTaken));
            }
            else val = Convert.ToUInt16(dmg.physical);

            TotalDamageTaken += Convert.ToUInt16(val);
            attacker.TotalDamageDealt += Convert.ToUInt16(val);
            ushort toshield = Convert.ToUInt16(shield[0]-val);
            if (toshield < 0)
            {
                shield[0] = 0;
                shield[1] -= Convert.ToUInt16(toshield * -1);
            }
            else
            {
                toshield *= Convert.ToUInt16(-1);
                shield[0] -= Convert.ToUInt16(toshield);
            }
            val -= toshield;
            link.InsertText($"{Name} sérült {attacker.Name} miatt pajzsba {toshield.ToString()}");
            link.InsertText($"{Name} sérült {attacker.Name} miatt életbe {val.ToString()}");
            if (val > 0)
            {
                Hitpoints[0] -= Convert.ToInt16(val);
                LastLostedHP = Convert.ToInt16(val);
                LostedHitpointsInRounds[LostedHitpointsInRounds.Count-1] += Convert.ToInt16(val);
            }

            AfterSelfGetDMG(attacker, dmg, Convert.ToInt16(val));
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.AfterTeammateGetDMG(attacker, this, dmg, Convert.ToInt16(val));
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.AfterEnemyGetDMG(attacker, this, dmg, Convert.ToInt16(val));
            }

            if (dmg.atktype == AttackType.Skill)
            {
                DMG counterDMG = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Counter);
                counter.Upgrade(counterDMG);
            }
            if (dmg.atktype != AttackType.Reflect)
            {
                DMG reflectDMG = new DMG(val, 0, 0, 0, 0, AttackType.Reflect);
                reflect.Upgrade(dmg);
            }

            EffectGroup? hpdrop = effects.FirstOrDefault(e => e.Have(Effect.hpDrop));
            if (hpdrop != null)
            {
                short minhp = Convert.ToInt16(hpdrop.GetValue(Effect.hpDrop)*MaxHitpoints[0]);
                if (Hitpoints[0] < minhp) Hitpoints[0] = Convert.ToInt16(minhp);
            }

            if (Hitpoints[0] <= 0)
            {
                ProbablyDead(attacker);
            }

            return new ushort[] { val, toshield, Convert.ToUInt16(val + toshield) }; // hp, pajzs, összes
        }

        // Before get effect
        internal virtual void BeforeTeammateGetEffect(EffectsBasic effect, Character teammate) { }
        internal virtual void BeforeEnemyGetEffect(EffectsBasic effect, Character enemy) { }
        internal virtual void BeforeSelfGetEffect(EffectsBasic effect) { }

        // After get effect
        internal virtual void AfterTeammateGetEffect(EffectsBasic effect, Character teammate, bool gived) { }
        internal virtual void AfterEnemyGetEffect(EffectsBasic effect, Character enemy, bool gived) { }
        internal virtual void AfterSelfGetEffect(EffectsBasic effect, bool gived) { }

        // get effect
        public virtual bool GetEffect(EffectsBasic effect, bool granted)
        {
            if (effects.Any(e => e.Have(Effect.Untouchable))) return false;
            BeforeSelfGetEffect(effect);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.BeforeTeammateGetEffect(effect, this);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.BeforeEnemyGetEffect(effect, this);
            }

            Random r = new Random();
            bool gived = true;
            if (!granted) 
            { 
                if (effect.GetType() == typeof(OverTime) && effect.positive && r.NextDouble() >= HoTImmunity[0])
                    HoTs.Add((OverTime)effect);
                else if (effect.GetType() == typeof(OverTime) && !effect.positive && r.NextDouble() >= DoTImmunity[0])
                    DoTs.Add((OverTime)effect);
                else if (effect.GetType() == typeof(EffectGroup) && effect.positive && r.NextDouble() >= BuffImmunity[0])
                    effects.Add((EffectGroup)effect);
                else if (effect.GetType() == typeof(EffectGroup) && !effect.positive && r.NextDouble() >= DebuffImmunity[0])
                    effects.Add((EffectGroup)effect);
                else gived = false;
            }

            AfterSelfGetEffect(effect, gived);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.AfterTeammateGetEffect(effect, this, gived);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.AfterEnemyGetEffect(effect, this, gived);
            }

            if (gived) effect.giver.TotalBuffing += 1;
            return gived;
        }

        // Körök eleje/vége
        public virtual void StartOfGame()
        {

        }
        public virtual void StartOfTurn()
        {
            shield[0] = shield[1];
            shield[1] = 0;
        }
        public virtual void EndOfTurn()
        {
            if (!effects.Any(e => e.Have(Effect.Untouchable)))
            {
                Healing reg = new Healing(HealingType.reg, Convert.ToInt16(MaxHitpoints[0] * regeneration * Immun[0]), this);
                Markers.ForEach(e => e.EndOfTurn(this));

                EffectGroup? absoluteOne = effects.FirstOrDefault(e => e.Have(Effect.absoluteOne));
                if (absoluteOne != null && absoluteOne.turn == 1) Hitpoints[0] *= Convert.ToInt16(-1);
                effects.ForEach(e => e.EndOfTurn(this));

                HoTs.ForEach(e => e.EndOfTurn(this));
                DoTs.ForEach(e => e.EndOfTurn(this));
                talent.EndOfTurn();
                shift.EndOfTurn();
            }
            LostedHitpointsInRounds.Add(0);
        }

        // before Heal
        public virtual void BeforeTeammateHealed(Healing heal, Character teammate) { }
        public virtual void BeforeEnemyHealed(Healing heal, Character enemy) { }
        public virtual void BeforeSelfHealed(Healing heal) { }

        // after Heal
        public virtual void AfterSelfHealed(Healing heal) { }
        public virtual void AfterTeammateHealed(Healing heal, Character teammate) { }
        public virtual void AfterEnemyHealed(Healing heal, Character enemy) { }

        // Heal
        public virtual ushort[] Healing(Healing heal)
        {
            BeforeSelfHealed(heal);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.BeforeTeammateHealed(heal, this);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.BeforeEnemyHealed(heal, this);
            }

            ushort val = 0;
            ushort unused = 0;
            if (heal.type == HealingType.reg)
            {
                Hitpoints[0] += Convert.ToInt16(heal.physical);
                TotalRegeneration += heal.physical;
            }
            else if (heal.type == HealingType.physi) val = Convert.ToUInt16(heal.physical * Immun[0]);
            else if (heal.type == HealingType.magic) val = Convert.ToUInt16(heal.magical * ManaSensitivity[0]);
            else if (heal.type == HealingType.both) val = Convert.ToUInt16(heal.physical * Immun[0] + heal.magical * ManaSensitivity[0]);
            else if (heal.type == HealingType.none) val = Convert.ToUInt16(heal.physical);
            Hitpoints[0] += Convert.ToInt16(val);
            if (Hitpoints[0] > MaxHitpoints[0])
            {
                unused = Convert.ToUInt16(Hitpoints[0] - MaxHitpoints[0]);
                Hitpoints[0] = MaxHitpoints[0];
            }

            AfterSelfHealed(heal);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.AfterTeammateHealed(heal, this);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.AfterEnemyHealed(heal, this);
            }
            heal.healer.TotalHealing += Convert.ToUInt16(val - unused);

            return new ushort[3] { val, Convert.ToUInt16(val - unused), unused }; // összes, használt, nem használt
        }

        // before shield
        public virtual void BeforeTeammateShielded(ushort shieldValue, Character teammate) { }
        public virtual void BeforeEnemyShielded(ushort shieldValue, Character enemy) { }
        public virtual void BeforeSelfShielded(ushort shieldValue) { }

        // after shield
        public virtual void AfterSelfShielded(ushort shieldValue) { }
        public virtual void AfterTeammateShielded(ushort shieldValue, Character teammate) { }
        public virtual void AfterEnemyShielded(ushort shieldValue, Character enemy) { }

        // shield
        public virtual void Shielding(ushort shieldValue, Character giver) 
        {
            BeforeSelfShielded(shieldValue);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.BeforeTeammateShielded(shieldValue, this);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.BeforeEnemyShielded(shieldValue, this);
            }
            shield[1] += shieldValue;
            AfterSelfShielded(shieldValue);
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.AfterTeammateShielded(shieldValue, this);
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.AfterEnemyShielded(shieldValue, this);
            }
            giver.TotalShielding += shieldValue;
        }
        
        // targeting
        public virtual Character[] GetCharacters(bool teammate, sbyte count, bool ignoreTaunt=false, TargetingMode mode=TargetingMode.normal)
        {
            var team = link.Characters(teamID, teammate);
            if (count > team.Count || count == -1) count = Convert.ToSByte(team.Count); 
            List<Character> characters;
            if (!ignoreTaunt)
            {
                List<Character> taunted = new List<Character>();
                List<Character> nontaunted = new List<Character>();
                foreach (Character enemy in team)
                {
                    if (enemy.effects.Any(e => e.Have(Effect.taunt))) taunted.Add(enemy);
                    else nontaunted.Add(enemy);
                }
                if (taunted.Count > 0)
                {
                    for (int i = 1; i < taunted.Count; i++)
                    {
                        taunted[i] = taunted[0];
                    }
                    characters = taunted;
                }
                characters = nontaunted;
            }
            else characters = team;
            if (mode == TargetingMode.lowestHp) characters.Sort((a, b) => a.Hitpoints[0].CompareTo(b.Hitpoints[0]));
            else if (mode == TargetingMode.highestHp) { characters.Sort((a, b) => a.Hitpoints[0].CompareTo(b.Hitpoints[0])); characters.Reverse(); }
            else if (mode == TargetingMode.lowestHpPercent) characters.Sort((a, b) => ((float)a.Hitpoints[0] / a.MaxHitpoints[0]).CompareTo((float)b.Hitpoints[0] / b.MaxHitpoints[0]));
            else if (mode == TargetingMode.highestHpPercent) { characters.Sort((a, b) => ((float)a.Hitpoints[0] / a.MaxHitpoints[0]).CompareTo((float)b.Hitpoints[0] / b.MaxHitpoints[0])); characters.Reverse(); }

            return characters.ToArray();
        }

        // death
        internal void ProbablyDead(Character killer)
        {
            if (!effects.Any(e => e.Have(Effect.absoluteOne)))
            { 
                EffectGroup? hpdrop = effects.FirstOrDefault(e => e.Have(Effect.reincarnation));
                if (hpdrop != null)
                {
                    short hp = Convert.ToInt16(hpdrop.GetValue(Effect.reincarnation) * MaxHitpoints[0]);
                    Hitpoints[0] = hp;
                }

                if (Hitpoints[0] <= 0)
                {
                    link.InsertText($"{Name} meghalt {killer.Name} által");
                    if (killer.teamID == teamID) OnTeammateDeath(this, killer);
                    else OnEnemyDeath(this, killer);
                    OnSelfDeath(killer);
                }
                else
                {
                    Markers.ForEach(e => e.Reset(this));
                    effects.ForEach(e => e.Reset(this));
                    HoTs.ForEach(e => e.Reset(this));
                    DoTs.ForEach(e => e.Reset(this));
                    talent.Reset();
                    shift.Reset();
                }
            }
        }
        internal virtual void OnEnemyDeath(Character enemy, Character killer) { }
        internal virtual void OnTeammateDeath(Character teammate, Character killer) { }
        internal virtual void OnSelfDeath(Character killer) { }
    }
    internal abstract class Boss : Character
    {
        internal string S3T = "";
        internal string ChanceSystem = "";
        internal byte LeftSkill = 2;

        internal virtual void SkillThree()
        {

        }
        internal override void UseSkill(Skill used)
        {
            LeftSkill -= 1;
            switch (used)
            {
                case Skill.first: this.SkillOne(); break;
                case Skill.second: this.SkillTwo(); break;
                case Skill.third: this.SkillThree(); break;
                case Skill.talent: this.talent.Activate(); LeftSkill++;  break;
                default: break;
            }
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            while (LeftSkill > 0)
            {
                UseSkill(RNDSKill());
            }
        }

        internal virtual Skill RNDSKill()
        {
            Random r = new Random();
            if (talent != null && talent.TalentStack[0] > 0 && r.Next(2) == 1) return Skill.talent;
            return Select(0.33f, 0.33f);
        }

        internal virtual Skill Select(float s1, float s2)
        {
            Random r = new Random();
            float chance = (float)r.NextDouble();
            if (chance < s1) return Skill.first;
            else if (chance < s2) return Skill.second;
            else return Skill.third;
        }
    }
}