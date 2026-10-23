from .CharacterMain import *

class Barrier(BaseCharacter):
    def __init__(self):
        super().__init__(5250000, 90, 80, 0, 50, 100, 80, 100, 100,
                        "Border\nAd minden társának 75% F-erő pajzsot", 
                        "Hard fall\nMegtámad 1 ellenfelet 150% F-erővel", 
                        "", 
                        "Minden védekezés után nő az Immunrendszere 3%-ot (véglegesen)", 
                        "")

    def AfterAttacked(self, Attacker: BaseCharacter, Dmg: list[DMG]):
        self.ImmuneSystem += 3

    def SkillOne(self):
        ShieldManager(self, self.GetTarget(True), self.PhysicalAttack*0.75, ShieldType.Normal)

    def SkillTwo(self):
        dmg = DMG(self.PhysicalAttack*1.5, self.Punctual, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=1))

class Guardian(BaseCharacter):
    def __init__(self):
        super().__init__(4300000, 100, 70, 60, 70, 80, 120, 80, 120,
                        "Selfish slash\nMegtámad minden ellenfelet 100% M-erővel háromszor és a sebzés mértékét megkapja pajzsként", 
                        "Selfless Defense\n25%-os Tanky hatást kap 3 körig", 
                        "", 
                        "Minden képesség használat után ad a többi társának 500% F-erő pajzsot", 
                        "")

    def AfterSkillUse(self, UsedSkill: Skill):
        ShieldManager(self, [self], self.PhysicalAttack*5, ShieldType.Normal)

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        dealt = self.Attack([dmg], self.GetTarget(False))[0]
        ShieldManager(self, [self], dealt, ShieldType.Stealed)

    def SkillTwo(self):
        EffectsGroup(self, "Tanky", (StateEffect.Taunt, 0), (BasicEffect.DMGResistance, 25), Turn=3, identify="Buff").Give(self)

class Bulldozer(BaseCharacter):
    def __init__(self):
        super().__init__(5800000, 120, 20, 0, 20, 100, 100, 100, 100,
                        "Horns\nMegtámad 2 ellenfelet 100% F-erővel", 
                        "Red\n40%-os Tanky hatást kap 1 körig", 
                        "", 
                        "Minden kör elején nő az Immunrendszere 5%-ot véglegesen, nem törölhető", 
                        "")

    def StartOfTurn(self):
        super().StartOfTurn()
        self.ImmuneSystem += 5

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=2))

    def SkillTwo(self):
        EffectsGroup(self, "Tanky", (StateEffect.Taunt, 0), (BasicEffect.DMGResistance, 40), Turn=1, identify="Buff").Give(self)

class Emerald(BaseCharacter):
    def __init__(self):
        super().__init__(4800000, 30, 40, 100, 30, 125, 130, 100, 100,
                        "Magical Deflect\nAd magának 130% M-erő Counter-t 1 körig + 30% Mágiaismeret", 
                        "Emerald Armor\nAd a csapatának 75% M-erő pajzsot", 
                        "", 
                        "Ha egy csapattag vagy ő maga meghal akkor minden társa kap 300% M-erő pajzsot és minden ellenfél 300% M-erővel sérül", 
                        "")

    def OnTeammateDead(self, teammate: BaseCharacter, attacker: BaseCharacter):
        self.OnSelfDead(attacker)

    def OnSelfDead(self, attacker: BaseCharacter):
        dmg = DMG(MagicalPower=self.MagicalAttack*3, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, ignoreTaunt=True))

        ShieldManager(self, self.GetTarget(True), self.MagicalAttack*3, ShieldType.Normal)

    def SkillOne(self):
        self.Counter.Set(1, 1.3, MagicalKnowledgeIncrease=30)

    def SkillTwo(self):
        ShieldManager(self, [self], self.MagicalAttack*0.75, ShieldType.Normal)

class Arthur_King(BaseCharacter):
    def __init__(self):
        super().__init__(4800000, 95, 60, 0, 20, 100, 100, 110, 90,
                        "King of Wisdom\nAd magának 1,5% maxélet pajzsot és Taunt-ol 2 körig", 
                        "Excalibur\nMegtámad 1 ellenfelet 5-ször 85% F-erővel és a sebzés 60%-át megkapja pajzsként", 
                        "", 
                        "Minden körben nő a maxhp-ja 3%-ot és a pontossága 1%-ot, végleges és nem törölhető", 
                        "")

    def StartOfTurn(self):
        super().StartOfTurn()
        EffectsGroup(self, "Arthur Passive", (BasicEffect.MaxHP, 5), (BasicEffect.Punctual, 1), removable=False, identify="Buff").Give(self)

    def SkillOne(self):
        ShieldManager(self, [self], self.MaxHitPoints*0.015, ShieldType.Stealed)

    def SkillTwo(self):
        dmg = DMG(self.PhysicalAttack*0.85, self.Punctual, DMGDealt=self.DamageDealt, hit=5)
        dealt = int(self.Attack([dmg], self.GetTarget(False, count=1))[0]*0.6)
        ShieldManager(self, [self], dealt, ShieldType.Normal)

class Fulmare(BaseCharacter):
    def __init__(self):
        super().__init__(4400000, 40, 40, 80, 60, 100, 100, 100, 100,
                        "Sleight of Phantom\nVálaszt 1 ellenfelet, ha van rajta pajzs ellopja a pajzs 50%-át. A kiválasztott ellenfelet 100% M-erővel megtámadja.", 
                        "Phantom Touch\nMegtámad 1 ellenfelet 100% F-erővel és 100% F-védelem ignorálással.", 
                        "", 
                        "Az előző köri pajzsából 30%-ot áthelyezz az új körbe", 
                        "")

    def StartOfTurn(self):
        self.Shield[1] += int(self.Shield[0]*0.3)
        return super().StartOfTurn()

    def SkillOne(self):
        target = self.GetTarget(False, count=1)[0]
        ShieldManager(self, [self], target.StealShield(0.5), ShieldType.Stealed)

        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        self.Attack([dmg], [target])

    def SkillTwo(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, IgnorePhysicalDefense=100)
        self.Attack([dmg], self.GetTarget(False, count=1))

class Phase(BaseCharacter):
    def __init__(self):
        super().__init__(4200000, 60, 60, 60, 60, 100, 100, 100, 100,
                        "Phase Shield\nAd minden társának 150% F-erő pajzsot / Ad magának 200% F-erő pajzsot", 
                        "Universal Trick\nMegtámad 1 ellenfelet kétszer 200% F-erővel / Növeli minden társának a DMGRes-ét 15%-al 2 körig", 
                        "", 
                        "Minden képesség után Shift-et vált (körkörösen)", 
                        "3-as stackje van, 4 körönként tölt 1-et, játék elején maxon van, a talent hatására Phase-t vált és gyógyul 100% F erővel")

        self.Shift = Shift(2, 0, ShiftMode.Circle)
        self.TalentState = Talent(4, 3, 3)

    def SkillOne(self):
        match self.Shift:
            case 1: ShieldManager(self, self.GetTarget(True), self.PhysicalAttack*1.5, ShieldType.Normal)
            case 2: ShieldManager(self, [self], self.PhysicalAttack*2, ShieldType.Normal)

    def SkillTwo(self):
        match self.Shift:
            case 1: 
                dmg=DMG(self.PhysicalAttack*2, self.Punctual, DMGDealt=self.DamageDealt)
                self.Attack([dmg], self.GetTarget(False, count=1))
            case 2: 
                effect = EffectsGroup(self, "DMG Resistance+", (BasicEffect.DMGResistance, 15), Turn=2, identify="Buff")
                for teammate in self.GetTarget(True):
                    effect.Give(teammate)

    def AfterSkillUse(self, UsedSkill: Skill):
        self.Shift.Shift()

    def Talent(self):
        heal = Heal(HealType.Ability, self.PhysicalAttack, HealMultiple=self.HealMultiple)
        self.Heal(heal, [self])

class Trash(BaseCharacter):
    def __init__(self):
        super().__init__(5300000, 150, 50, 30, 50, 115, 100, 115, 100,
                        "Mouse Horde\nMegtámad 1 ellenfelet 3-szor 75% F-erővel és Taunt hatást tesz rá 1 körig", 
                        "Trash Can\nGyógyul az elveszett életének 2,5%-val és Taunt-ol 1 körig", 
                        "", 
                        "Minden képesség használat után töröl magáról egy DoT hatást", 
                        "Minden debuff lejáratát a csapaton felgyorsítja 1 körrel (DoT-ra nem hat) (4 körös cooldown, 1-es stack)")

        self.TalentState = Talent(4, 0, 1)

    def AfterSkillUse(self, UsedSkill: Skill):
        self.RemoveEffect(self, 1, DoTEffects.ALL)

    def SkillOne(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(self.PhysicalAttack*0.75, self.Punctual, DMGDealt=self.DamageDealt, hit=3)
        self.Attack([dmg], target)
        EffectsGroup(self, "Taunt", (StateEffect.Taunt, 0), Turn=1, identify="Debuff").Give(target[0])

    def SkillTwo(self):
        EffectsGroup(self, "Taunt", (StateEffect.Taunt, 0), Turn=1, identify="Buff").Give(self)
        heal = Heal(HealType.Ability, int((self.MaxHitPoints-self.HitPoints)*0.025), HealMultiple=self.HealMultiple)
        self.Heal(heal, [self])

    def Talent(self):
        for teammate in self.GetTarget(True):
            for effect in teammate.Effects:
                effect.OverTime(-1, teammate)
