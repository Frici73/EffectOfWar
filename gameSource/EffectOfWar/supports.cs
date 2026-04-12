using System;
using System.Collections.Generic;
using System.Linq;
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
            type = HType.support;
            subclass = new Subclass[] { Subclass.Medic, Subclass.Buffer, Subclass.Sustain };
            init(280, 3, 3, 13, 8, 1.3f, 1.1f, 0.9f, 1f);
        }

        internal override void SkillOne()
        {
            Healing heal = new Healing(HealingType.magic, MagicalAttack[0] * 0.85f, this);
            foreach (var c in GetCharacters(true, 4, true))
            {
                c.Healing(heal);
            }
        }

        internal override void SkillTwo()
        {
            EffectGroup inc = new EffectGroup("Manasens", Effect.manasens, 0.15f, 3, true, true, this);
            foreach (var c in GetCharacters(true, 4, true))
            {
                inc.Give(c);
            }
        }

        internal override void AfterSkillUse(Skill used)
        {
            OverTime hot = new OverTime(this, "Recover", PhysicalAttack[0], 3, true, OverTimeType.Recover, true);
            hot.Give(this);
        }
    }

    
}