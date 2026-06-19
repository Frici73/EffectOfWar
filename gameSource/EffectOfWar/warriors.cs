using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EffectOfWar
{
    class Barrier : Character
    {
         
        public Barrier()
        {
            Name = "Barrier";
            S1T = "Ad minden társának 75% F-erő pajzsot";
            S2T = "Megtámad 1 ellenfelet 100% F-erővel";
            SpecialT = "Minden védekezés után nő az Immunrendszere 3%-ot";
            init(525, 17, 8, 0, 5, 1f, 0.8f, 1f, 1f);
        }

        public override void SkillOne()
        {
            GetCharacters(false, 1).ToList().ForEach(e => e.Shielding(Converter.ConvertingToUshort(PhysicalAttack[0] * 0.75f), this));
        }

        public override void SkillTwo()
        {
            GetCharacters(false, 1).ToList().ForEach(e => e.Defense(this, new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill)));
        }

        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            if (dmg.atktype == AttackType.Skill) Immun[0] += 0.03f;
        }
    }

    class Guardian : Character
    {
         
        public Guardian()
        {
            Name = "Guardian";
            S1T = "Megtámad 3 ellenfelet 100% M-erővel és 100% F-erővel és a sebzés mértékét megkapja pajzsként";
            S2T = "15% Tanky buffot ad magának";
            SpecialT = "Minden képesség használat után ad magának 50% F-erő pajzsot";
            init(430, 13, 7, 6, 7, 0.8f, 1.2f, 0.8f, 1.2f);
        }

        public override void AfterSkillUse(Skill skill)
        {
            Shielding(Converter.ConvertingToUshort(PhysicalAttack[0] * 0.5f), this);
        }

        public override void SkillOne()
        {
            ushort damageDealt = 0;
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            foreach (Character c in GetCharacters(false, 3))
            {
                damageDealt = c.Defense(this, dmg)[0];
            }
            Shielding(damageDealt, this);
        }

        public override void SkillTwo()
        {
            EffectGroup tanky = new EffectGroup("Tanky", new Effect[] { Effect.taunt, Effect.dmgR }, new float[] { 0, 0.15f }, 3, true, true, this);
            tanky.Give(this);
        }
    }

    class Bulldozer : Character
    {
         
        public Bulldozer()
        {
            Name = "Bulldozer";
            S1T = "Megtámad 1 ellenfelet 100% F-erővel";
            S2T = "Ad magának 30%-os Tanky buffot 1 körig";
            SpecialT = "Minden kör elején nő az Immunrendszere és Manaérzékenysége 5%-ot";
            init(580, 15, 2, 0, 2, 1f, 1f, 1f, 1f);
        }
        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        public override void SkillTwo()
        {
            EffectGroup tanky = new EffectGroup("Tanky", new Effect[] { Effect.taunt, Effect.dmgR }, new float[] { 0, 0.3f }, 1, true, true, this);
            tanky.Give(this);
        }
        public override void EndOfTurn()
        {
            base.EndOfTurn();
            Immun[0] += 0.05f;
            ManaSensitivity[0] += 0.05f;
        }
    }

    class Emerald : Character
    {
         
        public Emerald()
        {
            Name = "Emerald";
            S1T = "Ad magának 130% M-erős Counter-t 1 körig és +30% Mágiaismeret ide";
            S2T = "Ad a csapatának 75% Mágikuserő pajzsot";
            SpecialT = "Ha ő meghalna vagy egy társa akkor ad a csapatnak 300% M-erő pajzsot és sebzi az összes ellenfelet 200% M-erővel és 100% M-védelem ignorálással";
            init(475, 3, 4, 10, 3, 1.25f, 1.2f, 1, 1);
        }
        public override void SkillOne()
        {
            counter.Edit(1.3f, 0, 0, 1, 1);
            counter.Increase(0, 0, 0, 0.3f);
        }
        public override void SkillTwo()
        {
            foreach (var c in GetCharacters(true, 4, true))
            {
                c.Shielding(Converter.ConvertingToUshort(MagicalAttack[0] * 0.75f), this);
            }
        }
        public override void OnSelfDeath(Character killer)
        {
            base.OnSelfDeath(killer);
            SpecialTechnique();
        }
        public override void OnTeammateDeath(Character teammate, Character killer)
        {
            base.OnTeammateDeath(teammate, killer);
            SpecialTechnique();
        }

        public override void SpecialTechnique()
        {
            foreach (var c in GetCharacters(true, 4, true))
            {
                c.Shielding(Converter.ConvertingToUshort(MagicalAttack[0] * 3), this);
            }
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0] * 2, 0, 1, AttackType.Reflect);
            foreach (var c in GetCharacters(false, 4, true))
            {
                c.Defense(this, dmg);
            }
        }
    }

    class ArthurKing : Character
    {
         
        public ArthurKing()
        {
            Name = "Arthur King";
            S1T = "Ad magának 7% maxhp pajzsot és tauntol 2 körig";
            S2T = "Megtámad 1 ellenfelet 100% F erővel és a sebzés 60%-át megkapja pajzsként";
            SpecialT = "Körönként nő a regenerációs képessége 0,2%-ot, maxhp-ja 5%-ot és nő a punctualja 3%-ot";
            init(480, 14, 6, 0, 2, 1f, 1f, 1.2f, 0.7f);
        }

        public override void SkillOne()
        {
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, true, this);
            taunt.Give(this);
            Shielding(Converter.ConvertingToUshort(MaxHitpoints[0] * 0.07f), this);
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(PhysicalAttack[0], 0, Punctual[0], 0, DMGDealt, AttackType.Skill);
            Shielding(Converter.ConvertingToUshort(GetCharacters(false, 1)[0].Defense(this, dmg)[0] * 0.6f), this);
        }
        public override void EndOfTurn()
        {
            base.EndOfTurn();
            EffectGroup buff = new EffectGroup("Arthur Special", new Effect[] { Effect.reg, Effect.maxhp, Effect.punctual }, new float[] { 0.02f, 0.05f, 0.03f }, -1, true, false, this);
            buff.Give(this);
        }
    }

    class Phase : Character
    {
         
        public Phase()
        {
            Name = "Phase";
            S1T = "Minden társának ad 150% M erő pajzsot / Ad magának 200% M erő pajzsot";
            S2T = "Minden társát gyógyítja 125% F erővel és a túlgyógyulás pajzsá alakul / csökkenti minden társának a sérülését 15%-al 2 körig";
            SpecialT = "Minden képesség kijátszás után Phase-t vált (1 / 2) (körkörösen vált)";
            TalentT = "3-as stackje van, 4 körönként tölt 1-et, játék elején maxon van, a talent hatására Phase-t vált és gyógyul 100% F erővel";
            talent = new Talent(this, 4, 3);
            shift = new Shift(this, 2, ShiftMode.circle, 0);
            init(420, 6, 6, 6, 6, 1, 1, 1, 1);
        }

        public override void Talent()
        {
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0], this);
            Healing(heal);
        }

        public override void SkillOne()
        {
            if (shift.ActiveMode == 1)
            {
                foreach (var c in GetCharacters(true, 4, true))
                {
                    c.Shielding(Converter.ConvertingToUshort(MagicalAttack[0] * 1.5f), this);
                }
            }
            else
            {
                Shielding(Converter.ConvertingToUshort(MagicalAttack[0] * 2), this);
            }
        }

        public override void SkillTwo()
        {
            if (shift.ActiveMode == 1)
            {
                foreach (var c in GetCharacters(true, 4, true))
                {
                    Healing heal = new Healing(HealingType.physi, Converter.ConvertingToShort(PhysicalAttack[0] * 1.25f), this);
                    ushort overheal = Converter.ConvertingToUshort(c.Healing(heal)[2]);
                    if (overheal > 0)
                    {
                        c.Shielding(overheal, this);
                    }
                }
            }
            else
            {
                EffectGroup dmgRDec = new EffectGroup("Damage Reduction", Effect.dmgR, 0.15f, 2, true, true, this);
                foreach (var c in GetCharacters(true, 4, true))
                {
                    dmgRDec.Give(c);
                }
            }
        }

        public override void AfterSkillUse(Skill used)
        {
            shift.Shifting();
        }
    }

    class Fulmare : Character
    {
         
        public Fulmare()
        {
            Name = "Fulmare";
            S1T = "Ellopja minden ellenfél pajzsának a 30%-át és az előző körből való pajzsának 70%-át át rakja a jelenlegi körre";
            S2T = "Megtámad 1 ellenfelet 100% M & F erővel";
            SpecialT = "Minden képesség kijátszás után gyógyul az elvesztett életének 1,5%-ával";
            init(440, 4, 4, 8, 6, 1, 1, 1, 1);
        }

        public override void SkillOne()
        {
            foreach (var c in GetCharacters(false, 4, true))
            {
                ushort stolen = Converter.ConvertingToUshort((c.shield[0] + c.shield[1]) * 0.3f);
                Shielding(stolen, this);
                shield[1] -= Converter.ConvertingToUshort(stolen - shield[0]);
                shield[0] -= stolen;
            }
            shield[1] = Converter.ConvertingToUshort(shield[0] * 0.7f);
            shield[0] = Converter.ConvertingToUshort(shield[0] * 0.3f);
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        public override void AfterSkillUse(Skill used)
        {
            short lost = Converter.ConvertingToShort(MaxHitpoints[0] - Hitpoints[0]);
            Healing heal = new Healing(HealingType.none, Converter.ConvertingToShort(lost * 0.015f), this);
            Healing(heal);
        }
    }

    class Trash : Character
    {
         
        public Trash()
        {
            Name = "Trash";
            S1T = "Minden társa kevesebbet sérül 15%-al 2 körig";
            S2T = "Gyógyul az elvesztett életének 25%-ával és tauntol 2 körig";
            SpecialT = "Minden képesség használat után töröl magáról 1 DoT hatást";
            TalentT = "Minden debuff lejáratát a csapaton felgyorsítja 1 körrel (DoT-ra nem hat) (4 körös cooldown, 1-es stack)";
            talent = new Talent(this, 4, 1);
            init(530, 15, 5, 3, 5, 1.15f, 1, 1.15f, 1);
        }

        public override void Talent()
        {
            foreach (var c in GetCharacters(true, 4, true))
            {
                foreach (var e in c.effects)
                {
                    if (!e.positive)
                    {
                        e.EndOfTurn(c);
                    }
                }
            }
        }
    
        public override void SkillOne()
        {
            EffectGroup dmgRDec = new EffectGroup("Damage Reduction", Effect.dmgR, 0.15f, 2, true, true, this);
            foreach (var c in GetCharacters(true, 4, true))
            {
                dmgRDec.Give(c);
            }
        }

        public override void SkillTwo()
        {
            short lost = Converter.ConvertingToShort(MaxHitpoints[0] - Hitpoints[0]);
            Healing heal = new Healing(HealingType.none, Converter.ConvertingToShort(lost * 0.25f), this);
            Healing(heal);
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, true, this);
            taunt.Give(this);
        }
    }

    class Afterglow : Character
    {
         
        public Afterglow()
        {
            Name = "Afterglow";
            S1T = "Ad minden társának 75% M erő pajzsot";
            S2T = "Megtámad 2 ellenfelet 100% M erővel";
            SpecialT = "kör elején minden társa kap 7% sebzés csökkentést 2 körig. + Ha meghal helyett fog foglalni a csatatéren és véd a mögötte álló helyett, majd a bejövő sebzés felét tovább adja rá tiszta sebzésként";
            init(530, 0, 6, 12, 5, 1, 1.3f, 1, 1);
        }
        public override void OnSelfDeath(Character killer)
        {
            if (teamID == 1) link.Add(this, Team.first);
            else link.Add(this, Team.second);
            EffectGroup effect = new EffectGroup("Afterglow", Effect.Untouchable, 0, -1, true, false, this);
            effect.Give(this);
        }

        public override void SkillOne()
        {
            foreach (var c in GetCharacters(true, 4, true))
            {
                c.Shielding(Converter.ConvertingToUshort(MagicalAttack[0] * 0.75f), this);
            }
        }

        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            foreach (var c in GetCharacters(false, 2))
            {
                c.Defense(this, dmg);
            }
        }
    }

    class Cooldown : Character
    {
         
        bool inc;
        byte incCooldown;
        public Cooldown()
        {
            Name = "Cooldown";
            S1T = "Növeli a különleges képességének hatását 15%-al és tauntol 2 körig (max 30%-ig)";
            S2T = "Gyógyítja magát és pajzsot ad magának 100% M erővel";
            SpecialT = "Mikor megtámadják 15% eséllyel Sleep-et ad a támadónak 1 körig";
            init(350, 0, 7, 5, 7, 1.2f, 1.4f, 1, 1);
        }

        public override void SkillOne()
        {
            inc = true;
            incCooldown = 2;
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, true, this);
            taunt.Give(this);
        }

        public override void SkillTwo()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0], this);
            Healing(heal);
            Shielding(Converter.ConvertingToUshort(MagicalAttack[0]), this);
        }

        public override void EndOfTurn()
        {
            base.EndOfTurn();
            if (inc)
            {
                incCooldown -= 1;
                if (incCooldown == 0)
                {
                    inc = false;
                }
            }
        }
        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            if (Rnd.R(1f) < 0.15f*(Convert.ToByte(inc)+1))
            {
                EffectGroup sleep = new EffectGroup("Sleep", Effect.sleep, 0, 1, false, false, this);
                sleep.Give(attacker);
            }
        }
    }

    class Frame : Character
    {
         
        bool? skillUsed = null; // false = skill1 | true = skill2
        public Frame()
        { 
            Name = "Frame";
            S1T = "Megtámad 1 ellenfelet 120% F erővel és a sebzés 100%-át megkapja pajzsként";
            S2T = "Taunt-ol 2 körig és gyógyul 50% M erővel";
            SpecialT = "Minden védekezés után használja a utoljára kijátszott képességét";
            init(330, 8, 4, 8, 4, 0.8f, 1.1f, 0.8f, 1.1f);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            Shielding(GetCharacters(false, 1)[0].Defense(this, dmg)[0], this);
        }
        public override void SkillTwo() 
        {
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, true, this);
            taunt.Give(this);
            Healing heal = new Healing(HealingType.magic, Converter.ConvertingToShort(MagicalAttack[0] * 0.5f), this);
            Healing(heal);
        }
        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            if (skillUsed != null)
            {
                if (skillUsed == false) SkillOne();
                else SkillTwo();
            }
        }
    }

    class GodOfDeath : Character
    {
         
        public GodOfDeath()
        {
            Name = "God of Death";
            S1T = "Tauntol 2 körig (nem törölhető)";
            S2T = "Nő a DoT immunity-je 60%-ot 2 körig (nem törölhető)";
            SpecialT = "amikor ő maga meghal akkor az egész harc alatt elszenvedett összes sérülésének 70%-át az ellenfél legelső harcosára";
            init(430, 0, 6, 0, 6, 0.75f, 1, 0.75f, 1);
        }

        public override void SkillOne()
        {
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, false, this);
            taunt.Give(this);
        }

        public override void SkillTwo()
        {
            EffectGroup dotImm = new EffectGroup("DoT Immunity", Effect.DoTImmun, 0.6f, 2, true, false, this);
            dotImm.Give(this);
        }

        public override void OnSelfDeath(Character killer)
        {
            base.OnSelfDeath(killer);
            DMG dmg = new DMG(DMGType.none, TotalDamageTaken*0.7f, 1, 1, AttackType.Reflect);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
    }

    class Smoke : Character
    {
         
        public Smoke()
        {
            Name = "Smoke";
            S1T = "-";
            S2T = "-";
            SpecialT = "-";
            TalentT = "Megtámad minden ellenfelet a jelenlegi életük 50%-ával és a sebzés 20%-át pajzsként adja a csapatnak (stack 0, max stack 4, cooldown 5)";
            talent = new Talent(this, 5, 0);
            init(350, 0, 7, 0, 7, 1, 1, 1, 1);
        }

        public override void Talent()
        {
            DMG dmg = new DMG(DMGType.none, 0, 1, 1, AttackType.Skill);
            foreach (var c in GetCharacters(false, 4, true))
            {
                dmg.physical = Converter.ConvertingToShort(c.Hitpoints[0] * 0.5f);
                ushort damageDealt = c.Defense(this, dmg)[0];
                foreach (var a in GetCharacters(true, 4, true))
                {
                    a.Shielding(Converter.ConvertingToUshort(damageDealt * 0.2f), this);
                }
            }
        }
    }

    class Fortuneteller : Character
    {
         
        public Fortuneteller()
        {
            Name = "Fortune-teller";
            S1T = "Tauntol 2 körig (nem törölhető) és kap 10%-os Reflect-et 2 körig";
            S2T = "Megtámad 1 random ellenfelet 100% M erővel, a sebzése nő az önmagán lévő buffok után 10%-ot, majd 1 körre Sleep debuffot ad az ellenfélnek";
            SpecialT = "Védekezés után növeli a csapatának sebzését 7%-al 3 körig";
            init(410, 0, 5, 8, 4, 1, 1.3f, 1, 1);
        }

        public override void SkillOne()
        {
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, false, this);
            taunt.Give(this);
            reflect.Edit(0.1f, 2);
        }

        public override void SkillTwo()
        {
            Character enemy = GetCharacters(false, 1)[0];
            EffectGroup sleep = new EffectGroup("Sleep", Effect.sleep, 0, 1, false, true, this);
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt+effects.Count(e=>e.positive), AttackType.Skill);
            enemy.Defense(this, dmg);
            sleep.Give(enemy);
        }
        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            EffectGroup e = new EffectGroup("DMG dealt increase", Effect.dmgD, 0.07f, 3, true, true, this);
            foreach (Character teammate in GetCharacters(true, 4, true)) e.Give(teammate);
        }
    }

    class Szunvukung : Character
    {
        bool clone = false;
        public Szunvukung() : this(false) { }
        public Szunvukung(bool clone)
        {
            Name = "Szun-Vu Kung";
            this.clone = clone;
            short hp;
            if (!clone)
            {
                S1T = "Tauntol 2 körig és 100% F erővel támad meg egy ellenfelet (ha van klón akkor az tauntol és minden klón 75% f erővel támad)";
                S2T = "";
                SpecialT = "Ha sebeznek a klónjai akkor ő sérül a sebzés 50%-ával";
                TalentT = "Leidéz max 2 klónt (stack függő) mindegyik után sérül 3% maxhp-t. cooldown 2, stack 2, kezdő stack 1. (a klónok 50 hp-val rendelkeznek";
                talent = new Talent(this, 2, 2, 1);
                hp = 380;
            }
            else hp = 50;
            init(hp, 15, 4, 3, 3, 0.75f, 0.75f, 0.85f, 1.2f);
        }

        public override void Talent()
        {
            int count = talent.TalentStack[0] + 1;
            talent.TalentStack[0] = 0;
            for (int i = 0; i < count; i++)
                link.AddClone(new Szunvukung(true), link.GetTeam(this));
            DMG dmg = new DMG(MaxHitpoints[0] * 0.03f * count);
            Defense(this, dmg);
        }

        public override void SkillOne()
        {
            if (!clone)
            {
                List<Character> kung = GetCharacters(true, -1, true).ToList();
                kung.RemoveAll(c => c.Name != this.Name);
                EffectGroup e = new EffectGroup("Taunt", Effect.taunt, 0, 2, true, false, this);
                Szunvukung? clone = (Szunvukung)kung.FirstOrDefault(c =>
                {
                    Szunvukung k = (Szunvukung)c;
                    return k.clone;
                });
                e.Give(clone==null?this:clone, true);
                DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
                GetCharacters(false, 1)[0].Defense(this, dmg);
                kung.ForEach(c =>
                {
                    Szunvukung k = (Szunvukung)c;
                    if (k.clone) k.SkillOne();
                });
            }
            else
            {
                DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0] * 0.75f, Punctual[0], DMGDealt, AttackType.Skill);
                DMG tooriginal = new DMG(GetCharacters(false, 1)[0].Defense(this, dmg)[0]*0.5f);
                GetCharacters(true, -1, true).First(c =>
                {
                    if (c.Name == this.Name)
                    {
                        Szunvukung k = (Szunvukung)c;
                        return k.clone;
                    }
                    else return false;
                }).Defense(this, tooriginal);
            }
        }
    }
}