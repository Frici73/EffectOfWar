using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            type = HType.ranger;
            subclass = new Subclass[] { Subclass.Buffer, Subclass.Burst};
            init(360, 7, 5, 13, 6, 1, 1.2f, 1, 0.75f);
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            EffectGroup effect = new EffectGroup("Magicalknowledge increase", Effect.mknow, 0.07f, -1, true, false, this);
            effect.Give(this);
        }

        internal override void SkillOne()
        {
            DMG dmg = new DMG(PhysicalAttack[0]*1.25f, MagicalAttack[0]*1.25f, Punctual[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }

        internal override void SkillTwo()
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
            type = HType.ranger;
            subclass = new Subclass[] {Subclass.Debuffer};
            init(260, 17, 4, 10, 5, 0.8f, 1, 0.8f, 1);
        }

        internal override void SpecialTechnique()
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

        internal override void SkillOne()
        {
            SpecialTechnique();
            DMG dmg = new DMG(DMGType.physical, PhysicalAttack[0]*1.2f, Punctual[0], DMGDealt+0.3f*count, AttackType.Skill);
            GetCharacters(false, 1)[0].Defense(this, dmg);
        }
        internal override void SkillTwo()
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
            type = HType.ranger;
            subclass = new Subclass[] { Subclass.Stealer, Subclass.TankKiller };
            init(240, 0, 3, 12, 3, 1, 0.85f, 1, 1);
        }

        public override void StartOfTurn()
        {
            base.StartOfTurn();
            if (selected != null) MagicalAttack[0] -= Convert.ToByte(selected.MagicalAttack[1] * 0.25f);
            Random r = new Random();
            Character[] enemys = GetCharacters(false, -1, true);
            selected = enemys[r.Next(enemys.Length)];
            EffectGroup effect = new EffectGroup("Reaper magical steal", Effect.matk, 0.25f, 1, true, false, this);
            effect.Give(selected, true);
            MagicalAttack[0] += Convert.ToByte(selected.MagicalAttack[1] * 0.25f);
        }

        internal override void SkillOne()
        {
            Character enemy = GetCharacters(false, 1)[0];
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt + enemy.MaxHitpoints[0] / MaxHitpoints[0], AttackType.Skill);
            enemy.Defense(this, dmg);
        }
        internal override void SkillTwo()
        {
            DMG dmg = new DMG(DMGType.magical, MagicalAttack[0], MagicalKnowledge[0], DMGDealt, AttackType.Skill);
            foreach (Character enemy in GetCharacters(false, -1))
            {
                Healing heal = new Healing(HealingType.magic, enemy.Defense(this, dmg)[0], this);
                Healing(heal);
            }
        }
    }
}