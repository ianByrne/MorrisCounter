using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Gpio;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Handles a Passive Infrared (PIR) Sensor
    /// </summary>
    class PirSensor : IDisposable
    {
        private readonly string sensorLocation;
        private readonly IrSpotlight irSpotlight;
        private readonly Camera camera;
        private readonly HueLights hueLights;

        /// <summary>
        /// Enables the sensor, and listens for motion detection
        /// </summary>
        /// <param name="sensorLocation">The location of the sensor</param>
        /// <param name="iotHubDeviceId">The IoTHub Device Id</param>
        /// <param name="sensorPin">The GPIO pin of the PIR Sensor</param>
        /// <param name="spotlightPin">The GPIO pin of the IR spotlight</param>
        public PirSensor(string sensorLocation, string iotHubDeviceId, GpioPin sensorPin, GpioPin spotlightPin)
        {
            Console.WriteLine($"Enabling {sensorLocation} sensor");

            this.sensorLocation = sensorLocation;
            irSpotlight = new IrSpotlight(spotlightPin);
            camera = new Camera(sensorLocation, iotHubDeviceId);
            hueLights = new HueLights();

            sensorPin.PinMode = GpioPinDriveMode.Input;
            sensorPin.RegisterInterruptCallback(EdgeDetection.FallingEdge, MotionDetected);

            irSpotlight.SwitchOn();

            Console.WriteLine("Opening video stream");
            camera.StartVideoStream();
        }

        ~PirSensor()
        {
            Dispose();
        }

        public void Dispose()
        {
            Console.WriteLine("Closing video stream");
            camera.StopVideoStream();
            irSpotlight.SwitchOff();
        }

        /// <summary>
        /// The callback function for when the sensor detects motion
        /// </summary>
        private async void MotionDetected()
        {
            DateTime motionDetectedDateTime = DateTime.UtcNow;

            Console.WriteLine($"{motionDetectedDateTime.ToString("yyyy-MM-dd HH:mm:ss")} - Motion detected at {sensorLocation}!");

            // Take the video and flash the lights at the same time
            Task flashLights = Task.Run(() => hueLights.Alert());
            Task uploadVideo = Task.Run(async () =>
            {
                // Let the camera run for a bit to grab a tail-end buffer
                Thread.Sleep(3000);

                // Get the frames and then run an analysis on a sample of them
                byte[] bytes = camera.GetVideoBytes();

                using (VideoAnalyser videoAnalyser = new VideoAnalyser(bytes, new string[] { "mouse" }, motionDetectedDateTime))
                {
                    await videoAnalyser.AnalyseVideo(3);
                }

                // Upload to Azure
                //await camera.UploadVideoToAzure(motionDetectedDateTime, bytes);
            });
            await flashLights;
            await uploadVideo;
        }
    }
}
