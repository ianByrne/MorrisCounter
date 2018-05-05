using System;
using System.Collections.Generic;
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
        private string VideoFile { get; }
        private string ImageFile { get; }

        public bool ContainsSoughtTags { get; private set; } = false;

        public VideoAnalyser(byte[] bytes, DateTime timestamp)
        {
            Bytes = bytes;
            BaseDirectory = $"{RaspberryPiCameraTrap.Current.Settings.TempProcessingBaseDirectory}/{timestamp.ToString("yyyy-MM-dd-HH:mm:ss")}";

            VideoFile = BaseDirectory + "/" +
                RaspberryPiCameraTrap.Current.Settings.TempProcessingVideoFile + "." +
                RaspberryPiCameraTrap.Current.Settings.TempProcessingVideoFileExt;

            ImageFile = BaseDirectory + "/" +
                RaspberryPiCameraTrap.Current.Settings.TempProcessingImageFile + "%3d." + 
                RaspberryPiCameraTrap.Current.Settings.TempProcessingImageFileExt;

            Directory.CreateDirectory(BaseDirectory);
        }

        public async Task<List<string>> AnalyseVideo(int fps)
        {
            Console.WriteLine($"Analysing video {Bytes.Length} bytes");

            WriteVideoFile();
            ExtractFrames(fps);

            List<string> tags = new List<string>();

            try
            {
                Console.WriteLine("Connecting to Azure");
                
                ApiKeyServiceClientCredentials creds = new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("computerVisionApiKey"));
                IComputerVisionAPI azure = new ComputerVisionAPI(creds);
                azure.AzureRegion = AzureRegions.Westeurope;

                string[] frameFiles = Directory.GetFiles(BaseDirectory, $"*{RaspberryPiCameraTrap.Current.Settings.TempProcessingImageFileExt}")
                    .Select(Path.GetFileName).ToArray();

                // So that the latest ones are more likely to get scanned before 429 errors happen
                frameFiles.Reverse();

                foreach (string frameFile in frameFiles)
                {
                    Stream stream = new FileStream(BaseDirectory + "/" + frameFile, FileMode.Open);
                    TagResult tagResult = await azure.TagImageInStreamAsync(stream);
                    tags.AddRange(tagResult.Tags.Select(tag => tag.Name));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message} {ex.InnerException?.Message}");
            }

            tags = tags.Distinct().ToList();

            Console.WriteLine($"Tags retreived: {string.Join(",",tags)}");

            return tags;
        }

        private void WriteVideoFile()
        {
            using (FileStream fs = new FileStream(VideoFile, FileMode.Create, FileAccess.Write))
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
            process.StartInfo.Arguments = $"-i {VideoFile} -r {fps}/1 -f image2 {ImageFile} -hide_banner -loglevel fatal";
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
