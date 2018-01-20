from nltk.tree import Tree
from nltk.draw.tree import TreeView
from string_tree import *
import os

t = Tree.fromstring(ruleString)
TreeView(t)._cframe.print_to_file('output.ps')
os.system('C:/"Program Files"/ImageMagick-7.0.7-Q16/magick.exe convert output.ps output.png')