using System;
using Newtonsoft.Json;
using MorrisCounter.Entities;
using System.IO;
using System.Collections.Generic;
using Unosquare.RaspberryIO;
using System.Runtime.Loader;
using System.Threading;

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

                PirSensor pirSensor = new PirSensor("frontdoor", "FrontDoor", Pi.Gpio.Pin07, Pi.Gpio.Pin00);

                while (true)
                {
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}: {ex.InnerException?.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Runs when the service is stopped
        /// </summary>
        /// <param name="obj"></param>
        private static void SigTermEventHandler(AssemblyLoadContext obj)
        {
            Console.WriteLine("Application ended");
        }

        /// <summary>
        /// Dunno what triggers this
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CancelHandler(object sender, ConsoleCancelEventArgs e)
        {
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
