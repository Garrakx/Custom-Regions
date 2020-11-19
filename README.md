
  
# [BETA] Custom-Regions

## Lets you install region mods without modifying the base game's files and more. It works by automerging the world files at runtime and rerouting accesses to rooms.

![Custom Regions!](https://cdn.discordapp.com/attachments/305139167300550666/777644000529088532/unknown.png)

[Russian guide here 
<img src="https://twemoji.maxcdn.com/2/svg/1f1f7-1f1fa.svg" alt="drawing" width="40"/>](https://github.com/Garrakx/Custom-Regions/blob/master/README-RU.md)


## Index
* [Installing Custom Regions Mod](#index1)
* [Installing a new Region](#index2)
* [Uninstalling](#index3)
* [How does it work?](#index4)
* [Handling conflicts - Region Merging](#index5)
* [Information for Modders](#index6)
	* [FOLDER STRUCTURE](#index6.1)
	* [HOW TO ADD COMPATIBILITY BETWEEN TWO REGION MODS THAT MODIFY THE SAME ROOM](#index6.2)
	* [REGION ART](#index6.3)
	* [ELECTRIC GATES](#index6.4)
	* [CUSTOM DATA PEARLS (no code)](#index6.5)
	* [THUMBNAILS](#index6.6)
* [Known issues](#index7)
* [Credits](#index8)

### <a name="index1"></a>Installing the Custom Regions Mod
1) Download and install the latest version of the Partiality Launcher from [RainDB's Tool section](http://www.raindb.net/)
2) Download the latest CR release from [here](https://github.com/Garrakx/Custom-Regions/releases/)
3) Apply **all** (`EnumExtender.dll, ConfigMachine.dll, CustomAssets.dll `(if you want custom music)` and CustomRegions.dll`) the mods inside the `[DOWNLOAD_THIS_Custom-Regions-vX.X.zip]` file. You will receive updates automatically.

### <a name="index2"></a>Installing a Region
* ***Attention:** Most instructions from the modded regions are outdated. If you want to use Custom Regions Mod, you should follow these instructions.*
1) Create a new folder inside Rain World\Mods called "`CustomResources`" (*if you run the game with the mod it will be automatically created*)
2) Create a new folder inside Rain World\Mods\CustomResources with the name of your region (`i.e. Rain World\Mods\CustomResources\The Root`). This will determine the name of the region in game.
3) Inside this folder you must place the "`World`", "`Assets`" and / or "`Levels`" folders from the Region you are installing.
4) After running the game, the mod will create a file called "regionInfo.json". You can open this file with any text editor (notepad for example). Make sure all the information looks correct. You can add a description for each region. Region Order will be used to determine which region loads first. If you come from a version that used regionID.txt, the mod will try to upgrade it. To apply any changes, you need to restart the game or hit the reload button in the config menu **YOU WILL NEED TO START A FRESH SAVESLOT AFTER YOU MAKE ANY CHANGES**.
5) The mod will load decals, palettes, arena levels, illustrations, etc from this folder.
6) You can check the activated regions (in green) and the order they are being loaded.

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
### <a name="index6.1"></a>FOLDER STRUCTURE
* To create the folder structure for you region, just follow the Vanilla structure and create the mod as if you would install it merging files. **Important** If you want to delete a vanilla connection, you must put "DISCONNECTED". 
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

### <a name="index6.2"></a>HOW TO ADD COMPATIBILITY BETWEEN TWO REGION MODS THAT MODIFY THE SAME ROOM
* Example of a region that adds a single room to HI (made by LeeMoriya). [Click here](https://discordapp.com/channels/291184728944410624/431534164932689921/759459475328860160)
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

### <a name="index6.3"></a>REGION ART
* Apart from the "`positions.txt`" file for the Region Art, you will need to include a "`depths.txt`" to position the depth of your art. Follows the same order as "`positions.txt`".
* You can include as many layers as you want for the region art.
* You will probably to adjust the positions of the region art again.

### <a name="index6.4"></a>ELECTRIC GATES
* To add an Electric gate, create a new .txt file inside your mod's `Gates` folder (next to `locks.txt`) and call it `electricGates.txt`. Following the same format as `locks.txt`, write all the gate names that needs to be electric followed by the meter height:
```
GATE_SB_AR : 558
```
(`Rain World\Mods\CustomResources\"Your Region"\World\Gates\electricGates.txt`)
### <a name="index6.5"></a>CUSTOM DATA PEARLS
CR adds the ability to add custom data pearls without any code, and even include dialogue. These are the steps:
1. Navigate to the following folder (`Rain World\Mods\CustomResources\"your region name"\Assets\`). Here, you have to create a text file called `pearlData.txt`. This file will tell the game to create the pearls and make them available in Devtools' place object menu. 
2. Inside `Rain World\Mods\CustomResources\"your region name"\Assets\pearlData.txt`, you must indicate the pearls you want to create following this structure (make sure to follow it exactly, with all the spaces):
```
1 : first_pearl_name : mainColorInHex : highlightColorInHex(optional)
2 : another_pearl_name : mainColorHex2
3 ...
```
- The first number indicates the numberID of the pearl (later it will determine the name of dialogue file). 
- The second field is the name that it will appear in Devtools, it can be anything (for example: `root_pearl_CC`) 
- The third field is the color in hex (for example `00FF00`, use an HEX color picker online). 
- The fourth field is optional if you want your pearl to shine in a different color.

*If you want to add pearls without dialogue, you are done. If you want dialogue keep following the instructions*
3. Navigate to `Rain World\Mods\CustomResources\"your region name"\Assets\Text\Text_Eng\` folder. Here, you have to create as many text files as unique dialogue you want for your pearls. Following the names from above, if I want to add dialogue for *first_pearl_name*, I will create a text file called `1.txt`(since it was correspond to the first column, the pearl ID). Open the file and write the dialogue.  ***DO NOT USE THE ORIGINAL FILES HERE; MAKE A BACKUP***
Sample:
```
0-46
First line of the first text box.<LINE>Second line of the first text box.

This line will be shown in a second text box!
```
Quoting the modding wiki:
`The first line of the text file should be **0-##**, where **##** matches the number of the text file.
Copy and paste this file into the other language folders (Text_Fre, Text_Ger, etc). This will prevent the game from crashing if the player is playing in another language other than English. (If you could actually translate the text for these languages that'd be even better, but you probably don't have a localization budget for your mod...)`

4. Run the game once (with CR installed of course). The game will encrypt all dialogue files so it is harder to data mine. You should included this encrypted files and all the other created files in this steps when you make your mod available.

### <a name="index6.6"></a>THUMBNAILS
- The game first checks if a file called `thumb.png` exits (next to the `regionInfo.json`). It must be 360x250.
- If the game doesn't find the thumb, it will try to download it from raindb.net (same with descriptions).
- If your mod doesn't auto-get a thumbnail or description, contact me.

### <a name="index7"></a>Known issues
* Due to Rain World savefile system, you need to clear you save slot if you uninstall / install new regions.

### <a name="index8"></a>Credits
 loosely based on @topicular's EasyModPack. What it started as a simple mod, it turned to be a really big and challenging mod. Please be patient with bugs and errors. Thanks to LeeMoriya for helping and suggestions.
