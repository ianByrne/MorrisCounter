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
        public Detection()
        {
            Timestamp = DateTime.UtcNow;

            Console.WriteLine($"{Timestamp.ToString("yyyy-MM-dd HH:mm:ss")} - Motion detected at {RaspberryPiCameraTrap.Current.Location}!");
        }

        public byte[] VideoBytes { get; set; }

        private DateTime Timestamp { get; }
        private List<string> Tags { get; set; }

        public async Task AnalyseVideo()
        {
            if (VideoBytes != null && VideoBytes.Length > 0)
            {
                using (VideoAnalyser videoAnalyser = new VideoAnalyser(VideoBytes, Timestamp))
                {
                    Tags = await videoAnalyser.AnalyseVideo(3);
                }
            }
            else
            {
                Console.WriteLine("No video bytes to analyse");
            }
        }

        public async Task SendIoTMessageToAzure()
        {
            Console.WriteLine("Sending message to IoT Hub");

            string messageString = JsonConvert.SerializeObject(new
            {
                timestamp = Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                location = RaspberryPiCameraTrap.Current.Location,
                tags = Tags,
                videoBytes = VideoBytes?.Length
            });

            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            await RaspberryPiCameraTrap.Current.DeviceClient.SendEventAsync(message);

            Console.WriteLine("Sent message to IoT Hub");
        }

        /// <summary>
        /// Uploads the video to Azure IoTHub (which is linked to Cloud Storage)
        /// </summary>
        /// <param name="timestamp">The timestamp of the detection</param>
        /// <returns></returns>
        public async Task UploadVideoToAzure()
        {
            try
            {
                if (VideoBytes != null && VideoBytes.Length > 0)
                {
                    string filename = RaspberryPiCameraTrap.Current.Location + " " +
                        Timestamp.ToString("yyyy-MM-dd HH:mm:ss") + "." +
                        RaspberryPiCameraTrap.Current.Settings.TempProcessingVideoFileExt;

                    Console.WriteLine($"Uploading '{filename}'");

                    await RaspberryPiCameraTrap.Current.DeviceClient.UploadToBlobAsync(filename, new MemoryStream(VideoBytes));

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
