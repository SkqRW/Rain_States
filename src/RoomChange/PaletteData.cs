using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace RoomChange;

public class PaletteData
{
    [JsonProperty("palette", NullValueHandling = NullValueHandling.Ignore)]
    public List<int> BasePalette { get; set; }

    [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
    public List<float> BaseTime { get; set; }

    [JsonProperty("terrain", NullValueHandling = NullValueHandling.Ignore)]
    public List<string> TerrainPalette { get; set; }

    [JsonProperty("terrainTime", NullValueHandling = NullValueHandling.Ignore)]
    public List<float> TerrainTime { get; set; }


    [JsonProperty("effectA", NullValueHandling = NullValueHandling.Ignore)]
    public List<Color> EffectAPalette { get; set; }

    [JsonProperty("effectATime", NullValueHandling = NullValueHandling.Ignore)]
    public List<float> EffectATime { get; set; }

     [JsonProperty("effectB", NullValueHandling = NullValueHandling.Ignore)]
    public List<Color> EffectBPalette { get; set; }

    [JsonProperty("effectBTime", NullValueHandling = NullValueHandling.Ignore)]
    public List<float> EffectBTime { get; set; }

    public int BaseLength => BasePalette?.Count ?? 0;

    public int TerrainLength => TerrainPalette?.Count ?? 0;

    public int EffectLength => EffectAPalette?.Count ?? 0;

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


    public static void CalculatePaletteEffectIntervals(float timeNow, PaletteData data, ref int currentPaletteIndex, ref float lastPaletteTime, ref float nextPaletteTime)
    {
        
    }

}
