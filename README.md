# Morris Counter - Raspberry Pi camera trap, pushing to Azure IoTHub

I built this as part of a hackathon with work. It takes a photo when motion is detected (illuminating the area with infrared LEDs), and uploads it to Azure IoTHub. I've got it running on a Raspberry Pi 3B+, and it's sitting by my front door to try and track the comings and goings of the local mouse (Morris).

This is the first time I've mucked around with electronics. I'm expecting my dodgy wiring will cause my flat to burn down any day now.

As part of the hackathon, I also [recorded a small video](https://www.youtube.com/watch?v=nB2EQN6MYHM) to go with the project.

## To Do

It turns out Morris is a bit too quick for the camera to grab his picture, so in future versions I plan to instead use video - it will upload the most-recent 30s segment when motion is detected. I also want to make more use of the Azure IoTHub features, and see if I can extrapolate any trends in Morris' appearances (time of day, etc).