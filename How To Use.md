# How to Use Rain States

This tutorial will show you how to use Rain States in your mod. It assumes  you have already added the mod as a required dependency.

## Setup

You'll need to create a file named `NightCycle.json` inside the `palette` folder of your mod directory:

```
mod/palette/NightCycle.json
```

In this file, you can define how palettes change over time in each region.

## Example

```json
{
    "UW": {
        "palette": [1000, 20, 1002],
        "time": [0, 0.4, 1]
    }
}
```

## JSON Format

Each key in the JSON file should correspond to a region code (e.g., `UW` for the The Exterior region). Each region entry must include two lists:

* **`palette`**: A list of palette IDs that will be used over time in that region.
* **`time`**: A list of decimal values (between 0 and 1) that define when each palette should take full effect.

### Rules:

* The values in `time` must be in strictly increasing order.
* All values must be real numbers within the range `[0, 1]`.
* The length of `palette` and `time` must be the same.
* `0` is for the start of the cycle, and `1` is for the end of the cycle

### Overriding Specific Rooms

If you use a **full room name** instead of a region code (e.g., `UW_A01`), the configuration will override the region palette **only in that specific room**. This allows for more precise control over room-specific visuals.

## Notes

* This mod **overrides the default region palettes**, so any palettes created with Region Kit or developer tools **will not be shown** if that region is configured in `NightCycle.json`.
* Expect future features for regions without rain.

---

If you're creating a mod for Rain World and want dynamic visual changes based on time or weather, this setup allows you to control it easily using JSON and no code.

Happy modding!
