using System;
using Newtonsoft.Json;
using MorrisCounter.Elements;
using System.IO;
using System.Collections.Generic;
using Unosquare.RaspberryIO;

namespace MorrisCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SetEnvVars();

                PirSensor pirSensor = new PirSensor("frontdoor", "FrontDoor", Pi.Gpio.Pin07, Pi.Gpio.Pin00);
                Console.ReadKey();
                Console.WriteLine("Application ended");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}: {ex.InnerException?.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Loads up the environment variables from the "sekruts.json" file in project root. This is to avoid having
        /// to manually set them on the raspberry pi, and makes deployment much easier if they ever need to change
        /// </summary>
        private static void SetEnvVars()
        {
            Dictionary<string, object> sekruts = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(@"sekruts.json"));

            foreach(KeyValuePair<string, object> sekrut in sekruts)
            {
                Environment.SetEnvironmentVariable(sekrut.Key, sekrut.Value.ToString());
            }
        }
    }
}
