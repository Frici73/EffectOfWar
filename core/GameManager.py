lazy from inspect import getmembers, isclass
lazy from . import Warriors, Rangers, Supports, Bosses
lazy from .CharacterMain import BaseCharacter
lazy from .UserCharacters import Load
lazy from typing import Type, cast, Callable

class ConsoleText: # main.py-ból a callback a "konzolra" lesz állítva, így az folyton meg kapja a fontos adatokat
    callback:Callable[[str], None] = lambda text: None

    @classmethod
    def Text(cls, txt:str):
        cls.callback(txt)

WarriorsL:list[Type[BaseCharacter]] = []
RangersL:list[Type[BaseCharacter]] = []
SupportsL:list[Type[BaseCharacter]] = []
BossesL:list[Type[BaseCharacter]] = []

def LoadAll():
    WarriorsL.clear()
    RangersL.clear()
    SupportsL.clear()
    BossesL.clear()
    for module in ((Warriors, WarriorsL), (Rangers, RangersL), (Supports, SupportsL), (Bosses, BossesL)):
        for _, cls in getmembers(module[0], isclass):
            if cls.__module__ in (Warriors.__name__, Rangers.__name__, Supports.__name__) and issubclass(BaseCharacter, cls):
                module[1].append(cast(Type[BaseCharacter], cls))

    Customs = Load()
    WarriorsL.extend(Customs[0])
    RangersL.extend(Customs[1])
    SupportsL.extend(Customs[2])
    BossesL.extend(Customs[3])
