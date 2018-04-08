using System;
using Unosquare.RaspberryIO.Gpio;

namespace MorrisCounter.Elements
{
    /// <summary>
    /// Handles a Passive Infrared (PIR) Sensor
    /// </summary>
    class PirSensor
    {
        private readonly string sensorLocation;
        private readonly IrSpotlight irSpotlight;
        private readonly Photo photo;

        /// <summary>
        /// Enables the sensor, and listens for motion detection
        /// </summary>
        /// <param name="sensorLocation">The location of the sensor</param>
        /// <param name="iotHubDeviceId">The IoTHub Device Id</param>
        /// <param name="sensorPin">The GPIO pin of the PIR Sensor</param>
        /// <param name="spotlightPin">The GPIO pin of the IR spotlight</param>
        public PirSensor(string sensorLocation, string iotHubDeviceId, GpioPin sensorPin, GpioPin spotlightPin)
        {
            this.sensorLocation = sensorLocation;
            irSpotlight = new IrSpotlight(spotlightPin);
            photo = new Photo(sensorLocation, iotHubDeviceId);

            Console.WriteLine($"Enabling {sensorLocation} sensor");
            sensorPin.PinMode = GpioPinDriveMode.Input;
            sensorPin.RegisterInterruptCallback(EdgeDetection.FallingEdge, MotionDetected);
        }

        /// <summary>
        /// The callback function for when the sensor detects motion
        /// </summary>
        private async void MotionDetected()
        {
            DateTime motionDetectedDateTime = DateTime.UtcNow;

            Console.WriteLine($"{motionDetectedDateTime.ToString("yyyy-MM-dd HH:mm:ss")} - Motion detected at {sensorLocation}!");

            irSpotlight.SwitchOn();
            await photo.TakePhoto();
            irSpotlight.SwitchOff();

            await photo.UploadPhotoToAzure(motionDetectedDateTime);
        }
    }
}
