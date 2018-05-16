using Microsoft.Azure.Devices.Client;
using MorrisCounter.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MorrisCounter
{
    public class RaspberryPiCameraTrap : IDisposable
    {
        public RaspberryPiCameraTrap(string location, RaspberryPiCameraTrapSettings settings)
        {
            Location = location;
            Settings = settings;

            HueLights = new HueLights(settings.HueBridgeIp, settings.HueKey);

            // Connect to Azure IoT Hub
            DeviceClient = DeviceClient.Create(settings.IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(settings.IotHubDeviceId, settings.IotHubDeviceKey), TransportType.Mqtt);
            DeviceClient.ProductInfo = "MorrisCounter"; // I have no idea what this is

            // Turn on the IR spotlight
            Spotlight = new IrSpotlight(settings.SpotlightPin);
            Spotlight.SwitchOn();

            // Start the video stream
            Console.WriteLine("Opening video stream");
            Camera = new PiCamera(settings.CameraSettings, settings.VideoChunkDuration);
            Camera.StartVideoStream();

            // Enable the PIR Sensor and subscribe to the callback
            PirSensor sensor = new PirSensor(settings.SensorPin);
            sensor.MotionDetected += MotionDetected;
        }

        private RaspberryPiCameraTrapSettings Settings { get; }
        private string Location { get; }
        private DeviceClient DeviceClient { get; }
        private IrSpotlight Spotlight { get; }
        private PiCamera Camera { get; }
        private HueLights HueLights { get; }

        private async void MotionDetected(object obj, EventArgs args)
        {
            Detection detection = new Detection(Location);

            // Send off the request to flash the Hue lights
            Task flashLights = Task.Run(() => HueLights.Alert());

            // Let the camera run for a bit to grab a tail-end buffer
            Thread.Sleep(4000);
            detection.VideoBytes = Camera.GetVideoBytes();

            await flashLights;
            await detection.AnalyseVideo(
                Settings.TempProcessingBaseDirectory,
                Settings.TempProcessingImageFile,
                Settings.TempProcessingImageFileExt,
                Settings.TempProcessingVideoFile,
                Settings.TempProcessingVideoFileExt);

            await detection.SendIoTMessageToAzure(DeviceClient);
            await detection.UploadVideoToAzure(DeviceClient, Settings.TempProcessingVideoFileExt);
        }

        public void Dispose()
        {
            Spotlight.Dispose();
            Camera.Dispose();
        }

        ~RaspberryPiCameraTrap()
        {
            Dispose();
        }
    }
}
