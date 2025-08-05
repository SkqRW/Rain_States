using UnityEngine;

namespace RoomChange;

public class PaletteDrive
{
    public static void Terminate()
    {
        On.RoomCamera.UpdateDayNightPalette -= RoomCamera_UpdateDayNightPalette;
    }
    public static void Init()
    {
        On.RoomCamera.UpdateDayNightPalette += RoomCamera_UpdateDayNightPalette;
    }
    private static int paletteIndex = 1;
    private static int paletteLimit = 0;

    private static JsonGet.PaletteInfo activeRegionPalette;
    private static string regionName;
    private static int rainCycleLength;


    private static void RoomCamera_UpdateDayNightPalette(On.RoomCamera.orig_UpdateDayNightPalette orig, RoomCamera self)
    {
        if (self.room.world.region == null)
        {
            return;
        }
        
        if(regionName == null || regionName != self.room.world.region.name)
        {
            if (!JsonGet.PaletteManager.Palettes.ContainsKey(self.room.world.region.name))
            {
                PDEBUG.Log("NOT FOUND| No palettes found for region: " + self.room.world.region.name);
                orig(self);
                return;
            }

            activeRegionPalette = JsonGet.PaletteManager.Palettes[self.room.world.region.name];
            paletteLimit = activeRegionPalette.palette.Count;
        }


        if (paletteIndex >= paletteLimit)
        {
            PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
            self.room.game.cameras[0].ChangeMainPalette(activeRegionPalette.palette[paletteIndex - 1]);
            return;
        }

        rainCycleLength = self.room.world.rainCycle.cycleLength;
        

        float time = rainCycleLength * activeRegionPalette.time[paletteIndex];
        float pretime = rainCycleLength * activeRegionPalette.time[paletteIndex-1];
        float delta = RateChanges.Linear(self.room.world.rainCycle.timer, time, pretime);

        //Yeah, I know this is a bit hacky, but it works for now
        //Later see how to overrise the main palette and fade palette to the custom ones
        self.room.game.cameras[0].ChangeBothPalettes(activeRegionPalette.palette[paletteIndex - 1], activeRegionPalette.palette[paletteIndex], delta);

        //Custom Debug
        if(Input.GetKey(KeyCode.D))
        {
            PDEBUG.Log($"Region: {self.room.world.region.name} | paletteIndex: [{paletteIndex-1} - {paletteIndex}] | {delta}: ");
        }

        if (delta >= 1)
        {
            paletteIndex++;
        }
    }
}

public static class RateChanges
{
    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        float delta = (now - pretime) / (time - pretime);
        //PDEBUG.Log($"now: {now}, time: {time}, pretime: {pretime}, delta: {delta}");
        return delta;
    }
}

