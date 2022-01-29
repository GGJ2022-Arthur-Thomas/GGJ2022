namespace ExtensionMethods
{
    public static class FloatExtensions
    {
        /// <summary>
        /// Returns whether the given val is between min and max (inclusive)
        /// </summary>
        public static bool IsInRange(this float val, float min, float max)
        {
            return val >= min && val <= max;
        }
    }
}