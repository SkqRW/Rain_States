# How to Use Rain States

This tutorial will show you how to use Rain States in your mod. It assumes you have already added the mod as a **required dependency**.

## Setup

Create a file named `RainStates.json` inside the `palettes` folder of **your mod directory**:

```text
YourModName/
  palettes/
    RainStates.json
```

Rain States will **automatically detect and load** this file when the game starts.

## Example

```json
{
    "UW": {
        "palette": [1, 20, 3],
        "time": [0, 0.6, 1.0],
        "effectA": [0, 5, 10],
        "effectATime": [0, 0.5, 1.0]
    },
    "UW_A13": {
        "palette": [11, 5, 1],
        "time": [0, 0.90, 1.0]
    },
    "SU": {
        "palette": [1, 12, 11],
        "time": [0, 0.5, 0.6],
        "effectA": [2, 8],
        "effectATime": [0, 1.0],
        "effectB": [1, 3, 7],
        "effectBTime": [0, 0.4, 1.0]
    }
}
```

## JSON Format

Each key in the JSON file should correspond to a region code (e.g., `UW` for The Exterior region) or a room-specific name. Each entry can include the following properties:

### Base Palette (Required)

* **`palette`**: A list of palette IDs that will be used over time in that region.
* **`time`**: A list of decimal values (between 0 and 1) that define when each palette should take full effect.
  * This first value must always start at 0.

### Effect Palettes (Optional)

Effect palettes control additional visual effects independent from the main palette:

* **`effectA`**: A list of effect A palette IDs that will be applied over time.
* **`effectATime`**: A list of decimal values (between 0 and 1) defining when each effect A palette should be applied.
  * This first value must always start at 0.

* **`effectB`**: A list of effect B palette IDs that will be applied over time.
* **`effectBTime`**: A list of decimal values (between 0 and 1) defining when each effect B palette should be applied.
  * This first value must always start at 0.

Effect palettes allow you to change visual effects like fog, lighting, or atmospheric colors **independently** from the main room palette, giving you fine-grained control over the room's appearance throughout the cycle.

### Rules

* `time`, `effectATime`, and `effectBTime` values must be in **strictly increasing order**.
* Each palette list (`palette`, `effectA`, `effectB`) and its corresponding time list must be the **same length**.
* Time values must be real numbers within `[0, 1]`.
  * `0` = start of the cycle, and `1` = end of the cycle
* Always start each time list with `0`.
* Effect palettes are **optional** — you can include only `effectA`, only `effectB`, both, or neither.
* Each effect palette works **independently** with its own timing.

### Overriding Specific Rooms

If you use a **full room name** (e.g., `UW_A01`) instead of a region code, that entry will override the region's palette **only for that specific room**.
This allows for more precise control over room-specific visual palettes.

## Notes

* If multiple mods define palettes for the same region/room, the **last loaded mod takes priority** (this behavior may change in future updates).
* Files are **automatically reloaded** — you can edit and save your JSON to see changes instantly, without restarting the game.
* This mod **overrides the default region palettes**, so custom palettes putin with dev tools **won't be shown** if that region is configured in `RainStates.json`.
* The `$schema` line is **optional** but recommended for editor assistance in VS Code (auto-complete and validation).

Happy modding!
