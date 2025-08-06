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
        for(int i=0; i<totalPalettes; i++)
        {
            if(actualTime < activeRegionPalette.palette[i] * rainCycleLength)
            {
                paletteIndex = Mathf.Max(1, i);
                RefreshPaletteInterval(paletteIndex);
                return;
            }
        }
        paletteIndex = totalPalettes;
    }

    private static void RefreshPaletteInterval(int index)
    {
        if(index == 0)
        {
            PDEBUG.LogWarn($"Index {index} out of bounds in Palette Interval");
            return;
        }
        
        nextPaletteTime = rainCycleLength * activeRegionPalette.time[index];
        lastPaletteTime = rainCycleLength * activeRegionPalette.time[index - 1];
        paletteIndex = index;
    }
}

public static class RateChanges
{
    const float epsilon = 0.0001f;

    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        if(Mathf.Abs(time - pretime) < epsilon)
        {
            PDEBUG.Log("Division by zero in RateChanges.Linear");
            return 0f;
        } 

        float delta = (now - pretime) / (time - pretime);
        PDEBUG.Log($"Actual Time: {now}, nextPaletteTime: {time}, prevPaletteTime: {pretime}, paletteBlend: %{delta*100}");
        return delta;
    }
}

