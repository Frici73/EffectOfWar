from tkinter import Tk
from os.path import dirname, join

lazy from sys import exit as e
lazy from core.FendPieces import MainMenu


window = Tk()
window.title("Effect of War")
window.iconbitmap(join(dirname(__file__), "Resources", "icon.ico"))
window.state("zoomed")

Menu = MainMenu(window)

window.mainloop()
e(0) # close the app, when the window is closed
