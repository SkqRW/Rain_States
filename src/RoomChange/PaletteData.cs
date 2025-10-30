using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoomChange;

public class PaletteData
{
    public List<int> palette { get; set; }
    public List<float> time { get; set; }
}

public static class PaletteInfo
{
    public static Dictionary<string, RoomChange.PaletteData> Palettes = new Dictionary<string, RoomChange.PaletteData>();


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

        if (Palettes[room].palette.Count == 0)
        {
            PDEBUG.Log($"Palette not found for {room}");
            return false;
        }

        return true;
    }

    public static void CalculatePaletteIntervals(float timeNow, float rainCycleLength, PaletteData data, ref int currentPaletteIndex, ref float nextPaletteTime)
    {
        for (int i = 1; i < data.palette.Count; i++)
        {
            float endTimePalette = data.time[i] * rainCycleLength;
            if (timeNow < endTimePalette)
            {
                currentPaletteIndex = i - 1;
                nextPaletteTime = endTimePalette;
                return;
            }
        }
        currentPaletteIndex = data.palette.Count - 1;
        nextPaletteTime = Mathf.Infinity;
    }
}
