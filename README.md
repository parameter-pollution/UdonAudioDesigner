# Udon Audio Designer
An in-game audio soundscape designer for VRChat written in udon sharp.

Instead of the slow `change audio settings` -> `compile` -> `listen` (in VR) -> `change audio settings` ... loop you can play with the settings live in VR to find the right settings faster.

![UI_screenshot](https://github.com/parameter-pollution/UdonAudioDesigner/assets/4985522/dc99d85a-29f0-4a85-9129-36d1aa02d445)

### Try it in this VRChat World: [Udon Audio Designer](https://vrchat.com/home/world/wrld_2255fddb-ab92-4d3c-8b75-265d588f3130/)

## Installation
1. Create an "UdonSharp" World via the VRChat Creator Companion
2. Import [latest USharpVideo](https://github.com/MerlinVR/USharpVideo/releases)
3. Import [latest UdonAudioDesigner](https://github.com/parameter-pollution/UdonAudioDesigner/releases)

## Usage
1. Drag&Drop the `AudioDesigner\AudioDesigner.prefab` prefab into your scene tree
1. Add audio clips to your VRChat world in Unity
2. Click on the `AudioDesigner` object in the scene tree and drag&drop the audio clips you want to play with in your world into the `Audio Clips` list in the inspector
![AudioDesignerInspector](https://github.com/parameter-pollution/UdonAudioDesigner/assets/4985522/cd9c6d5f-98d1-4e46-8ef6-a1606741413c)

## How can I create the final audio source for my world once I found the right settings?
Sadly, for now, you have to manually configure your own audio source. I ran out of time and motivation trying to figure out how to implement an editor UI that would do that based on the exported settings.
Here are a few tips:
- Spatialized mode enable/disable is just enabling/disabling the `Spatialize` checkbox in the `Advanced Options` of the `VRC_SpatialAudioSource` component (that you have to add to your audio source as normal)
- Volume Falloff mode Linear and Logarithmic can be set by going to the `Advanced Options` of the `VRC_SpatialAudioSource` component and checking `Use AudioSource Volume Curve` and then selecting respective value in the `"Volume Rolloff` dropdown on the `AudioSource` component
- The `Near` and `Volumetric Radius` values are also hidden in the `Advanced Options` of the `VRC_SpatialAudioSource`
- The rest of the values should be 1:1 those on the `AudioSource` and `VRC_SpatialAudioSource` component

## Ambisonic Audio
If you have an ambisonic audio file (for Unity it needs to be in 1st order AmbiX (B-Format ANC), SN3D normalized); check out [Elevative's ambisonic recordings](https://www.patreon.com/posts/example-audio-53930477)), then add it to your unity project, click on it and check the `Ambisonic` checkbox in the inspector.
Then Udon Audio Designer will automatically treat it as an ambisonic file and configure the audio source settings correctly.
Ambisonic audio is directional, so if you rotate the audio source object then the directions of the sounds in the audio clip will also rotate around you.
Just grab the audio source and rotate it to play around with this.

## Audio Scene Presets
You can upload json containing multiple audio scenes as e.g. a github gist (only works with domains allowed by VRChat) and put the URL into the `Presets Download URL` field in the inspector on the `AudioDesigner` object and it will load (up to the first 6) them and show them as buttons on the main menu panel.
Example preset json file (which is used in the linked VRChat World) is [here](https://gist.githubusercontent.com/parameter-pollution/f0f14b73f7f99f7460b5aa1b85332e53/raw/gistfile1.txt).

## How is this code able to modify the `VRC_SpatialAudioSource` component settings, even though they are not exposed to udonsharp?
The unity audio source
```csharp
SetSpatializerFloat(int index, float value)
```
method is exposed in udonsharp, which is exactly what the `VRC_SpatialAudioSource` component from VRChat is using.
So instead of using the `VRC_SpatialAudioSource` component I "just" reimplemented the spatializer configuration myself by using that function.

## Disclaimer
This is just a pet project of mine. I wanted it to exist and it didn't, so I decided to try to create it. But I don't have much time that I can put into it. So if you come up with a better system than this then send me a link and I will link to it here prominently. I don't need my name attached to this, I just want it to exist.

This code has grown over many months and different goals. It should probably be completely refactored and cleaned up, but I don't have the time for that right now and thought maybe it can at least help somebody else instead of collecting dust on my disk.

USE AT YOUR OWN RISK

Not affiliated with VRChat/Udon.
