from typing import TYPE_CHECKING, Literal

lazy from .Enums import BasicEffect, ComplexEffects, DoTEffects, HoTEffects, StateEffect, AttackType, HealType
lazy from .EventManager import HealManager, AttackManager, DMG, Heal
lazy from collections.abc import Callable
lazy from random import randint
lazy from copy import deepcopy

if TYPE_CHECKING:
    from .CharacterMain import BaseCharacter, StatBlock

def Contains(effects:list[EffectsGroup], SEffect:BasicEffect|DoTEffects|HoTEffects|ComplexEffects|StateEffect|str):
    for effect in effects:
        if type(SEffect) == str and effect.Name == SEffect:
            return effect
        elif isinstance(SEffect, (BasicEffect,DoTEffects,HoTEffects,ComplexEffects,StateEffect)) and effect.Contains(SEffect):
            return effect
    return None

def MaxValue(effects:list[EffectsGroup], SEffect:BasicEffect|DoTEffects|HoTEffects|ComplexEffects, giver:BaseCharacter|None=None): # Vissza adja az egész listából a maximum értéket az adott effektre
    biggest:None|tuple[EffectsGroup, int] = None
    for effect in effects:
        if (biggest == None or effect.Value(SEffect) > biggest[1]) and (giver==None or effect.Giver == giver):
            biggest = (effect, effect.Value(SEffect))
    return biggest

def Count(effects:list[EffectsGroup], SEffect:BasicEffect|DoTEffects|HoTEffects|ComplexEffects|StateEffect|str):
    c = 0
    if isinstance(SEffect, str):
        for effect in effects:
            if effect.Name == SEffect:
                c += 1
    else:
        for effect in effects:
            if effect.Contains(SEffect):
                c += 1
    return c

class EffectsGroup:
    def __init__(self, giver:BaseCharacter, Name:str="", *effects:tuple[BasicEffect|DoTEffects|HoTEffects|ComplexEffects|StateEffect, int|None], Turn:int=-1, removable:bool=True, identify:Literal["HoT", "DoT", "Buff", "Debuff", "Marker"]): # 1 or more effect - value, 1 or more effect - value
         self.Name:str = Name
         self.Turn:int = Turn
         self.Giver:BaseCharacter = giver
         self.Removable:bool = removable
         self.BEffects:list[tuple[BasicEffect, int]] = []
         self.CEffects:list[tuple[ComplexEffects, int]] = []
         self.DoTEffects:list[tuple[DoTEffects, int]] = []
         self.HoTEffects:list[tuple[HoTEffects, int]] = []
         self.StateEffect:StateEffect = StateEffect(0)
         self.Identify:Literal["HoT", "DoT", "Buff", "Debuff", "Marker"] = identify

         self.__Selecting(*effects)

         # callbacks set by the giver
         self._OnExpires:Callable[[BaseCharacter, EffectsGroup], None] = self.__ExpireChocking # Owner, self
         self._OnRemove:Callable[[BaseCharacter, BaseCharacter, EffectsGroup], None] = self.__RemoveChocking # Owner, Remover, self

    def __Selecting(self, *effects:tuple[BasicEffect|DoTEffects|HoTEffects|ComplexEffects|StateEffect, int|None]):
        for effect in effects: # Típus szerinti válogatás és a hiányos adatok kihagyása
            if type(effect[0]) == BasicEffect and type(effect[1]) == int: 
                self.BEffects.append((effect[0], effect[1]))
            elif type(effect[0]) == DoTEffects and type(effect[1]) == int:
                self.DoTEffects.append((effect[0], effect[1]))
            elif type(effect[0]) == HoTEffects and type(effect[1]) == int:
                self.HoTEffects.append((effect[0], effect[1]))
            elif type(effect[0]) == ComplexEffects and type(effect[1]) == int:
                self.CEffects.append((effect[0], effect[1]))
            elif type(effect[0]) == StateEffect:
                self.StateEffect |= effect[0]
            else:
                continue
            
    @staticmethod
    def __ExpireChocking(c:BaseCharacter, e:EffectsGroup):
        return

    @staticmethod
    def __RemoveChocking(c1:BaseCharacter, c2:BaseCharacter, e:EffectsGroup):
          return

    def Remove(self, Owner:BaseCharacter, Remover:BaseCharacter, chance:int=100):
          """
          -1 granted
          """
          if (self.Removable or Owner == self.Giver) and (chance == -1 or randint(1, 100) <= chance):
            self._OnRemove(Owner, Remover, self)
            self.__RemoveEffects(Owner)
            Owner.Effects.remove(self)

    def Expire(self, Owner:BaseCharacter, chance:int=100):
         if randint(1, 100) <= chance:
            self._OnExpires(Owner, self)
            self.__RemoveEffects(Owner)

    def __RemoveEffects(self, Owner:BaseCharacter):
        self.__StatModify(Owner, False)

    def EndOfTurn(self, Owner:BaseCharacter, withEffect:bool=True):
        if withEffect:
            self.EffectEveryTurn(Owner)
        if self.Turn > 0:
            self.Turn -= 1
        if self.Turn == 0 or Owner.Dead:
            self.Expire(Owner)

    def EffectEveryTurn(self, Owner:BaseCharacter):
        TotalHpChange:int = 0
        for effect in self.DoTEffects:
            Type = effect[0]
            val = effect[1]
            for OneEffect in Type:
                match OneEffect:
                    case DoTEffects.Bleeding: TotalHpChange -= val
                    case DoTEffects.Explosion: TotalHpChange -= (Owner.PhysicalDefense+Owner.MagicalDefense)*val
                    case DoTEffects.Fall: TotalHpChange -= int(Owner.MaxHitPoints*val)
                    case DoTEffects.Poison: TotalHpChange -= (Owner.PhysicalAttack+Owner.MagicalAttack)*1000*val
                    case DoTEffects.Lifesteal: Change = int(Owner.MaxHitPoints*val); HealManager(self.Giver, [self.Giver], Heal(HealType.Lifesteal, Change)); TotalHpChange -= Change
                    case _: pass

        for effect in self.HoTEffects:
            Type = effect[0]
            val = effect[1]
            for OneEffect in Type:
                match OneEffect:
                    case HoTEffects.ManaCharge: TotalHpChange += int(Owner.ManaSensitive*val)
                    case HoTEffects.OverRegenerate: TotalHpChange += int(Owner.ImmuneSystem*val)
                    case HoTEffects.Recover: TotalHpChange += val

                    case _: pass

        if TotalHpChange > 0:
            HealManager(Owner, [Owner], Heal(HealType.Ability, TotalHpChange))
        elif TotalHpChange < 0:
            AttackManager(Owner, [Owner], [DMG(AType=AttackType.Reflect, PhysicalPower=TotalHpChange)])


    def Give(self, target:BaseCharacter, chance:int=100):
        """-1 is granted"""
        SBlock:StatBlock = None # type: ignore
        match self.Identify:
            case "Buff": SBlock = target.BuffImmunity
            case "Debuff": SBlock = target.DebuffImmunity
            case "DoT": SBlock = target.DoTImmunity
            case "HoT": SBlock = target.HoTImmunity
        if chance == -1 or randint(1, 100) <= chance-SBlock:
            target.Effects.append(deepcopy(self))
            self.EffectOnAdd(target)

    def EffectOnAdd(self, Owner:BaseCharacter):
        self.__StatModify(Owner, True)

    def IncreaseTurn(self, Turn:int=1):
        if self.Turn != 0:
            self.Turn += Turn

    def Contains(self, effect:BasicEffect|DoTEffects|HoTEffects|ComplexEffects|StateEffect):
        if type(effect) == BasicEffect:
            for e in self.BEffects:
                if effect in e[0]:
                    return True
        elif type(effect) == DoTEffects:
            for e in self.DoTEffects:
                if effect in e[0]:
                    return True
        elif type(effect) == HoTEffects:
            for e in self.HoTEffects:
                if effect in e[0]:
                    return True
        elif type(effect) == ComplexEffects:
            for e in self.CEffects:
                if effect in e[0]:
                    return True
        elif type(effect) == StateEffect and effect in self.StateEffect:
            return True
        return False

    def Value(self, effect:BasicEffect|DoTEffects|HoTEffects|ComplexEffects):
        if type(effect) == BasicEffect:
            for e in self.BEffects:
                if effect in e[0]:
                    return e[1]
        elif type(effect) == DoTEffects:
            for e in self.DoTEffects:
                if effect in e[0]:
                    return e[1]
        elif type(effect) == HoTEffects:
            for e in self.HoTEffects:
                if effect in e[0]:
                    return e[1]
        elif type(effect) == ComplexEffects:
            for e in self.CEffects:
                if effect in e[0]:
                    return e[1]
        return 0

    def SetCallbackExpire(self, callback:Callable[[BaseCharacter, EffectsGroup], None]|None):
        """
        Owner, self
        """
        if callback == None:
            self._OnExpires = self.__ExpireChocking
        else:
            self._OnExpires = callback

    def SetCallbackRemove(self, callback:Callable[[BaseCharacter, BaseCharacter, EffectsGroup], None]|None):
        """
        Owner, Remover, self
        """
        if callback == None:
            self._OnRemove = self.__RemoveChocking
        else:
            self._OnRemove = callback

    def __StatModify(self, Owner:BaseCharacter, add:bool=True):
        mul:Literal[-1, 1] = 1 if add else -1
        for effect in self.BEffects:
            Type = effect[0]
            val = effect[1]*mul
            SBlock:StatBlock|None = None
            PBlock:StatBlock|None = None
            for OneEffect in Type:
                match OneEffect:
                    case BasicEffect.MaxHP: SBlock = Owner.MaxHitPoints

                    case BasicEffect.MagicalAttack: SBlock = Owner.MagicalAttack
                    case BasicEffect.MagicalDefense: SBlock = Owner.MagicalDefense
                    case BasicEffect.MagicalKnowledge: PBlock = Owner.MagicalKnowledge
                    case BasicEffect.ManaSensitive: PBlock = Owner.ManaSensitive

                    case BasicEffect.PhysicalAttack: SBlock = Owner.PhysicalAttack
                    case BasicEffect.PhysicalDefense: SBlock = Owner.PhysicalDefense
                    case BasicEffect.Punctual: PBlock = Owner.Punctual
                    case BasicEffect.ImmuneSystem: PBlock = Owner.ImmuneSystem

                    case BasicEffect.DMGDealt: PBlock = Owner.DamageDealt
                    case BasicEffect.DMGResistance: PBlock = Owner.DamageResistance

                    case BasicEffect.BuffImmunity: PBlock = Owner.BuffImmunity
                    case BasicEffect.DebuffImmunity: PBlock = Owner.DebuffImmunity
                    case BasicEffect.HoTImmunity: PBlock = Owner.HoTImmunity
                    case BasicEffect.DoTImmunity: PBlock = Owner.DoTImmunity

                    case BasicEffect.Regeneration: Owner.Regeneration += val//100
                    case _: pass

                if SBlock is not None:
                    SBlock += SBlock.Init*val//100
                elif PBlock is not None:
                    PBlock += val

    def OverTime(self, roundModify:int, Owner:BaseCharacter):
        if self.Turn > -1:
            self.Turn += roundModify
            if self.Turn < 1:
                self.Expire(Owner)
