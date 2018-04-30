using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Handles the taking of a photo with the NoIR Camera, and uploading it to Azure Cloud Storage
    /// </summary>
    class Camera
    {
        private readonly string cameraLocation;
        private DeviceClient deviceClient;
        private int chunkCount = 0;
        private List<byte> firstChunk = new List<byte>();
        private List<byte> previousChunk = new List<byte>();
        private List<byte> currentChunk = new List<byte>();

        /// <summary>
        /// Prepares the IoTHub client
        /// </summary>
        /// <param name="cameraLocation">The location of the camera</param>
        public Camera(string cameraLocation, string deviceId)
        {
            this.cameraLocation = cameraLocation;

            string iotHubUri = Environment.GetEnvironmentVariable("iotHubUri");
            string deviceKey = Environment.GetEnvironmentVariable("iotHubDeviceKey");

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
            deviceClient.ProductInfo = "MorrisCounter"; // I have no idea what this is
        }

        public void StartVideoStream()
        {
            // Configure video settings
            var videoSettings = new CameraVideoSettings()
            {
                CaptureTimeoutMilliseconds = 0,
                CaptureDisplayPreview = false,
                ImageFlipVertically = true,
                CaptureExposure = CameraExposureMode.Night,
                CaptureWidth = 1280,
                CaptureHeight = 720,
                CaptureDisplayPreviewEncoded = false,
                ImageAnnotationsText = "Time %X",
                ImageAnnotations = CameraAnnotation.Time | CameraAnnotation.FrameNumber
            };

            // Start the video recording
            Console.WriteLine("Opening video stream");
            Pi.Camera.OpenVideoStream(videoSettings,
                onDataCallback: (data) => ProcessVideoStream(data),
                onExitCallback: null);
        }

        private void ProcessVideoStream(byte[] data)
        {
            chunkCount++;

            if (chunkCount <= 10)
            {
                firstChunk.AddRange(data);
            }
            else
            {
                currentChunk.AddRange(data);
            }

            // Keep only the last 5mb
            int bytesToKeep = 3 * 1024 * 1024;
            if(currentChunk.Count >= bytesToKeep)
            {
                StopVideoStream();
                previousChunk = currentChunk;
                currentChunk = new List<byte>();
                StartVideoStream();
            }
        }

        public void StopVideoStream()
        {
            Console.WriteLine("Closing video stream");
            Pi.Camera.CloseVideoStream();

            // Apparently it takes a while to actually stop the stream
            // and if the application ends before that, it doesn't close
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Uploads the video to Azure IoTHub (which is linked to Cloud Storage)
        /// </summary>
        /// <param name="timestamp">The timestamp of the detection</param>
        /// <returns></returns>
        public async Task UploadVideoToAzure(DateTime timestamp)
        {
            try
            {
                // Append the two chunks into one stream
                List<byte> bytes = new List<byte>();
                bytes.AddRange(firstChunk);
                bytes.AddRange(previousChunk);
                bytes.AddRange(currentChunk);

                if (bytes.Count > 0)
                {
                    string filename = cameraLocation + " " + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + ".h264";

                    Console.WriteLine($"Uploading '{filename}'");

                    await deviceClient.UploadToBlobAsync(filename, new MemoryStream(bytes.ToArray()));

                    Console.WriteLine($"'{filename}' uploaded");
                }
                else
                {
                    Console.WriteLine($"Stream hasn't been recorded to yet");
                }
            }
            catch(Exception ex)
            {
                if(ex is AggregateException)
                {
                    ex = ((AggregateException)ex).Flatten();
                }
                Console.WriteLine($"error uploading: {ex.Message} {ex.InnerException?.Message}");
            }
        }
    }
}
