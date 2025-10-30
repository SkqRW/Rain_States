using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace RoomChange;

public class PaletteData
{
    [JsonProperty("palette")]
    public List<int> BasePalette { get; set; }

    [JsonProperty("time")]
    public List<float> BaseTime { get; set; }

    public int BaseLength => BasePalette.Count;
}

public static class PaletteInfo
{
    public static Dictionary<string, RoomChange.PaletteData> Palettes = new Dictionary<string, RoomChange.PaletteData>();

    private static int RainCycleLength;

    public static void SetRainCycleLength(int length)
    {
        RainCycleLength = length;
    }

    public static bool IsRegionPaletteAvailable(Room self, ref string room)
    {
        if (PaletteInfo.Palettes == null)
        {
            PDEBUG.Log("Palettes no cargadas aún.");
            return false;
        }
        Region region = self.world.region;
        bool IsspecificRoom = false;


        // Especific room
        if (PaletteInfo.Palettes.ContainsKey(self.abstractRoom.name))
        {
            IsspecificRoom = true;
        }

        // Region name
        if (!IsspecificRoom && !PaletteInfo.Palettes.ContainsKey(region.name))
        {
            PDEBUG.Log($"NOT FOUND | No palettes found for region: {region.name}");
            return false;
        }

        room = IsspecificRoom ? self.abstractRoom.name : region.name;
        PaletteDrive.currentRegionName = room;

        if (Palettes[room].BasePalette.Count == 0)
        {
            PDEBUG.Log($"Palette not found for {room}");
            return false;
        }

        return true;
    }

    public static void CalculatePaletteIntervals(float timeNow, PaletteData data, ref int currentPaletteIndex, ref float lastPaletteTime, ref float nextPaletteTime)
    {
        for (int i = 1; i < data.BaseLength; i++)
        {
            float endTimePalette = data.BaseTime[i] * RainCycleLength;
            if (timeNow < endTimePalette)
            {
                currentPaletteIndex = i - 1;
                lastPaletteTime = data.BaseTime[currentPaletteIndex] * RainCycleLength;
                nextPaletteTime = endTimePalette;
                return;
            }
        }
        currentPaletteIndex = data.BasePalette.Count - 1;
        lastPaletteTime = RainCycleLength * data.BaseTime[currentPaletteIndex];
        nextPaletteTime = Mathf.Infinity;
    }

    
}
