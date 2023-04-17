# Augmented Reality Workshop

ECAL M&ID Block Week, April 2023


## Hololens 2 Development

Note: All this only works on Windows. If something doesn't work, please give a comment so we can update these instructions.

### Installation
- Install Unity 2021.3.22 with the "Visual Studio" and "Universal Windows Platform" modules selected.
- Open "Visual Studio Installer", install "Universal Windows Platform development" and activate "C++ (v142) Universal Windows Platform Tools" after clicking on "Modify" -> "Workloads"

### Fork & Download Project
- Create a fork of this project (click "fork" on top and follow instuctions)
- Copy the git address of your fork (on the main page of your fork, click the green button to see the link). Make sure you do it in your fork and *not* on ecal-mid/holo-23.
- Use a [Github client](https://desktop.github.com/) to clone the project to your hard drive.

### First build
- Build the project to a Visual Studio solution, with ARM-64 selected in the build settings. Just use the "Build" button, not "Build & Run". "Build & Run" also works, but shows an error because it can't be run directly from Unity. It may take a while (5-10 minutes) to build.
- Open the exported Visual Studio solution from that build process, select "ARM64" and run on "Device" while the Hololens is connected with USB 
- If it's the first build, you need to enter a pin code to pair the device. It's not the same pin code like when starting the Hololens. Instead, it can be found on the Hololens, in "Settings > Update & Security > For Developers", after clicking on "Pair". After pairing once, it should not ask for that anymore.
- After another build step (a few minutes or more), it *should* be deployed and running on the device.

### Subsequent builds
- Build the project from Unity to the same output folder like before (it should be slightly faster now, but still painfully slow).
- Run the build from Visual Studio

### Testing without building
- Open "Holographic Remoting" on Hololens
- In Unity, open "Mixed Reality > Holographic Remoting for Play Mode", enter the IP address displayed on the Hololens
- Enter play mode in Unity, it should automatically show the output on the device

### Docs
- https://learn.microsoft.com/en-us/windows/mixed-reality/
- https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/
