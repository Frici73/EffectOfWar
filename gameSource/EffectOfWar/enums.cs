using System;
using System.Collections.Generic;
using System.Text;

namespace EffectOfWar
{
    public enum Team : byte
    {
        first, second
    }
    public enum GameMode : byte
    {
        BossBattle, PvP
    }
    enum Direction : byte
    {
        up, down
    }
    public enum Operator : byte
    {
        plus, minusz, divide, multiplication
    }
    public enum TargetingMode : byte
    {
        normal, lowestHp, highestHp, lowestHpPercent, highestHpPercent, random
    }
    public enum DMGType : byte
    {
        magical, physical, both, none
    }
    public enum AttackType : byte
    {
        Skill, Counter, Reflect
    }
    public enum Effect : byte
    {
        allstat, maxhp, matk, patk, mdef, pdef, simmun, punctual, manasens, mknow, reg, dmgD, dmgR, debuffImmun, buffImmun, HoTImmun, DoTImmun, taunt, hpDrop, reincarnation, absoluteOne, sleep, Untouchable
    }
    public enum OverTimeType : byte
    {
        Bleeding, Explosion, Fall, Poison, Lifesteal, ManaCharge, OverRegenerate, Recover
    }
    public enum HealingType : byte
    {
        reg, magic, physi, none, both
    }
    public enum HType : byte
    {
        warrior, ranger, support, boss
    }
    public enum ShiftMode : byte
    {
        circle, line
    }
    public enum Skill : byte
    {
        first, second, third, talent
    }
}