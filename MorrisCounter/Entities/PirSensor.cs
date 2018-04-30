using System;
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
            camera.StartVideoStream();
        }

        ~PirSensor()
        {
            Dispose();
        }

        public void Dispose()
        {
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
            Task uploadVideo = Task.Run(async () => await camera.UploadVideoToAzure(motionDetectedDateTime));
            await flashLights;
            await uploadVideo;
        }
    }
}
