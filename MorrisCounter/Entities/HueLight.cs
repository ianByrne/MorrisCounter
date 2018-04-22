using Q42.HueApi;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Holds the state of a HueLight
    /// </summary>
    class HueLight
    {
        public Light Light { get; set; }
        public LightCommand PreviousState { get; set; }
    }
}
