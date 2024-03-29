
## You are seeing the version of CRS for Rain World v1.9 / Downpour, for the legacy version (v1.5), please visit [here](https://github.com/Garrakx/Custom-Regions/tree/master)!
***
# Custom Regions Support (CRS)
***
## Adds various meta features to Custom Regions
[![Twitter](https://img.shields.io/twitter/url?style=social&url=https%3A%2F%2Ftwitter.com%2F)](https://twitter.com/)  [![Downloads](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-steam-workshop.jross.me%2F2941565790%2Fsubscriptions-text?style=flat-square)]() [![Version](https://img.shields.io/github/release/Garrakx/Custom-Regions.svg?style=flat-square)](https://github.com/Garrakx/Custom-Regions/releases/latest)
[![Donate](https://img.shields.io/badge/support-DONATE-orange?style=flat-square)](https://ko-fi.com/garrakx)
![Custom Regions!](./assets/thumbnail.png)


## <a name="index"></a>Index
1) [Debugging](#debugging)
2) [Procedural Music](#procedural-music)
3) [Region Landscape Art](#region-landscape-art)
4) [Level Unlocks](#level-unlocks)
5) [Pearls](#pearls)
6) [Broadcasts](#broadcasts)
7) [Oracle Specific Text](#oracle-specific-text)
6) [Challenges](#challenges)
8) [MetaProperties](#metaproperties)
9) [Region Conditional Lines](#region-conditional-lines)
10) [ReplaceRoom](#replaceroom)
11) [Mod Priorities](#Mod-Priorities)

### <a name="HOW TO COMPILE"></a>How to compile
(for coders only)
1. Place references in "lib" folder

Creating the System Variable:
2. type env in the windows search bar, 
3. choose the first one
4. Press n
5. add a new system variable called `RainWorldDir`

## <a name="DEBUG">Debugging

CRS adds better debugging support when region inevitably break.  
Its log file generates in StreamingAssets\crsLog.txt  
This will contain a lot of potentially useful info, including exception messages that the base game fails to catch.


## <a name="MUSIC"></a>Procedural Music
Procedural music files can be loaded from .ogg files in music\procedural  
These files must have a sample rate of 44.100 kHz  
The filenames can also be appended with a slugcat's name in order to load slug-specific threat themes  
Read [this wiki page](https://rainworldmodding.miraheze.org/wiki/Threat_Music_File_Format) for more specific instructions on threat file formatting.


## <a name="LANDSCAPE"></a>Region Landscape Art
Region landscapes go in `Scenes\Landscape - XX`  
Inside this folder, a file called `Landscape - XX.txt` will define the landscape.  

### Layers
Format for each layer is as follows:  

    ImageName : depth : shader
    
The ImageName must be the name of a .png file (without the extension) inside `Scenes\Landscape - XX`  
The Depth is any number, and used to set how far back the image is.  
The Shader is the name of a shader type to apply to the image. The following shaders are supported:
- Normal
- Basic
- Lighten
- LightEdges
- Rain
- Overlay
- Multiply
- SoftLight

Both the Depth and Shader fields are optional, and can be omitted, defaulting to Depth of 1 and Shader of Normal. The Depth can't be omitted if the Shader field is used, however.  

    SI_Landscape - 5 : 15
    SI_Landscape - 4 : 6
    SI_Landscape - 3 : 2.2
    SI_Landscape - 2 : 0.9 : LightEdges
    SI_Landscape - 1 : 0.4

The images should be listed in draw order, with the foremost images listed last.  

### Focus Attributes

The following additional attributes of the landscape scene can be defined:
- blurMin (default 0.25)
- blurMax (default 0.8)
- idleDepths (if none are defined, all layers will be treated as idleDepths)

These control the 'focus' attributes of the landscape scene. The scene's focus will move between any idleDepths at random, and the closer a layer is to the current focus point, the less blurred it will be. Blur always ranges from 0 (no blur) to 1 (full blur,) but changing the min\max outside of this range can be useful.  

    HR_Landscape - 6 : 11
    HR_Landscape - 5 : 6.5 : LightEdges
    HR_Landscape - 4 : 4.8 : Basic
    HR_Landscape - 3 : 2.4 : Basic
    HR_Landscape - 2 : 1.1 : Lighten
    HR_Landscape - 1 : 1 : Basic
    
    blurMin : -0.1
    blurMax : 0.4
    idleDepths : 11
    idleDepths : 6.5
    idleDepths : 4.8
    idleDepths : 8
    
### Positions

Positions.txt can be included inside `Scenes\Landscape - XX` to define the positions of each layer. This file is automatically generated when [repositioning the layers using devtools](https://rainworldmodding.miraheze.org/wiki/Dev_Tools#Menu_controls) which is strongly recommended over manually writing Positions.txt

### Flat Mode

`Scenes\Landscape - XX` should also include a flat version of the region art called `Landscape - XX - Flat.png` that will be used for low performance settings.

### Title Text

Title images should be placed in the `Illustrations` folder named `Title_XX.png` and `Title_XX_Shadow.png` which will be loaded as the main text for the region.

### Menu Icon

Various menus such as the Safari menu or the Background menu use smaller icons for the regions. These go in `Illustrations` and are called `Safari_XX.png`. They should be 200x200 pixels with rounded edges.

## <a name="UNLOCKS"></a>Level Unlocks
Level unlocks are read from  CustomUnlocks.txt.  
Format is as follows:  


    TokenID : ArenaName1, ArenaName2, ArenaName3, ect.  
    

The TokenID must be unique,  
so it's good to pick a name that won't be likely to be picked  
by another region mod.  

Since this a single file,  
region mods should extend this file with modification files.  

As an example: 

    aetherridge\modify\CustomUnlocks.txt
        [ADD]AR1 : Station, Complex, Floor
        [ADD]AR2 : Exhaust, Array, Research
        [ADD]AR3 : Aeolian, Hull, Decay
    
## <a name="PEARLS"></a>Pearls
Custom Pearl data is read from  CustomPearls.txt.  
Format is as follows:  


    PearlID : MainColor : HighlightColor : ConversationFileName  
    
The PearlID must be unique, so it's good to pick a name that won't be likely to be picked by another region mod.  

Conversation files go inside `Text\text_lang\`  
and can have any filename, as long as it matches what's in CustomPearls.txt  
The first line of every conversation file should be `0-filename` with the following lines being the conversation itself


Since this a single file, region mods should extend this file with modification files.  

As an example: 

    aetherridge\modify\CustomPearls.txt
        [ADD]AR_Ridge_Pearl_1 : f56942 : e1603b : AR_Tram
        [ADD]AR_Ridge_Pearl_2 : e805e8 : ff72ff : AR_Picture
        [ADD]AR_Heat_Pearl : a6f781 : 7bff39 : AR_Shipping
        
    aetherridge\Text\text_eng\
        AR_Tram.txt
            0-AR_Tram
            First line of the first text box.<LINE>Second line of the first text box.
            This line will be shown in a second text box!
            
        AR_Picture.txt
            0-AR_Picture
            The 2nd number in the 1st line can be any number
            Although both numbers still need to be there
            
        AR_Shipping-Artificer.txt
            0-AR_Shipping-Artificer
            You can also append a slugcat name
            For slug-specific conversations

## <a name="BROADCASTS"></a>Broadcasts
Custom Broadcast data is read from CustomBroadcasts.txt
Basic format is as follows:
    
    BroadcastID : TextFileName

The text files use the same format as the pearl text files above.

A **sequence** of broadcast texts can be assigned to a single BroadcastID by using the `>` symbol. This allows you to specify a sequence of text files to be used in order.

    SequenceID : Sequence1 > Sequence2 > Sequence3
    

A **random pool** of broadcast texts can also be assigned to a single BroadcastID by using the `,` symbol. A random text file that has not yet been read will be chosen.

    RandomPoolID : Random1, Random2, Random3

Sequences and random pools can be mixed together. The sequence will not move on until all of the random pool is depleted.

    MixedID : SequenceStart > Random1, Random2, Random3 > SequenceEnd

Since this a single file, mods should extend this file with modification files.  

As an example: 

    oldnewhorizons\modify\CustomBroadcasts.txt
        [ADD]ONHFarlands : ONH_Special
        [ADD]ONHMainSequence : ONH_Seq_1 > ONH_Seq_2 > ONH_Ran_1, ONH_Ran_2, ONH_Ran_3 > ONH_Final
       [ADD]ONHMisc : ONH_Misc_1, ONH_Misc_2, ONH_Misc_3, ONH_Misc_4
       
    oldnewhorizons\Text\text_eng\
        ONH_Special.txt
            0-ONH_Special
            Same format as pearl text<LINE>Including line breaks.
            Line breaks will be in the same black box,
            While different lines will be in separated boxes.
            
        ONH_Seq_1.txt
            0-ONH_Seq_1
            This is a different text file with a different broadcast text
            
        ect...

## <a name="ORACLETEXT"></a>Oracle Specific Text

For coders or custom slugcat makers who can visit multiple iterator oracles, unique text files can be defined. This can be applied to any text file, including pearl texts, item dialogue, ect.

Oracle-specific text files are included in subfolders using the OracleID.  
For base game oracles, these would be

    Text\Text_Lang\SS\  (vanilla Five Pebbles)
    Text\Text_Lang\SL\  (Shoreline Looks to the Moon)
    
    Text\Text_Lang\DM\  (Spearmaster Looks to the Moon)
    Text\Text_Lang\CL\  (Saint Five Pebbles)
    Text\Text_Lang\ST\  (Challenge 70 oracle))

The text files are *not* the id of the region they're in, that's just how base game defines them.  
Oracle and slugcat specifics can be combined. The priority between them is as follows

    Text\Text_Eng\SL\PearlText-White.txt
    Text\Text_Eng\SL\PearlText.txt
    Text\Text_Eng\PearlText-White.txt
    Text\Text_Eng\PearlText.txt

## <a name="CHALLENGES"></a>Challenges

New CRS challenges can be found by pressing the 'CRS' button in the Challenge menu.  
They can be unlocked by collecting a new purple token in-game, or they can be unlocked by default.

Data for these challenges is read from CustomChallenges.txt  
Basic format is as follows

    UnlockName : Color : Challenge1Name, Challenge2Name, Challenge3Name

UnlockName will be the text displayed in the menus as well as the id for the token.  
Color is the display color of the text in the menu  

Each unlock can have a total of 10 challenges. They can have any name, but should be the name of the files placed in Levels\Challenges. This name is not displayed on the menus.  

Challenge files follow the same format as MSC challenges. You can learn more about how to format the files [here.](https://rainworldmodding.miraheze.org/wiki/Challenges)

By default each group of challenges will be locked until a purple challenge token is collected. Appending ` : UNLOCKED` to the line will leave them unlocked by default.  

### Special Unlock Requirements

Each individual challenge can be given special unlock requirements by appending them to the challenge name in `-{}` ie

    Challenge-{Beaten:Gourmand|Sandbox:PuffBall|Challenge:22-28}
    
The list of special requirements is as follows

- Beaten  
Unlocks when the specified slugcat campaign is complete.  
`{Beaten:Survivor|Beaten:Artificer}`  
Either the display name or the code id will work. Currently does not work for custom slugcats.

- Slugcat  
Unlocks when the arena token for the specified slugcat is collected.  
ie, `{Slugcat:White|Slugcat:Artificer}`  
Here the code id must be used.  

- Safari  
Unlocks when the Safari token for the specified region has been collected.  
`{Safari:LM|Safari:AR}`

- Level  
Unlocks when the token for the specified arena has been collected.  
`{LevelUnlocked:SU_Stoneheads|LevelUnlocked:Man Eater}`  
The level name must be the name of the file rather than the display name

- Sandbox  
Unlocks when the specified sandbox token for a creature or item has been collected.  
`{Sandbox:Mushroom|Sandbox:YellowLizard}`  
The name of the creature or item must exactly match their internal SandboxUnlockID

- Challenge  
Unlocks when the specified challenge or range of challenges has been beaten.  
`{Challenge:62|Challenge:29-35}`  
CRS challenges can also be required by including the challenge unlock ID  
`{Challenge:Cut Challenges 1:1-10|Challenge:Cut Challenges 2:5}`  

Since every mod's challenge info is contained in a single file, mods should extend this file with modification files.  

As an example:  

    CutChallenges\modify\CustomChallenges.txt
        [ADD]Cut Challenges 1 : DB8036 : Cut1, Cut2, Cut3-{Challenge:51}, Cut4, Cut5
        [ADD]Cut Challenges 2 : DB8036 : Cut6, Cut7-{Sandbox:TerrorLongLegs}, Cut8, Cut9, Cut10
        [ADD]Cut Challenges 3 : DB8036 : Cut11, Cut12, Cut13, Cut14, Cut15
        [ADD]Cut Challenges 4 : DB8036 : Cut16-{Sandbox:ElectricSpear|Level:Submerged}, Cut17-{Beaten:Saint}, Cut18,-{Safari:SL} Cut19-{Challenge:64|Challenge:Cut Challenges 4:1-3}
        
    Levels\Challenges\
        Cut1.txt
        Cut1_Meta.txt
        Cut2.txt
        Cut2_Meta.txt
        Cut2_Settings.txt
        Cut3.txt
        Cut3_Meta.txt
            ect...

## <a name="META"></a>MetaProperties
A new file called MetaProperties.txt can be placed in World\XX to define the following meta properties for the region:
* **Add Region to Story Regions (used for Wanderer requirements)**  
* **Add Region to Optional Regions (visitable and has Safari menu)**  
* **Remove Region from Safari Menu (active by default)**  

Proper useage goes like this:  


    White,Yellow,Rivulet : Story  
    Gourmand,Hunter : Optional  
    Safari  

The region will only be a part of the passage requirements for  
Survivor, Monk, and Rivulet  
but Hunter and Gourmand will be able to visit and passage to it  
And the region will appear in the Safari menu  

You can also use X- syntax from the world file  

    X-Saint : Story  

This is available to every slugcat except Saint  

To make a region a story region for every slugcat, simply write

    Story  

Same goes for optional regions.  

    Optional

Safari can't have slugcat conditional syntax,  
as it uses Story\Optional regions for slug-accessibility.  

## <a name="CONDITIONALS"></a>Region Conditional Lines 
A line can be excluded if it doesn't match defined conditions.  
The current available conditions are:  
- Is MSC enabled?  
- Is a specific region acronym present?  
- Is a specific modID active?  

The formatting looks like this  

    {MSC}SU_C04 : SU_CAVE01, SU_PC01, SU_A41
    {!MSC}SU_C04 : DISCONNECTED, SU_PC01, SU_A41

Put an ! anywhere in the condition to invert it  
which in this case makes the 1st line happen if MSC is active  
and the 2nd line happen if MSC is not active  

Conditions can be stacked, like so  

    //this line happens if AR is a region but TR isn't
    {AR,!TR} 
    
    //this line happens if MSC isn't active and PC is a region
    {!MSC,PC} 
    
    //# is used as the first character to mark it as an id
    {#!lb-fgf-m4r-ik.coral-reef} 
    
    //all of these can be applied to creatures as well
    //useful for soft creature dependencies
    {#lb-fgf-m4r-ik.coral-reef}OS_D09 : 3-Polliwog-2
    {#!lb-fgf-m4r-ik.coral-reef}OS_D09 : 3-Salamander
    
    //when combined with slug conditions, the slugcat should come first
    (White,Yellow){MSC}VI_A19 : 2-Yeek-3
    (White,Yellow){!MSC}VI_A19 : 2-CicadaB-3

Due to a bug, any time conditionals are used in the rooms section they should be at the bottom, otherwise items and creatures may start to disappear from shelters.

## <a name="REPLACE"></a>ReplaceRoom

A new conditional link can be used to replace the room files for a particular room for a specific slugcat.

    CONDITIONAL LINKS
    Saint : REPLACEROOM : SU_B14 : SU_B14SAINT
    END CONDITIONAL LINKS

The room will still be known internally as its original name, but the files will be loaded by the new room name instead. So all connections and spawns should not use the new room name and still use the old one.

## <a name="PRIORITIES"></a>Mod Priorities

Mods can now be automatically prioritized over others by including a 'priorities' line in the ModInfo.json. This has the same behavior as a dependency when the mods are there, but the mod can still be enabled without the priorities.  
Formatting is the same as for [requirements](https://rainworldmodding.miraheze.org/wiki/Downpour_Reference/Mod_Directories#ModInfo_JSON) in that it's an array of strings of the mod ids.

    "priorities": ["moreslugcats", "expanded_outskirts"]
