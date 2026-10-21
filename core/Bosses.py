from .CharacterMain import *

def WhichSkill(S1Chance:int=33, S2Chance:int=33):
    random = randint(1, 100)
    if random <= S1Chance:
        return Skill.SkillOne
    elif random <= S1Chance+S2Chance:
        return Skill.SkillTwo
    else:
        return Skill.SkillThree

def CanIUseTalent(talent:Talent, chance:int):
    if talent.Stack == 0:
        return False
    if randint(1, 100) <= chance:
        return True
    return False
