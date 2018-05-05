using Unosquare.RaspberryIO.Camera;
using Unosquare.RaspberryIO.Gpio;

namespace MorrisCounter.Entities
{
    public class RaspberryPiCameraTrapSettings
    {
        public string IotHubDeviceId { get; set; }
        public string IotHubUri { get; set; }
        public string IotHubDeviceKey { get; set; }
        public GpioPin SensorPin { get; set; }
        public GpioPin SpotlightPin { get; set; }
        public string HueBridgeIp { get; set; }
        public string HueKey { get; set; }
        public string ComputerVisionApiKey { get; set; }
        public string[] SoughtComputerVisionTags { get; set; }
        public string TempProcessingBaseDirectory { get; set; }
        public string TempProcessingVideoFile { get; set; }
        public string TempProcessingImageFile { get; set; }
        public string TempProcessingImageFileExt { get; set; }
        public string TempProcessingVideoFileExt { get; set; }
        public CameraVideoSettings CameraSettings { get; set; }
        public int VideoChunkSize { get; set; }
    }
}
