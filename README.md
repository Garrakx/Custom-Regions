# [BETA] Custom-Regions

## Load multiple regions at the same time from custom folder.

*disclaimer* this mod is in development so it has bugs!

* *How does it work?*

Instead of merging the new region folder into the game files, you place them in a separate folder. This will keep your Rain World installation clean, and adds the posibility of playing with multiple regions at the same time. You can even create spawn edits for vanilla or modded regions (you only need to include in your world_XX.txt file the changes you want to apply).

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

* *How to delete a vanilla connection*
If the vanilla world_XX.txt looks like:
```
A: C, B, D
```
And you want to delete a connection, you must put in your modded world_XX.txt file the following:
```
A: DISCONNECTED, B, D
```

* *How to install it*
1) Download latest version of partiality [here](https://github.com/PartialityModding/PartialityLauncher/releases "Partiality download").
2) Make sure to check and install EnumExtender.dll, AutoUpdate.dll and CustomRegions.dll. This mod will be autoupdated.
3) You are ready to go.


* *How to use it*
1) Create a new folder inside Rain World\Mods called "CustomResources (if you run the game with the mod it will be automatically created)"
2) Create a new folder inside Rain World\Mods\CustomResources with the name of your region (i.e. Rain World\Mods\CustomResources\The Root). This will determine the name of the region in game.
3) Inside this folder you must place your "World" and "Assets" folders containing any changes made 
4)**NEW** After running the game, the mod will create a file called "regionInfo.json". You can open this file with any text editor (notepad for example). Make sure all the information looks correct. You can add a description for each region. Region Order will be used to determine which region loads first. If you come from a version that used regionID.txt, the mod will try to upgrade it. To apply any changes, you need to restart the game and **YOU WILL NEED TO START A FRESH SAVESLOT**.
5) The mod will load decals, palettes, arena levels, illustrations, etc from this folder. If something is not being loaded contact me.
6) If you install Config Machine, you can check the activated regions (in green) and the order they are being loaded.


* *How to uninstall a region (two options, pick one)*
1) Option a) Go to Rain World\Mods\CustomResources\Your Region\regionInformation.json and set activated to false.
2) Option b) Delete the folder created in step 2 (i.e. Rain World\Mods\CustomResources\The Root)

* *Useful information for modders*
1) To create the folder structure for you region, just follow the Vanilla structure and create the mod as if you would install it merging files. **Important** If you want to delete a vanilla connection, you must put "DISCONNECTED". 
2) Apart from the "positions.txt" file for the Region Art, you will need to include a "depths.txt" to position the depth of your art. Follows the same order as "positions.txt".
3) You can include as many layers as you want for the region art.
4) You will probably to adjust the positions of the region art again.
5) This mod should be compatible with almost anything. If you find any incompabilities contact me.



* *Known issues*
1) Due to Rain World savefile system, you need to clear you save slot if you uninstall / install new regions.
2) Custom music is not loaded (TO DO)

* *Credits*
 loosely based on @topicular's EasyModPack. What it started as a simple mod, it turned to be a really big and challening mod. Please be patient with bugs and error. Thanks to LeeMoriya for helping and suggestions.
