using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media.Animation;
using System.Xml.Linq;

namespace EffectOfWar
{
    public abstract class EffectsBasic
    {
        public string name { get; protected set; }
        public sbyte turn { get; protected set; }
        public bool cancelable { get; protected set; }
        public bool positive { get; protected set; }
        public Character giver { get; protected set; }
        public abstract bool Give(Character c, bool granted=false);
        public abstract void EndOfTurn(Character c);

    }
    
    public class EffectGroup : EffectsBasic
    {
        private Effect[] effects;
        private float[] values;
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

        public EffectGroup Clone()
        {
            return new EffectGroup(name, effects, values, turn, positive, cancelable, giver);
        }
        public override bool Give(Character c, bool granted = false)
        {
            EffectGroup clone = Clone();
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
        private void ApplyCalc(Character c, bool apply)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                float value = values[i];

                // negatív effect kezelése
                if (!positive)
                    value *= -1;

                // levételkor invertáljuk
                if (!apply)
                    value *= -1;

                if (effects[i] == Effect.maxhp)
                {
                    c.MaxHitpoints[0] +=
                        Converter.ConvertingToShort(c.MaxHitpoints[1] * value);
                }
                else if (effects[i] == Effect.matk)
                {
                    c.MagicalAttack[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.MagicalAttack[0]) +
                            Converter.ConvertingToSbyte(c.MagicalAttack[1] * value));
                }
                else if (effects[i] == Effect.mdef)
                {
                    c.MagicalDefense[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.MagicalDefense[0]) +
                            Converter.ConvertingToSbyte(c.MagicalDefense[1] * value));
                }
                else if (effects[i] == Effect.patk)
                {
                    c.PhysicalAttack[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.PhysicalAttack[0]) +
                            Converter.ConvertingToSbyte(c.PhysicalAttack[1] * value));
                }
                else if (effects[i] == Effect.pdef)
                {
                    c.PhysicalDefense[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.PhysicalDefense[0]) +
                            Converter.ConvertingToSbyte(c.PhysicalDefense[1] * value));
                }
                else if (effects[i] == Effect.simmun)
                {
                    c.Immun[0] += value;
                }
                else if (effects[i] == Effect.punctual)
                {
                    c.Punctual[0] += value;
                }
                else if (effects[i] == Effect.mknow)
                {
                    c.MagicalKnowledge[0] += value;
                }
                else if (effects[i] == Effect.manasens)
                {
                    c.ManaSensitivity[0] += value;
                }
                else if (effects[i] == Effect.reg)
                {
                    c.regeneration += value;
                }
                else if (effects[i] == Effect.DoTImmun)
                {
                    c.DoTImmunity[0] += value;
                }
                else if (effects[i] == Effect.HoTImmun)
                {
                    c.HoTImmunity[0] += value;
                }
                else if (effects[i] == Effect.debuffImmun)
                {
                    c.DebuffImmunity[0] += value;
                }
                else if (effects[i] == Effect.buffImmun)
                {
                    c.BuffImmunity[0] += value;
                }
                else if (effects[i] == Effect.dmgD)
                {
                    c.DMGDealt += value;
                }
                else if (effects[i] == Effect.dmgR)
                {
                    c.DMGResistance += value;
                }
                else if (effects[i] == Effect.allstat)
                {
                    c.MaxHitpoints[0] +=
                        Converter.ConvertingToShort(c.MaxHitpoints[1] * value);

                    c.MagicalAttack[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.MagicalAttack[0]) +
                            Converter.ConvertingToSbyte(c.MagicalAttack[1] * value));

                    c.MagicalDefense[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.MagicalDefense[0]) +
                            Converter.ConvertingToSbyte(c.MagicalDefense[1] * value));

                    c.PhysicalAttack[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.PhysicalAttack[0]) +
                            Converter.ConvertingToSbyte(c.PhysicalAttack[1] * value));

                    c.PhysicalDefense[0] =
                        Converter.ConvertingToByte(
                            Converter.ConvertingToSbyte(c.PhysicalDefense[0]) +
                            Converter.ConvertingToSbyte(c.PhysicalDefense[1] * value));

                    c.Immun[0] += value;
                    c.Punctual[0] += value;
                    c.MagicalKnowledge[0] += value;
                    c.ManaSensitivity[0] += value;
                    c.regeneration += value;
                    c.DoTImmunity[0] += value;
                    c.HoTImmunity[0] += value;
                    c.DebuffImmunity[0] += value;
                    c.BuffImmunity[0] += value;
                    c.DMGDealt += value;
                    c.DMGResistance += value;
                }
            }

            if (!apply)
                c.effects.Remove(this);
        }

        private void ACalc(Character c) => ApplyCalc(c, true);

        private void RCalc(Character c) => ApplyCalc(c, false);

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
        public float GetValue(Effect e) => values[Array.IndexOf(effects, e)];
    }

    public class OverTime : EffectsBasic
    {
        public float val { get; private set; }
        private sbyte startingLifetime;
        public OverTimeType type { get; private set; }

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

        public OverTime Clone()
        {
            return new OverTime(giver, name, val, startingLifetime, cancelable, type, positive);
        }

        public override bool Give(Character c, bool granted = false)
        {
            return c.GetEffect(Clone(), granted);
        }
        public override void EndOfTurn(Character c)
        {
            ushort val = 0;
            if (positive)
            {
                if (type == OverTimeType.ManaCharge) val = Converter.ConvertingToUshort(this.val * c.ManaSensitivity[0]);
                else if (type == OverTimeType.OverRegenerate) val = Converter.ConvertingToUshort(this.val * c.Immun[0]);
                else if (type == OverTimeType.Recover) val = Converter.ConvertingToUshort(this.val);

                giver.TotalHealing += val;
                Healing healing = new Healing(HealingType.none, val, giver);
                c.Healing(healing);
            }
            else
            {
                if (type == OverTimeType.Bleeding) val = Converter.ConvertingToUshort(this.val);
                else if (type == OverTimeType.Explosion) val = Converter.ConvertingToUshort(this.val * (c.MagicalDefense[0] + c.PhysicalDefense[0]));
                else if (type == OverTimeType.Fall)
                {
                    float multiple = 1.1f;
                    for (int i = 0; i < startingLifetime - turn; i++) multiple *= 1.1f;
                    val = Converter.ConvertingToUshort(c.MaxHitpoints[0] * this.val * multiple);
                }
                else if (type == OverTimeType.Poison) val = Converter.ConvertingToUshort(this.val * (c.MagicalAttack[0] + c.PhysicalAttack[0]));
                else if (type == OverTimeType.Lifesteal) val = Converter.ConvertingToUshort(this.val * c.MaxHitpoints[0]);

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

    public class Counter
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

    public class Reflect
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
        public void Upgrade(DMG dmg) => dmg.physical *= percentage;
        public void Reset()
        {
            while (turn > 0) EndOfTurn();
        }
    }

    public class Marker : EffectsBasic
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
                ushort value = Converter.ConvertingToUshort(statScale?.Invoke(c)*PercentDamage ?? 0);
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
        public Marker Clone()
        {
            return new Marker(name, giver, id, fixeddamage, PercentDamage, statScale, sideeffect, sideOverTime, turn);
        }
        public override bool Give(Character c, bool granted=false)
        {
            c.Markers.Add(Clone());
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
        public static byte Count(Character c, Marker m) => (byte)c.Markers.Count(om => om.Equals(m));

        public byte Remove(int max, Character c, bool ignoreEffects = false)
        {
            int removable = 0;
            if (max > -1)
            removable = Math.Min(max, c.Markers.Count(m => m.Equals(this)));
            else removable = c.Markers.Count(m => m.Equals(this));
            int i = 0;
            while (i < removable)
            {
                if (c.Markers[i].Equals(this))
                {
                    c.Markers[i].Reset(c, ignoreEffects);
                    i++;
                }
            }
            return (byte)removable;
        }
    }

    public class Healing
    {
        public HealingType type = HealingType.none;
        public ushort magical;
        public ushort physical; // fizikai VAGY reg VAGY effect érték
        public Character healer;
        public Healing(HealingType type, float value, Character healer)
        {
            this.type = type;
            if (type==HealingType.magic) magical = Converter.ConvertingToUshort(value*healer.HealDealt);
            else if (type==HealingType.physi) physical = Converter.ConvertingToUshort(value*healer.HealDealt);
            this.healer = healer;
        }
        public Healing(float physical, float magical, Character healer)
        {
            type = HealingType.both;
            this.magical = Converter.ConvertingToUshort(magical * healer.HealDealt);
            this.physical = Converter.ConvertingToUshort(physical * healer.HealDealt);
            this.healer = healer;
        }
    }

    public class DMG
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

        public void EditAtkType(AttackType type)
        {
            atktype = type;
        }
    }

    public class Talent
    {
        public Character character;
        public sbyte[] TalentCooldown = new sbyte[] { 0, 0 }; // hátralévő, kezdő
        public byte[] TalentStack = new byte[] { 0, 0 }; // betöltött, maximum
        public Talent(Character master)
        {
            character = master;
        }
        public Talent(Character master, sbyte cooldown, byte stack, byte startstack=0)
        {
            character = master;
            TalentCooldown[1] = cooldown;
            TalentStack[1] = stack;
            TalentStack[0] = startstack;
            if (startstack < stack) TalentCooldown[0] = TalentCooldown[1];
        }

        public void Activate()
        {
            if (TalentStack[0] > 0)
            {
                character.link.InsertText($"{character.Name} Talent aktiválása:");
                TalentStack[0] -= 1;
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
            TalentStack[0] = Converter.ConvertingToByte(Math.Min(TalentStack[1], TalentStack[0]+1));
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

    public class Charge
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
            // A betöltött charge nullázása
            State[0] = 0;
        }
        public void Nullify() => /*A betöltött charge nullázása*/Reset();

        public void Load(int val)
        {
            State[0] = (ushort)Math.Min(State[0] + Converter.ConvertingToUshort(val), State[1]);
            if (character.OnChargeLoaded() == true) State[0] = 0;
        }
    }

    public class Shift
    {
        public Character character;
        public sbyte[] Mode = new sbyte[] { 1, 1 }; // aktív mód, maximum mód
        public ShiftMode Switch;
        public sbyte SwitchDirect = 1;
        public byte[] Cooldown = new byte[] { 0, 1 }; // hátralévő, kezdő
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