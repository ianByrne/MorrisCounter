using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MorrisCounter.Entities
{
    public class Detection
    {
        public Detection(string location)
        {
            Timestamp = DateTime.UtcNow;
            Location = location;

            Console.WriteLine($"{Timestamp.ToString("yyyy-MM-dd HH:mm:ss")} - Motion detected at {Location}!");
        }

        public byte[] VideoBytes { get; set; }

        private DateTime Timestamp { get; }
        private string Location { get; }
        private List<string> Tags { get; set; }

        public async Task AnalyseVideo(string baseDir, string imageFileName, string imageFileExt, string videoFileName, string videoFileExt)
        {
            if (VideoBytes != null && VideoBytes.Length > 0)
            {
                using (VideoAnalyser videoAnalyser = new VideoAnalyser(VideoBytes, Timestamp, baseDir, videoFileName, videoFileExt, imageFileName, imageFileExt))
                {
                    Tags = await videoAnalyser.AnalyseVideo(3);
                }
            }
            else
            {
                Console.WriteLine("No video bytes to analyse");
            }
        }

        public async Task SendIoTMessageToAzure(DeviceClient deviceClient)
        {
            Console.WriteLine("Sending message to IoT Hub");

            string messageString = JsonConvert.SerializeObject(new
            {
                timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                location = Location,
                tags = Tags,
                videoBytes = VideoBytes?.Length
            });

            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);

            Console.WriteLine("Sent message to IoT Hub");
        }

        /// <summary>
        /// Uploads the video to Azure IoTHub (which is linked to Cloud Storage)
        /// </summary>
        /// <param name="timestamp">The timestamp of the detection</param>
        /// <returns></returns>
        public async Task UploadVideoToAzure(DeviceClient deviceClient, string videoFileExt)
        {
            try
            {
                if (VideoBytes != null && VideoBytes.Length > 0)
                {
                    string filename = Location + " " +
                        Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "." + videoFileExt;

                    Console.WriteLine($"Uploading '{filename}'");

                    await deviceClient.UploadToBlobAsync(filename, new MemoryStream(VideoBytes));

                    Console.WriteLine($"'{filename}' uploaded");
                }
                else
                {
                    Console.WriteLine($"No video bytes to upload");
                }
            }
            catch (Exception ex)
            {
                if (ex is AggregateException)
                {
                    ex = ((AggregateException)ex).Flatten();
                }
                Console.WriteLine($"error uploading: {ex.Message} {ex.InnerException?.Message}");
            }
        }
    }
}
