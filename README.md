# [BETA] Custom-Regions

## Load multiple regions at the same time from custom folder.

*disclaimer* this mod is in development so it has bugs!

* *How does it work?*

Instead of merging the new region folder into the game files, you place them in a separate folder. This will keep your Rain World installation clean, and adds the posibility of playing with multiple regions at the same time.

* *How does it handle conflicts*
![Mergin visualized](https://cdn.discordapp.com/attachments/473881110695378964/670463211060985866/unknown.png)
If the world_XX.txt file of Mod 1 looks like:
```
A: C, B, DISCONNECTED
B: A, DISCONNECTED
C: A
```
and the world_XX.txt file of Mod 2 looks like:
```
A: DISCONNEDTED, B, C
B: A, DISCONNECTED
D: A
```
Custom Regions will merge both files and it will look like:
```
A: C, B, D
B: A, DISCONNECTED
C: A
D: A
```

* *How to install it*
1) Download latest version of partiality [here](https://github.com/PartialityModding/PartialityLauncher/releases "Partiality download").
2) Make sure to check and install EnumExtender.dll, AutoUpdate.dll and CustomRegions.dll. This mod will be autoupdated.
3) You are ready to go.

* *How to use it*
1) Create a new folder inside Rain World\Mods called "CustomResources (if you run the game with the mod it will be automatically created)"
2) Create a new folder inside Rain World\Mods\CustomResources with the name of your region (i.e. Rain World\Mods\CustomResources\The Root). This will determine the name of the region in game.
3) Inside this folder you must place your "World" and "Assets" folders containing any changes made, as well as a text file called "regionID.txt" with just the initials of your region (i.e TR). **If you don't include this file, the region won't be loaded**.
4) The mod will load decals, palettes, arena levels, illustrations, etc from this folder. If something is not being loaded contact me.

* *How to uninstall a region*
a) Rename the regionID.txt or delete it 
or
b) Delete the folder created in step 2 (i.e. Rain World\Mods\CustomResources\The Root)

* *Known issues*
1) Arena thumbnails are not loaded (TO DO)
2) Due to Rain World savefile system, you need to clear you save slot if you uninstall / install new regions.

* *Credits*
 loosely based on @topicular's EasyModPack. What it started as a simple mod, it turned to be one of the biggest and challening mods out there. Please be patient with bugs and error.
