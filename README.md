# SunAway

A tiny Dalamud plugin that removes the sun while you're indoors.

When you enter a housing interior, SunAway zeroes the environment state's
`SkyVisibility` (EnvState + 0xE4, in the fog block) every frame, which stops the
sun from rendering through the walls and windows. The game recomputes the
environment state each frame, so everything is back to normal the instant you
step outside, disable the plugin, or unload it — there is nothing to restore.

Optionally it can also turn off the glare graphics options (`Glare_DX11` /
`GlareRepresentation_DX11`) while indoors; those are real config writes and are
saved/restored around suppression.

## Usage

- `/sunaway` — open the settings window
- `/sunaway on` / `off` / `toggle` — switch indoor sun removal
- `/sunaway lab` — open the Env Lab (see below)

## How it works

Every frame it checks `HousingManager.IsInside()` (FFXIVClientStructs). The
override is applied from a hook on the game's `EnvStateCopy` function (same
signature Ktisis uses) so it lands after the game rebuilds the environment
state from weather data; if the signature ever breaks, it falls back to writing
on every framework tick.

The `SkyVisibility` mechanism was found empirically with the built-in **Env
Lab** window: it shows the live environment state (named fields from Ktisis'
offsets plus every unmapped float) and lets each value be forced to zero while
you watch the sky. It's left in for future environment spelunking.

Caveat: `SkyVisibility = 0` hides celestial sky rendering as a whole, so at
night the moon and stars are hidden indoors too.

## Building

Requires the Dalamud dev libraries (`~/.xlcore/dalamud/Hooks/dev` on Linux via
XIVLauncher-Core, or `%AppData%\XIVLauncher\addon\Hooks\dev` on Windows).

```
dotnet build -c Release
```

The packaged plugin lands in `SunAway/bin/Release/SunAway/`. To test, add
`SunAway/bin/Release/SunAway.dll` as a Dev Plugin Location in `/xlsettings`.
