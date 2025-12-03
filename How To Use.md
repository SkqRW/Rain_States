# How to Use Rain States

> **Version:** 0.4 | **Last Updated:** November 2025

This tutorial will show you how to use Rain States in your mod to create dynamic palette transitions throughout the rain cycle. **No programming knowledge required!**

Rain States offers two different systems for managing palettes:

1. **`palettes/` folder** - Quick testing and prototyping (Quick test and hot reaload available)
2. **`RainState/` folder** - Production-ready cycle system (recommended for publishing)

This guide assumes you have already added Rain States as a **required dependency** for your mod.

---

## Table of Contents

1. [Quick Start (palettes/ folder)](#quick-start-palettes-folder)
2. [JSON Format Reference](#json-format-reference)
3. [Step-by-Step Tutorial](#step-by-step-tutorial)
4. [Production System (RainState/ folder)](#production-system-rainstate-folder)
5. [Advanced Features](#advanced-features)
6. [Troubleshooting](#troubleshooting)

---

## Quick Start (palettes/ folder)

**Best for:** Testing, quick experiments, and learning the mod

### Setup

Create a file named `RainStates.json` inside the `palettes` folder of your mod:

```text
YourModName/
  palettes/
    RainStates.json
```

Rain States will **automatically detect and load** this file when the game starts.

> **Hot Reload:** Files in the `palettes/` folder are **automatically reloaded** when you save changes. You can edit your JSON and see results immediately without restarting the game!

> **Note:** This mod accepts any file that **starts with** `RainState` and **ends with** `.json`:
> - `RainState_SU.json` ✓
> - `RainState_Night.json` ✓
> - `RainStateWatcher.json` ✓
> - `RainStates.json` ✓

### Limitations

⚠️ **This system is for testing only.** For published mods, use the [RainState/ folder system](#production-system-rainstate-folder).

- Cannot define multiple cycle configurations per region
- Stuff for speed, but later a do a option to disable hot reaload for players by default

---

## Production System (RainState/ folder)

**Best for:** Publishing mods, complex cycle systems, per-room customization

### Why Use This System?

✅ **Multiple cycle configurations** - Define different palettes for different times of the cycle  
✅ **Per-room control** - Customize specific rooms independently  
✅ **Better organization** - Separate files for each cycle state

### Setup

Create a `RainState` folder in your mod directory with the following structure:

```text
YourModName/
  RainState/
    [RegionName]/           ← Region code (SU, UW, HI, etc.)
      01_morning.json       ← Cycle configuration files
      02_afternoon.json
      03_evening.json
      [RoomName]/           ← (Optional) Room-specific folder
        01_special.json
```

### File Naming Rules

**Format:** `[NN]_description.json`

- **NN** = Two-digit number (00-99)
- **description** = Any text you want (optional) (and without spaces)

**Examples:**
```
01_dawn.json          ← Loads first
02_morning.json       ← Loads second
05_noon.json          ← Loads third
10_afternoon.json     ← Loads fourth
20_dusk.json          ← Loads fifth
99_midnight.json      ← Loads sixth
```

**Important:** The order is determined by the **first 2 characters**, not the name. Files are automatically sorted and indexed sequentially (0, 1, 2, 3...).

### How It Works

1. **Files are sorted** by their 2-digit prefix (05 → 10 → 20)
2. **Indices are mapped** sequentially (05=0, 10=1, 20=2)
3. **Your code accesses** them by sequential index (0, 1, 2...)

**No hot reload** - You must restart the game to see changes (better performance).

---

## JSON Format Reference

### For palettes/ folder (Simple System)

Each key should be a region code (e.g., `UW`, `SU`, `HI`) or a full room name (e.g., `UW_A01`).

**Example:**
```json
{
    "UW": {
        "palette": [1, 20, 3],
        "time": [0, 0.6, 1.0],
        "effectA": ["#FF5500", "#00AAFF", "#FFFFFF"],
        "effectATime": [0, 0.5, 1.0]
    },
    "SU": {
        "palette": [1, 12, 11],
        "time": [0, 0.5, 1.0],
        "effectB": ["#FF1493", "#1E90FF"],
        "effectBTime": [0, 1.0]
    }
}
```

### For RainState/ folder (Cycle System)

Each file contains a **single configuration** without the region/room wrapper.

**Example (`RainState/SU/01_morning.json`):**
```json
{
    "palette": [1, 5, 10],
    "time": [0, 0.5, 1.0],
    "effectA": ["#FFA500", "#FFD700", "#FFFFFF"],
    "effectATime": [0, 0.5, 1.0]
}
```

**Example (`RainState/SU/10_evening.json`):**
```json
{
    "palette": [20, 25, 29],
    "time": [0, 0.5, 1.0],
    "effectA": ["#191970", "#000080", "#000000"],
    "effectATime": [0, 0.6, 1.0]
}
```

---

### Properties

#### Base Palette (Required)

* **`palette`**: List of palette numbers (IDs) that the room will transition through
  * Example: `[1, 5, 10, 15]` means palette 1 → 5 → 10 → 15
  * You can find palette numbers in the game's level editor
  
* **`time`**: When each palette becomes fully active (0.0 to 1.0)
  * Example: `[0, 0.3, 0.6, 1.0]`
  * `0` = start of rain cycle (no rain)
  * `0.5` = middle of cycle (moderate rain)
  * `1.0` = end of cycle (heavy rain)
  * ⚠️ **Must always start with 0**
  * ⚠️ Must be in **increasing order**

#### Effect Palettes (Optional)

Effect palettes control additional visual effects **independently** from the main palette. Use **color codes** to create custom colors!

**🎨 What are effect palettes?**
- `effectA` and `effectB` are separate color channels
- They control things like fog, lighting, atmospheric effects
- Each effect has its own timing, independent from the base palette
- Colors blend smoothly between transitions

**Effect A:**
* **`effectA`**: List of custom colors in hexadecimal format
  * Example: `["#FF5500", "#00AAFF", "#FFFFFF"]`
  * Colors transition: Orange → Blue → White
  
* **`effectATime`**: When each color becomes fully active (0.0 to 1.0)
  * Example: `[0, 0.5, 1.0]`
  * ⚠️ **Must always start with 0**
  * ⚠️ Must match the length of `effectA`

**Effect B:**
* **`effectB`**: List of custom colors in hexadecimal format
  * Example: `["#FF0000", "#00FF00", "#0000FF"]`
  * Colors transition: Red → Green → Blue
  
* **`effectBTime`**: When each color becomes fully active (0.0 to 1.0)
  * Example: `[0, 0.5, 1.0]`
  * ⚠️ **Must always start with 0**
  * ⚠️ Must match the length of `effectB`

**🎨 Hexadecimal Color Guide:**

Supported formats:
- `"#RRGGBB"` (recommended) - Full format with hash
- `"RRGGBB"` - Full format without hash
- `"#RGB"` - Short format (e.g., `#F0F` = `#FF00FF`)
- `"RGB"` - Short format without hash

**Common Colors:**
| Color | Hex Code | Visual |
|-------|----------|--------|
| Red | `"#FF0000"` | 🔴 |
| Green | `"#00FF00"` | 🟢 |
| Blue | `"#0000FF"` | 🔵 |
| Yellow | `"#FFFF00"` | 🟡 |
| Orange | `"#FFA500"` | 🟠 |
| Purple | `"#800080"` | 🟣 |
| Pink | `"#FF1493"` | 💖 |
| Cyan | `"#00FFFF"` | 🩵 |
| White | `"#FFFFFF"` | ⚪ |
| Black | `"#000000"` | ⚫ |

**Pro Tip:** Use online color pickers (like Google's "color picker") to find hex codes for any color you want!

---

## Step-by-Step Tutorial

### Tutorial 1: Simple Region Palette (palettes/ folder)

**Goal:** Make Outskirts (SU) transition from bright to dark throughout the rain cycle.

**Step 1:** Create the file structure
```text
YourMod/
  palettes/
    RainStates.json
```

**Step 2:** Add this to `RainStates.json`:
```json
{
    "SU": {
        "palette": [1, 5, 10],
        "time": [0, 0.5, 1.0]
    }
}
```

**What this does:**
- At cycle start (0): Uses palette 1 (bright)
- At cycle middle (0.5): Uses palette 5 (medium)
- At cycle end (1.0): Uses palette 10 (dark)
- The game **automatically blends** between these palettes!

**Step 3:** Save the file and load Outskirts in-game. You should see smooth palette transitions!

---

### Tutorial 2: Adding Custom Effects (palettes/ folder)

**Goal:** Add orange fog that turns blue as rain intensifies.

**Step 1:** Modify your `RainStates.json`:
```json
{
    "SU": {
        "palette": [1, 5, 10],
        "time": [0, 0.5, 1.0],
        "effectA": ["#FFA500", "#87CEEB", "#4682B4"],
        "effectATime": [0, 0.5, 1.0]
    }
}
```

**What this does:**
- Main palette still transitions 1 → 5 → 10
- Fog color transitions: Orange → Sky Blue → Steel Blue
- Both transitions happen smoothly and independently!

---

### Tutorial 3: Room-Specific Override (palettes/ folder)

**Goal:** Make one specific room different from the rest of the region.

**Step 1:** Add a room-specific entry:
```json
{
    "SU": {
        "palette": [1, 5, 10],
        "time": [0, 0.5, 1.0]
    },
    "SU_A01": {
        "palette": [15, 20, 25],
        "time": [0, 0.5, 1.0],
        "effectA": ["#FF0000", "#8B0000"],
        "effectATime": [0, 1.0]
    }
}
```

**What this does:**
- All Outskirts rooms use palettes 1 → 5 → 10
- **Except** room SU_A01, which uses palettes 15 → 20 → 25
- SU_A01 also has a red effect that darkens

---

### Tutorial 4: Multiple Cycle States (RainState/ folder)

**Goal:** Create different atmospheric states (morning, noon, evening) for your region.

**Step 1:** Create the folder structure:
```text
YourMod/
  RainState/
    SU/
      01_morning.json
      05_noon.json
      10_evening.json
```

**Step 2:** Create `01_morning.json`:
```json
{
    "palette": [1, 3, 5],
    "time": [0, 0.5, 1.0],
    "effectA": ["#FFE4B5", "#FFA500", "#FF8C00"],
    "effectATime": [0, 0.5, 1.0]
}
```

**Step 3:** Create `05_noon.json`:
```json
{
    "palette": [5, 10, 15],
    "time": [0, 0.5, 1.0],
    "effectA": ["#87CEEB", "#4682B4", "#1E90FF"],
    "effectATime": [0, 0.5, 1.0]
}
```

**Step 4:** Create `10_evening.json`:
```json
{
    "palette": [15, 20, 25],
    "time": [0, 0.5, 1.0],
    "effectA": ["#800080", "#4B0082", "#000000"],
    "effectATime": [0, 0.5, 1.0]
}
```
---

### Tutorial 5: Room-Specific Cycles (RainState/ folder)

**Goal:** One specific room has different cycle states than the rest of the region.

**Step 1:** Create the folder structure:
```text
YourMod/
  RainState/
    SU/
      01_default.json
      02_default.json
      SU_C04/              ← Room-specific folder
        01_special.json
        02_special.json
```

**Step 2:** The files in `SU/` apply to the **entire region**

**Step 3:** The files in `SU/SU_C04/` apply **only to room SU_C04**

**What this does:**
- All Outskirts rooms use the configurations from `SU/01_default.json` and `SU/02_default.json`
- **Except** room SU_C04, which uses `SU/SU_C04/01_special.json` and `SU/SU_C04/02_special.json`

---

## Advanced Features

### Flexible File Numbering

You don't have to use consecutive numbers! Use whatever makes sense for your organization:

```text
RainState/SU/
  01_dawn.json
  05_morning.json
  10_noon.json
  20_afternoon.json
  50_dusk.json
  99_night.json
```

**Result:** Files are automatically sorted (01 → 05 → 10 → 20 → 50 → 99) and indexed (0, 1, 2, 3, 4, 5).

### Multiple Effect Channels

You can use both `effectA` and `effectB` simultaneously:

```json
{
    "palette": [1, 10],
    "time": [0, 1.0],
    "effectA": ["#FFA500", "#FFD700"],
    "effectATime": [0, 1.0],
    "effectB": ["#87CEEB", "#4682B4"],
    "effectBTime": [0, 1.0]
}
```

### Different Timing for Effects

Effect timings don't have to match the base palette timing:

```json
{
    "palette": [1, 5, 10],
    "time": [0, 0.5, 1.0],
    "effectA": ["#FF0000", "#00FF00", "#0000FF", "#FFFF00"],
    "effectATime": [0, 0.3, 0.6, 1.0]
}
```

Here, the base palette transitions 3 times, but effectA transitions 4 times!

### Mixing Both Systems

You can use **both** systems in the same mod:
- Use `palettes/` for quick testing
- Use `RainState/` for final production configurations

**Priority:** If both exist for the same region, palettes/ takes priority.

---

## Rules and Requirements

✅ **Required:**
- `palette` and his `time` arrays are **mandatory**
- `time` arrays must **always start with 0**
- `time` values must be **in increasing order**
- Each palette/effect array must be **the same length** as its time array
- Time values must be between **0 and 1**

✅ **Optional:**
- `effectA` and `effectATime` (completely optional)
- `effectB` and `effectBTime` (completely optional)
- You can use one effect, both effects, or no effects

✅ **File naming (RainState/ folder only):**
- First 2 characters **must be digits** (00-99)
- Rest of the filename can be anything you want
- Extension must be `.json`

---

## Troubleshooting

### My palettes aren't loading!

**Check:**
1. Is your JSON file in the correct folder?
   - `palettes/` folder: Must start with `RainState` and end with `.json`
   - `RainState/` folder: Must be in `RainState/[RegionName]/NN_*.json`
2. Is your JSON valid? Use a JSON validator online
3. Check the game's log file for error messages
4. Make sure you're using the correct region codes (SU, UW, HI, etc.)

### My palettes aren't changing in-game!

**Check:**
1. Are you in the correct region?
2. For `palettes/` folder: Did you save the file? (Changes are instant)
3. For `RainState/` folder: Did you restart the game? (No hot reload)
4. Is the rain cycle actually progressing? Time must pass in-game

### My colors look wrong!

**Check:**
1. Are you using hexadecimal format? (`"#FF5500"` not `"255, 85, 0"`)
2. Did you include the quotes? (`"#FF5500"` not `#FF5500`)
3. Are your time values correct? (Between 0 and 1)

### File naming doesn't work (RainState/ folder)!

**Check:**
1. Do your filenames start with **exactly 2 digits**?
   - ✅ `01_dawn.json`
   - ✅ `99_night.json`
   - ❌ `1_dawn.json` (only 1 digit)
   - ❌ `dawn_01.json` (digits not at start)
2. Are the digits actually numbers?
   - ✅ `01_dawn.json`
   - ❌ `0O_dawn.json` (letter O instead of zero)

### Arrays have different lengths!

**Problem:** `palette` and `time` must be the same length!

**Wrong:**
```json
{
    "palette": [1, 5, 10],
    "time": [0, 1.0]
}
```

**Correct:**
```json
{
    "palette": [1, 5, 10],
    "time": [0, 0.5, 1.0]
}
```

### Time values are out of order!

**Problem:** Time values must be in **increasing order**!

**Wrong:**
```json
{
    "time": [0, 0.8, 0.5, 1.0]
}
```

**Correct:**
```json
{
    "time": [0, 0.5, 0.8, 1.0]
}
```

---

**Recommendation:** Start with `palettes/` to learn and experiment. When ready to publish, migrate to `RainState/` for better organization and features.

---

## Quick Reference for RainState/ JSON Format

### Minimal Configuration
```json
{
    "palette": [1, 10],
    "time": [0, 1.0]
}
```

### Full Configuration
```json
{
    "palette": [1, 5, 10, 15],
    "time": [0, 0.3, 0.6, 1.0],
    "effectA": ["#FFA500", "#FFD700", "#FFFFFF"],
    "effectATime": [0, 0.5, 1.0],
    "effectB": ["#87CEEB", "#4682B4", "#1E90FF"],
    "effectBTime": [0, 0.5, 1.0]
}
```


---

## Additional Notes

* **Mod conflicts:** If multiple mods define palettes for the same region/room, the last loaded mod takes priority (this can change in future versions)
* **Default palettes:** This mod overrides default region palettes. Custom palettes set with dev tools won't show if configured in Rain States
* **Performance:** RainState/ folder has better performance (loads once at startup)

---

**Need more help?** You can contact me at discord: Skq_rw

Happy modding! 🌧️
