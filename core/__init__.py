__ALL__ = ["Bosses", "CharacterMain", "Effects", "Enums", "FendPieces", "GameManager", "Rangers", "Supports", "Warriors"]

BASEPATH:str = ""
try:
    from sys import _MEIPASS # type: ignore
    BASEPATH = _MEIPASS # type: ignore
except:
    from os.path import dirname
    BASEPATH = dirname(dirname(__file__)) # type: ignore
