# Brick

Brick is a 2-player augmented reality game that challenges the players to accomplish collaborative tasks with their partner.


## Built With

* [Unity 2017.3.1f1](https://unity3d.com/unity/whats-new/unity-2017.3.1)
* [Google AR Core](https://developers.google.com/ar/develop/unity/quickstart)


### Prerequisites

* [AR Core Supported Devices](https://developers.google.com/ar/discover/#supported_devices)
* [AR Core SDK for Unity](https://github.com/google-ar/arcore-unity-sdk/releases/download/v1.1.0/arcore-unity-sdk-v1.1.0.unitypackage)


### Set Up

**1. Install AR Core SDK Unity Package**

You have to set up Unity(2017.3.1f1 is recommended) with AR Core SDK.

Install the Android SDK version 7.0 (API Level 24) or higher. Install Android Studio and Android SDK Manager tool in Android Studio. In Unity preferences(Edit -> Preferences -> External Tools), you have to assign the local address of SDK (See the figure below). If you don't have JDK, you should download it and assign the address as well. (NDK is optional.)

![External Tools](https://github.com/trie94/love/blob/master/References/external_tool.PNG)

If there is any conflict between SDK and Unity, you have to download the lower version of SDK and change the latest SDK tools folder with the lower version. Note that other folders should be kept as the latest.
* [Android SDK Platform Release Notes](https://developer.android.com/studio/releases/platforms)


**2. Set up devices**

When testing in the Unity editor, you have to make the device as a developer mode. You can change the mode in Settings -> System -> About phone -> Build number. When you hit Build number multiple times, it will notify you the developer mode is on. You will see Developer options once the mode is on.


**3. Install AR Core Instant Preview**

When you first connect your device to PC and hit play in Unity, it will automatically launch AR Core instant preview. When it successfully connected, you will see the screen below. However, you will not be able to see through the phone screen in the editor mode.

![AR Core Instant Preivew Example](https://github.com/trie94/love/blob/master/References/instant_preview2.jpg)


**4. Test interactions**

**Detect Plane** - At least one plane *must* be detected, otherwise the game will not run. Since AR Core is using a color camera to detect planes, using patterned background is highly recommended. It will be hard to detect pure and plane color. Note that the plane can be shifted by the changes in physical settings-- when the lighting changes, the device gets too close to the wall or shaked too much.

**Touch Input** - When testing touch interaction in Unity, you need to add the code below. In this project, it is already added in the PlayerBehaviorNetworking script.

```
#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif
```

**5. Networking**

**Comment Out Below Code in the ARCoreSession Script**
```
if (m_SessionComponent != null)
{
     Debug.LogError("Multiple session components cannot exist in the scene. " +
		"Destroying the newest.");
     GameObject.Destroy(session);
     return;
}
```
AR Core Session *does not allow multiple AR Core sessions* in one scene. Thus, if you don't disable the above code, one AR Core session will disable the other one.

## Author

* **Yein Jo** - *Initial work* - [GitHub](https://github.com/trie94)


## License

This project is from a small group study in HCII at CMU and licensed under the CMU students: Po Bhattacharya, Ketki Jadhav, Radha Nath, Yein Jo; Jessica Hammer as an instructor.

* **Ketki Jadhav** - *Designer*
* **Radha Nath** - *Researcher*
* **Po Bhattacharya** - *Product Manager* - [Portfolio](http://www.pobhattacharyya.com)
* **Yein Jo** - *Programmer* - [Portfolio](http://www.yeinjo.com/)