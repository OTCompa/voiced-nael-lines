# Voiced Nael Quotes
Voiced Nael Quotes aims to add voice acting to Nael quotes in The Unending Coil of Bahamut, a raid in FINAL FANTASY XIV.

## Main Points
- Adds voice acting to Nael quotes 
- Use different voice packs to your liking (currently only tik-tok and teto. LF VAs!)
- (Optional) Directional in-world audio with fall-off
- Support for different client languages (Have only tested EN, JA, DE, FR, YMMV for others)

## How It Works
The directional audio utilizes a custom VFX (no visuals) with a sound attached to simulate in-world audio. The custom VFX and sound files are loaded through a process based on Penumbra's
runtime mod loading/file replacements.

## How To Use
This plugin uses the "Sound Effects" volume in your game client. Please ensure sound effects are audible in-game.  
### Getting Started
- Type `/xlsettings` in the chatbox or open up Dalamud's settings menu
- Open the "Experimental" tab and scroll down to the "Custom Plugin Repositories" section

`https://raw.githubusercontent.com/OTCompa/frey-s-dalamud-plugins/refs/heads/main/plogon.json`
- Paste the link above into the bottom-most textbox of the section and click the "+" button to the right
- Click on the save button on the bottom right corner of the window
- Type `/xlplugins` in the chatbox or open up Dalamud's plugin installer menu
- Search for `Voiced Nael Quotes` and install.

Once installed, you can open the config menu through the plugin installer to select one of the available voicepacks. 

### Building
1. Open up `VoicedNaelQuotes.sln` in your C# editor of choice (likely [Visual Studio 2022](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release` in your IDE.
3. The resulting plugin can be found at `VoicedNaelQuotes/bin/x64/Debug/VoicedNaelQuotes.dll` (or `Release` if appropriate.)

### Making Soundpacks
Currently, the process for making soundpacks is very cumbersome. You must manually import each corresponding audio file 
and export each one as a .scd with [VfxEditor](https://github.com/0ceal0t/Dalamud-VFXEditor), using any of the existing
.scd or .vfxworkspace template in this repository as a base.  
In the future, I'd like to make this process easier and independent of VFXEditor but that requires looking into the .scd file format
which I am currently not keen on researching at this very moment.

## Credits
Voiced Nael Quotes heavily uses code from different projects and would not be possible if these did not exist.
Huge thanks to:
- [RaidsRewritten](https://github.com/Ricimon/FFXIV-RaidsRewritten)
- [VfxEditor](https://github.com/0ceal0t/Dalamud-VFXEditor)
- [Penumbra](https://github.com/xivdev/Penumbra)
- [BigNaelQuotes](https://github.com/hunter2actual/BigNaelQuotes)
