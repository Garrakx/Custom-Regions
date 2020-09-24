
# [BETA] Custom-Regions

## Lets you install region mods without modifying the base game's files and more. It works by automerging the world files at runtime and rerouting accesses to rooms.

#### # *disclaimer* this mod is in development so it has bugs! 
![Custom Regions!](http://www.raindb.net/previews/customregion.png?raw=true)
![Russian guide here]()

### Index
* [Installing Custom Regions Mod](#index1)
* [Installing a new Region](#index2)
* [Uninstalling](#index3)
* [How does it work?](#index4)
* [Handling conflicts - Region Merging](#index5)
* [Information for Modders](#index6)
* [Known issues](#index7)
* [Credits](#index8)

### <a name="index1"></a>Installing the Custom Regions Mod
1) Download and install the latest version of the Partiality Launcher from [RainDB's Tool section](http://www.raindb.net/)
2) Download the latest CR release from [here](https://github.com/Garrakx/Custom-Regions/releases/)
3) Apply **all** (`EnumExtender.dll, AutoUpdate.dll, CustomAssets.dll and CustomRegions.dll`) the mods inside the `[DOWNLOAD_THIS_Custom-Regions-vX.X.zip]` file. You will receive updates automatically.

### <a name="index2"></a>Installing a Region
* ***Attention:** Most instructions from the modded regions are outdated. If you want to use Custom Regions Mod, you should follow these instructions.*
1) Create a new folder inside Rain World\Mods called "`CustomResources`" (*if you run the game with the mod it will be automatically created*)
2) Create a new folder inside Rain World\Mods\CustomResources with the name of your region (`i.e. Rain World\Mods\CustomResources\The Root`). This will determine the name of the region in game.
3) Inside this folder you must place the "`World`", "`Assets`" and / or "`Levels`" folders from the Region you are installing.
4) After running the game, the mod will create a file called "regionInfo.json". You can open this file with any text editor (notepad for example). Make sure all the information looks correct. You can add a description for each region. Region Order will be used to determine which region loads first. If you come from a version that used regionID.txt, the mod will try to upgrade it. To apply any changes, you need to restart the game and **YOU WILL NEED TO START A FRESH SAVESLOT AFTER YOU MAKE ANY CHANGES**.
5) The mod will load decals, palettes, arena levels, illustrations, etc from this folder.
6) If you install Config Machine, you can check the activated regions (in green) and the order they are being loaded.

### <a name="index3"></a>Uninstalling a region (two options, pick one)
Option a). Go to `Rain World\Mods\CustomResources\Your Region\regionInformation.json` and set activated to false.
Option b). Delete the folder created in step 2 (`i.e. Rain World\Mods\CustomResources\The Root`)



### <a name="index4"></a>How does it work?
Instead of merging the new region folder into the game files, you place them in a separate folder. This will keep your Rain World installation clean, and adds the possibility of playing with multiple regions at the same time. You can even create spawn edits for vanilla or modded regions (you only need to include in your world_XX.txt file the changes you want to apply).


### <a name="index5"></a> Region conflicts
The mod will try to merge all the region mods so they are compatble:
![Mergin visualized](https://cdn.discordapp.com/attachments/473881110695378964/670463211060985866/unknown.png)
If the world_XX.txt file of Mod 1 looks like:
```
A: C, B, DISCONNECTED
B: A, DISCONNECTED
C: A
```
and the world_XX.txt file of Mod 2 looks like:
```
A: DISCONNECTED, B, C
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

### <a name="index6"></a>Useful information for modders
* CR will compare each room connection. If your room connection is being compared to a vanilla connection (i.e. it is the first one to load or the only one installed), it will replace completly the vanilla connection with the modded one.
```
Analized room [SB_J01 : DISCONNECTED, SB_E02, SB_G03, SB_C07 : SWARMROOM]. Vanilla [True]. NewRoomConnections [SB_ROOTACCESS, SB_E02, SB_G03, SB_C07]. IsBeingReplaced [True]. No Empty Pipes [True]
```
* If the mod is modifying a room that is either modded or modified by another mod, CR will try to merge both
```
Replaced [SB_J03 : DISCONNECT, SB_J02, SB_F01, SB_S02] with [SB_J03 : SB_ROOTACCESS, SB_J02, SB_F01, SB_S02]
```
* If the room CR is trying to merge doesn't have any DISCONNECTED exits, the two region mods will be incompatible.
```
ERROR! #Found incompatible room [SB_J01 : SB_Q01, SB_E02, SB_G03, SB_C07] from [AR] and [SB_J01 : SB_ROOTACCESS, SB_E02, SB_G03, SB_C07 : SWARMROOM] from [TR]. Missing compatibility patch?
```
* To create the folder structure for you region, just follow the Vanilla structure and create the mod as if you would install it merging files. **Important** If you want to delete a vanilla connection, you must put "DISCONNECTED". (See below for more info)
* Apart from the "`positions.txt`" file for the Region Art, you will need to include a "`depths.txt`" to position the depth of your art. Follows the same order as "`positions.txt`".
* You can include as many layers as you want for the region art.
* You will probably to adjust the positions of the region art again.
* This mod should be compatible with almost anything. If you find any incompabilities contact me.
* HOW TO ADD COMPATIBILITY BETWEEN TWO REGION MODS THAT MODIFY THE SAME ROOM
1) Create a region mod that it is loaded first and modifies a vanilla room by adding new connections:

how the *whole* world_HI.txt from the region NewPipes looks (you only need these lines)
```
ROOMS
HI_A07 : HI_A14, DISCONNECTED, DISCONNECTED, HI_B04, HI_C02
END ROOMS
```
*note* You might have to move around the DISCONNECTED to make sure the vanilla rooms maintains the same layout.

2) Create another region that connects to the vanilla room, but loads after NewPipes

how the *whole* world_HI.txt from the region ModA looks like
```
ROOMS
HI_A07 : HI_A14, HI_B04, HI_C02, HI_MODA, DISCONNECTED
HI_MODA : HI_A07
END ROOMS
```
3) Create another region that connects to the vanilla room, but loads after NewPipes
how the *whole* world_HI.txt from the region ModB looks like
```
ROOMS
HI_A07 : HI_A14, HI_B04, HI_C02, DISCONNECTED, HI_MODB
HI_MODB : HI_A07
END ROOMS
```
![Compatibility patch](https://cdn.discordapp.com/attachments/481900360324218880/758592126786863154/ezgif.com-optimize_1.gif)
<details>
  <summary> How to delete a vanilla connection</summary>

If the vanilla world_XX.txt looks like:
```
	A: C, B, D
```
you want to delete a connection, you must put in your modded world_XX.txt file the following:
```
	A: DISCONNECTED, B, D
```
</details>

### <a name="index7"></a>Known issues
* Due to Rain World savefile system, you need to clear you save slot if you uninstall / install new regions.

### <a name="index8"></a>Credits
 loosely based on @topicular's EasyModPack. What it started as a simple mod, it turned to be a really big and challening mod. Please be patient with bugs and error. Thanks to LeeMoriya for helping and suggestions.
