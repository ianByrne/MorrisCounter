using System;
using Unosquare.RaspberryIO.Gpio;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Handles a Passive Infrared (PIR) Sensor
    /// </summary>
    class PirSensor
    {
        public event EventHandler MotionDetected;

        /// <summary>
        /// Enables the sensor, and listens for motion detection
        /// </summary>
        /// <param name="sensorPin">The GPIO pin of the PIR Sensor</param>
        public PirSensor(GpioPin sensorPin)
        {
            sensorPin.PinMode = GpioPinDriveMode.Input;
            sensorPin.RegisterInterruptCallback(EdgeDetection.FallingEdge, () => MotionDetected.Invoke(this, EventArgs.Empty));
        }
    }
}
