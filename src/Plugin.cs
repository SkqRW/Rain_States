using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;


#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Plugin;

[BepInPlugin(ID, NAME, VER)]
public partial class Plugin : BaseUnityPlugin 
{
    public const string ID = "skeq.rainstates";
    public const string NAME = "Rain States";
    public const string VER = "0.0.2";

    private void OnDisable()
    {
    }
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);
        JsonGet.PaletteManager.LoadPalettes();
        Logger.LogInfo($"[{NAME}] {VER} Json loaded successfully!");
    }



    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (IsInit) return;

        try
        {
            IsInit = true;
            RoomChange.PaletteDrive.Init();
            DevTools.Init();
            Logger.LogInfo($"[{NAME}] {VER} loaded successfully!");

        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

}
