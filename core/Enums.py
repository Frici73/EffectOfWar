from enum import Flag

class HeroType(Flag):
    Warrior = 1
    Ranger = 2
    Support = 4
    Boss = 8

class TargetLogic(Flag):
    LowestHp = 1
    LowestHpPercent = 2
    BiggestHp = 4
    BiggestHpPercent = 8
    Random = 16
    Normal = 32

class Skill(Flag):
    SkillOne = 1
    SkillTwo = 2
    SkillThree = 3
    Talent = 4

class HealType(Flag):
    Regeneration = 1
    Ability = 2
    Lifesteal = 4

class ShieldType(Flag):
    Normal = 1
    Stealed = 2

class ShiftMode(Flag):
    Circle = 1
    Line = 2 # Amikor előre halad a maximum értékig
    LineReversed = 4 # Amikor hátra halad a minimum értékig

class CounterType(Flag):
    Counter = 1 # attack only the attacker
    Revenge = 2 # attack the whole enemy team

class AttackType(Flag):
    Attack = 1
    Counter = 2
    Reflect = 4

class BasicEffect(Flag):
    MaxHP = 1
    MagicalAttack = 2
    MagicalDefense = 4
    MagicalKnowledge = 8
    ManaSensitive = 16
    PhysicalAttack = 32
    PhysicalDefense = 64
    Punctual = 128
    ImmuneSystem = 256
    Regeneration = 512
    DMGDealt = 1024
    DMGResistance = 2028
    DebuffImmunity = 4056
    BuffImmunity = 8112
    HoTImmunity = 16224
    DoTImmunity = 32448

    Magical = MagicalKnowledge | ManaSensitive | MagicalAttack | MagicalDefense
    Physical = PhysicalAttack | PhysicalDefense | Punctual | ImmuneSystem

    Attack = MagicalAttack | PhysicalAttack | MagicalKnowledge | Punctual | DMGDealt
    Defense = MagicalDefense | PhysicalDefense | DMGResistance
    Heal = ManaSensitive | ImmuneSystem
    HealAll = Heal | Regeneration

    Immunity = HoTImmunity | DoTImmunity | DebuffImmunity | BuffImmunity
    NegativeImmunity = DoTImmunity | DebuffImmunity
    PositiveImmunity = HoTImmunity | BuffImmunity

    Tank = MaxHP | DMGResistance
    HP = MaxHP | Regeneration

    ALL = Magical | Physical | Immunity | Regeneration | DMGDealt | DMGResistance | MaxHP

class StateEffect(Flag):
    Taunt = 1
    Sleep = 2
    Untouchable = 4
    AbsoluteOne = 8
    Border = 16

class ComplexEffects(Flag):
    Reincarnation = 1
    HPDrop = 2
    Fear = 4
    LifePact = 8

class DoTEffects(Flag):
    Bleeding = 1
    Explosion = 2
    Fall = 4
    Poison = 8
    Lifesteal = 16

class HoTEffects(Flag):
    ManaCharge = 1
    OverRegenerate = 2
    Recover = 4
