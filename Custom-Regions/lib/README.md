# Getting references for Custom Regions Support (and most Partiality mods)

Custom Regions requires several DLLs to compile correctly.

* `MonoMod.exe`, `MonoMod.RuntimeDetour.dll`, `Partiality.dll`
	* These are all under MIT license and are directly referenced from this repository. No action is required for them. :)
* `EnumExtender.dll`,  `ConfigMachine.dll`
	* Mod DLLs can be found on [raindb.net](http://www.raindb.net/) under Tools or by asking around on the [Rain World Discord server](https://discord.gg/rainworld).
* `UnityEngine.dll`
	* This can be taken directly from the `Rain World\RainWorld_Data\Managed` directory and put into the references folder.
* `Assembly-CSharp.dll`, `HOOKS-Assembly-CSharp.dll`
	* These assemblies are publicized versions of their originals and acquiring them is described below.

## Steps to acquiring necessary references
The `Assembly-CSharp.dll` and `HOOKS-Assembly-CSharp.dll` assemblies need to be generated using the tools presented in `libs`.
1. Unzip MonoMod.HookGen.zip/move ConsoleStubber.exe into the Managed directory.
2. Run `ConsoleStubber Assembly-CSharp.dll` in cmd from the Managed directory. Output it to a new file. The result is the publicized Assembly-CSharp assembly.
3. Run `MonoMod.RuntimeDetour.HookGen Assembly-CSharp.refstub.dll` in cmd from the Managed directory. The result is the publicized HOOKS-Assembly-CSharp assembly.
4. Rename the assemblies accordingly and move them into your reference folder.
5. Clean up the Managed assembly and any mess you made.
