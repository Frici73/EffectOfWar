using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace EffectOfWar
{
    public abstract class Character
    {
        // leírások
        public string Name = "";
        public string S1T = "";
        public string S2T = "";
        public string TalentT = "";
        public string SpecialT = "";

        // általános adatok
        public Processing link;
        public byte teamID = 0;
        public byte Slot = 0;  // 0, 1, 2, 3
        public float DMGDealt = 20;
        public float DMGResistance = 10;
        public float HealDealt = 1;
        public float regeneration = 0.01f;
        public List<short> LostedHitpointsInRounds = new List<short>() { 0 };
        public short LastLostedHP = 0;

        // HP stats
        public ushort[] shield = new ushort[] { 0, 0 }; // from previous round, from current round
        public short[] MaxHitpoints = new short[] { 0, 0 }; // real, init
        public short[] Hitpoints = new short[] { 0, 0 }; // real, init

        // magic
        public byte[] MagicalAttack = new byte[] { 0, 0 }; // real, init
        public byte[] MagicalDefense = new byte[] { 0, 0 }; // real, init
        public float[] ManaSensitivity = new float[] { 0, 0 }; // real, init
        public float[] MagicalKnowledge = new float[] { 0, 0 }; // real, init

        // physic
        public byte[] PhysicalAttack = new byte[] { 0, 0 }; // real, init
        public byte[] PhysicalDefense = new byte[] { 0, 0 }; // real, init
        public float[] Punctual = new float[] { 0, 0 }; // real, init
        public float[] Immun = new float[] { 0, 0 }; // real, init

        // immunity
        public float[] BuffImmunity = new float[] { 0, 0 }; // real, init
        public float[] DebuffImmunity = new float[] { 0, 0 }; // real, init
        public float[] DoTImmunity = new float[] { 0, 0 }; // real, init
        public float[] HoTImmunity = new float[] { 0, 0 }; // real, init

        // kiegészítő adatok
        public Shift shift;
        public Talent talent;
        public Charge charge;

        public Counter counter;
        public Reflect reflect;
        public Marker[] OwnMarker;

        // effects
        public List<EffectGroup> effects = new List<EffectGroup>();
        public List<OverTime> DoTs = new List<OverTime>();
        public List<OverTime> HoTs = new List<OverTime>();
        public List<Marker> Markers = new List<Marker>();

        // Harci statisztika
        public ushort TotalBuffing = 0;
        public ushort TotalDebuffing = 0;

        public ushort TotalShielding = 0;
        public ushort TotalHealing = 0;

        public ushort TotalDamageDealt = 0;
        public ushort TotalDamageTaken = 0;

        public ushort TotalRegeneration = 0;
        public byte KillCount = 0;

        // functions
        /*public override bool Equals(object? obj) => ReferenceEquals(this, obj);
        public static bool operator ==(Character a, Character b) => a.Equals(b);
        public static bool operator !=(Character a, Character b) => !a.Equals(b);*/

        public override string ToString()
        {
            string Text = $"{Name}:\nS1: {S1T}\nS2: {S2T}\nSpecial: {SpecialT}\n";
            if (TalentT != null && TalentT != "") Text += $"{TalentT}\n";
            Text += $"Max HP: {MaxHitpoints[0]}\n" +
                $"Magical Attack: {MagicalAttack[0]}\n" +
                $"Magical Defense: {MagicalDefense[0]}\n" +
                $"Physical Attack: {PhysicalAttack[0]}\n" +
                $"Physical Defense: {PhysicalDefense[0]}\n" +
                $"Magical knowledge: {MagicalKnowledge[0]:F2}\n" +
                $"Mana sensitivy: {ManaSensitivity[0]:F2}\n" +
                $"Punctual: {Punctual[0]:F2}\n" +
                $"Immunsystem: {Immun[0]:F2}\n";
            return Text;
        }
        public string State()
        {
            string Text = $"{Name}:\nHP: {Hitpoints[0]} / {MaxHitpoints[0]} - {((float)Hitpoints[0] / (float)MaxHitpoints[0] * 100):F2}%\n" +
                $"Shield: {shield[0] + shield[1]}\n" +
                $"Effects: {(effects.Count > 0 ? string.Join(", ", effects.Select(e => e.name)) : "None")}\n" +
                $"DoTs: {(DoTs.Count > 0 ? string.Join(", ", DoTs.Select(e => e.name)) : "None")}\n" +
                $"HoTs: {(HoTs.Count > 0 ? string.Join(", ", HoTs.Select(e => e.name)) : "None")}\n" +
                $"Markers: {(Markers.Count > 0 ? string.Join(", ", Markers.Select(e => e.name)) : "None")}\n" +
                $"Talent: {(talent == null ? "None" : talent.TalentStack[0].ToString() + '/' + talent.TalentStack[1].ToString() + " cooldown: " + talent.TalentCooldown[0].ToString() + '/' + talent.TalentCooldown[1].ToString())}\n" +
                $"Charge: {(charge==null?"None": charge.State[0].ToString() + '/' + charge.State[1].ToString())}";

            return Text;
        }
        public virtual void TeamChange(Team team, Processing process)
        {
            if (team == Team.first) teamID = 1;
            else teamID = 2;
            link = process;
        }
        public virtual void init(short hp, byte pa, byte pd, byte ma, byte md, float sens, float know, float immune, float punct)
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
            counter = new Counter(this);
            reflect = new Reflect(this);
        }
        public virtual void SpecialTechnique(object arg) { }
        public virtual void SpecialTechnique() { }
        public virtual Character Clone() => (Character)Activator.CreateInstance(this.GetType());
        /*{
            
            Character clone = (Character)this.MemberwiseClone();
            clone.link = default(Processing);
            // 🔥 ÚJ LISTÁK (kritikus)
            clone.effects = new List<EffectGroup>();
            clone.DoTs = new List<OverTime>();
            clone.HoTs = new List<OverTime>();
            clone.Markers = new List<Marker>();
            clone.counter = new Counter(clone);
            clone.reflect = new Reflect(clone);

            clone.LostedHitpointsInRounds = LostedHitpointsInRounds.ToList();

            // ⚠️ Tömbök (ha módosítod őket runtime)
            clone.MaxHitpoints = (short[])MaxHitpoints.Clone();
            clone.Hitpoints = (short[])Hitpoints.Clone();

            clone.MagicalAttack = (byte[])MagicalAttack.Clone();
            clone.MagicalDefense = (byte[])MagicalDefense.Clone();
            clone.PhysicalAttack = (byte[])PhysicalAttack.Clone();
            clone.PhysicalDefense = (byte[])PhysicalDefense.Clone();

            clone.ManaSensitivity = (float[])ManaSensitivity.Clone();
            clone.MagicalKnowledge = (float[])MagicalKnowledge.Clone();

            clone.Punctual = (float[])Punctual.Clone();
            clone.Immun = (float[])Immun.Clone();

            clone.BuffImmunity = (float[])BuffImmunity.Clone();
            clone.DebuffImmunity = (float[])DebuffImmunity.Clone();
            clone.DoTImmunity = (float[])DoTImmunity.Clone();
            clone.HoTImmunity = (float[])HoTImmunity.Clone();

            clone.shield = (ushort[])shield.Clone();

            // ⚠️ Ezek külön objektumok → ha van bennük state, klónozni kell
            if (charge != default(Charge)) clone.charge = new Charge(clone, charge.State[1]);
            if (TalentT != "") clone.talent = new Talent(clone, talent.TalentCooldown[1], talent.TalentStack[1]);

            return clone;
        }*/

        // skilluse
        public virtual void BeforeSkillUse(Skill used) { }
        public virtual void AfterSkillUse(Skill used) { }

        public virtual void BeforeTeammateUseSkill(Skill used, Character user) { }
        public virtual void AfterTeammateUseSkill(Skill used, Character user) { }
        public virtual void BeforeEnemyUseSkill(Skill used, Character user) { }
        public virtual void AfterEnemyUseSkill(Skill used, Character user) { }
        public virtual void SkillOne() { }
        public virtual void SkillTwo() { }
        public virtual void UseSkill(Skill used)
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
        public virtual void Talent() { }

        // charge
        public virtual bool OnChargeLoaded() { return false; }

        // shift
        public virtual void OnShifting() { }

        // before DMGtaken
        public virtual void BeforeTeammateGetDMG(Character attacker, Character teammate, DMG dmg) { }
        public virtual void BeforeEnemyGetDMG(Character attacker, Character enemy, DMG dmg) { }
        public virtual void BeforeSelfGetDMG(Character attacker, DMG dmg) { }

        // after DMGtaken
        public virtual void AfterSelfGetDMG(Character attacker, DMG dmg, short taked) { }
        public virtual void AfterTeammateGetDMG(Character attacker, Character teammate, DMG dmg, short taked) { }
        public virtual void AfterEnemyGetDMG(Character attacker, Character enemy, DMG dmg, short taked) { }

        // DMGtaken hp, pajzs, összes
        public virtual ushort[] Defense(Character attacker, DMG dmg)
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
                val += Converter.ConvertingToUshort(dmg.physical * dmg.punctual - (float)PhysicalAttack[0] * Punctual[0]);
                val += Converter.ConvertingToUshort(dmg.magical * dmg.magicalknowledge - (float)MagicalDefense[0] * MagicalKnowledge[0]);
                val = Converter.ConvertingToUshort((float)val * (dmg.dmgD - DMGResistance));
            }
            else val = Converter.ConvertingToUshort(dmg.physical);
            TotalDamageTaken += Converter.ConvertingToUshort(val);
            attacker.TotalDamageDealt += Converter.ConvertingToUshort(val);

            short toshield = 0;
            if (shield[0] >= val)
            {
                shield[0] -= val;
                toshield = Converter.ConvertingToShort(val);
                val = 0;
            }
            else if (shield[0] + shield[1] >= val)
            {
                toshield = Converter.ConvertingToShort(shield[0]);
                val -= shield[0];
                shield[0] = 0;
                shield[1] -= Converter.ConvertingToUshort(val);
                toshield += Converter.ConvertingToShort(val);
                val = 0;
            }
            else
            {
                toshield = Converter.ConvertingToShort(shield[0] + shield[1]);
                val -= Converter.ConvertingToUshort(shield[0] + shield[1]);
                shield[0] = 0;
                shield[1] = 0;
            }

            link.InsertText($"{Name} sérült {attacker.Name} miatt pajzsba {Converter.ConvertingToUshort(toshield).ToString()}");
            link.InsertText($"{Name} sérült {attacker.Name} miatt életbe {Converter.ConvertingToUshort(val).ToString()}");
            if (val > 0)
            {
                Hitpoints[0] -= Converter.ConvertingToShort(val);
                LastLostedHP = Converter.ConvertingToShort(val);
                LostedHitpointsInRounds[LostedHitpointsInRounds.Count-1] += Converter.ConvertingToShort(val);
            }

            AfterSelfGetDMG(attacker, dmg, Converter.ConvertingToShort(val));
            foreach (Character teammate in link.Characters(teamID, true))
            {
                if (teammate != this) teammate.AfterTeammateGetDMG(attacker, this, dmg, Converter.ConvertingToShort(val));
            }
            foreach (Character enemy in link.Characters(teamID, false))
            {
                enemy.AfterEnemyGetDMG(attacker, this, dmg, Converter.ConvertingToShort(val));
            }

            if (dmg.atktype == AttackType.Skill)
            {
                DMG counterDMG = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Counter);
                counter.Upgrade(counterDMG);
            }
            if (dmg.atktype != AttackType.Reflect)
            {
                DMG reflectDMG = new DMG(val, 0, 0, 0, 0, AttackType.Reflect);
                reflect.Upgrade(reflectDMG);
            }

            EffectGroup? hpdrop = effects.FirstOrDefault(e => e.Have(Effect.hpDrop));
            if (hpdrop != null)
            {
                short minhp = Converter.ConvertingToShort(hpdrop.GetValue(Effect.hpDrop)*MaxHitpoints[0]);
                if (Hitpoints[0] < minhp) Hitpoints[0] = Converter.ConvertingToShort(minhp);
            }

            if (Hitpoints[0] <= 0)
            {
                ProbablyDead(attacker);
            }

            return new ushort[] { val, Converter.ConvertingToUshort(toshield), Converter.ConvertingToUshort(val + toshield) }; // hp, pajzs, összes
        }

        // Before get effect
        public virtual void BeforeTeammateGetEffect(EffectsBasic effect, Character teammate) { }
        public virtual void BeforeEnemyGetEffect(EffectsBasic effect, Character enemy) { }
        public virtual void BeforeSelfGetEffect(EffectsBasic effect) { }

        // After get effect
        public virtual void AfterTeammateGetEffect(EffectsBasic effect, Character teammate, bool gived) { }
        public virtual void AfterEnemyGetEffect(EffectsBasic effect, Character enemy, bool gived) { }
        public virtual void AfterSelfGetEffect(EffectsBasic effect, bool gived) { }

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

            bool gived = true;
            if (effect.GetType() == typeof(OverTime) && effect.positive && (granted || Rnd.R(1f) >= HoTImmunity[0])) HoTs.Add((OverTime)effect);
            else if (effect.GetType() == typeof(OverTime) && !effect.positive && (granted || Rnd.R(1f) >= DoTImmunity[0])) DoTs.Add((OverTime)effect);
            else if (effect.GetType() == typeof(EffectGroup) && effect.positive && (granted || Rnd.R(1f) >= BuffImmunity[0])) effects.Add((EffectGroup)effect);
            else if (effect.GetType() == typeof(EffectGroup) && !effect.positive && (granted || Rnd.R(1f) >= DebuffImmunity[0])) effects.Add((EffectGroup)effect);

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
                Healing reg = new Healing(HealingType.reg, Converter.ConvertingToShort(MaxHitpoints[0] * regeneration * Immun[0]), this);
                for (int i = Markers.Count-1; i >= 0; i--) Markers[i].EndOfTurn(this);

                EffectGroup? absoluteOne = effects.FirstOrDefault(e => e.Have(Effect.absoluteOne));
                if (absoluteOne != null && absoluteOne.turn == 1) Hitpoints[0] *= Converter.ConvertingToShort(-1);
                for (int i = effects.Count-1; i >= 0; i--) effects[i].EndOfTurn(this);
                for (int i = HoTs.Count-1; i >= 0; i--) HoTs[i].EndOfTurn(this);
                for (int i = DoTs.Count-1; i >= 0; i--) DoTs[i].EndOfTurn(this);
                talent?.EndOfTurn();
                shift?.EndOfTurn();
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
                Hitpoints[0] += Converter.ConvertingToShort(heal.physical);
                TotalRegeneration += heal.physical;
            }
            else if (heal.type == HealingType.physi) val = Converter.ConvertingToUshort(heal.physical * Immun[0]);
            else if (heal.type == HealingType.magic) val = Converter.ConvertingToUshort(heal.magical * ManaSensitivity[0]);
            else if (heal.type == HealingType.both) val = Converter.ConvertingToUshort(heal.physical * Immun[0] + heal.magical * ManaSensitivity[0]);
            else if (heal.type == HealingType.none) val = Converter.ConvertingToUshort(heal.physical);
            Hitpoints[0] += Converter.ConvertingToShort(val);
            if (Hitpoints[0] > MaxHitpoints[0])
            {
                unused = Converter.ConvertingToUshort(Hitpoints[0] - MaxHitpoints[0]);
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
            heal.healer.TotalHealing += Converter.ConvertingToUshort(val - unused);

            return new ushort[3] { val, Converter.ConvertingToUshort(val - unused), unused }; // összes, használt, nem használt
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
                else characters = nontaunted;
            }
            else characters = team;
            if (mode == TargetingMode.lowestHp) characters.Sort((a, b) => a.Hitpoints[0].CompareTo(b.Hitpoints[0]));
            else if (mode == TargetingMode.highestHp) { characters.Sort((a, b) => a.Hitpoints[0].CompareTo(b.Hitpoints[0])); characters.Reverse(); }
            else if (mode == TargetingMode.lowestHpPercent) characters.Sort((a, b) => ((float)a.Hitpoints[0] / a.MaxHitpoints[0]).CompareTo((float)b.Hitpoints[0] / b.MaxHitpoints[0]));
            else if (mode == TargetingMode.highestHpPercent) { characters.Sort((a, b) => ((float)a.Hitpoints[0] / a.MaxHitpoints[0]).CompareTo((float)b.Hitpoints[0] / b.MaxHitpoints[0])); characters.Reverse(); }
            else if (mode == TargetingMode.random) { characters = characters.Shuffle().ToList(); }
            return characters.Take(count).ToArray();
        }

        // death
        public void ProbablyDead(Character killer)
        {
            if (!effects.Any(e => e.Have(Effect.absoluteOne)))
            { 
                EffectGroup? hpdrop = effects.FirstOrDefault(e => e.Have(Effect.reincarnation));
                if (hpdrop != null)
                {
                    short hp = Converter.ConvertingToShort(hpdrop.GetValue(Effect.reincarnation) * MaxHitpoints[0]);
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
        public virtual void OnEnemyDeath(Character enemy, Character killer) { }
        public virtual void OnTeammateDeath(Character teammate, Character killer) { }
        public virtual void OnSelfDeath(Character killer) { }
    }
    public abstract class Boss : Character
    {
        public string S3T = "";
        public string ChanceSystem = "";
        public byte LeftSkill = 2;
        public override string ToString()
        {
            string Text = $"{Name}:\nS1: {S1T}\nS2: {S2T}\nS3: {S3T}\nSpecial: {SpecialT}\n";
            if (TalentT != null && TalentT != "") Text += $"{TalentT}\n";
            Text += $"HP: {Hitpoints[0]} / {MaxHitpoints[0]}\n" +
                $"Magical Attack: {MagicalAttack[0]}\n" +
                $"Magical Defense: {MagicalDefense[0]}\n" +
                $"Physical Attack: {PhysicalAttack[0]}\n" +
                $"Physical Defense: {PhysicalDefense[0]}\n" +
                $"Magical knowledge: {MagicalKnowledge[0]:F2}\n" +
                $"Mana sensitivity: {ManaSensitivity[0]:F2}\n" +
                $"Punctual: {Punctual[0]:F2}\n" +
                $"Immunsystem: {Immun[0]:F2}\n";
            return Text;
        }
        public virtual void SkillThree()
        {

        }
        public override void UseSkill(Skill used)
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

        public virtual Skill RNDSKill()
        {
            if (talent != null && talent.TalentStack[0] > 0 && Rnd.R(2) == 1) return Skill.talent;
            return Select(0.33f, 0.33f);
        }

        public virtual Skill Select(float s1, float s2)
        {
            float chance = Rnd.R(1f);
            if (chance < s1) return Skill.first;
            else if (chance < s2) return Skill.second;
            else return Skill.third;
        }
    }
}