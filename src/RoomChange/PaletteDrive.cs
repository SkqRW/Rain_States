using System;
using System.Linq;
using UnityEngine;


namespace RoomChange;

public partial class PaletteDrive
{
    private static int paletteIndex = 1;
    private static int totalPalettes;

    private static RoomChange.PaletteData activeRegionPalette;
    public static string currentRegionName;
    private static float rainCycleLength;
    private static float nextPaletteTime;
    private static float lastPaletteTime;
    private static float actualTime;

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
        if (self == null || self.room == null)
        {
            orig(self);
            return;
        }
        
        rainCycleLength = self.room.world.rainCycle.cycleLength;

        string room = "";
        if (!PaletteInfo.IsRegionPaletteAvailable(self.room, ref room))
        {
            if (DEBUGflagRegion)
            {
                PDEBUG.Log("Palette Config not available, using original method.");
            }
            orig(self);
            return;
        }

        activeRegionPalette = PaletteInfo.Palettes[room];
        totalPalettes = activeRegionPalette.palette.Count;
        PaletteInfo.CalculatePaletteIntervals(actualTime, rainCycleLength, activeRegionPalette, ref paletteIndex, ref nextPaletteTime);
        RefreshPaletteInterval(paletteIndex);



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

        int prevIndex = Math.Max(paletteIndex, 0);
        int nextIndex = Math.Min(paletteIndex + 1, activeRegionPalette.palette.Count - 1);
        self.room.game.cameras[0].ChangeBothPalettes(activeRegionPalette.palette[prevIndex], activeRegionPalette.palette[nextIndex], paletteBlend);

        //Custom Debug
        if (self.room.game.devToolsActive)
        {
            PDEBUG.Log($"Region: {self.room.world.region.name}, {self.room.abstractRoom.name} | paletteIndex: [{prevIndex} - {nextIndex}] -> {paletteIndex} | The percent of blend is  %{paletteBlend * 100}:  ");
            PDEBUG.Log($"Last Palette Time: {lastPaletteTime}, Next Palette Time: {nextPaletteTime}, Actual Time: {actualTime}");
        }    
    }

    public static string GetCurrentRegionName()
    {
        return currentRegionName;
    }
    public static void SetRegionPalette(RoomChange.PaletteData newPaletteInfo)
    {
        activeRegionPalette = newPaletteInfo;
        totalPalettes = activeRegionPalette.palette.Count;
        PaletteInfo.CalculatePaletteIntervals(actualTime, rainCycleLength, activeRegionPalette, ref paletteIndex, ref nextPaletteTime);
    }



    private static void RefreshPaletteInterval(int index)
    {
        if (index < 0)
        {
            PDEBUG.LogWarn($"Index {index} out of bounds in Palette Interval");
            return;
        }

        if(index == activeRegionPalette.palette.Count - 1)
        {
            // Last palette, no next
            PDEBUG.Log($"Last palette index {index} reached for region {currentRegionName}. No next palette.");
            nextPaletteTime = Mathf.Infinity;
            lastPaletteTime = rainCycleLength * activeRegionPalette.time[index - 1];
            paletteIndex = index;
            return;
        }

        nextPaletteTime = activeRegionPalette.time[index + 1] * rainCycleLength;
        lastPaletteTime = rainCycleLength * activeRegionPalette.time[index];
        paletteIndex = index;
        PDEBUG.Log($"Palette interval set: [{lastPaletteTime}, {nextPaletteTime}] for index {index} in region {currentRegionName}");
    }
}

public class PaintRoom
{
    
}