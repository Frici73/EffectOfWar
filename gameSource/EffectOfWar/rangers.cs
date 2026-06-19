using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EffectOfWar
{
    class Lightning : Character
    {
         
        public Lightning() 
        {
            Name = "Lightning";
            S1T = "125% F-erő-vel és 125% M-erővel megtámad 1 ellenfelet";
            S2T = "Megtámad 2 ellenfelet 130% M-erővel és növeli a saját sebzés kiosztását 5%-al 4 körig";
            SpecialT = "Minden körben nő a Mágiaismerete 7%-ot";
            init(360, 7, 5, 13, 6, 1, 1.2f, 1, 0.75f);
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            EffectGroup effect = new EffectGroup("Magicalknowledge increase", Effect.mknow, 0.07f, -1, true, false, this);
            effect.Give(this);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0]*1.25f, MagicalAttack[0]*1.25f, Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }

        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0]*1.3f, MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(false, 2)) enemy.Defense(this, dmg);
            EffectGroup effect = new EffectGroup("DMGDealt increase", Effect.dmgD, 0.05f, 4, true, true, this);
            effect.Give(this);
        }
    }

    class Breaker : Character
    {
         
        byte count = 0;
        public Breaker() 
        {
            Name = "Breaker";
            S1T = "Megtámad 1 ellenfelet 120% F-erővel";
            S2T = "Megtámad 1 ellenfelet 100% M-erővel és csökkenti az összes védelmét 3%-al 5 körig";
            SpecialT = "Minden képessége okozta sérülés nő 3%-ot a pályán lévő debuffok után";
            init(260, 17, 4, 10, 5, 0.8f, 1, 0.8f, 1);
        }

        public override void SpecialTechnique()
        {
            count = 0;
            foreach (Character enemy in GetCharacters(false, -1, true))
            {
                count += (byte)enemy.effects.Count(e=>e.positive == true);
            }
            foreach (Character teammate in GetCharacters(true, -1, true))
            {
                count += (byte)teammate.effects.Count(e => e.positive == true);
            }
        }

        public override void SkillOne()
        {
            SpecialTechnique();
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.2f, Punctual[0], DMGDealt+0.3f*count, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        public override void SkillTwo()
        {
            SpecialTechnique();
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt+0.3f*count, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            enemy.Defense(this, dmg);
            EffectGroup effect = new EffectGroup("Defense-related stat decrease", new Effect[] {Effect.pdef, Effect.mdef}, new float[] {0.03f, 0.03f}, 5, false, true, this);
            effect.Give(enemy);
        }
    }

    class Reaper : Character
    {
         
        Character? selected;
        public Reaper() 
        {
            Name = "Reaper";
            S1T = "Megtámad 1 ellenfelet 100% M-erővel és a sebzés nő (ellenfél max hp / saját maxhp)-val";
            S2T = "Megtámad 3 ellenfelet 100% M-erővel és mágikusan gyógyul a sebzés 25%-val";
            SpecialT = "Minden kör elején az random ellenfél M-erejét csökkenti 25%-al és a változás értékével növeli a sajátját 1 körig";
            init(240, 0, 3, 12, 3, 1, 0.85f, 1, 1);
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            if (selected != null) MagicalAttack[0] -= Converter.ConvertingToByte(selected.MagicalAttack[1] * 0.25f);
            Character[] enemys = GetCharacters(false, -1, true);
            selected = enemys[Rnd.R(0, enemys.Length - 1)];
            EffectGroup effect = new EffectGroup("Reaper magical steal", Effect.matk, 0.25f, 1, false, false, this);
            effect.Give(selected, true);
            MagicalAttack[0] += Converter.ConvertingToByte((float)selected.MagicalAttack[1] * 0.25f);
        }

        public override void SkillOne()
        {
            Character enemy = GetCharacters(false, 1)[0];
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt + (float)enemy.MaxHitpoints[0] / MaxHitpoints[0], AttackType.Skill);
            enemy.Defense(this, dmg);
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(false, -1))
            {
                Healing heal = new Healing(HealingType.magic, enemy.Defense(this, dmg)[0], this);
                Healing(heal);
            }
        }
    }

    class Mage : Character
    {
         
        ushort inc;
        public Mage()
        {
            Name = "Mage";
            S1T = "Megtámad 3 ellenfelet 100% M-erővel és 100% F-erővel, a sebzéssel nő a charge-a (max 100-ig)";
            S2T = "Megtámad 1 ellenfelet a charge értékének 150%-ával M-erőként, x1.5 Mágiaismeret itt (0 lesz a charge)";
            SpecialT = "Minden rajta lévő buff után nő az M-ereje 5%-ot képesség használatkor";
            init(320, 3, 3, 10, 5, 1, 1.5f, 0.8f, 0.8f);
            charge = new Charge(this, 100);
        }

        public override void BeforeSkillUse(Skill used)
        {
            inc = (ushort)(effects.Count(e => e.positive == true) * 5);
            MagicalAttack[0] += Converter.ConvertingToByte(MagicalAttack[1] * inc / 100);
        }
        public override void AfterSkillUse(Skill used)
        {
            MagicalAttack[0] -= Converter.ConvertingToByte(MagicalAttack[1] * inc / 100);
            inc = 0;
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            ushort totaldmg = 0;
            foreach (Character enemy in GetCharacters(false, -1)) totaldmg += enemy.Defense(this, dmg)[2];
            charge.Load(totaldmg);
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, Converter.ConvertingToByte(charge.State[0] * 1.5f), MagicalKnowledge[0]*1.5f, DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
            charge.Reset();
        }
    }

    class Robin : Character
    {
         
        public Robin()
        {
            Name = "Robin";
            S1T = "Megtámad 1 ellenfelet 100% M-erővel és 100% F-erővel és ad 1 társának 1 Arrow jelölőt";
            S2T = "Megtámad 2 ellenfelet 80% M-erővel és 80% F-erővel és növeli a sebzését minden egyes Arrow jelölő után ami rajta van egy csapattagon 7%-al";
            SpecialT = "A játék elején minden csapattag kap 3-3 Arrow jelölőt, minden eltávolított jelölő után Robin összes statisztikája nő 3%-al, minden kör elején minden társáról töröl 1-1 Arrow jelölőt";
            init(220, 8, 4, 6, 2, 0.85f, 0.85f, 0.85f, 0.85f);
            OwnMarker = new Marker[1]
            {
                new Marker("Arrow", this, 1, 0, null, null, -1)
            };
        }

        public override void StartOfGame()
        {
            GetCharacters(true, -1, true).ToList().ForEach(c=>
            {
                for (int i = 0; i < 3; i++) OwnMarker[0].Give(c, true);
            });
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            byte removed = 0;
            GetCharacters(true, -1, true).ToList().ForEach(c=>
            {
                for (int i = c.Markers.Count - 1; i >= 0; i--)
                {
                    if (c.Markers[i].Equals(OwnMarker[0]))
                    {
                        c.Markers[i].Reset(c, true);
                        removed++;
                        break;
                    }
                }
            });
            EffectGroup e = new EffectGroup("Robin arrow scaling", Effect.allstat, (float)removed*0.03f, -1, true, false, this);
            e.Give(this, true);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
            Character[] teammates = GetCharacters(true, -1, true);
            OwnMarker[0].Give(teammates[Rnd.R(teammates.Length)]);
        }
        public override void SkillTwo()
        {
            byte arrows = (byte)GetCharacters(true, -1).Sum(c => c.Markers.Count(m => m == OwnMarker[0]));
            DMG dmg = new DMG(PhysicalAttack[0]*0.8f, MagicalAttack[0]*0.8f, Punctual[0], MagicalKnowledge[0], DMGDealt+arrows*0.07f, AttackType.Skill);
            foreach (Character c in GetCharacters(false, 2)) c.Defense(this, dmg);
        }
    }

    class Zoro : Character
    {
         
        public Zoro() 
        {
            Name = "Zoro";
            S1T = "Megtámad 1 ellenfelet 120% M-erővel";
            S2T = "Megtámad 1 ellenfelet 80% M-erővel és 30%-os bleedinget ad neki 2 körig";
            SpecialT = "100-ig töltheti a charge-át, charge% eséllyel a támadása crit-el, ha critel akkor 110-charge%-al nő a sebzése és a charge 0 lesz, ha nem critel akkor a charge +10";
            charge = new Charge(this, 100);
            init(320, 0, 3, 12, 4, 1, 1.1f, 1, 1);
        }

        public override void SpecialTechnique(object arg)
        {
            DMG dmg = arg as DMG;
            bool crit = Rnd.R(0, 100) < charge.State[0];
            if (crit)
            {
                dmg.dmgD += Converter.ConvertingToByte(1.1f - charge.State[0] / 100f);
                charge.Reset();
            }
            else charge.Load(10);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0] * 1.2f, MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            SpecialTechnique(dmg);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0] * 0.8f, MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            SpecialTechnique(dmg);
            Character enemy = GetCharacters(false, 1)[0];
            ushort damage = enemy.Defense(this, dmg)[0];
            OverTime over = new OverTime(this, "Bleeding", damage * 0.3f, 2, true, OverTimeType.Bleeding, false);
            over.Give(enemy);
            
        }
    }

    class Time : Character
    {
         
        Character? selected;
        bool attack = false;
        public Time()
        {
            Name = "Time";
            S1T = "Megtámad 1 ellenfelet 100% M-erővel és a Mágiaismerete nő 3%-ot";
            S2T = "Megtámad 1 ellenfelet 100% F-erővel és a pontossága nő 3%-ot";
            SpecialT = "Amikor megtámad valakit akkor a következő támadásának is az a karakter lesz a célpontja, ha 1 körig nem támad vagy a célpont időközben meghalna akkor hatályba lép a szokványos célpont választás";
            init(350, 10, 4, 10, 4, 1, 1, 1, 1);
        }

        public override void BeforeSkillUse(Skill used)
        {
            if (selected == null || selected.Hitpoints[0] <= 0) selected = GetCharacters(false, 1)[0];
            attack = true;
        }
        public override void StartOfTurn()
        {
            base.StartOfTurn();
            if (!attack) selected = null;
            attack = false;
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
            EffectGroup effect = new EffectGroup("Magical knowledge inc", Effect.mknow, 0.03f, -1, true, false, this);
            effect.Give(this);
        }

        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
            EffectGroup effect = new EffectGroup("Punctual inc", Effect.pdef, 0.03f, -1, true, false, this);
            effect.Give(this);
        }
    }

    class Dynamic : Character
    {
         
        public Dynamic()
        {
            Name = "Dynamic";
            S1T = "Megtámad 1 ellenfelet 120% M erővel és a sebzés 100%-át gyógyulja";
            S2T = "Megtámad 1 ellenfelet 100% F erővel és 1-es Explosion-t ad neki 1 körig";
            SpecialT = "A játék elején az összes csapattárs és ellenfél statisztikáinak átlagával növeii a sajátjait";
            TalentT = "A nagyobbik értékkel gyógyítja a társait: mágikus erő&védelem / fizikai erő&védelem (cooldown 5, stack 1)";
            init(50, 0, 0, 0, 0, 0, 0, 0, 0);
            talent = new Talent(this, 5, 1);
        }

        public override void StartOfGame()
        {
            string[] stats = { "MaxHitpoints", "MagicalAttack", "MagicalDefense", "MagicalKnowledge", "Punctual", "PhysicalAttack", "PhysicalDefense", "ManaSensitivity", "Immun" };
            this.MaxHitpoints[0] = 0; this.MaxHitpoints[1] = 0;
            List<Character> list = new List<Character>();
            GetCharacters(false, -1, true).ToList().ForEach(x => list.Add(x));
            GetCharacters(true, -1, true).ToList().ForEach(x => list.Add(x));
            list.RemoveAll(x=>x.Name == this.Name);
            foreach (string stat in stats)
            {
                foreach (Character c in list)
                {
                    link.ChangePropertyValue(this, stat, link.GetPropertyValue(c, stat, 1), Operator.plus, 1);
                }
                link.ChangePropertyValue(this, stat, list.Count, Operator.divide, 1);
                link.SetPropertyValue(this, stat, link.GetPropertyValue(this, stat, 1), 0);
            }
            this.MaxHitpoints[0] += 50;
            this.MaxHitpoints[1] += 50;
            Hitpoints[0] = MaxHitpoints[0];
            Hitpoints[1] = MaxHitpoints[1];
        }
        public override void Talent()
        {
            float value;
            HealingType t;
            if (PhysicalAttack[0] + PhysicalDefense[0] > MagicalDefense[0] + MagicalAttack[0])
            {
                value = PhysicalAttack[0] + PhysicalDefense[0];
                t = HealingType.physi;
            }
            else
            {
                value = MagicalDefense[0] + MagicalAttack[0];
                t = HealingType.magic;
            }
            Healing heal = new Healing(t, value, this);
            GetCharacters(true, -1, true).ToList().ForEach(t => t.Healing(heal));
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0]*1.2f, MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            Healing(new Healing(HealingType.none, GetCharacters(false, 1)[0].Defense(this, dmg)[0], this));
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            enemy.Defense(this, dmg);
            OverTime over = new OverTime(this, "Explosion", 1, 1, false, OverTimeType.Explosion, false);
            over.Give(enemy);
        }
    }

    class Gravity : Character
    {
         
        public Gravity()
        {
            Name = "Gravity";
            S1T = "Kioszt minden ellenfélnek 3%-os Fall DoT hatást 3 körig";
            S2T = "Megtámad 1 ellenfelet a célpont maxéletének 1%-ával (fizikailag)";
            SpecialT = "Minden képesség használat előtt kioszt 1 Weight jelölőt 1 csapattagnak, ha önmagának akkor azonnal törli és gyógyul minden aktív Weight jelölő után 75% F erővel és a többi kiosztott Weight jelölőt is törli (a hatásukat megadja)";
            init(270, 10, 3, 7, 5, 1, 1, 1, 1.5f);
            OwnMarker = new Marker[]
            {
                new Marker("Weight", this, 1, 0, null, new OverTime(this, "Over regenerate", 0.1f, 1, true, OverTimeType.OverRegenerate, false), 3)
            };
        }

        public override void BeforeSkillUse(Skill used)
        {
            Character[] teammates = GetCharacters(true, -1, true);
            Character target = teammates[Rnd.R(0, teammates.Length)];
            OwnMarker[0].Give(target);
            if (target == this)
            {
                int count = Marker.Count(this, OwnMarker[0]);
                OwnMarker[0].Reset(this);
                Healing heal = new Healing(HealingType.physi, Converter.ConvertingToByte(count * 0.75f * PhysicalAttack[0]), this);
                Healing(heal);
                teammates.ToList().ForEach(c =>
                {
                    if (c.Markers.Count(m => m == OwnMarker[0]) > 0)
                        c.Markers.Where(m => m == OwnMarker[0]).ToList().ForEach(m => m.Reset(c));
                });
            }
        }

        public override void SkillOne()
        {
            GetCharacters(false, -1, true).ToList().ForEach(enemy =>
            {
                OverTime over = new OverTime(this, "Fall", 0.03f, 3, true, OverTimeType.Fall, false);
                over.Give(enemy);
            });
        }

        public override void SkillTwo()
        {
            Character enemy = GetCharacters(false, 1)[0];
            DMG dmg = new DMG(DMGType.physical, enemy.MaxHitpoints[0] * 0.01f, Punctual[0], DMGDealt, AttackType.Skill);
            enemy.Defense(this, dmg);
        }
    }

    class Sacrifice : Character
    {
         
        public Sacrifice()
        {
            Name = "Sacrifice";
            S1T = "Megtámad 1 ellenfelet 125% F erővel";
            S2T = "Megtámad 1 ellenfelet 80% F erővel és 15%-os Bleeding-et ad 4 körig";
            SpecialT = "Minden kör elején kap egy Sword jelölőt (A képességei sebzése nő 10%-ot mindegyik után)";
            TalentT = "Töröl maximum 10 sword jelölőt és gyógyítja mindegyik után magát 35% F erővel (ha 10 vagy kevesebb Sword jelölő van rajta akkor mindet törli) (cooldown: nincs)";
            init(340, 13, 5, 0, 1, 1, 1, 1.05f, 1.05f);
            talent = new Talent(this, 0, 1);
            OwnMarker = new Marker[]
            {
                new Marker("Sword", this, 1, 0, null, null, -1)
            };
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            if (Marker.Count(this, OwnMarker[0]) < 10)
            OwnMarker[0].Give(this);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0] * 1.25f, Punctual[0], DMGDealt+Markers.Count(m => m.Equals(OwnMarker[0]))*0.1f, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0] * 0.8f, Punctual[0], DMGDealt + Markers.Count(m => m.Equals(OwnMarker[0])) * 0.1f, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            ushort value = enemy.Defense(this, dmg)[0];
            OverTime ot = new OverTime(this, "Bleeding", value, 4, true, OverTimeType.Bleeding, false);
            ot.Give(enemy);
        }
        public override void Talent()
        {
            int count = 10;
            for (int i = Markers.Count - 1; i > -1; i--) 
            {
                if (Markers[i].Equals(OwnMarker[0]))
                {
                    Markers[i].Reset(this);
                    count--;
                }
                if (Markers.Count(m => m == OwnMarker[0]) == 0 || count == 0)
                    break;
            }
            Healing heal = new Healing(HealingType.physi, Converter.ConvertingToByte((10 - count) * 0.35f * PhysicalAttack[0]), this);
            Healing(heal);
        }
    }

    class Shard : Character
    {
         
        public Shard()
        {
            Name = "Shard";
            S1T = "Megtámad 1-3 ellenfelet (minden +célpont -1 essential) 100% F erővel";
            S2T = "Megtámad 1 ellenfelet 110% F erővel és a sebzés 20-60%-ával gyógyul fizikailag (minden +20% -1 essential)";
            SpecialT = "Maximum 5 essential-ja lehet, ez játék elején teli van, ha képességet használ akkor a lehetséges legmagasabb fokozatot használja, kör végén annyi essential-t tölt vissza ahányan a pályán élnek -> Ha nem tölt maxra gyógyul 50% F erővel, ha feltölt 25% F erővel gyógyul";
            init(360, 12, 2, 0, 0, 0.6f, 0.7f, 1.3f, 1.3f);
            OwnMarker = new Marker[]
            {
                new Marker("Essential", this, 1, 0, null, null, -1)
            };
        }

        public override void EndOfTurn()
        {
            int giveable = Math.Min(5-Markers.Count(m => OwnMarker[0].Equals(m)), GetCharacters(false, -1).Length + GetCharacters(true, -1).Length - 1);
        }

        public override void SkillOne()
        {
            Character[] targets = GetCharacters(false, -1);
            int targetnum = Math.Min(3, targets.Length);
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            for (int i = 0; i < OwnMarker[0].Remove(targetnum, this)+1; i++)
                targets[i].Defense(this, dmg);
        }

        public override void SkillTwo()
        {
            float lifesteal = 0.2f + 0.2f * OwnMarker[0].Remove(2, this);
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.1f, Punctual[0], DMGDealt, AttackType.Skill);
            ushort value = GetCharacters(false, 1)[0].Defense(this, dmg)[0];
            Healing heal = new Healing(HealingType.physi, Converter.ConvertingToByte(value * lifesteal), this);
            Healing(heal);
        }
    }

    class Raven : Character
    {
         
        public Raven()
        {
            Name = "Raven";
            S1T = "Megtámad 1 ellenfelet 100% F erővel";
            S2T = "Megtámad 1 ellenfelet 60% F erővel és 1-es Explosion-t ad neki 1 körig";
            SpecialT = "Minden képessége ad a 1 random ellenfélnek wing jelölőt, a célponton lévő wing jelölők felrobbanak és a célpont pontosságának 15%-ával sebez";
            init(410, 15, 3, 0, 0, 1, 1, 1, 1.3f);
            OwnMarker = new Marker[]
            {
                new Marker("Wing", this, 1, 0.15f, c=>c.Punctual[0], null, null, 3)
            };
        }

        public override void SpecialTechnique(object arg)
        {
            Character enemy = arg as Character;
            OwnMarker[0].Remove(-1, enemy);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            enemy.Defense(this, dmg);
            SpecialTechnique(enemy);
        }

        public override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0] * 0.6f, Punctual[0], DMGDealt, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            enemy.Defense(this, dmg);
            SpecialTechnique(enemy);
            OverTime ot = new OverTime(this, "Explosion", 1, 1, false, OverTimeType.Explosion, false);
            ot.Give(enemy);
        }
    }

    class Berserker : Character
    {
         
        byte lastmodifier = 0;
        public Berserker()
        {
            Name = "Berserker";
            S1T = "Megtámad 1 ellenfelet 100% M&F erővel";
            S2T = "Megtámad 1 ellenfelet annak M&F védelmének duplájával (külön-külön)";
            SpecialT = "Minden % elvesztett élete után nő a sebződés 1%-ot és a sebzése is 1%-ot";
            init(410, 10, 4, 10, 4, 1.25f, 1, 1, 1.25f);
        }

        public override void AfterSkillUse(Skill used) => SpecialTechnique();
        public override void AfterSelfHealed(Healing heal) => SpecialTechnique();
        public override void AfterSelfGetDMG(Character attacker, DMG dmg, short taked) => SpecialTechnique();

        public override void SpecialTechnique()
        {
            int modifier = (int)((1 - (float)Hitpoints[0] / MaxHitpoints[0]) * 100);
            if (lastmodifier > modifier)
                modifier -= lastmodifier;
            else if (lastmodifier < modifier) modifier = modifier - lastmodifier;
            DMGResistance += (float)modifier / 100;
            DMGDealt += (float)modifier / 100;
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }

        public override void SkillTwo()
        {
            Character enemy = GetCharacters(false, 1)[0];
            DMG dmg = new DMG(enemy.PhysicalDefense[0]*2, enemy.MagicalDefense[0] * 2, Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            enemy.Defense(this, dmg);
        }
    }

    class Rat : Character
    {
         
        public Rat() 
        {
            Name = "Rat";
            S1T = "Megtámad 1 ellenfelet 150% M&F erővel";
            S2T = "Megtámad 1 ellenfelet 100% M&F erővel és töröl róla 1 buffot";
            SpecialT = "Amikor kap 1 buffot vagy töröl 1 ellenfélről buffot akkor nő a fizikai ereje és maximum élete 3%-ot 8 körig";
            init(210, 8, 4, 3, 2, 1, 1, 1.3f, 1.25f);
        }

        public override void AfterSelfGetEffect(EffectsBasic effect, bool gived)
        {
            if (effect.GetType() == typeof(EffectGroup))
            {
                EffectGroup group = (EffectGroup)effect;
                if (group.positive) SpecialTechnique();
            }
        }

        public override void SpecialTechnique()
        {
            EffectGroup e = new EffectGroup("Rat buff inc", new Effect[] { Effect.patk, Effect.maxhp }, new float[] { 0.03f, 0.03f }, 8, true, false, this);
            e.Give(this, true);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0] * 1.5f, MagicalAttack[0] * 1.5f, Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }

        public override void SkillTwo()
        {
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            enemy.Defense(this, dmg);
            if (enemy.effects.Count(e => e.positive) > 0)
            {
                EffectGroup group = enemy.effects.First(e => e.positive) as EffectGroup;
                group.Reset(enemy);
                SpecialTechnique();
            }
        }
    }

    class Trap : Character
    {
         
        public Trap() 
        {
            Name = "Trap";
            S1T = "Megtámad 1 ellenfelet 130% F erővel és Glass debuffot ad neki 2 körre (törölhető)";
            S2T = "Megtámad 3 ellenfelet 100% F erővel és gyógyul a sebzése 10%-ával";
            SpecialT = "A támadásának alap értéke nő a célpontjának megnövelt fizikai védelmével";
            init(260, 9, 4, 0, 2, 1, 1.5f, 0.9f, 1.3f);
        }

        public override void BeforeEnemyGetDMG(Character attacker, Character enemy, DMG dmg)
        {
            if (attacker == this)
                dmg.physical += Math.Max(enemy.PhysicalDefense[0] - enemy.PhysicalDefense[1], 0);
        }

        public override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0] * 1.3f, Punctual[0], DMGDealt, AttackType.Skill);
            Character enemy = GetCharacters(false, 1)[0];
            enemy.Defense(this, dmg);
            EffectGroup effect = new EffectGroup("Glass", new Effect[] { Effect.taunt, Effect.dmgR }, new float[] { 0, 0.3f }, 2, false, true, this);
            effect.Give(enemy);
        }

        public override void SkillTwo()
        {
            ushort value = 0;
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            foreach (Character character in GetCharacters(false, -1))
                value += character.Defense(this, dmg)[0];
            Healing heal = new Healing(HealingType.physi, Converter.ConvertingToByte(value * 0.1f), this);
            Healing(heal);
        }
    }
}