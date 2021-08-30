# Required libraries
You must include the following libraries in this folder:

* Rain World's `Assembly.CSharp.dll` and `HOOKS-Assembly-CSharp.dll`, both made public.
* topicular's `ConfigMachine.dll`
* bee's `EnumExtender.dll`
* 0x0ade's `MonoMod.exe` and `MonoMod.RuntimeDetour.dll`
* ZandrXandr's `Partiality.dll`
* `UnityEngine.dll`

If you are not familiar with any of these, it is probably a good idea to get learn about Rain World Modding first:
* [Modding wiki](https://rain-world-modding.github.io/rain-world-modding/) (it is under construction).
* Rain World's [official discord](discord.gg/rainworld).

The VisualStudio project for this mod is configured to run `build.bat` file in this folder as a post-build event, add any commands you want to run in your own version of this file.