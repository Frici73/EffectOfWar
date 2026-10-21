from .CharacterMain import * # Amit a CharacterMain importál azt átveszi ezzel


CustomWarriors:list[type[BaseCharacter]] = []
CustomRangers:list[type[BaseCharacter]] = []
CustomSupports:list[type[BaseCharacter]] = []
CustomBosses:list[type[BaseCharacter]] = []

def Load():


    return CustomWarriors, CustomRangers, CustomSupports, CustomBosses
