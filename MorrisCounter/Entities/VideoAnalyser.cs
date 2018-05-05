using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace MorrisCounter.Entities
{
    public class VideoAnalyser : IDisposable
    {
        private byte[] Bytes { get; }
        private string BaseDirectory { get; }
        private string H264File { get; }
        private string MP4File { get; }
        private string ImageFileStem { get; }
        private string ImageFileExt { get; }
        private string[] SoughtTags { get; }

        public bool ContainsSoughtTags { get; private set; } = false;

        public VideoAnalyser(byte[] bytes, string[] soughtTags, DateTime timestamp)
        {
            Bytes = bytes;
            SoughtTags = soughtTags;
            BaseDirectory = $"/home/pi/images/{timestamp.ToString("yyyy-MM-dd-HH:mm:ss")}";
            H264File = $"/input.h264";
            MP4File = $"/input.mp4";
            ImageFileStem = $"frame";
            ImageFileExt = $".jpg";

            Directory.CreateDirectory(BaseDirectory);
        }

        public async Task AnalyseVideo(int fps)
        {
            Console.WriteLine($"Analysing video {Bytes.Length} bytes");

            WriteH264File();
            ExtractFrames(fps);

            try
            {
                Console.WriteLine("Connecting to Azure");
                
                ApiKeyServiceClientCredentials creds = new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("computerVisionApiKey"));
                IComputerVisionAPI azure = new ComputerVisionAPI(creds);
                azure.AzureRegion = AzureRegions.Westeurope;

                string[] jpgFiles = Directory.GetFiles(BaseDirectory, $"*{ImageFileExt}").Select(Path.GetFileName).ToArray();

                foreach (string jpgFile in jpgFiles)
                {
                    Console.WriteLine($"{jpgFile}");
                    Stream stream = new FileStream(BaseDirectory + "/" + jpgFile, FileMode.Open);
                    var wat = await azure.TagImageInStreamAsync(stream);
                    foreach (var tag in wat.Tags)
                    {
                        Console.WriteLine($"Tag: {tag.Name}");
                    }
                    break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        private void WriteH264File()
        {
            // First, write the h264 to disk
            using (FileStream fs = new FileStream(BaseDirectory + H264File, FileMode.Create, FileAccess.Write))
            {
                fs.Write(Bytes, 0, Bytes.Length);
            }
        }

        private void ExtractFrames(int fps)
        {
            Console.WriteLine("Extracting frames");

            // Run ffmpeg to extract the frames
            Process process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = $"-i {BaseDirectory + H264File} -r {fps}/1 -f image2 {BaseDirectory}/{ImageFileStem}%3d{ImageFileExt} -hide_banner -loglevel fatal";
            process.Start();
            process.WaitForExit();

            Console.WriteLine("Finished extracting frames");
        }

        public void Dispose()
        {
            Directory.Delete(BaseDirectory, true);
        }
    }
}
