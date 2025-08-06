using System.Linq;
using JsonGet;
using UnityEngine;

namespace RoomChange;

public class PaletteDrive
{
    private static int paletteIndex = 1;
    private static int totalPalettes;

    private static JsonGet.PaletteInfo activeRegionPalette;
    private static string currentRegionName;
    private static int rainCycleLength;
    private static float nextPaletteTime;
    private static float lastPaletteTime;
    private static float actualTime;
    private static bool devMode = true;


    public static void Terminate()
    {
        On.RoomCamera.UpdateDayNightPalette -= RoomCamera_UpdateDayNightPalette;
    }
    public static void Init()
    {
        On.RoomCamera.UpdateDayNightPalette += RoomCamera_UpdateDayNightPalette;
    }

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


    private static void RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        if (!IsRegionPaletteAvailable(self)) { 
            orig(self); 
            return; 
        }


        if (paletteIndex >= totalPalettes)
        {
            PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
            self.room.game.cameras[0].ChangeMainPalette(activeRegionPalette.palette[totalPalettes-1]);
            return;
        }

        //TO DO: move this later into a room ctor
        rainCycleLength = self.room.world.rainCycle.cycleLength;

        //Only can take values from [0, 1]
        actualTime = self.room.world.rainCycle.timer;
        float paletteBlend = RateChanges.Linear(actualTime, nextPaletteTime, lastPaletteTime);

        //Yeah, I know this is a bit hacky, but it works for now
        //Later see how to overrise the main palette and fade palette to the custom ones
        self.room.game.cameras[0].ChangeBothPalettes(activeRegionPalette.palette[paletteIndex - 1], activeRegionPalette.palette[paletteIndex], paletteBlend);

        //Custom Debug
        if(Input.GetKey(KeyCode.D))
        {
            PDEBUG.Log($"Region: {self.room.world.region.name} | paletteIndex: [{paletteIndex-1} - {paletteIndex}] | {paletteBlend}: ");
        }

        if (paletteBlend > 1)
        {
            paletteIndex++;
            RefreshPaletteInterval(paletteIndex);
        }
    }

    private static bool IsRegionPaletteAvailable(RoomCamera self)
    {
        Region region = self.room.world.region;
        if (region == null) return false;

        if (currentRegionName == null || currentRegionName != region.name)
        {
            if (!JsonGet.PaletteManager.Palettes.ContainsKey(region.name))
            {
                PDEBUG.Log($"NOT FOUND | No palettes found for region: {region.name}");
                return false;
            }

            currentRegionName = region.name;
            activeRegionPalette = JsonGet.PaletteManager.Palettes[region.name];
            totalPalettes = activeRegionPalette.palette.Count;
            if(totalPalettes == 1)
            {
                PDEBUG.Log($"ONLY ONE PALETTE | No need to cycle palettes for region: {region.name}");
                paletteIndex = totalPalettes; //Skip the update
                return false;
            }
            RefreshPaletteInterval(1);
        }

        return true;
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
    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        float delta = (now - pretime) / (time - pretime);
        PDEBUG.Log($"now: {now}, nextPaletteTime: {time}, lastPaletteTime: {pretime}, paletteBlend: {delta}");
        return delta;
    }
}

