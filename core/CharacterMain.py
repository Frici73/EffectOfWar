lazy from .Enums import HeroType, TargetLogic, Skill, HealType, ShieldType, AttackType, ShiftMode, CounterType, BasicEffect, ComplexEffects, DoTEffects, HoTEffects, StateEffect

lazy from collections.abc import Callable
lazy from random import shuffle, randint
lazy from os.path import join
lazy from . import BASEPATH
lazy from .EffectBase import Contains, MaxValue, EffectsGroup, Count
lazy from .EventManager import AttackManager, HealManager, ShieldManager, EffectManager, DMG, Heal
lazy from .ClassProperty import classproperty


Team1:list[BaseCharacter] = []
Team2:list[BaseCharacter] = []
Team1Focused:int|None = None
Team2Focused:int|None = None


def ContainsFocused(targets:list[BaseCharacter]):
    for char in targets:
        if char.Focused:
            return char
    return None


class StatBlock:
    def __init__(self, InitialValue:int, onChange:Callable[[int, int, StatBlock], None]|None=None):
        """
        onChange: Three parameter: previous value, new value, self
        """
        self.__INITIAL:int = InitialValue
        self.__current:int = InitialValue
        self._onChange:Callable[[int, int, StatBlock], None] = onChange if onChange != None else self.__chocking

    @staticmethod
    def __chocking(f:int, s:int, t:StatBlock): # this function is a chocking the callback if the callback is not a function
        return
    
    def OnChange(self, onChange:Callable[[int, int, StatBlock], None]):
        """
        onChange: Three parameter: previous value, new value, self
        """
        self._onChange = onChange

    def Reset(self, newInit:int|None=None):
        if newInit != None:
            self.__INITIAL = newInit # type: ignore
        self.__current = self.__INITIAL

    def __eq__(self, value:int|StatBlock|object):
        if type(value) == StatBlock:
            return self.Value == value.Value
        return self.Value == value

    def __ne__(self, value:int|StatBlock|object):
        return not self==value

    def __lt__(self, value:int|StatBlock):
        if type(value) == StatBlock:
            return self.Value < value.Value
        return self.Value < int(value)

    def __le__(self, other:int|StatBlock):
        return self < other or self == other

    def __gt__(self, value:int|StatBlock):
        if type(value) == StatBlock:
            return self.Value > value.Value
        return self.Value > int(value)

    def __ge__(self, other:int|StatBlock):
        return self > other or self == other

    def __add__(self, other:int|StatBlock):
        if type(other) == StatBlock:
            return self.Value + other.Value
        return self.Value + int(other)

    def __radd__(self, other:int|StatBlock):
        return self+other

    def __iadd__(self, other:int|StatBlock):
        new = self + other
        self._onChange(self.Value, new, self)
        self.__current = new
        return self

    def __sub__(self, other:int|StatBlock):
        if type(other) == StatBlock:
            return self.Value - other.Value
        return self.Value - int(other)

    def __rsub__(self, other:int|StatBlock):
        return abs(self-other)

    def __isub__(self, other:int|StatBlock):
        new = self - other
        self._onChange(self.Value, new, self)
        self.__current = new
        return self

    def __mul__(self, other:int|float|StatBlock):
        if type(other) == StatBlock:
            return self.Value * other.Value
        elif type(other) == float:
            return int(self.Value*other)
        elif type(other) == int:
            return self.Value * other
        return 0

    def __rmul__(self, other:int|float|StatBlock):
        return self*other

    def __imul__(self, other:int|StatBlock|float):
        new = int(self*other)
        self._onChange(self.Value, new, self)
        self.__current = new
        return self

    def __truediv__(self, other:int|float|StatBlock):
        if type(other) == StatBlock:
            return self.Value / other.Value
        elif type(other) == float:
            return self.Value / other
        return self.Value / int(other)

    def __rtruediv__(self, other:int|float|StatBlock):
        if type(other) == StatBlock:
            return other.Value / self.Value
        elif type(other) == float:
            return other / self.Value
        return other / self.Value

    def __itruediv__(self, other:int|StatBlock):
        new = int(self / other)
        self._onChange(self.Value, new, self)
        self.__current = new
        return self

    def __floordiv__(self, other:int|float|StatBlock):
        if type(other) == StatBlock:
            return self.Value // other.Value
        elif type(other) == float:
            return int(self.Value // other)
        return self.Value // int(other)

    def __rfloordiv__(self, other:int|float|StatBlock):
        if type(other) == StatBlock:
            return other.Value // self.Value
        elif type(other) == float:
            return int(other // self.Value)
        return other // self.Value

    def __int__(self):
        return int(self.__current)

    def __str__(self):
        return str(self.Value)

    @property
    def Value(self):
        return self.__current

    @Value.setter
    def Value(self, new:int):
        self._onChange(self.Value, new, self)
        self.__current = new

    @property
    def Init(self):
        return self.__INITIAL

class CheckedStatBlock(StatBlock):
    def __init__(self, InitialValue: int):
        super().__init__(InitialValue)
        self.Value = 0
        del self.OnChange

    @staticmethod
    def __chocking(prev: int, current: int, block: StatBlock):
        if block.Value > block.Init:
            block.Reset()

    def __isub__(self, other:int|StatBlock):
        new = max(self - other, 0)
        self._onChange(self.Value, new, self)
        self.__current = new
        return self

class Shift:
    def __init__(self, modeCount:int=1, cooldown:int=1, mode:ShiftMode=ShiftMode.Line):
        self.Phase = StatBlock(modeCount, self.__onChange) # from 1 to modeCount
        self.Phase -= (self.Phase.Init-1) # start value is 1
        self.mode = mode
        self.Cooldown = StatBlock(cooldown)

    def EndOfTurn(self):
        if self.Cooldown > 0:
            self.Cooldown -= 1

    def __onChange(self, prev:int, new:int, phase:StatBlock):
        if phase.Value > phase.Init:
            if self.mode == ShiftMode.Line:
                self.mode = ShiftMode.LineReversed
                phase -= 2
            if self.mode == ShiftMode.Circle: 
                phase -= phase.Init # Value - Init = 1 in my mind
        elif phase.Value == 0:
            self.mode = ShiftMode.Line 
            self.Phase += 1

    def Shift(self):
        if self.Cooldown == 0:
            self.Cooldown.Reset()
            if self.mode == ShiftMode.LineReversed:
                self.Phase -= 1
            else:
                self.Phase += 1

    def __eq__(self, value: object) -> bool:
        return self.mode == value


class Talent:
    def __init__(self, cooldown:int, StartStack:int, MaxStack:int=1):
        self.Cooldown = StatBlock(cooldown)
        self.Stack = StatBlock(MaxStack)
        if StartStack < MaxStack:
            self.Stack -= (MaxStack-StartStack)
        else:
            self.Cooldown -= cooldown

    def EndOfTurn(self):
        if self.Cooldown > 0:
            self.Cooldown -= 1

        if self.Cooldown == 0 and self.Stack < self.Stack.Init:
            self.Cooldown.Reset()
            self.Stack += 1

    def CanUse(self):
        if self.Stack > 0:
            self.Stack -= 1
            return True
        return False
        

class Counter:
    def __init__(self, Owner:BaseCharacter):
        self.Owner:BaseCharacter = Owner
        self.Turn:int = 0
        self.CType = CounterType.Counter
        self.Hit:int = 0

        # Increases in Percentage
        self.MagicalAttackIncrease:float = 0
        self.PhysicalAttackIncrease:float = 0
        self.PunctualIncrease:int = 0
        self.MagicalKnowledgeIncrease:int = 0
        self.DMGDealtIncrease:int = 0

    def Set(self, Turn:int=0, MagicalAttackIncrease:float=0, PhysicalAttackIncrease:float=0, PunctualIncrease:int=0, MagicalKnowledgeIncrease:int=0, DMGDealtIncrease:int=0, Type:CounterType=CounterType.Counter, Hit:int=1):
        self.MagicalAttackIncrease = MagicalAttackIncrease
        self.PhysicalAttackIncrease = PhysicalAttackIncrease
        self.PunctualIncrease = PunctualIncrease
        self.MagicalKnowledgeIncrease = MagicalKnowledgeIncrease
        self.DMGDealtIncrease = DMGDealtIncrease

        self.Turn = Turn
        self.CType = Type
        self.Hit = Hit

    def EndOfTurn(self):
        if self.Turn > 0:
            self.Turn -= 1
        if self.Turn == 0:
            self.Set()

    def Counter(self, Attacker:BaseCharacter):
        if self.Turn == 0:
            return
        dmg = DMG(self.Owner.PhysicalAttack*self.PhysicalAttackIncrease, self.Owner.Punctual+self.PunctualIncrease, self.Owner.MagicalAttack*self.MagicalAttackIncrease, self.Owner.MagicalKnowledge+self.MagicalKnowledgeIncrease, self.Hit, self.Owner.DamageDealt+self.DMGDealtIncrease, AType=AttackType.Counter)
        targets = None
        if self.CType == CounterType.Counter:
            targets = [Attacker]
        else:
            targets = Attacker.Teammates
        AttackManager(self.Owner, targets, [dmg])


class Reflect:
    def __init__(self):
        self.Percent = 0
        self.Turn = 0

    def Set(self, percent:int=0, turn:int=0):
        self.Percent = percent
        self.Turn = turn

    def Reflecting(self, TakedDMG:int, Attacker:BaseCharacter, Defender:BaseCharacter):
        if self.Turn == 0:
            return
        dmg = DMG(AType=AttackType.Reflect, PhysicalPower=int(TakedDMG*self.Percent))
        AttackManager(Defender, [Attacker], [dmg])

    def EndOfTurn(self):
        if self.Turn > 0:
            self.Turn -=1
        if self.Turn == 0:
            self.Percent = 0


class BaseCharacter:
    def __init__(self, HitPoints:int, PhysicalAttack:int, PhysicalDefense:int, MagicalAttack:int, MagicalDefense:int, ManaSensitive:int=100, MagicalKnowledge:int=100, ImmuneSystem:int=100, Punctual:int=100, SkillOne:str="", SkillTwo:str="", SkillThree:str="", Special:str="", Talent:str=""):
        # HP properties
        self.MaxHitPoints:StatBlock = StatBlock(HitPoints)
        self.HitPoints:StatBlock = StatBlock(HitPoints)
        self.Shield:list[int] = [0, 0] # previous turn / current turn

        # Physical properties
        self.PhysicalAttack:StatBlock = StatBlock(PhysicalAttack)
        self.PhysicalDefense:StatBlock = StatBlock(PhysicalDefense)
        self.ImmuneSystem:StatBlock = StatBlock(ImmuneSystem)
        self.Punctual:StatBlock = StatBlock(Punctual)

        # Magical properties
        self.MagicalAttack:StatBlock = StatBlock(MagicalAttack)
        self.MagicalDefense:StatBlock = StatBlock(MagicalDefense)
        self.ManaSensitive:StatBlock = StatBlock(ManaSensitive)
        self.MagicalKnowledge:StatBlock = StatBlock(MagicalKnowledge)

        # Multiply properties
        self.DamageDealt:StatBlock = StatBlock(100)
        self.DamageResistance:StatBlock = StatBlock(100)
        self.HealMultiple:StatBlock = StatBlock(100)
        self.Regeneration:StatBlock = StatBlock(1)

        # Immunities
        self.BuffImmunity:StatBlock = StatBlock(0)
        self.DebuffImmunity:StatBlock = StatBlock(0)
        self.DoTImmunity:StatBlock = StatBlock(0)
        self.HoTImmunity:StatBlock = StatBlock(0)

        # effects and recover data lists
        self.DMGFromLastHits:list[int] = []
        self.Effects:list[EffectsGroup] = []
        self.UsableSkills:CheckedStatBlock = CheckedStatBlock(1)

        self.SkillOneDesc:str = SkillOne
        self.SkillTwoDesc:str = SkillTwo
        self.SkillThreeDesc:str = SkillThree
        self.SpecialDesc:str = Special
        self.TalentDesc:str = Talent

        # special abilities
        self.Shift:Shift = None # type: ignore
        self.Charge:CheckedStatBlock = None # type: ignore
        self.TalentState:Talent = None # type: ignore

        # DMG dealing in the enemy's turn
        self.Counter:Counter = Counter(self)
        self.Reflect:Reflect = Reflect()

        # Battle Statistic
        self.TotalBuffing:int = 0
        self.TotalDebuffing:int = 0
        self.TotalShielding:int = 0
        self.TotalHealing:int = 0
        self.TotalDamageDealt:int = 0
        self.TotalDamageTaken:int = 0
        self.TotalRegeneration:int = 0
        self.KillCount:int = 0

        self.Identity:BaseCharacter = self # When a character copy an another character, the character make a fake version of the copied character and change the fake version identity to self else the Identity is the character

    @classproperty
    def Name(cls) -> str:
        return str(cls).split(".")[-1][:-2].replace("_", " ")

    @classproperty
    def Custom(cls):
        return cls.__module__.split(".")[-1] == "UserCharacters"

    @classproperty
    def HeroType(self):
        types = {"Warriors":"warrior", "Rangers":"ranger", "Supports":"support", "Bosses":"boss"}
        for type, DirName in types:
            if self.__class__.__module__.split(".")[-1] == type:
                return DirName
        return ""

    @classproperty
    def IMGPath(cls):
        SecDir = ""
        if cls.Custom:
            SecDir = "custom"
        else:
            SecDir = cls.HeroType
        return join(BASEPATH, "Resources", SecDir, cls.Name.replace(" ", ""))

    @property
    def Dead(self):
        return self.HitPoints < 0 and not Contains(self.Effects, StateEffect.AbsoluteOne)

    @property
    def HideUI(self):
        return self.Dead and not Contains(self.Effects, StateEffect.Untouchable)

    @property
    def Taunt(self):
        return Contains(self.Effects, StateEffect.Taunt)

    @property
    def Teammates(self):
        if self in Team1:
            return Team1
        return Team2

    @property
    def Enemies(self):
        if self in Team1:
            return Team2
        return Team1

    @property
    def Slot(self):
        return self.Teammates.index(self)

    @property
    def Focused(self):
        slot = self.Slot
        if self in Team1 and Team1Focused == slot:
            return True
        elif self in Team2 and Team2Focused == slot:
            return True
        return False

    def isTeammate(self, other:BaseCharacter):
        return other in self.Teammates

    # Skill related functions
    def BeforeSkillUse(self, UsedSkill:Skill):
        pass 

    def SkillUse(self, UsedSkill:Skill):
        if UsedSkill == Skill.Talent and not (type(self.TalentState) == Talent and self.TalentState.CanUse()) or self.UsableSkills == 0 and UsedSkill != Skill.Talent:
            return
        self.BeforeSkillUse(UsedSkill)
        for teammate in self.Teammates:
            if teammate == self:
                continue
            teammate.BeforeTeammateSkillUse(UsedSkill, self)
        for enemy in self.Enemies:
            if enemy == self:
                continue
            enemy.BeforeEnemySkillUse(UsedSkill, self)

        if UsedSkill == Skill.Talent:
            self.Talent()
        elif UsedSkill == Skill.SkillOne:
            self.SkillOne()
        elif UsedSkill == Skill.SkillTwo:
            self.SkillTwo()
        else:
            self.SkillThree()

        for teammate in self.Teammates:
            if teammate == self:
                continue
            teammate.AfterTeammateSkillUse(UsedSkill, self)
        for enemy in self.Enemies:
            if enemy == self:
                continue
            enemy.AfterEnemySkillUse(UsedSkill, self)

        self.AfterSkillUse(UsedSkill)
        self.UsableSkills-=1
        
    def AfterSkillUse(self, UsedSkill:Skill):
        pass

    def SkillOne(self):
        pass

    def SkillTwo(self):
        pass

    def SkillThree(self):
        pass

    def Talent(self):
        pass


    def BeforeTeammateSkillUse(self, UsedSkill:Skill, Teammate:BaseCharacter):
        pass

    def AfterTeammateSkillUse(self, UsedSkill:Skill, Teammate:BaseCharacter):
        pass

    def BeforeEnemySkillUse(self, UsedSkill:Skill, Enemy:BaseCharacter):
        pass

    def AfterEnemySkillUse(self, UsedSkill:Skill, Enemy:BaseCharacter):
        pass

    def GetTarget(self, Teammate:bool, Logic:TargetLogic=TargetLogic.Normal, count:int=-1, ignoreTaunt:bool|None = None) -> list[BaseCharacter]:
        SelectedTeam = self.Teammates if Teammate else self.Enemies
        ignoreTaunt = ignoreTaunt if ignoreTaunt != None else not Teammate # ha van érték megadva azt nézi, különben Csapatnál ignorálja, ellenfélnél figyeli
        Taunted:list[BaseCharacter] = []
        Targets:list[BaseCharacter] = []

        # kiszűrjük a Taunt-tal rendelkező célpontokat
        for Character in SelectedTeam:
            if Contains(Character.Effects, StateEffect.Taunt):
                Taunted.append(Character)
            else:
                Targets.append(Character)
        if len(Taunted) > 0 and not ignoreTaunt:
            Taunted.sort(key=lambda char:char.Slot)
            target = Taunted[0]
            Targets = [target for _ in range(len(Taunted) if count == -1 else min(count, len(Taunted)))]
            return Targets

        Targets.extend(Taunted)
        if Logic == TargetLogic.LowestHp:
            Targets.sort(key=lambda char:char.HitPoints)
        elif Logic == TargetLogic.BiggestHp:
            Targets.sort(key=lambda char:char.HitPoints, reverse=True)
        elif Logic == TargetLogic.LowestHpPercent:
            Targets.sort(key=lambda char:char.HitPoints/char.MaxHitPoints)
        elif Logic == TargetLogic.BiggestHpPercent:
            Targets.sort(key=lambda char:char.HitPoints/char.MaxHitPoints, reverse=True)
        elif Logic == TargetLogic.Normal:
            Targets.sort(key=lambda char:char.Slot)
        elif Logic == TargetLogic.Random:
            shuffle(Targets)


        count = len(Targets) if count == -1 else min(count, len(Targets))
        TargetNO:list[BaseCharacter] = []
        if len(Targets) >= count:
            TargetNO = Targets[count:]

        focused = ContainsFocused(TargetNO)
        if focused != None:
            Targets[-1] = focused

        return Targets


    # Skill Event related functions
    def Attack(self, dmg:list[DMG], targets:list[BaseCharacter]):
        """DMGDealt, Hitted enemy"""
        DMGDealt = AttackManager(self.Identity, targets, dmg)
        Hitted = [False for _ in range(len(targets))]
        DMGDealtSummed = 0
        for Hits in DMGDealt:
            for index, Hit in enumerate(Hits):
                if Hit > 0:
                    Hitted[index] = True
                DMGDealtSummed += Hit
        self.TotalDamageDealt += DMGDealtSummed
        return DMGDealtSummed, Hitted.count(True)

    def Heal(self, heal:Heal, targets:list[BaseCharacter]):
        Healing = HealManager(self, targets, heal)
        return sum(Healing), len(Healing)-Healing.count(0)


    # Defense related functions
    def BeforeGetHit(self, Attacker:BaseCharacter, Dmg:DMG):
        pass

    def GetHit(self, Attacker:BaseCharacter, Dmg:DMG):
        TakedDMG:int = 0

        Border:None|EffectsGroup = Contains(self.Effects, StateEffect.Border)
        if Border != None and len(self.DMGFromLastHits) == 0:
            self.DMGFromLastHits.append(0)
            Border.Remove(self, Attacker, 100)
            return 0

        if Dmg.Type == AttackType.Reflect:
            TakedDMG = Dmg.PhysicalPower
        else: 
            val = MaxValue(self.Effects, ComplexEffects.Fear)
            Fear:int = 0 if val == None else val[1]
            Dealt:int = max(Dmg.DMGDealt+Fear-self.DamageResistance, 0)
            Physical:int = max(int(Dmg.PhysicalPower-self.PhysicalDefense*Dmg.IgnorePhysicalDefense)*Dmg.Punctual, 0)
            Magical:int = max(int(Dmg.MagicalPower-self.MagicalDefense*Dmg.IgnoreMagicalDefense)*Dmg.MagicalKnowledge, 0)
            TakedDMG = (Physical+Magical)*Dealt

        self.DMGFromLastHits.append(TakedDMG)
        return TakedDMG

    def AfterGetHit(self, Attacker:BaseCharacter, Dmg:DMG):
        pass

    def DefenseEND(self, Attacker:BaseCharacter, Dmg:list[DMG]):
        Taked = sum(self.DMGFromLastHits)
        FullDMG = Taked
        self.DMGFromLastHits.clear()

        # Minden karaktert akin van Life-pact sebez a sérülésének Life Pact %-val
        for hero in self.Teammates+self.Enemies:
            val = MaxValue(hero.Effects, ComplexEffects.LifePact, self)
            if val != None:
                val = FullDMG*val[1]//100
                Taked -= val
                dmg = DMG(val, AType=AttackType.Reflect)
                self.Attack([dmg], [hero])
            
        for index, shield in enumerate(self.Shield):
            Taked -= shield
            if Taked < 0:
                self.Shield[index] = abs(Taked)
            else:
                self.Shield[index] = 0

        if Taked > 0:

            # Ha a HPDrop által megszabott élet alá csökkent már / csökkenne akkor a sebzés lecsökken annyira, hogy az élet ne menjen HPDrop érték alá
            MaxFalloff = MaxValue(self.Effects, ComplexEffects.HPDrop)
            if MaxFalloff != None:
                Taked = min(max(self.HitPoints - int(self.HitPoints*MaxFalloff[1]/100), 0), Taked) # Megnézem a maximum lehetséges sérülést és utána a sérülés mértékét annyira állítom, ha az meghaladná alapból ezt az értéket

            self.HitPoints -= Taked
            Attacker.TotalDamageDealt += Taked
            self.TotalDamageTaken += Taked

            if self.Dead and not Contains(self.Effects, StateEffect.Untouchable):
                Attacker.KillCount += 1
                for teammate in self.Teammates:
                    teammate.OnTeammateDead(self, Attacker)
                for enemy in self.Enemies:
                    enemy.OnEnemyDead(self, Attacker)
                self.OnSelfDead(Attacker)

    def BeforeAttacked(self, Attacker:BaseCharacter, Dmg:list[DMG]):
        pass

    def AfterAttacked(self, Attacker:BaseCharacter, Dmg:list[DMG]):
        pass


    def BeforeTeammateGetHit(self, Attacker:BaseCharacter, Dmg:DMG, Teammate:BaseCharacter):
        pass

    def AfterTeammateGetHit(self, Attacker:BaseCharacter, Dmg:DMG, Teammate:BaseCharacter):
        pass

    def BeforeEnemyGetHit(self, Attacker:BaseCharacter, Dmg:DMG, Enemy:BaseCharacter):
        pass

    def AfterEnemyGetHit(self, Attacker:BaseCharacter, Dmg:DMG, Enemy:BaseCharacter):
        pass


    def BeforeTeammateAttacked(self, Attacker:BaseCharacter, Dmg:list[DMG], Teammate:BaseCharacter):
        pass

    def AfterTeammateAttacked(self, Attacker:BaseCharacter, Dmg:list[DMG], Teammate:BaseCharacter):
        pass

    def BeforeEnemyAttacked(self, Attacker:BaseCharacter, Dmg:list[DMG], Enemy:BaseCharacter):
        pass

    def AfterEnemyAttacked(self, Attacker:BaseCharacter, Dmg:list[DMG], Enemy:BaseCharacter):
        pass


    # Heal related functions
    def BeforeGetHeal(self, Healer:BaseCharacter, Heal:Heal):
        pass

    def GetHeal(self, Healer:BaseCharacter, Heal:Heal):
        """
        Return NonTaked, Taked
        """
        if self.Dead:
            return (0, 0)
        Taked:int = int((self.ImmuneSystem * Heal.Physical + self.ManaSensitive * Heal.Magical) * Heal.HealMultiple) if Heal.Type != HealType.Lifesteal else Heal.Physical
        NonTaked:int = 0
        Maximum = max(0, self.MaxHitPoints-self.HitPoints)
        if Maximum == 0:
            NonTaked = Taked
            Taked = 0
        else:
            NonTaked = max(Taked-Maximum, 0)
            Taked = min(Maximum, Taked)

        self.HitPoints += Taked
        if Heal.Type == HealType.Regeneration:
            self.TotalRegeneration += Taked
        else:
            Healer.TotalHealing += Taked

        return (NonTaked, Taked)

    def AfterGetHeal(self, Healer:BaseCharacter, Heal:Heal):
        pass


    def BeforeTeammateGetHeal(self, Healer:BaseCharacter, Heal:Heal, TargetAlly:BaseCharacter):
        pass

    def AfterTeammateGetHeal(self, Healer:BaseCharacter, Heal:Heal, TargetAlly:BaseCharacter):
        pass


    def BeforeEnemyGetHeal(self, Healer:BaseCharacter, Heal:Heal, TargetEnemy:BaseCharacter):
        pass

    def AfterEnemyGetHeal(self, Healer:BaseCharacter, Heal:Heal, TargetEnemy:BaseCharacter):
        pass


    # Shield related functions
    def BeforeGetShield(self, Shielder:BaseCharacter, Shield:int):
        pass

    def GetShield(self, Shielder:BaseCharacter, Shield:int):
        if self.Dead:
            return
        Shielder.TotalShielding += Shield
        self.Shield[1] += Shield

    def AfterGetShield(self, Shielder:BaseCharacter, Shield:int):
        pass

    def StealShield(self, steal:float) -> int:
        summed = sum(self.Shield)
        stealed = int(summed*steal)
        stealed1 = stealed
        for index, shield in enumerate(self.Shield):
            stealed1 -= shield
            if stealed1 < 0:
                self.Shield[index] = abs(stealed1)
            else:
                self.Shield[index] = 0
        return stealed


    def BeforeTeammateGetShield(self, Shielder:BaseCharacter, Shield:int, TargetAlly:BaseCharacter):
        pass

    def AfterTeammateGetShield(self, Shielder:BaseCharacter, Shield:int, TargetAlly:BaseCharacter):
        pass


    def BeforeEnemyGetShield(self, Shielder:BaseCharacter, Shield:int, TargetEnemy:BaseCharacter):
        pass

    def AfterEnemyGetShield(self, Shielder:BaseCharacter, Shield:int, TargetEnemy:BaseCharacter):
        pass


    # Effect related functions
    def RemoveEffect(self, Remover:BaseCharacter, Count:int=0, Effect:BasicEffect|StateEffect|ComplexEffects|DoTEffects|HoTEffects|str|None=None, chance:int=100):
        """
        chance -1 = granted
        """
        removed = 0
        if Effect == None:
            if Count == 0:
                for i in range(len(self.Effects)-1, -1, -1):
                    self.Effects[i].Remove(self, Remover, chance)
                    removed += 1
            else:
                shuffle(self.Effects)
                for i in range(Count): 
                    self.Effects[0].Remove(self, Remover, chance) # 0 because, the 0 is always changing after remove
                    removed += 1
        elif isinstance(Effect, str):
            for i in range(len(self.Effects)-1,-1,-1):
                effect = self.Effects[i]
                if effect.Name == Effect or effect.Identify == Effect:
                    Count-=1
                    effect.Remove(self, Remover, chance)
                    removed += 1
                    if Count == 0:
                        break
        else:
            for i in range(len(self.Effects)-1,-1,-1):
                effect = self.Effects[i]
                if any(effect.Contains(e) for e in Effect): # effect.Contains(Effect)
                    Count-=1
                    effect.Remove(self, Remover, chance)
                    removed += 1
                    if Count == 0:
                        break

        return removed

    def BeforeGetEffect(self, giver:BaseCharacter, effect:EffectsGroup):
        pass

    def AfterGetEffect(self, giver:BaseCharacter, effect:EffectsGroup):
        pass


    def BeforeTeammateGetEffect(self, giver:BaseCharacter, Teammate:BaseCharacter, effect:EffectsGroup):
        pass
    
    def AfterTeammateGetEffect(self, giver:BaseCharacter, Teammate:BaseCharacter, effect:EffectsGroup):
        pass


    def BeforeEnemyGetEffect(self, giver:BaseCharacter, Enemy:BaseCharacter, effect:EffectsGroup):
        pass
    
    def AfterEnemyGetEffect(self, giver:BaseCharacter, Enemy:BaseCharacter, effect:EffectsGroup):
        pass

    # Turn related functions
    def StartOfGame(self):
        pass

    def StartOfTurn(self):
        self.Shield.pop(0); self.Shield.append(0)
        HealManager(self, [self], Heal(HealType.Regeneration, int(self.MaxHitPoints*self.Regeneration//100)))
        self.UsableSkills.Reset()

    def EndOfTurn(self):
        if self.TalentState != None:
            self.TalentState.EndOfTurn()
        if self.Shift != None:
            self.Shift.EndOfTurn()
        self.Counter.EndOfTurn()
        self.Reflect.EndOfTurn()
        for effect in self.Effects:
            effect.EndOfTurn(self, True)

    # Dead related functions
    def OnTeammateDead(self, teammate:BaseCharacter, attacker:BaseCharacter):
        pass

    def OnEnemyDead(self, enemy:BaseCharacter, attacker:BaseCharacter):
        pass

    def OnSelfDead(self, attacker:BaseCharacter):
        pass
