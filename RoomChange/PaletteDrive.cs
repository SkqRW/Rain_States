
namespace RoomChange;

public class PaletteDrive
{
    public static void Terminate()
    {
        On.RoomCamera.Update -= RoomCamera_Update;
    }
    public static void Init()
    {
        On.RoomCamera.Update += RoomCamera_Update;
    }
    private static int idx = 1;

    private static void RoomCamera_Update(On.RoomCamera.orig_Update orig, RoomCamera self)
    {
        orig(self);

        if (self.room.world.region == null)
        {
            return;
        }
        if (!JsonGet.PaletteManager.Palettes.ContainsKey(self.room.world.region.name))
        {
            PDEBUG.Log("No palettes found for region: " + self.room.world.region.name);
            return;
        }
        if(idx >= JsonGet.PaletteManager.Palettes[self.room.world.region.name].palette.Count)
        {
            PDEBUG.Log("No more palettes to apply for region: " + self.room.world.region.name);
            return;
        }

        


        int len = self.room.world.rainCycle.cycleLength;
        

        var paletteInfo = JsonGet.PaletteManager.Palettes[self.room.world.region.name];


        float time = len * paletteInfo.time[idx];
        float pretime = len * paletteInfo.time[idx-1];
        float delta = RateChanges.Linear(self.room.world.rainCycle.timer, time, pretime);

        //Yeah, I know this is a bit hacky, but it works for now
        //Later see how to overrise the main palette and fade palette to the custom ones
        self.room.game.cameras[0].ChangeMainPalette(paletteInfo.palette[idx - 1]);
        self.room.game.cameras[0].ChangeFadePalette(paletteInfo.palette[idx], delta);


        PDEBUG.Log("YESS :# Loading palettes for region: " + self.room.world.region.name + " | idx: " + idx);
        

        if (delta >= 1)
        {
            idx++;
        }
    }
}

public static class RateChanges
{
    //Relative path in A to B
    public static float Linear(float now, float time, float pretime)
    {
        float delta = (now - pretime) / (time - pretime);
        PDEBUG.Log($"now: {now}, time: {time}, pretime: {pretime}, delta: {delta}");
        return delta;
    }
}

