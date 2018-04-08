using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace MorrisCounter.Elements
{
    /// <summary>
    /// Sends messages to Azure IoTHub
    /// </summary>
    class IotHubMessage
    {
        private DeviceClient deviceClient;

        /// <summary>
        /// Prepares the IoTHub device client
        /// </summary>
        /// <param name="deviceId">The IotHub device Id to connect to</param>
        public IotHubMessage(string deviceId)
        {
            string iotHubUri = Environment.GetEnvironmentVariable("iotHubUri");
            string deviceKey = Environment.GetEnvironmentVariable("iotHubDeviceKey");

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);
            deviceClient.ProductInfo = "MorrisCounter"; // I have no idea what this is
        }

        /// <summary>
        /// Sends a message to IoTHub
        /// </summary>
        /// <param name="timestamp">The timestamp of the detection</param>
        /// <param name="location">The location of the detection</param>
        /// <returns></returns>
        public async Task SendMessageToIotHub(DateTime timestamp, string location)
        {
            string messageString = JsonConvert.SerializeObject(new
            {
                timestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                location = location
            });
            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            Console.WriteLine("Sending message to IoTHub");
            await deviceClient.SendEventAsync(message);
        }
    }
}
