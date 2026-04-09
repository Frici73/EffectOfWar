using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace EffectOfWar
{
    class Chaos : Boss
    {
        public Chaos()
        {
            Name = "Chaos";
            S1T = "Megtámad 1 ellenfelet a célpont maxéletének 20%-ával (mágikusan)";
            S2T = "Megtámad 2 ellenfelet 200% M erővel és 100% F erővel";
            S3T = "Megtámad minden ellenfelet 130% M erővel, minden hp-ban sérülő célpont után nő minden stata 5%-ot";
            SpecialT = "Minden kör elején 90% eséllyel: 1 ellenfél kettő különböző statját felcseréli (kivéve hp), 10% eséllyel: az egyik ellenfél maximum életét az aktuális életére csökkenti";
            ChanceSystem = "Az ellenfél frontliner hp-ja > 60%: k1 70%, k2 15%, k3 15%; Ha nem -> Az ellenfél átlagos maxéletének felénél több életük maradt akkor: k1 20%, k2 25%, k3 55%; Ha nem -> kettő ellenfél van csak életben: k1 0%, k2 80%, k3 20%; Ha nem -> k1 0%, k2 50%, k3 50%";
            init(2000, 4, 4, 12, 3, 1, 1.5f, 1, 1);
            subclass = new Subclass[] { Subclass.Unknown };
        }

        public override void StartOfTurn()
        {
            Random r = new Random();
            Character[] enemyTeam = GetCharacters(false, 4, false);
            Character enemy = enemyTeam[r.Next(enemyTeam.Length)];
            if (r.Next(10) == 9)
            {
                List<Array> stats = new List<Array>
                {
                enemy.MagicalAttack,
                enemy.MagicalDefense,
                enemy.ManaSensitivity,
                enemy.MagicalKnowledge,

                enemy.PhysicalAttack,
                enemy.PhysicalDefense,
                enemy.Punctual,
                enemy.Immun
                };
                int i = r.Next(stats.Count);
                int j;

                do
                {
                    j = r.Next(stats.Count);
                } while (i == j);

                object? temp = stats[i].GetValue(0);
                stats[i].SetValue(stats[j].GetValue(0), 0);
                stats[j].SetValue(temp, 0);
            }
            else enemy.MaxHitpoints[0] = enemy.Hitpoints[0];
                base.StartOfTurn();
        }

        internal override void SkillOne()
        {
            Character enemy = GetCharacters(false, 1)[0];
            DMG dmg = new DMG(DMGType.magical, enemy.MaxHitpoints[0]*0.2f, MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            enemy.Defense(this, dmg);
        }

        internal override void SkillTwo()
        {
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(true, 2))
            {
                enemy.Defense(this, dmg);
            }
        }

        internal override void SkillThree() 
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0]*1.3f, MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach(Character enemy in GetCharacters(true, 4))
            {
                ushort[] taked = enemy.Defense(this, dmg);
                if (taked[0] > 0)
                {
                    EffectGroup e = new EffectGroup("All stat increase", Effect.allstat, 0.05f, -1, true, false, this);
                    e.Give(this);
                }
            }
        }

        internal override Skill RNDSKill()
        {
            float s1 = 0; float s2 = 0;
            Character chars = GetCharacters(false, 1, true)[0];
            if (chars.Hitpoints[0] > chars.MaxHitpoints[0]*0.6f)
            {
                s1 = 0.7f; s2 = 0.15f;
            }
            else if (GetCharacters(false, 4, true).Average(e => e.Hitpoints[0])/ GetCharacters(false, 4, true).Average(e => e.MaxHitpoints[0]) > 0.5)
            {
                s1 = 0.2f; s2 = 0.25f;
            }
            else if (GetCharacters(false, 4, true).Length == 2)
            {
                s2 = 0.8f;
            }
            else
            {
                s2 = 0.5f;
            }
            return Select(s1, s2);
        }
    }

    class Fate : Boss
    {
        public Fate()
        {
            Name = "Fate";
            S1T = "gyógyul 150% M erővel";
            S2T = "kap countert 100% M erővel töltve 3 körig és kap 7%-os M erő növelést 2 körig";
            S3T = "töröl minden (de)buffot és countert a pályán, mindegyik után 15%-al többet fog sebezni, támad 100% M erővel minden ellenfelet";
            SpecialT = "Kör elején: - ha van rajta debuff: nő a mágikus és fizikai védelme 50%-ot -ha van rajta counter: nő a mágikus ereje 25%-ot -ha van rajta buff: nő a dmg dealtje 10%-ot (a kövi kör elején törlődnek a hatások) + 10% eséllyel mikor megtámadják akkor befejezteti a kört";
            ChanceSystem = "80%+ hp: k1 10%, k2 45%, k3 45%; 60%+ hp: k1 17%, k2 43%, k3 40%; 40%+ hp: k1 17%, k2 50%, k3 33%; 20%+ hp: k1 33%, k2 40%, k3 27%; 0%+: k1 garantált, k2 20%, k3 80%";
            init(1750, 0, 5, 15, 5, 1, 1.05f, 1, 1.2f);
            subclass = new Subclass[] { Subclass.Unknown };
        }

        internal override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0]*1.5f, this);
            Healing(heal);
        }

        internal override void SkillTwo()
        {
            EffectGroup effect = new EffectGroup("Magical Attack increase", Effect.matk, 0.07f, 2, true, true, this);
            effect.Give(this);
            counter.Edit(1, 0, 0, 1, 2);
        }

        internal override void SkillThree() 
        {
            byte removed = 0;
            foreach (Character c in GetCharacters(false, 4, true))
            {
                removed += (byte)c.effects.Count(e=>e.Remove(c));
                if (c.counter.Reset()) removed++;
            }
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt+0.15f*removed, AttackType.Skill);
            foreach (Character c in GetCharacters(false, 4, true))
            {
                c.Defense(this, dmg);
            }
        }

        internal override Skill RNDSKill()
        {
            float s1 = 0; float s2 = 0;
            float hp = Hitpoints[0] / MaxHitpoints[0];
            if (hp >= 0.8) { s1 = 0.1f; s2 = 0.45f; }
            else if (hp >= 0.6) { s1 = 0.17f; s2 = 0.43f; }
            else if (hp >= 0.4) { s1 = 0.17f; s2 = 0.5f; }
            else if (hp >= 0.2) { s1 = 0.33f; s2 = 0.40f; }
            else { UseSkill(Skill.first); s2 = 0.2f; }
            return Select(s1, s2);
        }
    }

    class Werewolf : Boss
    {
        float inc = 0;
        public Werewolf()
        {
            Name = "Werewolf";
            S1T = "Megtámad 1 ellenfelet 150% F erővel és gyógyul a sebzés 50%-ával";
            S2T = "Megtámad 1 ellenfelet 125% F erővel és a sebzése nő minden halott után 15%-al és minden élő után 25%-al";
            S3T = "Megtámad minden ellenfelet 250% F erővel";
            SpecialT = "A sebzése nő 1%-ot minden elveszett % élete után";
            ChanceSystem = "Ha minden ellenfél élete 75%+: k1 10%, k2 15%, k3 75%; Ha nem -> ha a saját élete kevesebb mint 40%: k1 60%, k2 30%, k3 10%; ha nem -> ha a saját élete nagyobb mint 40%: k1 30%, k2 60%, k3 10%";
            init(1300, 15, 4, 0, 4, 1, 1, 1.2f, 1);
            subclass = new Subclass[] { Subclass.Unknown };
        }

        internal override void BeforeSkillUse(Skill used)
        {
            inc = Hitpoints[0] / MaxHitpoints[0];
        }

        internal override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.5f, Punctual[0], DMGDealt+inc, AttackType.Skill);
            Healing(new Healing(HealingType.physi, GetCharacters(false, 1)[0].Defense(this, dmg)[0], this));
        }
        internal override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.25f, Punctual[0], DMGDealt+inc+link.DeadCharacters*0.15f+link.LiveCharacters*0.25f, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        internal override void SkillThree()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*2.5f, Punctual[0], DMGDealt + inc, AttackType.Skill);
            foreach (Character c in GetCharacters(false, 4)) c.Defense(this, dmg);
        }
        internal override Skill RNDSKill()
        {
            float s1 = 0; float s2 = 0;
            Character[] enemys = GetCharacters(false, 4, true);
            if (enemys.Count(c => c.Hitpoints[0] / c.MaxHitpoints[0] > 0.75f) == enemys.Length) 
            {
                s1 = 0.1f; s2 = 0.15f;
            }
            else if (Hitpoints[0] / MaxHitpoints[0] < 0.4f)
            {
                s1 = 0.6f; s2 = 0.3f;
            }
            else
            {
                s1 = 0.3f; s2 = 0.6f;
            }
            return Select(s1, s2);
        }
    }

    class Goblins : Boss
    {
        private byte round = 0;
        public Goblins(short maxhp=600)
        {
            Name = "Goblin";
            S1T = "Megtámad 1 ellenfelet 175% F erővel és a sebzése nő minden élő goblinnal 10%-ot";
            S2T = "Megtámad minden ellenfelet 100% F erővel és a sebzése nő minden célpont után 15%-al";
            S3T = "Gyógyít minden goblint 100% F erővel és nő az F erejük 5%-ot 5 körig";
            SpecialT = "Minden harmadik körben egy új goblin jelenik meg és a játék kezdetén 4 goblin él. (ha 1 gobli él 600, ha kevesebb mint 4 akkor 450, ha 4 vagy több akkor 300 élettel jelennek meg)";
            ChanceSystem = "True random -> k1, k2, k3 egyaránt 33%";
            init(maxhp, 13, 0, 0, 0, 1, 1, 1.75f, 1.5f);
            subclass = new Subclass[] { Subclass.Unknown };
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            round++;
            Character[] teammates = GetCharacters(true, 6, true);
            if (round != 2 || teammates[0] != this || teammates.Length == 6) return;
            round = 0;
            short hp = 0;
            if (teammates.Length == 1) hp = 600;
            if (teammates.Length < 4) hp = 450;
            else hp = 300;
            link.Add(new Goblins(hp), link.GetTeam(this));
        }

        internal override void SkillOne()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.75f, Punctual[0], DMGDealt+0.1f*GetCharacters(true, 6).Length, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        internal override void SkillTwo()
        {
            Character[] enemys = GetCharacters(true, 4);
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt+enemys.Length*0.15f, AttackType.Skill);
            foreach (Character c in enemys) 
            {
                c.Defense(this, dmg);
            }
        }
        internal override void SkillThree()
        {
            EffectGroup effect = new EffectGroup("Phyisical Attack Increase", Effect.patk, 0.05f, 5, true, true, this);
            Healing heal = new Healing(HealingType.physi, PhysicalAttack[0], this);
            foreach (Character c in GetCharacters(true, 6, true))
            {
                c.Healing(heal);
            }
        }
    }

    class Vampire : Boss
    {
        public Vampire()
        {
            Name = "Vampire";
            S1T = "Megtámad 1 ellenfelet 150% M & F erővel és nő a sebzése minden blood jelölő után ami az ellenfeleken van 15%-al";
            S2T = "Megtámad minden ellenfelet 100% M erővel és nő a mágiaismerete 30%-ot minden blood jelölő után ami az aktuális célponton van";
            S3T = "Kioszt minden ellenfélre 2-2 blood jelölőt 7 körig és minden most kiosztott blood jelölő után gyógyul 100% M erővel";
            SpecialT = "Minden támadás után kap 5%-os maxélet növelő hatást végtelen körig, de törölhető";
            ChanceSystem = "Ha minden ellenfelen legalább 3 blood jelölő van: k1 40%, k2 40%, k3 20% Ha nem -> Ha 2 vagy kevesebb ellenfél él: k1 60%, k2 20%, k3 20% Ha nem -> Ha kevesebb életen van mint 50%: k1 25%, k2 25%, k3 50% Ha nem -> minden 33%";
            init(1100, 6, 4, 10, 3, 1.2f, 1.5f, 1.2f, 1);
            subclass = new Subclass[] { Subclass.Unknown };
            OverTime ot = new OverTime(this, "Explosion", 5, 1, true, OverTimeType.Explosion, false);
            OwnMarker = new Marker[] { new Marker("Blood", this, 1, 0, null, ot, 7) };
        }

        internal override void AfterSkillUse(Skill used)
        {
            if (used == Skill.third) return;
            EffectGroup effect = new EffectGroup("Maxhp increase", Effect.maxhp, 0.05f, -1, true, true, this);
            effect.Give(this);
        }

        internal override void SkillOne()
        {
            Marker mlink = OwnMarker[0];
            byte count = 0;
            GetCharacters(false, 4).ToList().ForEach(c => count += (byte)c.Markers.Count(m => m.name == mlink.name && m.id == mlink.id));
            DMG dmg = new DMG(PhysicalAttack[0]*1.5f, MagicalAttack[0]*1.5f, Punctual[0], MagicalAttack[0], DMGDealt+0.15f*count, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        internal override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            Marker mlink = OwnMarker[0];
            foreach (Character c in GetCharacters(false, 4))
            {
                byte count = (byte)c.Markers.Count(m => m.name == mlink.name && m.id == mlink.id);
                dmg.magicalknowledge += count * 0.3f;
                c.Defense(this, dmg);
                dmg.magicalknowledge -= count * 0.3f;
            }
        }
        internal override void SkillThree()
        {
            foreach (Character c in GetCharacters(false, 4, true))
            {
                for (int i = 0; i < 2; i++)
                {
                    OwnMarker[0].Give(c);
                    Healing(new Healing(HealingType.magic, MagicalAttack[0], this));
                }
            }
        }
        internal override Skill RNDSKill()
        {
            Character[] enemys = GetCharacters(false, 4, true);
            Marker mlink = OwnMarker[0];
            float s1 = 0.33f; float s2 = 0.33f;
            if (!enemys.Any(c=>c.Markers.Count(m => m.name == mlink.name && m.id == mlink.id) < 3)) 
            {
                s1 = 0.4f; s2 = 0.4f;
            }
            else if (enemys.Length <= 2)
            {
                s1 = 0.6f; s2 = 0.2f;
            }
            else if (Hitpoints[0] / MaxHitpoints[0] < 0.5)
            {
                s1 = 0.25f; s2 = 0.25f;
            }
            return Select(s1, s2);
        }
    }

    class Moon : Boss
    {
        public Moon()
        {
            Name = "Moon";
            S1T = "Megtámad 1 ellenfelet 150% F erővel / Megtámad 1 ellenfelet 150% M erővel / Megtámad 1 ellenfelet 140% M és F erővel";
            S2T = "Kap 100% F erő countert 2 körig / Kap 120% M erő countert 2 körig / Kap 120% M & F erő countert 2 körig és itt + 30% Mágiaismeret  & pontosság";
            S3T = "Megtámad minden ellenfelet 100% F erővel /100% M erővel / 250% M&F erővel az aktuális célponton csökken a sebzés az aktuális célpont slot*15%-al";
            SpecialT = "Kap 100% -os Reincarnation hatást a játék elején és még 1-szer miután 1-szer meghal, mindig mikor meghal mode-ot vált, alapból 1-esen van";
            ChanceSystem = "Ha nincs rajta counter: k1 40% ,k2 50%, k3 10% HA van -> Ha legalább 3 ellenfél él k1 25%, k2 15%, k3 60% Ha nem -> k1 50%, k2 10% k3 40%";
            init(800, 12, 4, 14, 4, 1, 1.3f, 1, 1.3f);
            shift = new Shift(this, 3, ShiftMode.line, 0);
        }

        internal override void SkillOne()
        {
            DMG dmg;
            switch (shift.ActiveMode)
            {
                case 1: dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.5f, Punctual[0], DMGDealt, AttackType.Skill); 
                    break;
                case 2: dmg = new DMG(DMGType.physical, MagicalAttack[0] * 1.5f, MagicalKnowledge[0], DMGDealt, AttackType.Skill); ; 
                    break;
                case 3: dmg = new DMG(PhysicalAttack[0]*1.4f, MagicalAttack[0]*1.4f, Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
                    break;
                default: throw new Exception("Moon mode is invalid");
            }
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        internal override void SkillTwo()
        {
            switch (shift.ActiveMode) 
            {
                case 1: counter.Edit(0, 1, 1, 0, 2); break;
                case 2: counter.Edit(1, 0, 0, 1, 2); break;
                case 3: counter.Edit(2.5f, 2.5f, 1, 1, 2); counter.Increase(0, 0, 0.3f, 0.3f); break;
                default: throw new Exception("Moon mode is invalid");
            }
        }
        internal override void SkillThree()
        {
            DMG dmg;
            switch (shift.ActiveMode) 
            {
                case 1: dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill); break;
                case 2: dmg = new DMG(DMGType.physical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill); break;
                case 3: dmg = new DMG(PhysicalAttack[0]*2.5f, MagicalAttack[0]*2.5f, Punctual[0], MagicalKnowledge[0], DMGDealt-0.15f, AttackType.Skill); break;
                default: throw new Exception("Moon mode is invalid");
            }
            var prev = 0;
            foreach (Character enemy in GetCharacters(false, 4))
            {
                if (enemy.Slot != prev)
                {
                    prev = enemy.Slot;
                    dmg.dmgD -= 0.15f;
                }
                enemy.Defense(this, dmg);
            }
        }
        internal override Skill RNDSKill()
        {
            float s1 = 0; float s2 = 0;
            if (counter.turn == 0) { s1 = 0.4f; s2 = 0.5f; }
            else if (GetCharacters(false, 4, true).Length > 2) { s1 = 0.25f; s2 = 0.15f; }
            else { s1 = 0.5f; s2 = 0.1f; }
            return Select(s1, s2);
        }
        public override void StartOfGame()
        {
            EffectGroup effect = new EffectGroup("Reincarnation", Effect.reincarnation, 1, -1, true, false, this);
            effect.Give(this);
        }
        internal override void OnSelfDeath(Character killer)
        {
            link.Add(this, link.GetTeam(this));
            StartOfGame();
            shift.Shifting();
        }
    }

    class Solmir : Boss
    {
        byte used = 0;
        bool forskill = false;
        public Solmir()
        {
            Name = "Solmir";
            S1T = "Megtámad minden ellenfelet 120% M erővel";
            S2T = "Megtámad 1 ellenfelet 100% F erővel és 10%-os Fall hatást tesz rá 2 körig";
            S3T = "Gyógyul 100% M erővel";
            SpecialT = "Játék elején 15 Manával indul (ennyi a maximum) és kör elején mindig 5-öt visszatölt. Minden védekezéskor 1-2 db-t felhasznál, hogy 10-20%-al kevesebbet sérüljön. Képesség használatkor felhasznál 1-3-at, hogy a képességeihez használt erő %-ot megnövelje 20%-al (pl 1 manáért a k3 100% helyett 120% M erővel gyógyítja";
            ChanceSystem = "Ha a manája 10+: k1 40%, k2 30%, k3 30%, talent 5%, Ha a manája 5+: k1 20%, k2 40%, k3 40%, talent 10%, Ha a manája 0+: k1 5%, k2 45%, k3 50%, talent 80%";
            TalentT = "Beállítja a manáját 4-re (stack 2, cooldown 3)";
            init(1600, 7, 4, 15, 6, 1.2f, 1.3f, 1, 1.15f);
            talent = new Talent(this, 3, 2);
            OwnMarker = new Marker[] { new Marker("Mana", this, 1, 0, null, null, -1)};
        }

        internal override void SkillOne()
        {
            forskill = true;
            SpecialTechnique();
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0]*(1.2f+used*0.2f), MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(false, 4)) enemy.Defense(this, dmg);
        }
        internal override void SkillTwo()
        {
            forskill = true;
            SpecialTechnique();
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0] * (1 + used * 0.2f), Punctual[0], DMGDealt, AttackType.Skill);
            OverTime ot = new OverTime(this, "Fall", 0.1f, 2, true, OverTimeType.Fall, false);
            Character enemy = GetCharacters(false, 4)[0];
            enemy.Defense(this, dmg);
            ot.Give(enemy);
        }

        internal override void SkillThree()
        {
            forskill = true;
            SpecialTechnique();
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0]*(1+used*0.2f), this);
            Healing(heal);
        }

        internal override ushort[] Defense(Character attacker, DMG dmg)
        {
            forskill = false;
            SpecialTechnique();
            DMGTaken += 0.1f * used;
            var back = base.Defense(attacker, dmg);
            DMGTaken -= 0.1f * used;
            return back;
        }

        internal override void SpecialTechnique()
        {
            Random r = new Random();
            byte count = (byte)Markers.Count(m => m == OwnMarker[0]);
            if (count == 0) used = 0;
            else
            {
                byte removed = 0;
                if (forskill) used = (byte)r.Next(1, (int)Math.Min((byte)3, count));
                else used = (byte)r.Next(1, (int)Math.Min((byte)2, count));
                foreach (Marker m in Markers.ToList()) if (m == OwnMarker[0] && removed < used) Markers.Remove(m);
            }
        }

        public override void StartOfGame()
        {
            for (int i = 0; i < 15; i++) OwnMarker[0].Give(this);
        }
        internal override void Talent()
        {
            Markers.RemoveAll(m=>m == OwnMarker[0]);
            for (int i = 0; i < 4; i++) OwnMarker[0].Give(this);
        }
    }

    class Tarantula : Boss
    {
        public Tarantula() 
        {
            Name = "Tarantula";
            S1T = "Megtámadja a legkisebb életű ellenfelet 120% F&M erővel";
            S2T = "Megtámad minden ellenfelet 100% M&F erővel és mindenkinek 50%-os Poison hatást ad örökké (törölhető)";
            S3T = "Megtámad minden ellenfelet 100% F erővel és töröl róluk minden buffot";
            SpecialT = "Célpont választáshoz nem figyeli a taunt-ot, taunt-oló ellenfelekbe 50%-al többet sebez";
            init(1450, 7, 4, 7, 4, 1, 1.2f, 1, 1.2f);
        }

        internal override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0]*1.2f, MagicalAttack[0]*1.2f, Punctual[0], MagicalAttack[0], DMGDealt, AttackType.Skill);
            Character enemy = GetCharacters(false, 1, true, TargetingMode.lowestHp)[0];
            if (enemy.effects.Any(e => e.Have(Effect.taunt))) dmg.dmgD += 0.5f;
            enemy.Defense(this, dmg);
        }
        internal override void SkillTwo()
        {
            OverTime poison = new OverTime(this, "Poison", 0.5f, -1, true, OverTimeType.Poison, false);
            DMG dmg = new DMG(PhysicalAttack[0], MagicalAttack[0], Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(false, 4, true))
            {
                float bonus = (enemy.effects.Any(e => e.Have(Effect.taunt))) ? 0.5f : 0;
                dmg.dmgD += bonus;
                enemy.Defense(this, dmg);
                dmg.dmgD -= bonus;
                poison.Give(enemy);
            }
        }
        internal override void SkillThree()
        {
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0], Punctual[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(false, 4, true))
            {
                float bonus = (enemy.effects.Any(e => e.Have(Effect.taunt))) ? 0.5f : 0;
                dmg.dmgD += bonus;
                enemy.Defense(this, dmg);
                dmg.dmgD -= bonus;
                enemy.effects.ForEach(e => e.Remove(this));
            }
        }
    }
}