# [BETA] Custom-Regions

## Load multiple regions at the same time from custom folder.

*disclaimer* this mod is heavily based on @topicular#7218 's EasyMod Pack & and it is in development so it has bugs!

* *How does it work?*

Instead of merging the new region folder into the game files, you place them in a separate folder so you can load multiple regions at the same time. In case of overlapping, it will choose the first loaded region.

* *How to use it*
1) Create a new folder inside Rain World\Mods called "CustomResources"
2) Create a new folder inside Rain World\Mods\CustomResources with the name of your region (i.e. Rain World\Mods\CustomResources\The Root)
3) Inside this folder you must place your "World" folder containing any changes made, as well as a text file called "regionID.txt" with just the initials of your region (i.e TR)

* *What's missing*
1) An algorithm that merges all the world_XX.txt files or
2) Funcionality to choose which world_XX.txt gets loaded on top.
