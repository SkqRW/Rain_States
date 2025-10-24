using System.Linq;
using JsonGet;
using UnityEngine;

namespace RoomChange;

public partial class PaletteDrive
{

    public static string GetCurrentRegionName()
    {
        return currentRegionName;
    }
    public static void SetRegionPalette(JsonGet.PaletteInfo newPaletteInfo)
    {
        activeRegionPalette = newPaletteInfo;
        totalPalettes = activeRegionPalette.palette.Count;
        NewRangePalette();
    }

    private static void NewRangePalette()
    {
        for (int i = 0; i < totalPalettes; i++)
        {
            float endTimePalette = GetRelativeCycleTimeConfig(activeRegionPalette.time[i], rainCycleLength);
            if (actualTime < endTimePalette)
            {
                RefreshPaletteInterval(i);
                return;
            }
        }
        paletteIndex = totalPalettes;
    }

    private static float GetRelativeCycleTimeConfig(float time, int cycleLength)
    {
        return time * cycleLength;
    }

    private static void RefreshPaletteInterval(int index)
    {
        if (index < 0)
        {
            PDEBUG.LogWarn($"Index {index} out of bounds in Palette Interval");
            return;
        }
        
        nextPaletteTime = GetRelativeCycleTimeConfig(activeRegionPalette.time[index], rainCycleLength);
        lastPaletteTime = 0;
        paletteIndex = index;

        if (index == 0)
        {
            // Palette interval here is [0, 0]
            PDEBUG.LogWarn($"Palette index 0 detected — forcing single-palette mode\"");
            return;
        }
        lastPaletteTime = rainCycleLength * activeRegionPalette.time[index - 1];
        PDEBUG.Log($"Palette interval set: [{lastPaletteTime}, {nextPaletteTime}] for index {index} in region {currentRegionName}");
    }
}



public static class RateChanges
{
    const float epsilon = 0.0001f;

    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        if (Mathf.Abs(time - pretime) < epsilon)
        {
            PDEBUG.Log("Division by zero in RateChanges.Linear");
            return 0f;
        }

        float delta = (now - pretime) / (time - pretime);
        PDEBUG.Log($"Actual Time: {now}, nextPaletteTime: {time}, prevPaletteTime: {pretime}, paletteBlend: %{delta * 100}");
        return delta;
    }
}

