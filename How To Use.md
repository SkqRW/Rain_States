# How to Use Rain States

This tutorial will show you how to use Rain States in your mod. It assumes you have already added the mod as a required dependency.

## Setup

Create a file named `RainStates.json` inside the `palettes` folder of **your mod directory**:

```
YourModName/
  palettes/
    RainStates.json
```

Rain States will **automatically discover and load** all `RainStates.json` files from every installed mod during game startup.

## Example

```json
{
    "$schema": "./palette.schema.json",
    "UW": {
        "palette": [1000, 20, 1002],
        "time": [0.2, 0.6, 1.0]
    }
}
```

## JSON Format

Each key in the JSON file should correspond to a region code (e.g., `UW` for the The Exterior region). Each region entry must include two lists:

* **`palette`**: A list of palette IDs that will be used over time in that region.
* **`time`**: A list of decimal values (between 0 and 1) that define when each palette should take full effect.

### Rules

* The values in `time` must be in strictly increasing order.
* All values must be real numbers within the range `(0, 1]` (the last value is usually `1`).
* The length of `palette` and `time` must be the same (last time marks the end of the last palette).
* `0`�is for the start of the cycle, and�`1`�is for the end of the cycle

### Overriding Specific Rooms

If you use a **full room name** instead of a region code (e.g., `UW_A01`), the configuration will override the region palette **only in that specific room**. This allows for more precise control over room-specific visuals.

## Notes

* Rain States **automatically scans all installed mods** for `RainStates.json` files in their `palettes` folders.
* If multiple mods define palettes for the same region/room, the **last loaded mod wins** (overwrite behavior).
* Each file is monitored for changes, so you can **edit and save** your JSON and see changes in real-time without restarting.
* This mod **overrides the default region palettes**, so any palettes created with Region Kit or developer tools **will not be shown** if that region is configured in `RainStates.json`.
* The `$schema` line is **optional** but recommended for editor assistance in VS Code (auto-complete and validation).
* Expect future features for regions without rain.

## Multiple Mods Support

You can have multiple mods each providing their own `RainStates.json`:

```plaintext
RainWorld_Data/StreamingAssets/mods/
  ModA/
    palettes/
      RainStates.json  <- Defines palettes for "SU", "HI"
  ModB/
    palettes/
      RainStates.json  <- Defines palettes for "UW", "SL"
  YourMod/
    palettes/
      RainStates.json  <- Your custom regions
```

All files will be loaded and merged automatically. If two mods define the same region/room key, the one loaded last will take priority.

---

If you're creating a mod for Rain World and want dynamic visual changes based on time or weather, this setup allows you to control it easily using JSON and no code.

Happy modding!
