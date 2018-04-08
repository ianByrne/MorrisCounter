using Microsoft.Azure.Devices.Client;
using System;
using System.IO;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace MorrisCounter.Elements
{
    /// <summary>
    /// Handles the taking of a photo with the NoIR Camera, and uploading it to Azure Cloud Storage
    /// </summary>
    class Photo
    {
        private readonly string cameraLocation;
        private byte[] pictureBytes;
        private DeviceClient deviceClient;

        /// <summary>
        /// Prepares the IoTHub client
        /// </summary>
        /// <param name="cameraLocation">The location of the camera</param>
        public Photo(string cameraLocation, string deviceId)
        {
            this.cameraLocation = cameraLocation;

            string iotHubUri = Environment.GetEnvironmentVariable("iotHubUri");
            string deviceKey = Environment.GetEnvironmentVariable("iotHubDeviceKey");

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
            deviceClient.ProductInfo = "MorrisCounter"; // I have no idea what this is
        }

        /// <summary>
        /// Takes a photo
        /// </summary>
        /// <returns></returns>
        public async Task TakePhoto()
        {
            CameraStillSettings cameraStillSettings = new CameraStillSettings()
            {
                CaptureDisplayPreview = false,
                CaptureEncoding = CameraImageEncodingFormat.Jpg,
                CaptureDynamicRangeCompensation = CameraDynamicRangeCompensation.Medium,
                CaptureExposure = CameraExposureMode.Night,
                CaptureHeight = 0,
                CaptureWidth = 0,
                CaptureJpegQuality = 100,
                CaptureMeteringMode = CameraMeteringMode.Spot,
                VerticalFlip = true
            };

            Console.WriteLine($"Taking photo");

            pictureBytes = await Pi.Camera.CaptureImageAsync(cameraStillSettings);

            Console.WriteLine($"Photo taken: {pictureBytes.Length} bytes");
        }

        /// <summary>
        /// Uploads the photo to Azure IoTHub (which is linked to Cloud Storage)
        /// </summary>
        /// <param name="timestamp">The timestamp of the detection</param>
        /// <returns></returns>
        public async Task UploadPhotoToAzure(DateTime timestamp)
        {
            if (pictureBytes != null && pictureBytes.Length > 0)
            {
                string filename = cameraLocation + " " + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + ".jpeg";

                Console.WriteLine($"Uploading '{filename}'");

                await deviceClient.UploadToBlobAsync(filename, new MemoryStream(pictureBytes));

                Console.WriteLine($"'{filename}' uploaded");
            }
            else
            {
                Console.WriteLine($"Photo hasn't been taken yet");
            }
        }
    }
}
