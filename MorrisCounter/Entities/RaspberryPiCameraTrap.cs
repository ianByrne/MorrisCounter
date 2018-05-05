using System;
using System.Collections.Generic;
using System.Text;

namespace MorrisCounter.Entities
{
    public class RaspberryPiCameraTrap
    {
        private RaspberryPiCameraTrapSettings Settings { get; }

        public RaspberryPiCameraTrap(string location, RaspberryPiCameraTrapSettings settings)
        {
            Settings = settings;
        }
    }
}
