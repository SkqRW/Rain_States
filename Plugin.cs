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
    private void OnEnable()
    {
        
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void OnDisable()
    {
    }

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        if (IsInit) return;

        try
        {
            IsInit = true;

            JsonGet.PaletteManager.LoadPalettes();
            RoomChange.PaletteDrive.Init();
            DevTools.Init();

        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

}
