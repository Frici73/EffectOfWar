from core.CharacterMain import BaseCharacter
from core.EffectBase import EffectsGroup
from core.Enums import BasicEffect, ComplexEffects, DoTEffects, HoTEffects, Skill, StateEffect

from .CharacterMain import *

class Lightning(BaseCharacter):
    def __init__(self):
        super().__init__(3600000, 70, 50, 130, 60, 100, 120, 100, 75, 
                        "Lightning\nMegtámad 1 ellenfelet 125% F-erővel, majd 125% M-erővel", 
                        "Shock-Chain\nMegtámad 2 ellenfelet 130% M-erővel és előtte növeli a saját sebzés kiosztását 5%-al 4 körig", 
                        "", 
                        "Minden kör elején nő a Mágiaismerete 7%-ot (törölhető)",
                        "")

    def SkillOne(self):
        dmg:DMG = DMG(PhysicalPower=self.PhysicalAttack*1.25, Punctual=self.Punctual, DMGDealt=self.DamageDealt)
        dmg1:DMG = DMG(MagicalPower=self.MagicalAttack*1.25, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        target = self.GetTarget(False, count=1)
        self.Attack([dmg, dmg1], target)

    def SkillTwo(self):
        effect = EffectsGroup(self, "Damage Dealt+", (BasicEffect.DMGDealt, 5), Turn=4, identify="Buff")
        effect.Give(self)
        dmg = DMG(MagicalPower=self.MagicalAttack*1.30, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=2))

    def StartOfTurn(self):
        super().StartOfTurn()
        effect = EffectsGroup(self, "Magical Knowledge+", (BasicEffect.MagicalKnowledge, 7), identify="Buff")
        effect.Give(self)

class Breaker(BaseCharacter):
    def __init__(self):
        super().__init__(2600000, 170, 40, 100, 50, 80, 100, 80, 100, 
                        "Boom\nMegtámad 1 ellenfelet 120% F erővel", 
                        "Defenseless\nMegtámad 1 ellenfelet 100% M-erővel és csökkenti a teljes védelmét 3%-al 5 körig",
                        "",
                        "Minden képessége okozta sérülés nő 3%-ot a pályán lévő debuffok után", 
                        "")

    def SkillOne(self):
        dmg = DMG(PhysicalPower=self.PhysicalAttack*1.2, Punctual=self.Punctual, DMGDealt=self.DamageDealt+self.Unique())
        self.Attack([dmg], self.GetTarget(False, count=1))

    def SkillTwo(self):
        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt+self.Unique())
        effect = EffectsGroup(self, "Defense-related-", (BasicEffect.Defense, -3), Turn=5, identify="Debuff")
        target = self.GetTarget(False, count=1)
        self.Attack([dmg], target)
        effect.Give(target[0])

    def Unique(self):
        count = 0
        for hero in [*self.Teammates, *self.Enemies]:
            for effect in hero.Effects:
                if effect.Identify == "Debuff":
                    count += 1
        return count*3

class Reaper(BaseCharacter):
    def __init__(self):
        super().__init__(2400000, 0, 30, 120, 30, 100, 85, 100, 100, 
                        "Scythe of Death\nMegtámad 1 ellenfelet 100% M-erővel és a sebzés nő (ellenfél max hp / saját maxhp)-val", 
                        "Soul Eat: Megtámad minden ellenfelet 100% M-erővel és gyógyul a sebzés 25%-val", 
                        "", 
                        "Minden kör elején az random ellenfél M-erejét csökkenti 25%-al és a változás értékével növeli a sajátját 1 körig", 
                        "")

    def SkillOne(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt+(target[0].MaxHitPoints//self.MaxHitPoints))
        self.Attack([dmg], target)

    def SkillTwo(self):
        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        dealt = self.Attack([dmg], self.GetTarget(False))[0]
        HealManager(self, [self], Heal(HealType.Lifesteal, dealt))

    def StartOfTurn(self):
        super().StartOfTurn()
        target = self.GetTarget(False, TargetLogic.Random, 1, True)[0]
        effect = EffectsGroup(self, "Reaper Curse", (BasicEffect.MagicalAttack, -25), Turn=1, removable=False, identify="Debuff")
        effect.Give(target, -1)
        effect.SetCallbackExpire(self.Unique)
        self.MagicalAttack += int(target.MagicalAttack.Init*0.25)

    def Unique(self, owner:BaseCharacter, curse:EffectsGroup):
        self.MagicalAttack -= int(owner.MagicalAttack.Init*0.25)

class Mage(BaseCharacter):
    def __init__(self):
        super().__init__(3200000, 30, 30, 100, 50, 100, 150, 80, 80, 
                        "Explosion\nMegtámad 3 ellenfelet 100% M-erővel", 
                        "Curse Missile\nMinden ellenfélnek 15%-os Infect-et ad 1 körig és növeli a charge-át 25%-al", 
                        "", 
                        "Játék elején kap egy Mage Magic hatást ami minden rajta lévő buff után növeli minden mágikus tulajdonságát 2%-ot. Ha feltölt a charge-a kap 20%-os Magical+-t 2 körig ami nem törölhető", 
                        "")
        self.Charge = CheckedStatBlock(4)

    def AfterGetEffect(self, giver: BaseCharacter, effect: EffectsGroup):
        self.Unique()

    def EndOfTurn(self):
        super().EndOfTurn()
        self.Unique()

    def Unique(self):
        self.RemoveEffect(self, Effect="Mage Magic", chance=-1)
        buffs = 0
        for effect in self.Effects:
            if effect.Identify == "Buff":
                buffs += 1
        
        EffectsGroup(self, "Mage Magic", (BasicEffect(30), 2*buffs), removable=False, identify="Buff")

    def SkillOne(self):
        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=3))

    def SkillTwo(self):
        effect = EffectsGroup(self, "Infect", (BasicEffect(96), -15), Turn=1, identify="Debuff")
        for target in self.GetTarget(False, ignoreTaunt=True):
            effect.Give(target)
        self.Charge += 1 # type: ignore
        if self.Charge == 4:
            self.Charge -= 4 # type: ignore
            EffectsGroup(self, "Magical+", (BasicEffect(30), 20), Turn=4, removable=False, identify="Buff").Give(self, -1)

class Robin(BaseCharacter):
    def __init__(self):
        super().__init__(2200000, 80, 40, 0, 20, 85, 85, 85, 85,
                        "Give to the Poor\nMegtámad 1 ellenfelet 3-szor 100% F-erővel és ad 2 random társának 1 nyíl jelölőt", 
                        "Collect the losted arrows\nMegtámad 2 ellenfelet 80% F-erővel és a sebzést növeli 7%-kal minden csapattagokon lévő nyíl jelölő után", 
                        "", 
                        "A játék elején minden csapattag kap 3-3 Arrow jelölőt, minden eltávolított jelölő után Robin összes statisztikája nő 3%-al, minden kör elején minden társáról töröl 1-1 Arrow jelölőt", 
                        "")

    def giveArrow(self, target:BaseCharacter):
        effect = EffectsGroup(self, "Arrow", removable=False, identify="Marker")
        effect.SetCallbackRemove(self.Unique)

    def Unique(self, owner:BaseCharacter, remover:BaseCharacter, effect:EffectsGroup):
        EffectsGroup(self, "All Stat+", (BasicEffect.ALL, 3), removable=False, identify="Buff")

    def StartOfGame(self):
        for mate in self.GetTarget(True):
            for i in range(3):
                self.giveArrow(mate)

    def StartOfTurn(self):
        super().StartOfTurn()
        for mate in self.GetTarget(True):
            mate.RemoveEffect(self, 1, "Arrow", -1)

    def SkillOne(self):
        for target in self.GetTarget(True, TargetLogic.Random, 2):
            self.giveArrow(target)

        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt, hit=3)
        self.Attack([dmg], self.GetTarget(False))

    def SkillTwo(self):
        c = sum(Count(mate.Effects, "Arrow") for mate in self.GetTarget(True))*7
        dmg = DMG(self.PhysicalAttack*0.8, self.Punctual, DMGDealt=self.DamageDealt+c)
        self.Attack([dmg], self.GetTarget(False, count=2))

class Zoro(BaseCharacter):
    def __init__(self):
        super().__init__(3200000, 140, 30, 0, 40, 100, 110, 100, 100,
                        "Slash\nMegtámad 1 ellenfelet 100% F-erővel", 
                        "Magical Slash\nMegtámad 1 ellenfelet 70% F-erővel és 30%-os Bleeding-et ad neki 3 körig", 
                        "", 
                        "100-ig töltheti a charge-át, charge% eséllyel a támadása crit-el, ha critel akkor 110-charge%-al nő a sebzése és a charge 0 lesz, ha nem critel akkor a charge +10", 
                        "")
        self.Charge = CheckedStatBlock(100)

    def Unique(self) -> int:
        if randint(1, 100) <= self.Charge:
            v = self.Charge.Value
            self.Charge.Reset()
            return 110-v
        self.Charge += 10
        return 0

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt+self.Unique())
        self.Attack([dmg], self.GetTarget(False, count=1))

    def SkillTwo(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(self.PhysicalAttack*0.7, self.Punctual, DMGDealt=self.DamageDealt)
        dealt = self.Attack([dmg], target)[0]
        EffectsGroup(self, "Bleeding", (DoTEffects.Bleeding, int(dealt*0.3)), Turn=3, identify="DoT").Give(target[0])

class Time(BaseCharacter):
    def __init__(self):
        super().__init__(3500000, 100, 40, 100, 40, 100, 100, 100, 100,
                        "to the Future\nMegtámad 1 ellenfelet 100% F-erővel és nő a Pontossága 2%-ot (nem törölhető)", 
                        "to the Past\nMegtámad 1 ellenfelet 100% M-erővel és nő a Mágiaismerete 2%-ot (nem törölhető)", 
                        "", 
                        "A következő támadása azt támadja meg kötelezően akit utoljára támadott, kivéve, ha az utolsó körben nem támadott", 
                        "")
        
        self.last:tuple[BaseCharacter, int]|None

    def target(self):
        if self.last == None:
            self.last = (self.GetTarget(False, count=1)[0], 2)
        return [self.last[0]]

    def EndOfTurn(self):
        super().EndOfTurn()
        if self.last != None:
            if self.last[1] == 1:
                self.last = None
            else:
                self.last = (self.last[0], self.last[1]-1)

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.target())
        EffectsGroup(self, "Punctual+", (BasicEffect.Punctual, 2), removable=False, identify="Buff")

    def SkillTwo(self):
        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.target())
        EffectsGroup(self, "Magical Knowledge+", (BasicEffect.MagicalKnowledge, 2), removable=False, identify="Buff")

class Dynamic(BaseCharacter):
    def __init__(self):
        super().__init__(500000, 70, 20, 70, 20, 25, 25, 25, 25,
                        "Dynamic Magic\nMegtámad 1 ellenfelet 120% M erővel és a sebzés 100%-át gyógyulja", 
                        "Dynamic Punch\nMegtámad 1 ellenfelet 100% F erővel és 1-es Explosion-t ad neki 1 körig", 
                        "", 
                        "A játék elején az összes csapattárs és ellenfél statisztikáinak átlagával növeli a sajátjait", 
                        "A nagyobbik értékkel gyógyítja a csapatot: mágikus erő&védelem / fizikai erő&védelem (cooldown 5, stack 1)")
        
        self.TalentState = Talent(5, 1, 1)

    def StartOfGame(self):
        for stat in ["MaxHitPoints", "PhysicalAttack", "PhysicalDefense", "MagicalAttack", "MagicalDefense",
                     "ImmuneSystem", "Punctual", "ManaSensitive", "MagicalKnowledge"]:
            avg = 0
            heros = self.Teammates + self.Enemies
            for hero in heros:
                avg += getattr(hero, stat)
            avg //= len(heros)
            setattr(self, stat, StatBlock(avg))

    def Talent(self):
        heal:Heal
        if self.MagicalAttack+self.MagicalDefense > self.PhysicalAttack+self.PhysicalDefense:
            heal = Heal(HealType.Ability, Magical=self.MagicalAttack+self.MagicalDefense, HealMultiple=self.HealMultiple)
        else:
            heal = Heal(HealType.Ability, self.PhysicalAttack+self.PhysicalDefense, HealMultiple=self.HealMultiple)
        self.Heal(heal, self.GetTarget(True))

    def SkillOne(self):
        dmg = DMG(MagicalPower=self.MagicalAttack*1.2, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        dealt = self.Attack([dmg], self.GetTarget(False, count=1))[0]
        heal = Heal(HealType.Lifesteal, dealt)
        self.Heal(heal, [self])

    def SkillTwo(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        target = self.GetTarget(False, count=1)
        self.Attack([dmg], target)
        EffectsGroup(self, "Explosion", (DoTEffects.Explosion, 1), Turn=1, identify="DoT")

class Gravity(BaseCharacter):
    def __init__(self):
        super().__init__(2700000, 0, 30, 70, 50, 100, 100, 100, 150,
                        "Gravity of Me\nKioszt minden ellenfélnek 3% Fall hatást 3 körig", 
                        "Impact\nMegtámad 1 ellenfelet a célpont életének 0.05%-ával Fizikailag", 
                        "", 
                        "Minden képesség használat előtt kioszt 1 Weight jelölőt valakinek a csapatban. Ha önmagának adja akkor minden társáról és magáról törli, minden törölt után gyógyul 100% M-erővel. A weight jelölő 1%-al növel minden mágikus tulajdonságot", 
                        "")

    def BeforeSkillUse(self, UsedSkill: Skill):
        effect = EffectsGroup(self, "Weight", (BasicEffect.Magical, 1), identify="Marker")
        effect.SetCallbackRemove(self.Unique)
        effect.Give(self.GetTarget(True, TargetLogic.Random, 1)[0])

    def AfterGetEffect(self, giver: BaseCharacter, effect: EffectsGroup):
        if effect.Name != "Weight":
            return
        for mate in self.GetTarget(True):
            mate.RemoveEffect(self, chance=-1)

    def Unique(self, owner:BaseCharacter, remover:BaseCharacter, effect:EffectsGroup):
        heal = Heal(HealType.Ability, Magical=self.MagicalAttack, HealMultiple=self.HealMultiple)
        self.Heal(heal, [self])

    def SkillOne(self):
        effect = EffectsGroup(self, "Fall", (DoTEffects.Fall, 3), Turn=3, identify="DoT")
        for enemy in self.GetTarget(False, ignoreTaunt=True):
            effect.Give(enemy)

    def SkillTwo(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(target[0].MaxHitPoints*0.05, self.Punctual, DMGDealt=self.DamageDealt)
        self.Attack([dmg], target)

class Sacrifice(BaseCharacter):
    def __init__(self):
        super().__init__(3400000, 130, 50, 0, 10, 100, 100, 105, 105,
                        "Blood Sword\nMegtámad 1 ellenfelet 125% F-erővel", 
                        "Deadly Slash\nMegtámad 1 ellenfelet 80% F-erővel és 15%-os Bleeding-et ad neki 4 körig", 
                        "", 
                        "Minden kör elején kap egy Sword jelölőt (A képességei sebzése nő 10%-ot mindegyik után)", 
                        "Töröl maximum 10 sword jelölőt és gyógyítja mindegyik után magát 35% F erővel (ha 10 vagy kevesebb Sword jelölő van rajta akkor mindet törli) (cooldown: nincs)")
        self.TalentState = Talent(0, 1, 1)

    def StartOfTurn(self):
        super().StartOfTurn()
        e = EffectsGroup(self, "Sword", (BasicEffect.DMGDealt, 10), removable=False, identify="Marker")
        e.SetCallbackRemove(self.Unique)

    def Unique(self, o:BaseCharacter, r:BaseCharacter, e:EffectsGroup):
        self.Heal(Heal(HealType.Ability, Physical=self.PhysicalAttack*0.35, HealMultiple=self.HealMultiple), [self])

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack*1.25, self.Punctual, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=1))

    def SkillTwo(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(self.PhysicalAttack*0.8, self.Punctual, DMGDealt=self.DamageDealt)
        dealt = self.Attack([dmg], target)[0]
        EffectsGroup(self, "Bleeding", (DoTEffects.Bleeding, int(dealt*0.15)), Turn=4, identify="DoT")

    def Talent(self):
        heal = Heal(HealType.Ability, self.PhysicalAttack*0.35*self.RemoveEffect(self, 10, "Sword"), HealMultiple=self.HealMultiple)
        self.Heal(heal, [self])

class Shard(BaseCharacter):
    def __init__(self):
        super().__init__(3600000, 120, 20, 0, 0, 60, 70, 130, 130,
                        "Piercing Glass\nMegtámad 1-3 ellenfelet (minden +célpont -1 essential) 100% F erővel", 
                        "Repair stab\nMegtámad 1 ellenfelet 110% F erővel és a sebzés 20-60%-ával gyógyul fizikailag (minden +20% -1 essential)", 
                        "", 
                        "Maximum 5 essential-ja lehet, ez játék elején teli van, ha képességet használ akkor a lehetséges legmagasabb fokozatot használja, kör végén annyi essential-t tölt vissza ahányan a pályán élnek -> Ha nem tölt maximumra gyógyul 50% F erővel, ha feltölt 25% F erővel gyógyul", 
                        "")
        self.Charge = CheckedStatBlock(5)
        self.Charge += 5

    def EndOfTurn(self):
        super().EndOfTurn()
        self.Charge += len(self.Teammates)+len(self.Enemies)-1

    def SkillOne(self):
        count = min(self.Charge.Value, 2)+1 if self.Charge.Value < 3 else 3
        self.Charge -= (count-1)
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=count))

    def SkillTwo(self):
        steal = 20 if self.Charge == 0 else min(self.Charge.Value, 3)*20
        self.Charge -= (steal//20)-1
        dmg = DMG(self.PhysicalAttack*1.1, self.Punctual, DMGDealt=self.DamageDealt)
        dealt = self.Attack([dmg], self.GetTarget(False, count=1))[0]
        heal = Heal(HealType.Lifesteal, dealt//100*steal)
        self.Heal(heal, [self])

class Raven(BaseCharacter):
    def __init__(self):
        super().__init__(4100000, 150, 30, 0, 0, 100, 100, 100, 130,
                        "Black Feather\nMegtámad 1 ellenfelet 100% F-erővel", 
                        "Explosive Feather\nMinden ellenfélen felrobbantja a Wing jelölőket", 
                        "", 
                        "Minden képesség kijátszás után a célpontjainak ad 1 Wing jelölőt", 
                        "")

    def Unique(self, target:BaseCharacter):
        effect = EffectsGroup(self, "Wing", removable=False, identify="Marker")
        effect.SetCallbackRemove(self.Boom)
        effect.Give(target, -1)

    def Boom(self, owner:BaseCharacter, giver:BaseCharacter, e:EffectsGroup):
        dmg = DMG(owner.MaxHitPoints*0.03, AType=AttackType.Reflect)
        self.Attack([dmg], [owner])

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        target = self.GetTarget(False, count=1)
        self.Attack([dmg], target)
        self.Unique(target[0])

    def SkillTwo(self):
        for enemy in self.GetTarget(False, ignoreTaunt=True):
            enemy.RemoveEffect(self, chance=-1)

class Berserker(BaseCharacter):
    def __init__(self):
        super().__init__(4100000, 100, 40, 100, 40, 125, 100, 100, 125,
                        "Rage\nMegtámad 1 ellenfelet 125% M&F-erővel", 
                        "Cold Blooded\nMegtámad 1 ellenfelet 100% F-erővel és 40% F-védelem ignorálással", 
                        "", 
                        "Minden elvesztett % elvesztett élete után nő a sebzése 1%-ot és csökken a DMGRes-e 1%-ot", 
                        "")
        self.HitPoints.OnChange(self.Unique)

    def Unique(self, prev:int, new:int, stat:StatBlock):
        change = self.MaxHitPoints//new - self.MaxHitPoints//prev
        self.DamageDealt -= change
        self.DamageResistance += change

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack*1.25, self.Punctual, self.MagicalAttack*1.25, self.MagicalKnowledge, self.DamageDealt)
        self.Attack([dmg], self.GetTarget(False, count=1))

    def SkillTwo(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, IgnorePhysicalDefense=40)
        self.Attack([dmg], self.GetTarget(False, count=1))

class Rat(BaseCharacter):
    def __init__(self):
        super().__init__(2100000, 80, 40, 30, 20, 100, 100, 130, 125,
                        "Bite\nMegtámad egy ellenfelet 90% F-erővel 7-szer", 
                        "Steal the Cheese\nMegtámad 1 ellenfelet 300% M-erővel, ha sebez akkor töröl a célpontról 1 random buffot", 
                        "", 
                        "Amikor buffot kap vagy töröl nő a sebzése és a maxélete 2%-ot 8 körig (nem törölhető)", 
                        "")

    def Unique(self):
        EffectsGroup(self, "Rat buff", (BasicEffect(1025), 2), Turn=8, removable=False, identify="Buff").Give(self)

    def AfterGetEffect(self, giver: BaseCharacter, effect: EffectsGroup):
        self.Unique()

    def SkillOne(self):
        dmg = DMG(self.PhysicalAttack*0.9, self.Punctual, DMGDealt=self.DamageDealt, hit=7)
        self.Attack([dmg], self.GetTarget(False, count=1))

    def SkillTwo(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(MagicalPower=self.MagicalAttack*3, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        self.Attack([dmg], target)
        if target[0].RemoveEffect(self, 1, "Buff") == 1:
            self.Unique()

class Trap(BaseCharacter):
    def __init__(self):
        super().__init__(2600000, 90, 40, 0, 20, 100, 150, 90, 130,
                        "Bear Trap\nMegtámad 1 ellenfelet 110% F-erővel és 20%-os Glass hatást ad neki 1 körig", 
                        "Vampiric Bullet\nMegtámad 3 ellenfelet 100% F-erővel és gyógyul a sebzés 100%-ával", 
                        "", 
                        "A sebzése nő a célpont megnövelt fizikai védelmével egyenlően (+5% védelem = +5% DMGDealt)", 
                        "")

    def Unique(self, target:BaseCharacter):
        return int(max(target.PhysicalDefense.Init / target.PhysicalDefense, 0)*10)

    def SkillOne(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt+self.Unique(target[0]))
        self.Attack([dmg], target)
        EffectsGroup(self, "Glass", (StateEffect.Taunt, 0), (BasicEffect.DMGResistance, -20), Turn=1, identify="Debuff").Give(target[0])

    def SkillTwo(self):
        prev = 0
        dealt = 0
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        for target in self.GetTarget(False, count=3):
            prev += self.Unique(target)
            dmg.DMGDealt += prev
            dealt += self.Attack([dmg], [target])[0]
        self.Heal(Heal(HealType.Lifesteal, dealt), [self])

class Cowboy(BaseCharacter):
    def __init__(self):
        super().__init__(3100000, 115, 35, 0, 15, 80, 60, 110, 130,
                        "One Shot\nEgy töltényért sebez 1 ellenfélbe 100% F-erővel és 15%-os Bleedinget ad neki 1 körig", 
                        "Full Magazine\nMinden megmaradt töltényéből megtámad 1 ellenfelet 70% F-erővel", 
                        "", 
                        "6 tölténye van, ha az elfogy sleep-et kap 1 körig. Minden újratöltés után nő a pontossága 10%-ot", 
                        "")
        self.Charge = CheckedStatBlock(6)
        self.Charge.OnChange(self.Unique)

    def Unique(self, prev:int, new:int, s:StatBlock):
        if new == 0:
            self.Charge += 6
            EffectsGroup(self, "Sleep", (StateEffect.Sleep, 1), Turn=1, identify="Debuff").Give(self)
            self.Punctual += 10

    def SkillOne(self):
        target = self.GetTarget(False, count=1)
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt)
        dealt = int(self.Attack([dmg], target)[0]*0.15)
        EffectsGroup(self, "Bleeding", (DoTEffects.Bleeding, dealt), Turn=1, identify="DoT").Give(target[0])

    def SkillTwo(self):
        dmg = DMG(self.PhysicalAttack, self.Punctual, DMGDealt=self.DamageDealt, hit=self.Charge.Value)
        self.Charge -= self.Charge
        self.Attack([dmg], self.GetTarget(False, count=1))

class Sorcerer(BaseCharacter):
    def __init__(self):
        super().__init__(3500000, 0, 30, 90, 50, 110, 120, 90, 90,
                        "Magical Meteor\nMegtámad minden ellenfelet 100% M-erővel és gyógyul a sebzés 15%-val", 
                        "Dark pact\nMinden ellenfélre rak 5%-os Life Pact-et 2 körig", 
                        "", 
                        "Játék elején kap egy végleges Sorcerer Curse-t az egész csapata, amely növeli 15%-kal az alapstatisztikákat, de 7%-os Fall hatást kapnak (buff)", 
                        "")

    def SkillOne(self):
        dmg = DMG(MagicalPower=self.MagicalAttack, MagicalKnowledge=self.MagicalKnowledge, DMGDealt=self.DamageDealt)
        dealt = int(self.Attack([dmg], self.GetTarget(False, count=3))[0]*0.15)
        self.Heal(Heal(HealType.Lifesteal, dealt), [self])

    def SkillTwo(self):
        effect = EffectsGroup(self, "Life Pact", (ComplexEffects.LifePact, 5), Turn=2, identify="Debuff")
        for target in self.GetTarget(False, ignoreTaunt=True):
            effect.Give(target)

    def StartOfGame(self):
        effect = EffectsGroup(self, "Sorcerer Curse", (BasicEffect.ALL, 15), (DoTEffects.Fall, 7), removable=False, identify="Buff")
        for target in self.GetTarget(True):
            effect.Give(target)
