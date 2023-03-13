## Warning! You are seeing the *legacy* version of CRS (Rain World v1.5), for the Downpour/RW v1.9, please visit [here](https://github.com/Garrakx/Custom-Regions/tree/dp-release)!
***
# Custom Regions Support
***
## Adds various meta features to custom regions
![Custom Regions!](./Images/CRS_thumb.png)

## <a name="index"></a>Index

### <a name="HOW TO COMPILE"></a>How to compile
1. Place references in "lib" folder

Creating the System Variable:
2. type env in the windows search bar, 
3. choose the first one
4. Press n
5. add a new system variable called `RainWorldDir`

### <a name="FEATURES"></a>Feature List

* **Custom Procedural Music**  
(Add just like vanilla does)


* **Region Landscape Art**  
Uses a new config file with the same name as landscape folder 
to assign depths and optionally shaders - old depths file will still work.


* **Custom Level Unlocks**  
Same as before but customUnlocks.txt goes in the root mods folder instead of the Levels folder  
(don't want the game trying to load it as an arena)  

* **Add Region to Story Regions (used for Wanderer requirements)**  
* **Add Region to Optional Regions (visitable and has Safari menu)**  
* **Remove Region from Safari Menu (active by default)**  

The above are assigned with the new MetaProperties.txt  
Which goes in World\XX  
Proper useage goes like this:  

    White,Yellow,Rivulet : Story  
    Gourmand,Hunter : Optional  
    NoSafari  

The region will only be a part of the passage requirements for  
Survivor, Monk, and Rivulet  
but Hunter and Gourmand will be able to visit and passage to it  
And the region will not appear in the safari menu  

You can also use X- syntax from the world file  

    X-Saint : Story  

This is available to every slugcat except Saint  
Currently, the most efficient way to 
make a region accessible to every slug is to  

    X-. : Story  

Because this is only not available to a slug that doesn't exist  
(I need to change the implementation of this lol)  

* **Custom Pearls**  

Add a folder in your regionpack called 'CustomPearls'  
inside it, put a file with your pearl's name  
and with the contents of the file being  

    Color : HighlightColor : ConvoFileName  

and then inside Text\text-eng\ (or another language)  
put the file with the pearl conversation in with the old formatting  

Here's what this might look like  

    oldnewhorizons\CustomPearls
        CF_Intake.txt
            eae275 : fbff00 : ONH1
            
        OS_Refinery.txt
            F28130 : f4ea7a : ONH2
            
    oldnewhorizons\text\text_eng
        ONH1.txt
            0-46
            First line of the first text box.<LINE>Second line of the first text box.
            This line will be shown in a second text box!
            
        ONH2.txt
            0-118
            The 2nd number in the 1st line literally doesn't matter rn
            I removed that check because it's annoying
	