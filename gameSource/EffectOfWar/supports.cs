using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EffectOfWar
{
    class Joker : Character
    {
        public Joker()
        {
            Name = "Joker";
            S1T = "Minden társát gyógyítja 85% M-erővel";
            S2T = "Minden társának növeli a Manaérzékenységét 15%-al 3 körig";
            SpecialT = "Minden képesség használat után ad magának 100% P-erő Recover-t 3 körig";
            init(280, 3, 3, 13, 8, 1.3f, 1.1f, 0.9f, 1f);
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, (float)MagicalAttack[0] * 0.85f, this);
            foreach (var c in GetCharacters(true, 4, true))
            {
                c.Healing(heal);
            }
        }

        public override void SkillTwo()
        {
            EffectGroup inc = new EffectGroup("Manasens", Effect.manasens, 0.15f, 3, true, true, this);
            foreach (var c in GetCharacters(true, 4, true))
            {
                inc.Give(c);
            }
        }

        public override void AfterSkillUse(Skill used)
        {
            OverTime hot = new OverTime(this, "Recover", PhysicalAttack[0], 3, true, OverTimeType.Recover, true);
            hot.Give(this);
        }
    }

    class Doctor : Character
    {
        public Doctor()
        {
            Name = "Doctor";
            S1T = "Minden társát gyógyítja 100% F-erővel";
            S2T = "Minden társának növeli a MaxÉletét 10%-al 3 körig";
            SpecialT = "Minden körben gyógyítja a százalékosan legkevesebb életen lévő társát 150% F-erővel";
            init(320, 15, 10, 0, 0, 0.7f, 1, 1.2f, 1.05f);
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0], this);
            GetCharacters(true, -1, true).ToList().ForEach(c => c.Healing(heal));
        }

        public override void SkillTwo()
        {
            EffectGroup inc = new EffectGroup("MaxHP", Effect.maxhp, 0.1f, 3, true, true, this);
            GetCharacters(true, -1, true).ToList().ForEach(c => inc.Give(c));
        }

        public override void EndOfTurn()
        {
            base.EndOfTurn();
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0] * 1.5f, this);
            GetCharacters(true, 1, true, TargetingMode.lowestHpPercent)[0].Healing(heal);
        }
    }

    class Virus : Character
    {
        public Virus() 
        {
            Name = "Virus";
            S1T = "Minden társát gyógyítja 100% M-erővel";
            S2T = "Gyengíti minden ellenfél Immunrendszerét és Manaérzékenységét  25%-al 1 körig (Nem törölhető)";
            SpecialT = "Minden sérülés után egy random ellenfél immunrendszerét csökkenti 2%-al a játék végéig (törölhető)";
            init(380, 10, 2, 7, 4, 0.9f, 1.1f, 0.9f, 1.1f);
        }

        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            EffectGroup debuff = new EffectGroup("Immunrendszer", Effect.simmun, 0.02f, -1, false, true, this);
            debuff.Give(GetCharacters(false, -1, true, TargetingMode.random)[0]);
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0], this);
            GetCharacters(true, -1, true).ToList().ForEach(c => c.Healing(heal));
        }

        public override void SkillTwo()
        {
            EffectGroup debuff = new EffectGroup("Reg stats", new[] { Effect.manasens, Effect.simmun }, new[] { 0.25f, 0.25f }, 1, false, false, this);
            GetCharacters(false, -1, true).ToList().ForEach(c => debuff.Give(c));
        }
    }

    class Alchemist : Character
    {
        public Alchemist() 
        {
            Name = "Alchemist";
            S1T = "Gyógyít minden csapattagot 100% M- és 100% F-erővel";
            S2T = "Gyógyít 1 társat 200% M- és 200% F-erővel";
            SpecialT = "70 a maximum charge-a, ha azt eléri akkor törli az összes debuff-ot a társairól, a charge a gyógyítás értékével nő";
            init(340, 7, 4, 7, 4, 0.85f, 1.1f, 0.85f, 1.1f);
            charge = new Charge(this, 70);
        }

        public override void SpecialTechnique(object arg) => charge.Load((int)arg);

        public override bool OnChargeLoaded()
        {
            GetCharacters(true, -1, true).ToList().ForEach(c => {
                for (int i = c.effects.Count - 1; i >= 0; i--)
                    if (!c.effects[i].positive)
                        c.effects[i].Remove(c);
            });
            return true;
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(PhysicalAttack[0], MagicalAttack[0], this);
            GetCharacters(true, -1, true).ToList().ForEach(c => c.Healing(heal));
        }

        public override void SkillTwo()
        {
            Healing heal = new Healing(PhysicalAttack[0] * 2, MagicalAttack[0] * 2, this);
            GetCharacters(true, 1, true)[0].Healing(heal);
        }
    }

    class Merlin : Character
    {
        public Merlin()
        {
            Name = "Merlin";
            S1T = "Gyógyítja a csapatot az ellenfél maxhp átlagának 3%-ával";
            S2T = "A legkisebb életű ellenfélre taunt hatást rak 1 körig";
            SpecialT = "Kör végén a legkisebb életű társa kap 15-os reincarnation hatás 1 körig, a másik két társát pedig gyógyítja 60% M erővel";
            init(260, 0, 2, 10, 3, 1.2f, 1.2f, 1, 1);
        }

        public override void SkillOne()
        {
            float healAmount = (float)GetCharacters(false, -1, false).Average(c => c.MaxHitpoints[0]) * 0.03f;
            Healing heal = new Healing(HealingType.magic, healAmount, this);
            GetCharacters(true, -1, true).ToList().ForEach(c => c.Healing(heal));
        }
        public override void SkillTwo()
        {
            EffectGroup taunt = new EffectGroup("Taunt", Effect.taunt, 0, 1, false, true, this);
            taunt.Give(GetCharacters(false, -1, true, TargetingMode.lowestHp)[0]);
        }
        public override void EndOfTurn()
        {
            base.EndOfTurn();
            EffectGroup reinc = new EffectGroup("Reincarnation", Effect.reincarnation, 0.15f, 1, false, true, this);
            Character[] Allys = GetCharacters(true, 3, true, TargetingMode.lowestHp);
            reinc.Give(Allys[0]);
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 0.6f, this);
            for (int i = 1; i < 3; i++)
            {
                Allys[i].Healing(heal);
            }
        }
    }

    class Garden : Character
    {
        byte leafCounter = 0;
        public Garden()
        {
            Name = "Garden";
            S1T = "Gyógyít minden társat 100% M & F erővel";
            S2T = "Gyógyít minden társat 60% M erővel és megnöveli a Mágia ismeretével";
            SpecialT = "Minden képesség használat előtt ad az egész csapatnak 1-1 leaf jelölőt +minden aktív leaf jelölő után ami a csapaton van nő az M ereje 7%-ot. Leaf: 3  körig tart, törlődéskor 17%-os Mana charger HoT-ot 3 körig. Maximum 3 leaf lehet egy társon.";
            OwnMarker = new Marker[]
            {
                new Marker("Leaf", this, 1, 0, null, new OverTime(this, "Mana charger", 0.17f, 3, true, OverTimeType.ManaCharge, true), 3)
            };
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(PhysicalAttack[0], MagicalAttack[0], this);
            GetCharacters(true, -1, true).ToList().ForEach(c => c.Healing(heal));
        }

        public override void SkillTwo()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * MagicalKnowledge[0], this);
            GetCharacters(true, -1, true).ToList().ForEach(c => c.Healing(heal));
        }
        public override void BeforeSkillUse(Skill used)
        {
            byte count = 0;
            List<Character> allys = GetCharacters(true, -1, true).ToList();
            foreach (var item in allys)
            {
                if (Marker.Count(item, OwnMarker[0]) < 3)
                    OwnMarker[0].Give(item);
                count += Marker.Count(item, OwnMarker[0]);
            }
            if (count > leafCounter)
            {
                byte inc = (byte)(count - leafCounter);
                leafCounter = count;
                MagicalAttack[0] += Converter.ConvertingToByte(MagicalAttack[1]*0.07f*inc);
            }
            else
            {
                byte dec = (byte)(leafCounter - count);
                MagicalAttack[0] -= Converter.ConvertingToByte(MagicalAttack[1]*0.07f*dec);
            }
        }
    }
}