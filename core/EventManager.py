from typing import TYPE_CHECKING
from .Enums import HealType, AttackType, ShieldType

if TYPE_CHECKING:
    from .CharacterMain import BaseCharacter, StatBlock
    from .EffectBase import EffectsGroup

def AttackManager(Attacker:BaseCharacter, targets:list[BaseCharacter], dmgs:list[DMG]):
    DmgDealt:list[list[int]] = []
    for target in targets:
        if target.Dead:
            continue
        target.BeforeAttacked(Attacker, dmgs)
        for teammate in Attacker.Teammates:
            teammate.BeforeEnemyAttacked(Attacker, dmgs, target)
        for enemyTeammate in target.Teammates:
            if target == enemyTeammate:
                continue
            enemyTeammate.BeforeTeammateAttacked(Attacker, dmgs, target)
    for dmg in dmgs:
        while dmg.Hit > 0:
            DmgDealt.append([])
            for target in targets:
                if target.Dead:
                    continue

                target.BeforeGetHit(Attacker, dmg)
                for teammate in Attacker.Teammates:
                    teammate.BeforeEnemyGetHit(Attacker, dmg, target)
                for enemyTeammate in target.Teammates:
                    if target == enemyTeammate:
                        continue
                    enemyTeammate.BeforeTeammateGetHit(Attacker, dmg, target)

                if dmg.Hit == 0:
                    break
                DmgDealt[-1].append(target.GetHit(Attacker, dmg))

                for enemyTeammate in target.Teammates:
                    if target == enemyTeammate:
                        continue
                    enemyTeammate.AfterTeammateGetHit(Attacker, dmg, target)
                for teammate in Attacker.Teammates:
                    teammate.AfterEnemyGetHit(Attacker, dmg, target)
                target.AfterGetHit(Attacker, dmg)

            dmg.Hit -= 1
    for target in targets:
        target.DefenseEND(Attacker, dmgs)
        for enemyTeammate in target.Teammates:
            if target == enemyTeammate:
                continue
            enemyTeammate.AfterTeammateAttacked(Attacker, dmgs, target)
        for teammate in Attacker.Teammates:
            teammate.AfterEnemyAttacked(Attacker, dmgs, target)
        target.AfterAttacked(Attacker, dmgs)

    return DmgDealt

def HealManager(Healer:BaseCharacter, targets:list[BaseCharacter], heal:Heal):
    ReturnData:list[int] = [0, 0] # Unused Heal, Used Heal
    for target in targets:
        if target.Dead:
            continue
        target.BeforeGetHeal(Healer, heal)
        for teammate in target.Teammates:
            if target == teammate:
                continue
            teammate.BeforeTeammateGetHeal(Healer, heal, target)
        for enemy in target.Enemies:
            enemy.BeforeEnemyGetHeal(Healer, heal, target)

        value = target.GetHeal(Healer, heal)
        ReturnData[0] += value[0]
        ReturnData[1] += value[1]

        for enemy in target.Enemies:
            enemy.AfterEnemyGetHeal(Healer, heal, target)
        for teammate in target.Teammates:
            if target == teammate:
                continue
            teammate.AfterTeammateGetHeal(Healer, heal, target)
        target.AfterGetHeal(Healer, heal)

    return ReturnData

def ShieldManager(Shielder:BaseCharacter, targets:list[BaseCharacter], shield:int|StatBlock, ShieldMode:ShieldType):
    shield = shield.Value if isinstance(shield, StatBlock) else shield
    shield *= 1000 if ShieldMode == ShieldType.Normal else 1
    for target in targets:
        if target.Dead:
            continue
        target.BeforeGetShield(Shielder, shield)
        for teammate in target.Teammates:
            if target == teammate:
                continue
            teammate.BeforeTeammateGetShield(Shielder, shield, target)
        for enemy in target.Enemies:
            enemy.BeforeEnemyGetShield(Shielder, shield, target)

        target.GetShield(Shielder, shield)

        for enemy in target.Enemies:
            enemy.AfterEnemyGetShield(Shielder, shield, target)
        for teammate in target.Teammates:
            if target == teammate:
                continue
            teammate.AfterTeammateGetShield(Shielder, shield, target)
        target.AfterGetShield(Shielder, shield)

def EffectManager(Giver:BaseCharacter, targets:list[BaseCharacter], effect:EffectsGroup, chance:int=100):
    for target in targets:
        if target.Dead:
            continue
        target.BeforeGetEffect(Giver, effect)
        for teammate in target.Teammates:
            if target == teammate:
                continue
            teammate.BeforeTeammateGetEffect(Giver, target, effect)
        for enemy in target.Enemies:
            enemy.BeforeEnemyGetEffect(Giver, target, effect)

        effect.Give(target, chance)

        for enemy in target.Enemies:
            enemy.AfterEnemyGetEffect(Giver, target, effect)
        for teammate in target.Teammates:
            if target == teammate:
                continue
            teammate.AfterTeammateGetEffect(Giver, target, effect)
        target.AfterGetEffect(Giver, effect)

class DMG:
    def __init__(self, PhysicalPower:StatBlock|int=0, Punctual:StatBlock|int=0, MagicalPower:StatBlock|int=0, MagicalKnowledge:StatBlock|int=0, DMGDealt:StatBlock|int=0, hit:int=1, IgnoreMagicalDefense:int=0, IgnorePhysicalDefense:int=0, AType:AttackType=AttackType.Attack):
        self.Type = AType

        if type(PhysicalPower) == StatBlock:
            self.PhysicalPower = PhysicalPower.Value
        elif type(PhysicalPower) == int:
            self.PhysicalPower = PhysicalPower

        if type(Punctual) == StatBlock:
            self.Punctual = Punctual.Value
        elif type(Punctual) == int:
            self.Punctual = Punctual

        if type(MagicalKnowledge) == StatBlock:
            self.MagicalKnowledge = MagicalKnowledge.Value
        elif type(MagicalKnowledge) == int:
            self.MagicalKnowledge = MagicalKnowledge

        if type(MagicalPower) == StatBlock:
            self.MagicalPower = MagicalPower.Value
        elif type(MagicalPower) == int:
            self.MagicalPower = MagicalPower

        if type(DMGDealt) == StatBlock:
            self.DMGDealt = DMGDealt.Value
        elif type(DMGDealt) == int:
            self.DMGDealt = DMGDealt

        self.Hit:int = hit

        self.Ignores:list[int] = [IgnorePhysicalDefense, IgnoreMagicalDefense]

    @property
    def IgnorePhysicalDefense(self):
        return (100-self.Ignores[0])/100

    @property
    def IgnoreMagicalDefense(self):
        return (100-self.Ignores[1])/100

class Heal:
    def __init__(self, HType:HealType, Physical:int|StatBlock=0, Magical:int|StatBlock=0, HealMultiple:int|StatBlock=0):
        self.Type:HealType = HType
        self.Physical:int = Physical if type(Physical) == int else Physical.Value # type: ignore
        self.Magical:int = Magical if type(Magical) == int else Magical.Value # type: ignore
        self.HealMultiple:int = HealMultiple if type(HealMultiple) == int else HealMultiple.Value # type: ignore
