using Microsoft.Azure.Devices.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MorrisCounter.Entities
{
    public class RaspberryPiCameraTrap : IDisposable
    {
        private RaspberryPiCameraTrap(string location, RaspberryPiCameraTrapSettings settings)
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
            Camera = new PiCamera(settings.CameraSettings, settings.VideoChunkSize);
            Camera.StartVideoStream();

            // Enable the PIR Sensor and subscribe to the callback
            PirSensor sensor = new PirSensor(settings.SensorPin);
            sensor.MotionDetected += MotionDetected;
        }

        public static RaspberryPiCameraTrap Current
        {
            get
            {
                if (current == null)
                {
                    throw new Exception($"This needs to be wrapped by {nameof(Execute)}");
                }

                return current;
            }
            private set
            {
                current = value;
            }
        }

        private static RaspberryPiCameraTrap current = null;

        public RaspberryPiCameraTrapSettings Settings { get; }
        public string Location { get; }
        public DeviceClient DeviceClient { get; }

        private IrSpotlight Spotlight { get; }
        private PiCamera Camera { get; }
        private HueLights HueLights { get; }

        public static void Execute(string location, RaspberryPiCameraTrapSettings settings)
        {
            Current = new RaspberryPiCameraTrap(location, settings);

            while (true)
            {
                Thread.Sleep(2000);
            }
        }

        private async void MotionDetected(object obj, EventArgs args)
        {
            Detection detection = new Detection();

            // Send off the request to flash the Hue lights
            Task flashLights = Task.Run(() => HueLights.Alert());

            // Let the camera run for a bit to grab a tail-end buffer
            Thread.Sleep(3000);
            detection.VideoBytes = Camera.GetVideoBytes();

            await flashLights;
            await detection.AnalyseVideo();

            await detection.SendIoTMessageToAzure();
            await detection.UploadVideoToAzure();
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
