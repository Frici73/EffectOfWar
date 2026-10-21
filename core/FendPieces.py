from typing import TYPE_CHECKING, Literal

lazy from tkinter import Tk, Button, Frame, Label, Text, Scale, PhotoImage, Event, Checkbutton, END, NORMAL, DISABLED, BooleanVar, StringVar
lazy from os import listdir, remove
lazy from os.path import join, relpath
lazy from .CharacterMain import Team1Focused, Team2Focused, Team1, Team2
lazy from .GameManager import WarriorsL, RangersL, SupportsL, BossesL, LoadAll
lazy from .Enums import HeroType
lazy from . import BASEPATH

if TYPE_CHECKING:
    from .CharacterMain import BaseCharacter

FRAMEBG = "#CBC1AE"
BUTTONBG = "#DAD9C9"
FG = "black"

modifier:StringVar = StringVar(value="Select") #["Select", "Random", "Mixed"]
ban:BooleanVar = BooleanVar(value=False)


def genFrame(master:Frame|Tk, withPlace:bool=True) -> Frame:
    frame = Frame(master, bg=FRAMEBG)
    if withPlace:
        frame.place(relx=0, rely=0, relheight=1, relwidth=1)
        frame.lift() # type: ignore
    return frame


class MainMenu:
    def __init__(self, master:Tk):
        self.master = master
        self.Frame:Frame = genFrame(master, False)
        self.Frame.config(width=master.winfo_screenwidth(), height=master.winfo_screenheight())
        self.Frame.pack()

        self.Shade = ShadeShower(master)

        self.ModifierButton = Label(self.Frame, bg=BUTTONBG, fg=FG, text="Modifiers", relief="raised")
        self.ModifierButton.place(relx=0.3, rely=0.2, relheight=0.1, relwidth=0.4)
        self.ModifierButton.bind("<Enter>", lambda e: self.Modifier())

        self.PvPButton = Button(self.Frame, text="PvP", command=lambda: self.SelectMenu("PvP"), bg=BUTTONBG, fg=FG)
        self.PvPButton.place(relx=0.3, rely=0.3, relheight=0.1, relwidth=0.4)

        self.BossBattleButton = Button(self.Frame, text="Boss Battle", command=lambda: self.SelectMenu("BossBattle"), bg=BUTTONBG, fg=FG)
        self.BossBattleButton.place(relx=0.3, rely=0.4, relheight=0.1, relwidth=0.4)

        self.RulesButton = Button(self.Frame, text="Rules Button", command=self.ShowRules, bg=BUTTONBG, fg=FG)
        self.RulesButton.place(relx=0.3, rely=0.5, relheight=0.1, relwidth=0.4)

        self.CustomCharacterButton = Button(self.Frame, text="Custom Character\nComing soon!", state=DISABLED, command=self.Custom, bg=BUTTONBG, fg=FG)
        self.CustomCharacterButton.place(relx=0.3, rely=0.6, relheight=0.1, relwidth=0.4)

        self.ViewButton = Button(self.Frame, text="CACHE", command=self.View, fg=FG, bg=BUTTONBG)
        self.ViewButton.place(relx=0.3, rely=0.7, relheight=0.1, relwidth=0.4)

    def ShowRules(self):
        RuleFrame(self.Frame)

    def Custom(self):
        CustomCharacterCreate(self.Frame)

    def SelectMenu(self, gamemode:Literal["PvP", "BossBattle"]):
        Selector(self.master, self.Shade)

    def Modifier(self):
        def change():
            mod = modifier.get()
            if mod == "Select":
                modifier.set("Random")
            elif mod == "Random":
                modifier.set("Mixed")
            elif mod == "Mixed":
                modifier.set("Select")
        frame = genFrame(self.master, False)
        frame.bind("<Leave>", lambda e: frame.destroy())
        frame.place(relx=0.3, rely=0.2, relwidth=0.4, relheight=0.6)
        Checkbutton(frame, bg=BUTTONBG, fg=FG, text="ALLOW BAN", variable=ban).place(relx=0, relwidth=1, relheight=0.1, rely=0)
        Label(frame, text="Character Select Mode:", fg=FG, bg=BUTTONBG).place(relx=0, relwidth=1, rely=0.1, relheight=0.1)
        Button(frame, textvariable=modifier, command=change, fg=FG, bg=BUTTONBG).place(relx=0, relwidth=1, relheight=0.1, rely=0.2)
    
    def View(self):
        ViewCache(self.master)


class ShadeShower:
    def __init__(self, master:Tk):
        self.master:Tk = master
        self.Text = Text(self.master, width=100, height=100, bg=FRAMEBG, fg=FG)
        self.Text.bind("<Leave>", lambda e: self.Hide())

    def Show(self, Character:type[BaseCharacter]|BaseCharacter, x:int, y:int):
        self.Text.pack(x=x, y=y)
        obj:BaseCharacter = None # type: ignore
        Get:bool = isinstance(Character, BaseCharacter)
        if Get: # if a live Character object
            obj = Character
        else: # if a class
            obj = Character() # type: ignore

        Txt = f"Name: {obj.Name}\n\n"

        if Get:
            Txt += f"HP: {obj.HitPoints} / {obj.MaxHitPoints} - {str(obj.HitPoints/obj.MaxHitPoints*100).split(".")[0]}\nShield: {sum(obj.Shield)}\n"
        else:
            Txt += f"HP: {obj.MaxHitPoints}\n"

        Txt += f"MagicalAttack: {obj.MagicalAttack}\n"
        Txt += f"MagicalDefense: {obj.MagicalDefense}\n"
        Txt += f"MagicalKnowledge: {obj.MagicalKnowledge}\n"
        Txt += f"ManaSensitive: {obj.ManaSensitive}\n"

        Txt += f"PhysicalAttack: {obj.PhysicalAttack}\n"
        Txt += f"PhysicalDefense: {obj.PhysicalDefense}\n"
        Txt += f"Punctual: {obj.Punctual}\n"
        Txt += f"ImmuneSystem: {obj.ImmuneSystem}\n"

        Txt += f"Regeneration: {obj.Regeneration}\n"

        if Get:
            if obj.Charge != None:
                Txt += f"Charge: {obj.Charge.Value} / {obj.Charge.Init}\n"
            if obj.Shift != None:
                Txt += f"Shift: {obj.Shift.Phase.Value} cooldown: {obj.Shift.Cooldown.Value}\n"
            if obj.TalentState != None:
                Txt += f"Talent Stack: {obj.TalentState.Stack.Value} / {obj.TalentState.Stack.Init} cooldown: {obj.TalentState.Cooldown.Value}\n"
            Txt += f"Counter Attack: {str(obj.Counter.CType)}\n"
            Txt += f"Reflect: {str(obj.Reflect.Percent)}\n"
            Txt += "Effects:\n"
            for effect in obj.Effects:
                Txt += f"\t{effect.Name} - {effect.Giver.Name}"
        else:
            if obj.Charge != None:
                Txt += f"Charge: {obj.Charge.Init}\n"
            if obj.Shift != None:
                Txt += f"Shift: {obj.Shift.Phase.Init} cooldown: {obj.Shift.Cooldown.Init}\n"
            if obj.TalentState != None:
                Txt += f"Talent Max Stack: {obj.TalentState.Stack.Init} cooldown: {obj.TalentState.Cooldown.Init}\n"

        self.Text.insert("1.0", Txt)

    def Hide(self):
        self.Text.delete("1.0", END)
        self.Text.pack_forget()


class RuleFrame:
    def __init__(self, master:Frame):
        self.Frame = genFrame(master)

        self.Text = Text(self.Frame, state=DISABLED, wrap="word", bg=FRAMEBG, fg=FG)
        self.Text.place(relx=0, rely=0.1, relheight=0.9, relwidth=1)

        path = join(BASEPATH, "Resources", "rules")
        paths = listdir(path)
        Len = len(paths)+1 # plus 1 for the X button

        Button(self.Frame, text="X", command=self.Close, bg=BUTTONBG, fg=FG).place(relx=1-1/Len, relheight=0.1, relwidth=1/Len, rely=0)

        for index, file in enumerate(paths):
            name = file.split("\\")[-1].split(".")[0]
            Button(self.Frame, text=name, command=lambda n=name: self.Open(n), bg=BUTTONBG, fg=FG).place(relx=1/Len*index, rely=0, relheight=0.1, relwidth=1/Len)


    def Close(self):
        self.Frame.place_forget()
        del self

    def Open(self, file:str):
        file = join(BASEPATH, "Resources", "rules", f"{file}.txt")
        self.Text.config(state=NORMAL)
        self.Text.delete("1.0", END)
        text = open(file, encoding="utf8").readlines()
        for line in text:
            self.Text.insert(END, line)
        self.Text.config(state=DISABLED)


class CustomCharacterCreate:
    def __init__(self, master:Frame):
        self.Frame = genFrame(master)


class HorizontalHeroList:
    def __init__(self, master:Frame, shower:ShadeShower, pathList:list[type[BaseCharacter]], type:HeroType=HeroType.Warrior):
        self.Frame = genFrame(master, False)
        self.shower = shower

        self.pathList:list[type[BaseCharacter]] = pathList
        count = min(4, len(pathList))

        self.Scale = Scale(self.Frame, bg=FRAMEBG, troughcolor=BUTTONBG, highlightthickness=0, from_=0, to=len(pathList)-count, showvalue=False, border=0, command=lambda e: self.__onScroll(), orient="horizontal")
        self.Scale.place(relx=0.1, relwidth=0.9, relheight=0.1, rely=0.9)

        self.Label = Label(self.Frame, bg=BUTTONBG, fg=FG, font=("TkDefaultFont", 24))
        self.Label.place(relx=0, relwidth=0.1, relheight=1, rely=0)
        if HeroType.Warrior in type:
            self.Label.config(text="     🛡️")
        elif HeroType.Ranger in type:
            self.Label.config(text="⚔️")
        elif HeroType.Support in type:
            self.Label.config(text="⚕️")
        else: # Boss
            self.Label.config(text="👺")

        self.imgs:list[tuple[Button, PhotoImage]] = []
        for i in range(0, count):
            photo = PhotoImage(file=self.pathList[i]().IMGPath) # type: ignore
            button = Button(master=self.Frame, image=photo)
            button.bind("<Button-3>", lambda e, ind=i: self.__rightclick(e, ind))
            width = 0.9/4
            button.place(relx=width*i, relwidth=width, rely=0, relheight=0.9)

            self.imgs.append((button, photo))

    def __onScroll(self):
        for i in range(int(self.Scale.get()), min(4, len(self.pathList))):
            self.imgs[i][1].configure(file=self.pathList[i]().IMGPath) # type: ignore
            self.Frame.update_idletasks()

    def place(self, relx:float, rely:float, relwidth:float, relheight:float):
        self.Frame.place(relx=relx, rely=rely, relwidth=relwidth, relheight=relheight)

    def place_forget(self):
        self.Frame.place_forget()

    def __rightclick(self, e:Event, ind:int):
        self.shower.Show(self.pathList[ind], e.x, e.y)


class Arena:
    def __init__(self, master:Frame):
        self.Frame = genFrame(master)


class ViewCache:
    def __init__(self, master:Tk):
        self.Frame = genFrame(master)

        self.Text = Text(self.Frame, bg=FRAMEBG, fg=FG, state=DISABLED)
        self.Text.place(relx=0.15, relwidth=0.85, rely=0.1, relheight=0.9)

        Button(self.Frame, bg=BUTTONBG, fg=FG, text="Clear Cache", command=self.Clear).place(relx=0.15, relwidth=0.425, rely=0, relheight=0.1)
        Button(self.Frame, fg=FG, bg=BUTTONBG, text="X", command=self.Close).place(relx=0.575, relwidth=0.425, rely=0, relheight=0.1)

        self.CacheFrame = genFrame(self.Frame, False)
        self.CacheFrame.place(relx=0, relwidth=0.15, relheight=1, rely=0)
        self.scale = Scale(self.CacheFrame, showvalue=False, bg=BUTTONBG)

        self.index = 0
        Button(self.Frame, bg=BUTTONBG, fg=FG, text="^").place(relx=0, relwidth=0.15, relheight=0.05, rely=0)
        Button(self.Frame, bg=BUTTONBG, fg=FG, text="ˇ").place(relx=0, relheight=0.05, rely=0.95, relwidth=0.15)
        self.fileBtns:list[Button] = []

        files = self.Files()
        for count in range(min(len(files), 9)):
            btn = Button(self.Frame, fg=FG, bg=BUTTONBG, text=files[count])
            btn.place(relx=0, relwidth=0.15, relheight=0.1, rely=0.1+0.1*count)

    def Clear(self):
        for file in self.Files():
            remove(file)

    def Close(self):
        self.Frame.destroy()
        del self

    def Files(self):
        return [relpath(path) for path in listdir(join(BASEPATH, "Resources", "cache"))]

    def inc(self):
        files = self.Files()
        if self.index < len(files)-9:
            self.index += 1
        for i in range(9):
            t:str = ""
            try:
                t = files[self.index+i]
            except:
                pass
            self.fileBtns[i].config(text=t, command=lambda t=t: self.show(t))

    def dec(self):
        files = self.Files()
        if self.index < len(files)-9:
            self.index -= 1
        for i in range(9):
            t:str = ""
            try:
                t = files[self.index+i]
            except:
                pass
            self.fileBtns[i].config(text=t, command=lambda t=t: self.show(t))

    def show(self, path:str):
        self.Text.config(state=NORMAL)
        self.Text.delete("1.0", END)
        self.Text.insert("1.0", "\n".join(open(join(BASEPATH, "Resources", "cache", path)).readlines()))
        self.Text.config(state=DISABLED)


class Selector:
    def __init__(self, master:Tk, shade:ShadeShower):
        pass
