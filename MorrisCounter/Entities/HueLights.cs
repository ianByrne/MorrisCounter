using Q42.HueApi;
using Q42.HueApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Controls the HueLights
    /// </summary>
    class HueLights
    {
        private readonly ILocalHueClient client;
        private readonly LightCommand alarmState;
        private List<HueLight> onLights;

        /// <summary>
        /// Connects to the HueBridge and sets up the 'alarm' state of the lights
        /// </summary>
        public HueLights()
        {
            // Connect to the Hue Bridge
            client = new LocalHueClient(Environment.GetEnvironmentVariable("hueBridgeIp"));
            client.Initialize(Environment.GetEnvironmentVariable("hueKey"));

            // Setup the 'alarm' state of the lights (red and bright)
            alarmState = new LightCommand();
            alarmState.Hue = 65535;
            alarmState.Saturation = 254;
            alarmState.Brightness = 254;
        }

        /// <summary>
        /// Flickers the lights to the alarm state, and then back to where there were before
        /// </summary>
        public void Alert()
        {
            // Get all the current lights that are on
            onLights = new List<HueLight>();
            IEnumerable<Light> allLights = client.GetLightsAsync().GetAwaiter().GetResult();
            foreach (Light light in allLights)
            {
                if (light.State.On)
                {
                    onLights.Add(new HueLight
                    {
                        Light = light,
                        PreviousState = new LightCommand() { Hue = light.State.Hue, Saturation = light.State.Saturation, Brightness = light.State.Brightness }
                    });
                }
            }

            // Flicker 'em
            if (onLights.Count > 0)
            {
                onLights
                    .AsParallel()
                    .WithDegreeOfParallelism(onLights.Count)
                    .ForAll(hueLight =>
                {
                    client.SendCommandAsync(alarmState, new List<string> { hueLight.Light.Id }).GetAwaiter().GetResult();
                    Thread.Sleep(500);
                    client.SendCommandAsync(hueLight.PreviousState, new List<string> { hueLight.Light.Id }).GetAwaiter().GetResult();
                });
            }
        }
    }
}
