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

* *How to use it*
1) Create a new folder inside Rain World\Mods called "CustomResources"
2) Create a new folder inside Rain World\Mods\CustomResources with the name of your region (i.e. Rain World\Mods\CustomResources\The Root)
3) Inside this folder you must place your "World" and "Assets" folders containing any changes made, as well as a text file called "regionID.txt" with just the initials of your region (i.e TR). **If you don't include this file, the region won't be loaded**.

* *Known issues*
1) Map does not work.
2) Fast Travel screen does not work.

* *Credits*
 based on @topicular's EasyModPack
