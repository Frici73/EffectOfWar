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

    class Feather : Character
    {
         
        public Feather()
        {
            Name = "Feather";
            S1T = "Gyógyítja 1 társát 250% F erővel";
            S2T = "Gyógyítja 2 társát 150% M erővel";
            SpecialT = "Minden kör elején gyógyul a regeneráción felül az elveszett életének 7%-ával, a képességeit befolyásolja a mágiaismeret és pontosság";
            init(270, 8, 0, 5, 0, 1.5f, 1.3f, 1.5f, 1.3f);
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            Healing heal = new Healing(HealingType.none, (MaxHitpoints[0] - Hitpoints[0])*0.07f, this);
            Healing(heal);
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0] * 2.5f * Punctual[0], this);
            GetCharacters(true, 1, false)[0].Healing(heal);
        }

        public override void SkillTwo()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 1.5f * MagicalKnowledge[0], this);
            GetCharacters(true, 2, true).ToList().ForEach(c => c.Healing(heal));
        }
    }

    class Connection : Character
    {
        public Connection()
        {
            Name = "Connection";
            S1T = "Megtámad 1 ellenfelet 150% M erővel és a sebzés 150%-ával gyógyítja a %-osan legkisebb életű társát";
            S2T = "A %-osan legmagasabb életű társát sebzi a csapat többi tagjának az 5% elvesztett életével és őket pedig gyógyítja annyival";
            SpecialT = "Minden képesség használat után gyógyítja a társait 100% F-erővel";
            init(340, 5, 5, 10, 4, 1.1f, 1.25f, 1, 1.1f);
        }

        public override void AfterSkillUse(Skill used)
        {
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0], this);
            foreach (var c in GetCharacters(true, -1, true))
                if (c != this) c.Healing(heal);
        }

        public override void SkillOne()
        {
            Character enemy = GetCharacters(false, 1)[0];
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            Healing heal = new Healing(HealingType.none, enemy.Defense(this, dmg)[0]*1.5f, this);
            GetCharacters(true, 1, true, TargetingMode.lowestHpPercent)[0].Healing(heal);
        }

        public override void SkillTwo()
        {
            List<Character> allys = GetCharacters(true, -1, true, TargetingMode.highestHpPercent).ToList();
            Character high = allys[0];
            allys.RemoveAt(0);
            float value = 0;
            allys.ForEach(c => { value += (c.MaxHitpoints[0] - c.Hitpoints[0]) * 0.05f; });
            DMG dmg = new DMG(value);
            Healing heal = new Healing(HealingType.none, value, this);
            high.Defense(this, dmg);
            allys.ForEach(c=>c.Healing(heal));
        }
    }

    class Eternal : Character
    {
        public Eternal()
        {
            Name = "Eternal";
            S1T = "Minden társát gyógyítja 100% M vagy F erővel (amelyik nagyobb)";
            S2T = "Gyógyítja 1 társát 100% M&F erővel";
            SpecialT = "Ha sérül akkor a sérülés értékének 50%-ával gyógyítja a társait (önmagát nem) (mágikusan)";
            TalentT = "Minden társának ad 1%-os HP Drop hatást 1 körig (cooldown 5 kör, stack 1, játék elején 0 a stack)";
            init(370, 7, 4, 7, 4, 1.2f, 1, 1, 1.2f);
            talent = new Talent(this, 5, 1);
            talent.TalentStack[0] = 0;
        }

        public override void Talent()
        {
            EffectGroup b = new EffectGroup("HP Drop", Effect.hpDrop, 0.01f, 1, true, false, this);
            GetCharacters(true, -1).ToList().ForEach(c=>b.Give(c, true));
        }

        public override void SkillOne()
        {
            Healing heal;
            if (PhysicalAttack[0] >= MagicalAttack[0]) heal = new Healing(HealingType.physi, PhysicalAttack[0], this);
            else heal = new Healing(HealingType.magic, MagicalAttack[0], this);
            GetCharacters(true, -1, true).ToList().ForEach(c=>c.Healing(heal));
        }
        public override void SkillTwo()
        {
            Healing heal = new Healing(PhysicalAttack[0], MagicalAttack[0], this);
            GetCharacters(true, 1)[0].Healing(heal);
        }
        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            Healing heal = new Healing(HealingType.magic, taked*0.5f, this);
            foreach (var c in GetCharacters(true, -1, true))
                if (c != this) c.Healing(heal);
        }
    }

    class Snake : Character
    {
        bool use = false;
        public Snake()
        {
            Name = "Snake";
            S1T = "Gyógyít 1 társat 100% F erővel";
            S2T = "Ad minden társának Over regenerate-et ami 3%*speed Jelölő Snaken 3 körig";
            SpecialT = "Ha játszott ki képességet akkor a kör végén kap 1 Speed jelölőt. Védekezéskor Speed jelölő Snaken * 3% eséllyel visszagyógyulja a támadás 75%-át (halálból nem hozza vissza)";
            init(380, 12, 4, 0, 2, 1, 1, 1.2f, 1);
            OwnMarker = new Marker[]
            {
                new Marker("Speed", this, 1, 0,null, null, 7)
            };
        }
        public override void AfterSkillUse(Skill used) => use = true;

        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked)
        {
            int count = Marker.Count(this, OwnMarker[0]);
            if (Rnd.R(100) < count*3)
            {
                Healing heal = new Healing(HealingType.none, taked * 0.75f, this);
                Healing(heal);
            }
        }
        public override void EndOfTurn()
        {
            base.EndOfTurn();
            if (use) OwnMarker[0].Give(this, true);
            use = false;
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0], this);
            GetCharacters(true, 1)[0].Healing(heal);
        }

        public override void SkillTwo()
        {
            OverTime ot = new OverTime(this, "Over Regenerate", 0.03f* Marker.Count(this, OwnMarker[0]), 3, true, OverTimeType.OverRegenerate, true);
            GetCharacters(true, -1, true).ToList().ForEach(c=>ot.Give(c));
        }
    }

    class Grandmother : Character
    {
        public Grandmother()
        {
            Name = "Grandmother";
            S1T = "Gyógyít 1 társat 120% M erővel";
            S2T = "Minden társának növeli a pontosságát és mágia ismeretét 20%-al 3 körig";
            SpecialT = "Minden képesség használat után kioszt max 4 csapattagnak 1-1 különböző ízű fortune cookie: 1) Vanília: m és f erő +20% 2) Csoki: manaérzékenység és immunrendszer +20% 3) eper: m és f védelem +20% 4) banán: debuffimmunity és maxhp +15%";
            init(275, 3, 3, 11, 5, 0.9f, 1.25f, 0.85f, 0.95f);
        }

        public override void AfterSkillUse(Skill used)
        {
            EffectGroup[] effects = (EffectGroup[])new EffectGroup[]
            {
                new EffectGroup("Vanillia", new[] { Effect.matk, Effect.patk}, new[] {0.2f, 0.2f}, 2, true, false, this),
                new EffectGroup("Csoki", new[] { Effect.manasens, Effect.simmun }, new[] { 0.2f, 0.2f}, 2, true, false, this),
                new EffectGroup("Eper", new[] { Effect.mdef, Effect.pdef}, new[] { 0.2f, 0.2f}, 2, true, false, this),
                new EffectGroup("Banán", new[] { Effect.maxhp, Effect.debuffImmun}, new[] {0.15f, 1f }, 2, true, false, this)
            }.Shuffle();
            Character[] allys = GetCharacters(true, 4, true);
            for (int i = 0; i < allys.Length; i++) effects[i].Give(allys[i], true);
        }

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 1.2f, this);
            GetCharacters(true, 1, true)[0].Healing(heal);
        }

        public override void SkillTwo()
        {
            EffectGroup buff = new EffectGroup("Punctual & Magicalknowledge inc", new[] { Effect.mknow, Effect.punctual }, new[] { 0.2f, 0.2f }, 3, true, true, this);
            GetCharacters(true, -1, true).ToList().ForEach(c=>buff.Give(c));
        }
    }

    class Equality : Character
    {
        public Equality() 
        {
            Name = "Equality";
            S1T = "Megtámad 1 ellenfelet 100% F erővel és ignorálja a védelmének 40%-át, a sebzés 70%-ával gyógyítja a teljes csapatot (fizikailag)";
            S2T = "Megtámad 1 ellenfelet 100% F erővel és ignorálja a védelmének 40%-át, a sebzés 70%-át pajszként adja a csapatnak";
            SpecialT = "Minden 3. képessége ad a társainak 25% Debuff immunity-t 3 körig";
            TalentT = "Minden csapattársat és ellenfelet maxéletre gyógyít  (stack 1, cooldown: végtelen)";
            init(420, 8, 3, 3, 3, 1, 0.7f, 1, 1.05f);
            talent = new Talent(this, -1, 1);
            charge = new Charge(this, 3);
        }
        public override void Talent()
        {
            Character[] allys = GetCharacters(true, -1, true);
            Character[] enemys = GetCharacters(false, -1, true);
            for (int i = 0; i < Math.Max(allys.Length, enemys.Length); i++)
            {
                try
                {
                    Healing heal = new Healing(HealingType.none, allys[i].MaxHitpoints[0], this);
                    allys[i].Healing(heal);
                }
                catch { }
                try
                {
                    Healing heal1 = new Healing(HealingType.none, enemys[i].MaxHitpoints[0], this);
                    enemys[i].Healing(heal1);
                }
                catch { }
            }
        }
        public override bool OnChargeLoaded()
        {
            EffectGroup imm = new EffectGroup("Debuffimmunity", Effect.debuffImmun, 0.25f, 3, true, false, this);
            Character[] allys = GetCharacters(true, -1, true);
            foreach (var ally in allys) if (ally != this) imm.Give(ally, true);
            return true;
        }
        public override void AfterSkillUse(Skill used) => charge.Load(1);

        public override void SkillOne() => SpecialTechnique(Skill.first);
        public override void SkillTwo() => SpecialTechnique(Skill.second);

        public override void SpecialTechnique(object arg)
        {
            Skill skill = (Skill)arg;
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            dmg.ignoredPhysicalDefense = 0.4f;
            ushort value = GetCharacters(false, 1)[0].Defense(this , dmg)[0];
            List<Character> allys = GetCharacters(true, -1, true).ToList();
            if (skill == Skill.first)
            {
                Healing heal = new Healing(HealingType.physi, value, this);
                allys.ForEach(c => c.Healing(heal));
            }
            else allys.ForEach(c => c.Shielding(value, this));
        }
    }

    class Collect : Character
    {
        public Collect()
        {
            Name = "Collect";
            S1T = "Gyógyítja 1 társat (tauntot figyelve) 75*csapat létszám% M erővel";
            S2T = "Ad minden társának 20% Reflect-et 2 körig";
            SpecialT = "Mikor egy ellenfél kap buffot gyógyul 20% M erővel";
            init(280, 3, 2, 10, 5, 1.3f, 1, 1, 1);
        }

        public override void AfterEnemyGetEffect(EffectsBasic effect, Character enemy, bool gived)
        {
            if (effect.positive && gived)
            {
                Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 0.2f, this);
                Healing(heal);
            }
        }

        public override void SkillOne()
        {
            Character[] allys = GetCharacters(true, -1);
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0]*0.75f*allys.Length, this);
            allys[0].Healing(heal);
        }

        public override void SkillTwo()
        {
            Character[] allys = GetCharacters(true, -1, true);
            foreach (Character character in allys) character.reflect.Edit(0.2f, 2);
        }
    }

    class Further : Character
    {
        public Further()
        {
            Name = "Further";
            S1T = "Gyógyítja 1 társát 100% M-erővel";
            S2T = "Gyógyítja 2 társát 75% M-erővel";
            SpecialT = "Mikor sérül 1 társa akkor minden társának megnöveli a manaérzékenységét 3%-al 5 körig";
            init(240, 0, 3, 9, 2, 1, 1.2f, 1, 1.3f);
        }

        public override void BeforeTeammateGetDMG(Character attacker, Character teammate, DMG dmg)
        {
            EffectGroup e = new EffectGroup("Manasens inc", Effect.manasens, 0.03f, 5, true, true, this);
            GetCharacters(true, -1, true).ToList().ForEach(c=>e.Give(c));
        }
        public override void BeforeSelfGetDMG(Character attacker, DMG dmg) => BeforeTeammateGetDMG(attacker, this, dmg);

        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0], this);
            GetCharacters(true, 1)[0].Healing(heal);
        }
        public override void SkillTwo()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0]*0.75f, this);
            GetCharacters(true, 2, true).ToList().ForEach(c=>c.Healing(heal));
        }
    }

    class Reverse : Character
    {
        public Reverse()
        {
            Name = "Reverse";
            S1T = "Gyógyít 1 társat 140% M erővel";
            S2T = "Gyógyít 2 társat 75% M erővel";
            SpecialT = "A gyógyítását befolyásolja a sebzés %-a";
            init(270, 0, 3, 9, 4, 1.3f, 1, 1.3f, 1);
        }
        public override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 1.4f * DMGDealt, this);
            GetCharacters(true, 1)[0].Healing(heal);
        }

        public override void SkillTwo()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 0.75f * DMGDealt, this);
            GetCharacters(true, 2,true).ToList().ForEach(c=> c.Healing(heal));
        }
    }
}