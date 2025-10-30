namespace RoomChange
{
    /// <summary>
    /// Defines a palette transition behavior between Palette A and Palette B.
    /// Implementations can provide custom interpolation or blending logic.
    /// </summary>
    public interface IPaletteTransition
    {
        /// <summary>
        /// Calculates the blend factor between palettes based on current and target times.
        /// </summary>
        float GetBlend(float currentTime, float nextPaletteTime, float lastPaletteTime);
    }
}
