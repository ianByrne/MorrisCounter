using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
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
        private readonly StorageCredentials storageCredentials;
        private readonly CloudStorageAccount cloudStorageAccount;
        private readonly CloudBlobClient cloudBlobClient;
        private readonly CloudBlobContainer cloudBlobContainer;

        /// <summary>
        /// Prepares the Cloud Storage client and container
        /// </summary>
        /// <param name="cameraLocation">The location of the camera</param>
        public Photo(string cameraLocation)
        {
            this.cameraLocation = cameraLocation;

            string storageAccountName = Environment.GetEnvironmentVariable("storageAccountName");
            string storageAccountKey = Environment.GetEnvironmentVariable("storageAccountKey");

            storageCredentials = new StorageCredentials(storageAccountName, storageAccountKey);
            cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            cloudBlobContainer = cloudBlobClient.GetContainerReference(cameraLocation);
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
        /// Uploads the photo to Azure Cloud Storage
        /// </summary>
        /// <param name="timestamp">The timestamp of the detection</param>
        /// <returns></returns>
        public async Task UploadPhotoToAzure(DateTime timestamp)
        {
            if (pictureBytes != null && pictureBytes.Length > 0)
            {
                string filename = cameraLocation + " " + timestamp.ToString("yyyy-MM-dd HH:mm:ss") + ".jpeg";
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);

                Console.WriteLine($"Uploading '{filename}'");

                await cloudBlockBlob.UploadFromByteArrayAsync(pictureBytes, 0, pictureBytes.Length);

                Console.WriteLine($"'{filename}' uploaded");
            }
            else
            {
                Console.WriteLine($"Photo hasn't been taken yet");
            }
        }
    }
}
