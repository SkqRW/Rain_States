using System.Linq;
using UnityEngine;


namespace RoomChange;

public partial class PaletteDrive
{
    private static int paletteIndex = 1;
    private static int totalPalettes;

    private static RoomChange.PaletteData activeRegionPalette;
    private static string currentRegionName;
    private static int rainCycleLength;
    private static float nextPaletteTime;
    private static float lastPaletteTime;
    private static float actualTime;
    private static bool devMode = true;
    private static bool firstTime = true;

    private static bool DEBUGflagRegion = true;
    private static bool DEBUGflagCycle = true;

    public static void Terminate()
    {
        On.RoomCamera.UpdateDayNightPalette -= UpdateRainStatePaletteRoom;
    }
    public static void Init()
    {
        On.RoomCamera.UpdateDayNightPalette += UpdateRainStatePaletteRoom;
    }

    /// <summary>
    /// Method that update the palette if found
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    private static void UpdateRainStatePaletteRoom(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        rainCycleLength = self.room.world.rainCycle.cycleLength;

        if (!IsRegionPaletteAvailable(self))
        {
            if (DEBUGflagRegion)
            {
                PDEBUG.Log("Palette Config not available, using original method.");
            }
            orig(self);
            return;
        }

        if (paletteIndex >= totalPalettes)
        {
            if (DEBUGflagCycle)
            {
                PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
                self.room.game.cameras[0].ChangeMainPalette(activeRegionPalette.palette[totalPalettes - 1]);
                DEBUGflagCycle = false;
            }
            return;
        }


        //Only can take values from [0, 1]
        // If have more than 1, just change to the next palette
        actualTime = self.room.world.rainCycle.timer;
        float paletteBlend = RateChanges.Linear(actualTime, nextPaletteTime, lastPaletteTime);
        paletteBlend = Mathf.Clamp01(paletteBlend);

        //Yeah, I know this is a bit hacky, but it works for now
        //Later see how to overrise the main palette and fade palette to the custom ones
        int prevIndex = Mathf.Max(0, paletteIndex - 1);
        int currIndex = Mathf.Clamp(paletteIndex, 0, activeRegionPalette.palette.Count - 1);
        self.room.game.cameras[0].ChangeBothPalettes(activeRegionPalette.palette[prevIndex], activeRegionPalette.palette[currIndex], paletteBlend);

        //Custom Debug
        if (self.room.game.devToolsActive)
        {
            PDEBUG.Log($"Region: {self.room.world.region.name}, {self.room.abstractRoom.name} | paletteIndex: [{paletteIndex - 1} - {paletteIndex}] | The percent of blend is  %{paletteBlend * 100}: ");
        }    

        if (paletteBlend > 1)
        {
            PDEBUG.Log($"Palette blend is greater than 1, updating palette index: {paletteIndex}");
            RefreshPaletteInterval(paletteIndex + 1);
            PDEBUG.Log($"|||| Palette blend is greater than 1, updating palette index: {paletteIndex}");
        }
    }

    private static bool IsRegionPaletteAvailable(RoomCamera self)
    {
        if (self == null || self.room == null) return false;
        if (PaletteInfo.Palettes == null)
        {
            PDEBUG.Log("Palettes no cargadas aún.");
            return false;
        }
        Region region = self.room.world.region;
        bool IsspecificRoom = false;


        // Especific room
        if (PaletteInfo.Palettes.ContainsKey(self.room.abstractRoom.name))
        {
            IsspecificRoom = true;
        }
        
        // Region name
        if (!IsspecificRoom && !PaletteInfo.Palettes.ContainsKey(region.name))
        {
            PDEBUG.Log($"NOT FOUND | No palettes found for region: {region.name}");
            return false;
        }

        string room = IsspecificRoom ? self.room.abstractRoom.name : region.name;
        currentRegionName = room;
        activeRegionPalette = PaletteInfo.Palettes[room];
        totalPalettes = activeRegionPalette.palette.Count;

        if (totalPalettes == 0)
        {
            PDEBUG.Log($"Palette not found for {room}");
            return false;
        }

        if (totalPalettes == 1)
        {
            PDEBUG.Log($"ONLY ONE PALETTE | No need to cycle palettes for region: {room}");
            paletteIndex = totalPalettes; //Skip the update
            return false;
        }

        PDEBUG.Log($"[{IsspecificRoom}] Made a refresh in the region {currentRegionName}, now actual is {nextPaletteTime} and prev is {lastPaletteTime}");
        NewRangePalette();
        PDEBUG.Log($"The cycle time is {rainCycleLength} and actualTime are {actualTime}");
        
        return true;
    }


    public static string GetCurrentRegionName()
    {
        return currentRegionName;
    }
    public static void SetRegionPalette(RoomChange.PaletteData newPaletteInfo)
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

public class PaintRoom
{
    
}