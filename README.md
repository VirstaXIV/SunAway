# SunAway

A tiny Dalamud plugin that gets rid of the void sun — the sun that shines
through the walls while you're inside a housing interior.

When you enter a housing interior, SunAway zeroes the environment state's
`SkyVisibility` (EnvState + 0xE4, in the fog block) every frame, which stops the
sun from rendering through the walls and windows. The game recomputes the
environment state each frame, so everything is back to normal the instant you
step outside, disable the plugin, or unload it — there is nothing to restore.

## Installation

1. In game, open Dalamud settings with `/xlsettings`.
2. Go to the **Experimental** tab and find **Custom Plugin Repositories**.
3. Add the following URL and click the + button, then save:

   ```
   https://raw.githubusercontent.com/VirstaXIV/SunAway/master/repo.json
   ```

4. Open the plugin installer with `/xlplugins`, search for **SunAway**, and
   install it.

## Usage

`/sunaway` opens the settings window — a single on/off checkbox.

## How it works

Every frame it checks `HousingManager.IsInside()` (FFXIVClientStructs). The
override is applied from a hook on the game's `EnvStateCopy` function (same
signature Ktisis uses) so it lands after the game rebuilds the environment
state from weather data; if the signature ever breaks, it falls back to writing
on every framework tick.

## Building

Requires the Dalamud dev libraries (`~/.xlcore/dalamud/Hooks/dev` on Linux via
XIVLauncher-Core, or `%AppData%\XIVLauncher\addon\Hooks\dev` on Windows).

```
dotnet build -c Release
```

The packaged plugin lands in `SunAway/bin/Release/SunAway/`. To test, add
`SunAway/bin/Release/SunAway.dll` as a Dev Plugin Location in `/xlsettings`.
