using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Camera;

namespace MorrisCounter.Entities
{
    /// <summary>
    /// Handles the taking of a photo with the NoIR Camera, and uploading it to Azure Cloud Storage
    /// </summary>
    class PiCamera : IDisposable
    {
        private CameraVideoSettings CameraSettings { get; }
        private int ChunkSize { get; }

        private List<byte> currentBytes = new List<byte>();
        private List<byte> previousBytes = new List<byte>();

        public PiCamera(CameraVideoSettings cameraSettings, int chunkSize)
        {
            CameraSettings = cameraSettings;
            ChunkSize = chunkSize;
        }

        public void StartVideoStream()
        {
            // Start the video recording
            Pi.Camera.OpenVideoStream(CameraSettings,
                onDataCallback: (data) => ProcessVideoStream(data),
                onExitCallback: null);
        }

        private void ProcessVideoStream(byte[] data)
        {
            currentBytes.AddRange(data);

            // Keep segments in chunks
            int bytesToKeep = ChunkSize * 1024 * 1024;
            if(currentBytes.Count >= bytesToKeep)
            {
                StopVideoStream();
                previousBytes = currentBytes;
                currentBytes = new List<byte>();
                StartVideoStream();
            }
        }

        public void StopVideoStream()
        {
            Pi.Camera.CloseVideoStream();

            // Apparently it takes a while to actually stop the stream
            // and if the application ends before that, it doesn't close
            Thread.Sleep(1000);
        }

        public byte[] GetVideoBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(previousBytes);
            bytes.AddRange(currentBytes);
            
            return bytes.ToArray();
        }

        public void Dispose()
        {
            Console.WriteLine("Closing video stream");
            StopVideoStream();
        }
    }
}
