# Morris Counter

Raspberry Pi camera trap, tagging with the Azure Computer Vision API, and pushing to Azure IoTHub

I built this as part of a hackathon with work. It constantly records video, and when motion is detected, tags it with the detected objects from the Computer Vision API, and then uploads it to Azure IoTHub. I've got it running on a Raspberry Pi 3B+, and it's sitting by my front door to try and track the comings and goings of the local mouse (Morris).

This is the first time I've mucked around with electronics. I'm expecting my dodgy wiring will cause my flat to burn down any day now.

As part of the hackathon, I also [recorded a small video](https://www.youtube.com/watch?v=zjU3nseEYTE) to go with the project.

## To Do

I want to make more use of the Azure IoTHub features, and see if I can extrapolate any trends in Morris' appearances (time of day, etc).