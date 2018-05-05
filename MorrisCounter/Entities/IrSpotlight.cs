using System;
using Unosquare.RaspberryIO.Gpio;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Handles an infrared spotlight to illuminate dark areas at night
    /// </summary>
    class IrSpotlight : IDisposable
    {
        private GpioPin spotlightPin;

        /// <summary>
        /// Prepares the IR spotlight
        /// </summary>
        /// <param name="spotlightPin">The location of the GPIO pin for the IR spotlight</param>
        public IrSpotlight(GpioPin spotlightPin)
        {
            this.spotlightPin = spotlightPin;
            spotlightPin.PinMode = GpioPinDriveMode.Output;
        }

        /// <summary>
        /// Turns on the IR spotlight
        /// </summary>
        public void SwitchOn()
        {
            Console.WriteLine("Switching on IR spotlight");
            spotlightPin.Write(true);
        }

        /// <summary>
        /// Turns off the IR spotlight
        /// </summary>
        public void SwitchOff()
        {
            Console.WriteLine("Switching off IR spotlight");
            spotlightPin.Write(false);
        }

        public void Dispose()
        {
            SwitchOff();
        }
    }
}
