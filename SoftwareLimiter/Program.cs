using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareLimiter
{
    class Program
    {
        static void Main(string[] args)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice dev = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            dev.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;

            dev.AudioEndpointVolume.MasterVolumeLevelScalar = 45.0f / 100.0f;
        }

        private static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            
        }
    }
}
