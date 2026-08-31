# SunAway

A tiny Dalamud plugin that removes the sun while you're indoors.

When you enter a housing interior, SunAway turns off the game's glare options
(`Glare_DX11` and, optionally, `GlareRepresentation_DX11`) so sunlight stops
blasting through the walls and windows. The moment you step back outside — or
disable/unload the plugin — your original graphics settings are restored
exactly as they were.

## Usage

- `/sunaway` — open the settings window
- `/sunaway on` / `off` / `toggle` — switch indoor sun removal

## How it works

Every frame it checks `HousingManager.IsInside()` (FFXIVClientStructs). On the
transition to indoors it saves your current glare values and zeroes them via
Dalamud's `IGameConfig`; on the transition back out it writes the saved values
back. No hooks, no memory patching — just the same settings the in-game
graphics menu writes, so it's stable across game patches.

Known limitation: if you change the glare settings in the game's own graphics
menu *while* SunAway is suppressing them, the pre-suppression values win when
you go back outside.

## Building

Requires the Dalamud dev libraries (`~/.xlcore/dalamud/Hooks/dev` on Linux via
XIVLauncher-Core, or `%AppData%\XIVLauncher\addon\Hooks\dev` on Windows).

```
dotnet build -c Release
```

The packaged plugin lands in `SunAway/bin/x64/Release/SunAway/`.
