﻿
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Scoops.service
{
    public class PhoneSoundManager
    {
        public static AudioClip phoneRingCaller;
        public static AudioClip phoneRingReciever;
        public static AudioClip phonePickup;
        public static AudioClip phoneHangup;
        public static AudioClip phoneRotaryForward;
        public static AudioClip phoneRotaryBackward;
        public static AudioClip phoneRotaryStopper;
        public static AudioClip phoneRotaryFinish;

        public static void Init()
        {
            Plugin.Log.LogInfo($"Loading Sounds...");
            Plugin.Log.LogInfo(String.Join(", ", Plugin.LethalPhoneAssets.GetAllAssetNames()));
            phoneRingCaller = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("PhoneRing_Caller");
            phoneRingReciever = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("PhoneRing_Reciever");
            phonePickup = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("PhonePickup");
            phoneHangup = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("PhoneHangup");
            phoneRotaryForward = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("RotaryDialForwardOptionTwo");
            phoneRotaryBackward = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("RotaryDialBackOptionOne");
            phoneRotaryStopper = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("RotaryStopperOptionOne");
            phoneRotaryFinish = (AudioClip)Plugin.LethalPhoneAssets.LoadAsset("RotaryFinishOptionOne");
        }
    }
}
