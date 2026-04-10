using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace EffectOfWar
{

    enum TargetingMode
    {
        normal, lowestHp, highestHp, lowestHpPercent, highestHpPercent
    }
    internal abstract class EffectsBasic
    {
        public string name { get; private set; }
        public sbyte turn { get; private set; }
        public bool cancelable { get; private set; }
        public bool positive { get; private set; }
        public Character giver { get; private set; }
        public abstract bool Give(Character c, bool granted=false);
        public abstract void EndOfTurn(Character c);

    }
    enum Effect
    {
        allstat, maxhp, matk, patk, mdef, pdef, simmun, punctual, manasens, mknow, reg, dmgD, dmgT, debuffImmun, buffImmun, HoTImmun, DoTImmun, taunt, hpDrop, reincarnation, absoluteOne, sleep, Untouchable
    }
    internal class EffectGroup : EffectsBasic
    {
        public string name { get; private set; }
        private Effect[] effects;
        private float[] values;
        public sbyte turn { get; private set; }
        public bool positive { get; private set; }
        public bool cancelable { get; private set; }
        public Character giver { get; private set; }
        public EffectGroup(string name, Effect[] e, float[] val, sbyte turn, bool buff, bool cancel, Character giver) 
        {
            this.name = name;
            effects = e;
            values = val;
            this.turn = turn;
            positive = buff;
            cancelable = cancel;
            this.giver = giver;
        }
        public EffectGroup(string name, Effect e, float val, sbyte turn, bool buff, bool cancel, Character giver)
        {
            this.name = name;
            effects = new Effect[] { e };
            values = new float[] { val };
            this.turn = turn;
            positive = buff;
            cancelable = cancel;
            this.giver = giver;
        }
        public override bool Give(Character c, bool granted = false)
        {
            EffectGroup clone = (EffectGroup)this.MemberwiseClone();
            bool gived = c.GetEffect(clone, granted);
            if (gived)
            {
                clone.ACalc(c);
                return true;
            }
            return false;
        }
        public override void EndOfTurn(Character c)
        {
            if (turn > 0) turn -= 1;
            if (turn == 0)
            {
                RCalc(c);
            }
        }
        private void ACalc(Character c) // rá rakáskor
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] == Effect.maxhp)
                {
                    if (!positive) values[i] *= -1;
                    c.MaxHitpoints[0] += Convert.ToInt16(c.MaxHitpoints[1] * values[i]);
                }
                else if (effects[i] == Effect.matk)
                {
                    if (!positive) values[i] *= -1;
                    c.MagicalAttack[0] += Convert.ToByte(c.MagicalAttack[1] * values[i]);
                }
                else if (effects[i] == Effect.mdef)
                {
                    if (!positive) values[i] *= -1;
                    c.MagicalDefense[0] += Convert.ToByte(c.MagicalDefense[1] * values[i]);
                }
                else if (effects[i] == Effect.patk)
                {
                    if (!positive) values[i] *= -1;
                    c.PhysicalAttack[0] += Convert.ToByte(c.PhysicalAttack[1] * values[i]);
                }
                else if (effects[i] == Effect.pdef)
                {
                    if (!positive) values[i] *= -1;
                    c.PhysicalDefense[0] += Convert.ToByte(c.PhysicalDefense[1] * values[i]);
                }
                else if (effects[i] == Effect.simmun)
                {
                    if (!positive) values[i] *= -1;
                    c.Immun[0] += values[i];
                }
                else if (effects[i] == Effect.punctual)
                {
                    if (!positive) values[i] *= -1;
                    c.Punctual[0] += values[i];
                }
                else if (effects[i] == Effect.mknow)
                {
                    if (!positive) values[i] *= -1;
                    c.MagicalKnowledge[0] += values[i];
                }
                else if (effects[i] == Effect.manasens)
                {
                    if (!positive) values[i] *= -1;
                    c.ManaSensitivity[0] += values[i];
                }
                else if (effects[i] == Effect.reg) 
                {
                    if (!positive) values[i] *= -1;
                    c.regeneration += values[i];
                }
                else if (effects[i] == Effect.DoTImmun) 
                {
                    if (!positive) values[i] *= -1;
                    c.DoTImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.HoTImmun) 
                {
                    if (!positive) values[i] *= -1;
                    c.HoTImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.debuffImmun) 
                {
                    if (!positive) values[i] *= -1;
                    c.DebuffImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.buffImmun) 
                {
                    if (!positive) values[i] *= -1;
                    c.BuffImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.dmgD) 
                {
                    if (!positive) values[i] *= -1;
                    c.DMGDealt += values[i];
                }
                else if (effects[i] == Effect.dmgT) 
                {
                    if (!positive) values[i] *= -1;
                    c.DMGTaken += values[i];
                }
                else if (effects[i] == Effect.allstat)
                {
                    if (!positive) values[i] *= -1;
                    c.MaxHitpoints[0] += Convert.ToInt16(c.MaxHitpoints[1] * values[i]);
                    c.MagicalAttack[0] += Convert.ToByte(c.MagicalAttack[1] * values[i]);
                    c.MagicalDefense[0] += Convert.ToByte(c.MagicalDefense[1] * values[i]);
                    c.PhysicalAttack[0] += Convert.ToByte(c.PhysicalAttack[1] * values[i]);
                    c.PhysicalDefense[0] += Convert.ToByte(c.PhysicalDefense[1] * values[i]);
                    c.Immun[0] += values[i];
                    c.Punctual[0] += values[i];
                    c.MagicalKnowledge[0] += values[i];
                    c.ManaSensitivity[0] += values[i];
                    c.regeneration += values[i];
                    c.DoTImmunity[0] += values[i];
                    c.HoTImmunity[0] += values[i];
                    c.DebuffImmunity[0] += values[i];
                    c.BuffImmunity[0] += values[i];
                    c.DMGDealt += values[i];
                    c.DMGTaken += values[i];
                }
            }
        }

        private void RCalc(Character c) // levételkor
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] == Effect.maxhp)
                {
                    if (positive) values[i] *= -1;
                    c.MaxHitpoints[0] += Convert.ToInt16(c.MaxHitpoints[1] * values[i]);
                }
                else if (effects[i] == Effect.matk)
                {
                    if (positive) values[i] *= -1;
                    c.MagicalAttack[0] += Convert.ToByte(c.MagicalAttack[1] * values[i]);
                }
                else if (effects[i] == Effect.mdef)
                {
                    if (positive) values[i] *= -1;
                    c.MagicalDefense[0] += Convert.ToByte(c.MagicalDefense[1] * values[i]);
                }
                else if (effects[i] == Effect.patk)
                {
                    if (positive) values[i] *= -1;
                    c.PhysicalAttack[0] += Convert.ToByte(c.PhysicalAttack[1] * values[i]);
                }
                else if (effects[i] == Effect.pdef)
                {
                    if (positive) values[i] *= -1;
                    c.PhysicalDefense[0] += Convert.ToByte(c.PhysicalDefense[1] * values[i]);
                }
                else if (effects[i] == Effect.simmun)
                {
                    if (positive) values[i] *= -1;
                    c.Immun[0] += values[i];
                }
                else if (effects[i] == Effect.punctual)
                {
                    if (positive) values[i] *= -1;
                    c.Punctual[0] += values[i];
                }
                else if (effects[i] == Effect.mknow)
                {
                    if (positive) values[i] *= -1;
                    c.MagicalKnowledge[0] += values[i];
                }
                else if (effects[i] == Effect.manasens)
                {
                    if (positive) values[i] *= -1;
                    c.ManaSensitivity[0] += values[i];
                }
                else if (effects[i] == Effect.reg)
                {
                    if (positive) values[i] *= -1;
                    c.regeneration += values[i];
                }
                else if (effects[i] == Effect.DoTImmun)
                {
                    if (positive) values[i] *= -1;
                    c.DoTImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.HoTImmun)
                {
                    if (positive) values[i] *= -1;
                    c.HoTImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.debuffImmun)
                {
                    if (positive) values[i] *= -1;
                    c.DebuffImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.buffImmun)
                {
                    if (positive) values[i] *= -1;
                    c.BuffImmunity[0] += values[i];
                }
                else if (effects[i] == Effect.dmgD)
                {
                    if (positive) values[i] *= -1;
                    c.DMGDealt += values[i];
                }
                else if (effects[i] == Effect.dmgT)
                {
                    if (positive) values[i] *= -1;
                    c.DMGTaken += values[i];
                }
                else if (effects[i] == Effect.allstat)
                {
                    if (positive) values[i] *= -1;
                    c.MaxHitpoints[0] += Convert.ToInt16(c.MaxHitpoints[1] * values[i]);
                    c.MagicalAttack[0] += Convert.ToByte(c.MagicalAttack[1] * values[i]);
                    c.MagicalDefense[0] += Convert.ToByte(c.MagicalDefense[1] * values[i]);
                    c.PhysicalAttack[0] += Convert.ToByte(c.PhysicalAttack[1] * values[i]);
                    c.PhysicalDefense[0] += Convert.ToByte(c.PhysicalDefense[1] * values[i]);
                    c.Immun[0] += values[i];
                    c.Punctual[0] += values[i];
                    c.MagicalKnowledge[0] += values[i];
                    c.ManaSensitivity[0] += values[i];
                    c.regeneration += values[i];
                    c.DoTImmunity[0] += values[i];
                    c.HoTImmunity[0] += values[i];
                    c.DebuffImmunity[0] += values[i];
                    c.BuffImmunity[0] += values[i];
                    c.DMGDealt += values[i];
                    c.DMGTaken += values[i];
                }
            }
            if (positive)
            {
                c.effects.Remove(this);
            }
            else
            {
                c.effects.Remove(this);
            }
        }
    
        public bool Have(Effect e) => effects.Contains(e); // Amikor egy (de)buff típust (pl Taunt, Reincarnation) keresek akkor a karakter listján Any(e=>e.Have(keresett)==true)
    
        public bool Remove(Character c)
        {
            if (cancelable)
            {
                RCalc(c);
                return true;
            }
            return false;
        }
        public void Reset(Character c) => RCalc(c);
        public float GetValue(Effect e)
        {
             return values[Array.IndexOf(effects, e)];
        }
    }

    enum OverTimeType
    {
        Bleeding, Explosion, Fall, Poison, Lifesteal, ManaCharge, OverRegenerate, Recover
    }
    internal class OverTime : EffectsBasic
    {
        public Character giver { get; private set; }
        public string name { get; private set; }
        public float val { get; private set; }
        private sbyte turn;
        private sbyte startingLifetime;
        public bool cancelable { get; private set; }
        public OverTimeType type { get; private set; }
        private bool positive; // t:HoT, f:DoT

        public OverTime(Character giver, string name, float val, sbyte lifetime, bool cancelable, OverTimeType type, bool p)
        {
            this.giver = giver;
            this.name = name;
            this.val = val;
            turn = lifetime;
            startingLifetime = lifetime;
            this.cancelable = cancelable;
            this.type = type;
            positive = p;
        }

        public override bool Give(Character c, bool granted = false)
        {
            return c.GetEffect((OverTime)this.MemberwiseClone(), granted);
        }
        public override void EndOfTurn(Character c)
        {
            ushort val = 0;
            if (positive)
            {
                if (type == OverTimeType.ManaCharge) val = Convert.ToUInt16(this.val * c.ManaSensitivity[0]);
                else if (type == OverTimeType.OverRegenerate) val = Convert.ToUInt16(this.val * c.Immun[0]);
                else if (type == OverTimeType.Recover) val = Convert.ToUInt16(this.val);

                giver.TotalHealing += val;
                Healing healing = new Healing(HealingType.none, val, giver);
                c.Healing(healing);
            }
            else
            {
                if (type == OverTimeType.Bleeding) val = Convert.ToUInt16(this.val);
                else if (type == OverTimeType.Explosion) val = Convert.ToUInt16(this.val * (c.MagicalDefense[0] + c.PhysicalDefense[0]));
                else if (type == OverTimeType.Fall)
                {
                    float multiple = 1.1f;
                    for (int i = 0; i < startingLifetime - turn; i++) multiple *= 1.1f;
                    val = Convert.ToUInt16(c.MaxHitpoints[0] * this.val * multiple);
                }
                else if (type == OverTimeType.Poison) val = Convert.ToUInt16(this.val * (c.MagicalAttack[0] + c.PhysicalAttack[0]));
                else if (type == OverTimeType.Lifesteal) val = Convert.ToUInt16(this.val * c.MaxHitpoints[0]);

                giver.TotalDamageDealt += val;
                DMG dmg = new DMG(val);
                ushort[] taked = c.Defense(giver, dmg);
                if (type == OverTimeType.Lifesteal) giver.Healing(new Healing(HealingType.none, taked[0], giver));
            }
            if (turn > 0) turn--;
            if (turn == 0)
            {
                if (positive) c.HoTs.Remove(this);
                else c.DoTs.Remove(this);
            }
        }
        public void Reset(Character c)
        {
            while (turn > 0) EndOfTurn(c);
        }
    }

    internal class Counter
    {
        public Character character { get; private set; }
        public sbyte turn {  get; private set; }
        private float[] matk;
        private float[] patk;
        private float[] punctual;
        private float[] magicalknowledge;
        private float[] dmgD;
        public Counter(Character c) 
        { 
            character = c;
            turn = 0;
            matk = new float[2] { 0, 0 };
            patk = new float[2] { 0, 0 };
            punctual = new float[2] { 0, 0 };
            magicalknowledge = new float[2] { 0, 0 };
            dmgD = new float[2] { 0, 0 };
        }
        public void Edit(float matk, float patk, float punctual, float magicalknowlegde, sbyte turn, float dmgD=1)
        {
            this.patk[0] = patk;
            this.matk[0] = matk;
            this.punctual[0] = punctual;
            this.magicalknowledge[0] = magicalknowlegde;
            this.turn = turn;
            this.dmgD[0] = dmgD;
        }
        public void Increase(float matk, float patk=0, float punctual=0, float magicalknowlegde=0, float dmgD=0)
        {
            this.patk[1] += patk;
            this.matk[1] += matk;
            this.punctual[1] += punctual;
            this.magicalknowledge[1] += magicalknowlegde;
            this.dmgD[1] += dmgD;
        }
        public bool Reset()
        {
            if (turn == 0) return false;
            while (turn > 0) EndOfTurn();
            return true;
        }
        public void EndOfTurn()
        {
            if (turn > 0) turn -= 1;
            if (turn == 0)
            {
                matk[0] = 0;
                patk[0] = 0;
                punctual[0] = 0;
                magicalknowledge[0] = 0;
                dmgD[0] = 0;
                matk[1] = 0;
                patk[1] = 0;
                punctual[1] = 0;
                magicalknowledge[1] = 0;
                dmgD[1] = 0;
            }
        }
    
        public void Upgrade(DMG dmg)
        {
            if (turn == 0) return;
            dmg.atktype = AttackType.Counter;
            dmg.physical *= patk[0];
            dmg.physical += patk[1];
            dmg.magical *= matk[0];
            dmg.magical += matk[1];
            dmg.punctual *= punctual[0];
            dmg.punctual += punctual[1];
            dmg.magicalknowledge *= magicalknowledge[0];
            dmg.magicalknowledge += magicalknowledge[1];
            dmg.dmgD *= dmgD[0];
            dmg.dmgD += dmgD[1];
        }
    }

    internal class Reflect
    {
        public Character character { get; private set; }
        public sbyte turn { get; private set; }
        private float percentage;
        public Reflect(Character c)
        {
            character = c;
            turn = 0;
            percentage = 0;
        }
    
        public void EndOfTurn()
        {
            if (turn > 0) turn -= 1;
            if (turn == 0) 
            { 
                percentage = 0;
            }
        }
        public void Edit(float percent, sbyte turn)
        {
            percentage = percent;
            this.turn = turn;
        }
        public void Upgrade(DMG dmg)
        {
            dmg.atktype = AttackType.Reflect;
            dmg.physical *= percentage;
        }
        public void Reset()
        {
            while (turn > 0) EndOfTurn();
        }
    }

    internal class Marker : EffectsBasic
    {
        private ushort fixeddamage;
        private float PercentDamage;
        private Func<Character, float> statScale;
        public string name { get; private set; }
        public byte id { get; private set; }
        private EffectGroup? sideeffect;
        private OverTime? sideOverTime;
        private sbyte turn;
        public Character giver { get; private set; }
        public Marker(string name, Character giver, byte id, ushort fixdmg, float percentdmg, Func<Character, float> scale, EffectGroup effect, OverTime overTime, sbyte turn) 
        {
            this.giver = giver;
            this.id = id;
            fixeddamage = fixdmg;
            PercentDamage = percentdmg;
            statScale = scale;
            sideeffect = effect;
            sideOverTime = overTime;
            this.name = name;
            this.turn = turn;
        }
        public Marker(string name, Character giver, byte id, ushort fixdmg, EffectGroup? effect, OverTime? overTime, sbyte turn)
        {
            this.name = name;
            this.giver = giver;
            this.id = id;
            fixeddamage = fixdmg;
            sideeffect = effect;
            sideOverTime = overTime;
            this.turn = turn;
        }
        public Marker(string name, Character giver, byte id, float percentdmg, Func<Character, float> scale, EffectGroup effect, OverTime overTime, sbyte turn)
        {
            this.name = name;
            this.giver = giver;
            this.id = id;
            PercentDamage = percentdmg;
            statScale = scale;
            sideeffect = effect;
            sideOverTime = overTime;
            this.turn = turn;
        }
    
        public override void EndOfTurn(Character c)
        {
            if (turn > 0) turn -= 1;
            if (turn == 0)
            {
                ushort value = Convert.ToUInt16(statScale?.Invoke(c)*PercentDamage ?? 0);
                if (fixeddamage != null) value += fixeddamage;
                giver.TotalDamageDealt += value;
                if (sideeffect != null)
                {
                    if (sideeffect.Give(c))
                    {
                        if (giver.teamID == c.teamID) giver.TotalBuffing += 1;
                        else giver.TotalDebuffing += 1;
                    }
                }
                if (sideOverTime != null) sideOverTime.Give(c);
                c.Markers.RemoveAll(e=>ReferenceEquals(e, this));
            }
        }
        public override bool Give(Character c, bool granted=false)
        {
            c.Markers.Add((Marker)this.MemberwiseClone());
            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Marker)obj);
        }

        public bool Equals(Marker m)
        {
            return m.id == id && m.name == name;
        }
        public void Reset(Character c, bool ignoreEffects = false)
        {
            if (!ignoreEffects) while (turn > 0) EndOfTurn(c);
            else c.Markers.RemoveAll(e => ReferenceEquals(e, this));
        }
        public static byte Count(Character c, Marker m) => (byte)(c.Markers.Count(om => om.name == m.name && om.id == m.id));
    }

    enum HealingType
    {
        reg, magic, physi, none, both
    }
    internal class Healing
    {
        public HealingType type = HealingType.none;
        public ushort magical;
        public ushort physical; // fizikai VAGY reg VAGY effect érték
        public Character healer;
        public Healing(HealingType type, float value, Character healer)
        {
            this.type = type;
            if (type==HealingType.magic) magical = Convert.ToUInt16(value);
            else if (type==HealingType.physi) physical = Convert.ToUInt16(value);
            this.healer = healer;
        }
        public Healing(ushort physical, ushort magical, Character healer)
        {
            type = HealingType.both;
            this.physical = physical;
            this.magical = magical;
            this.healer = healer;
        }
    }

    enum DMGType
    {
        magical, physical, both, none
    }
    enum AttackType
    {
        Skill, Counter, Reflect
    }
    internal class DMG
    {
        public AttackType atktype = AttackType.Skill;
        public DMGType type = DMGType.both;
        public float magical;
        public float physical; // fizikai VAGY reflect VAGY effect
        public float punctual;
        public float magicalknowledge;
        public float ignoredMagicalDefense = 0f;
        public float ignoredPhysicalDefense = 0f;
        public float ignoredPunctual = 0f;
        public float ignoredMagicalKnowledge = 0f;
        public float dmgD = 1f;

        public DMG(float physical, float magical, float punctual, float magicalknowledge, float dmgD, AttackType atk)
        {
            this.physical = physical;
            this.magical = magical;
            this.punctual = punctual;
            this.magicalknowledge = magicalknowledge;
            this.atktype = atk;
            this.dmgD = dmgD;
        }
        public DMG(DMGType type, float value, float value2, float dmgD, AttackType atk)
        {
            this.dmgD = dmgD;
            atktype = atk;
            this.type = type;
            if (type == DMGType.magical)
            {
                magical = value;
                magicalknowledge = value2;
            }
            else if (type == DMGType.physical)
            {
                physical = value;
                punctual = value2;
            } 
        }
        public DMG(float value)
        {
            atktype = AttackType.Reflect;
            type = DMGType.none;
            physical = value;
        }

        public void ignored(float mdef, float pdef, float punc, float know)
        {
            ignoredMagicalDefense = mdef;
            ignoredPhysicalDefense = pdef;
            ignoredPunctual = punc;
            ignoredMagicalKnowledge = know;
        }
    }

    internal class Talent
    {
        internal Character character;
        internal sbyte[] TalentCooldown = new sbyte[] { 0, 0 }; // hátralévő, kezdő
        internal byte[] TalentStack = new byte[] { 0, 0 }; // betöltött, maximum
        public Talent(Character master)
        {
            character = master;
        }
        public Talent(Character master, sbyte cooldown, byte stack)
        {
            character = master;
            TalentCooldown[1] = cooldown;
            TalentStack[1] = stack;
        }

        public void Activate()
        {
            if (TalentStack[0] > Convert.ToByte(0))
            {
                character.link.tb.Text += $"{character.Name} Talent aktiválása:";
                TalentStack[0] -= Convert.ToByte(1);
                character.Talent();
                if (TalentCooldown[0] == 0) TalentCooldown[0] = TalentCooldown[1];
            }
        }

        public void Reset()
        {
            TalentStack[0] = TalentStack[1];
            TalentCooldown[0] = 0;
        }

        public void Charge()
        {
            TalentStack[0] = Convert.ToByte(Math.Min(TalentStack[1], TalentStack[0]+1));
            TalentCooldown[0] = 0;
        }

        public void Nullify()
        {
            TalentStack[0] = 0;
            TalentCooldown[0] = TalentCooldown[1];
        }

        public void EndOfTurn()
        {
            if (TalentStack[0] < TalentStack[1]) TalentCooldown[0]--;
            if (TalentCooldown[0] == 0)
            {
                TalentStack[0]++;
                TalentCooldown[0] = TalentCooldown[1];
            }
        }
    }

    internal class Charge
    {
        Character character;
        public ushort[] State = new ushort[] { 0, 0 }; // betöltött, maximum
        public Charge(Character master, ushort limit)
        {
            character = master;
            State[1] = limit;
        }

        public void Reset()
        {
            State[0] = 0;
        }
        public void Nullify() => Reset();

        public void Load(int val)
        {
            State[0] = (ushort)Math.Min(State[0] + Convert.ToUInt16(val), State[1]);
            if (character.OnChargeLoaded() == true) State[0] = 0;
        }
    }

    internal class Shift
    {
        public Character character;
        internal sbyte[] Mode = new sbyte[] { 1, 1 }; // aktív mód, maximum mód
        internal ShiftMode Switch;
        internal sbyte SwitchDirect = 1;
        internal byte[] Cooldown = new byte[] { 0, 1 }; // hátralévő, kezdő
        public sbyte ActiveMode => Mode[0];

        public Shift(Character master, sbyte count, ShiftMode mode, byte cooldown)
        {
            character = master;
            Switch = mode;
            Mode[1] = count;
            Cooldown[1] = cooldown;
        }
        public void Shifting()
        {
            if (Cooldown[0] == 0)
            {
                Mode[0] += SwitchDirect;
                if (Mode[0] > Mode[1])
                {
                    if (Switch == ShiftMode.line) Mode[0] = 1;
                    else
                    {
                        SwitchDirect = -1;
                        Mode[0] += SwitchDirect;
                        Mode[0] += SwitchDirect;
                    }
                }
                else if (Mode[0] < 1)
                {
                    SwitchDirect = 1;
                    Mode[0] += SwitchDirect;
                    Mode[0] += SwitchDirect;
                }
                Cooldown[0] = Cooldown[1];
                character.OnShifting();
            }
        }
        public void EndOfTurn()
        {
            if (Cooldown[0] > 0) Cooldown[0]--;
        }
        public void Reset()
        {
            Mode[0] = 1;
            Cooldown[0] = 0;
            SwitchDirect = 1;
        }
    }
}