using System;
using Newtonsoft.Json;
using MorrisCounter.Entities;
using System.IO;
using System.Collections.Generic;
using Unosquare.RaspberryIO;
using System.Runtime.Loader;
using Unosquare.RaspberryIO.Camera;

namespace MorrisCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Subscribe to the daemon service cancel/end events
                AssemblyLoadContext.Default.Unloading += SigTermEventHandler;
                Console.CancelKeyPress += CancelHandler;

                SetEnvVars();

                // Configure video settings
                var cameraSettings = new CameraVideoSettings()
                {
                    CaptureTimeoutMilliseconds = 0,
                    //CaptureQuantisation = 10,
                    CaptureDisplayPreview = false,
                    ImageFlipVertically = true,
                    //CaptureFramerate = 25,
                    //CaptureKeyframeRate = 1,
                    CaptureExposure = CameraExposureMode.Night,
                    CaptureWidth = 1280,
                    CaptureHeight = 720,
                    //CaptureProfile = CameraH264Profile.High,
                    CaptureDisplayPreviewEncoded = false,
                    ImageAnnotationsText = "Time %X",
                    ImageAnnotations = CameraAnnotation.Time | CameraAnnotation.FrameNumber
                };

                // Configure trap settings
                RaspberryPiCameraTrapSettings trapSettings = new RaspberryPiCameraTrapSettings()
                {
                    ComputerVisionApiKey = Environment.GetEnvironmentVariable("computerVisionApiKey"),
                    HueBridgeIp = Environment.GetEnvironmentVariable("hueBridgeIp"),
                    HueKey = Environment.GetEnvironmentVariable("hueKey"),
                    IotHubDeviceId = Environment.GetEnvironmentVariable("iotHubDeviceId"),
                    IotHubUri = Environment.GetEnvironmentVariable("iotHubUri"),
                    IotHubDeviceKey = Environment.GetEnvironmentVariable("iotHubDeviceKey"),
                    SensorPin = Pi.Gpio.Pin07,
                    SpotlightPin = Pi.Gpio.Pin00,
                    TempProcessingBaseDirectory = "/home/pi/images",
                    TempProcessingImageFile = "frame",
                    TempProcessingImageFileExt = "jpg",
                    TempProcessingVideoFile = "input",
                    TempProcessingVideoFileExt = "h264",
                    VideoChunkSize = 2,
                    CameraSettings = cameraSettings
                };

                // Do the needful
                RaspberryPiCameraTrap.Execute("frontdoor", trapSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}: {ex.InnerException?.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                Console.WriteLine("Finally block");
                RaspberryPiCameraTrap.Current.Dispose();
            }
        }

        /// <summary>
        /// Runs when the service is stopped
        /// </summary>
        /// <param name="obj"></param>
        private static void SigTermEventHandler(AssemblyLoadContext obj)
        {
            RaspberryPiCameraTrap.Current.Dispose();
            Console.WriteLine("Application ended");
        }

        /// <summary>
        /// Dunno what triggers this
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
            RaspberryPiCameraTrap.Current.Dispose();
            Console.WriteLine("Application ended");
        }

        /// <summary>
        /// Loads up the environment variables from the "sekruts.json" file in project root. This is to avoid having
        /// to manually set them on the raspberry pi, and makes deployment much easier if they ever need to change
        /// </summary>
        private static void SetEnvVars()
        {
            Dictionary<string, object> sekruts = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(@"/home/pi/morriscounter/sekruts.json"));

            foreach(KeyValuePair<string, object> sekrut in sekruts)
            {
                Environment.SetEnvironmentVariable(sekrut.Key, sekrut.Value.ToString());
            }
        }
    }
}
