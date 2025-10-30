using System.Collections.Generic;

namespace RoomChange;

public class PaletteData
{
    public List<int> palette { get; set; }
    public List<float> time { get; set; }
}

public static class PaletteInfo
{
    public static readonly Dictionary<string, RoomChange.PaletteData> _allPalettes = new Dictionary<string, RoomChange.PaletteData>();
    public static Dictionary<string, RoomChange.PaletteData> Palettes => _allPalettes;
    

}
