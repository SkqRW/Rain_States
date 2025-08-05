using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Plugin;

public partial class DevTools
{
    private void Terminate()
    {

    }
    public static void Init()
    {
        //On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        //On.Player.Update += Player_Update;
    }
    public static bool notify = true;
    public static int devTimer = 0;
    public static int pal = 0;
    public static int fpal = 0;
    public static float val = 0;

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (notify)
        {
            PDEBUG.Log("Init dev tools palette");
            notify = false;
        }

        devTimer++;
        if (devTimer < 0)
        {
            return;
        }

        if (Input.GetKey(KeyCode.P))
        {
            PDEBUG.Log("Start testing dinamic palette");

            pal = ((pal + 1) % 3);
            RainCycle r = self.room.world.rainCycle;

            var screenCount = self.room.cameraPositions.Length;
            PDEBUG.Log($"change palette to {pal + 1000}");
            self.room.roomSettings.pal = pal + 1000;

            PDEBUG.Log($"see the changes :3");
            self.room.game.cameras[0].ChangeMainPalette(pal + 1000);
            //RKMFP.SetExtraFadePalette(self.room.roomSettings, 0, newFade);

            SetDevTimer(1);
        }
        if (Input.GetKey(KeyCode.F))
        {
            PDEBUG.Log("Start testing dinamic fade palette");

            fpal = ((fpal + 1) % 3);
            RainCycle r = self.room.world.rainCycle;

            var screenCount = self.room.cameraPositions.Length;
            PDEBUG.Log($"change fade palette to {fpal + 1000}");
            self.room.roomSettings.pal = fpal + 1000;

            PDEBUG.Log($"see new the changes :3");
            self.room.game.cameras[0].ChangeFadePalette(fpal + 1000, val);
            //RKMFP.SetExtraFadePalette(self.room.roomSettings, 0, newFade);

            SetDevTimer(1);
        }
        if (Input.GetKey(KeyCode.T))
        {
            PDEBUG.Log("Start testing tic transicion palette");

            val += 0.1f;
            RainCycle r = self.room.world.rainCycle;

            var screenCount = self.room.cameraPositions.Length;
            PDEBUG.Log($"change palette from {self.room.roomSettings.pal} to {self.room.roomSettings.fadePalette.palette} in tic [{val}]");

            self.room.game.cameras[0].ChangeFadePalette(fpal + 1000, val);
            PDEBUG.Log($"see the changes :3");

            SetDevTimer(1);
        }
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        sLeaser.sprites[2].color = UnityEngine.Color.blue;
    }
    //Regresive count in seconds
    private static void SetDevTimer(int seconds)
    {
        devTimer = -seconds * 40;
    }



    public static void Log(string message)
    {
        UnityEngine.Debug.Log($"[Palette] {message}");
    }

    public static void LogWarn(string message)
    {
        UnityEngine.Debug.Log($"[Warn Palette] {message}");
    }
}
