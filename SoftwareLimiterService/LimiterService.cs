using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using LimiterConfig;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace SoftwareLimiterService
{
    public partial class LimiterService : ServiceBase
    {
        LimitConfig config;
        MMDevice outputDevice;

        public LimiterService()
        {
            InitializeComponent();
            XmlSerializer s = new XmlSerializer(typeof(LimitConfig));

            config = (LimitConfig)s.Deserialize(XmlReader.Create(@"C:\ProgramData\SoftwareLimiter\config.xml"));
        }

        protected override void OnStart(string[] args)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            outputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            // if it's too high on startup, limit
            if(outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar > config.CurrentMaxVolume)
            {
                outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar = config.CurrentMaxVolume;
            }

            outputDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;

        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
           if (data.MasterVolume > config.CurrentMaxVolume)
           {
                outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar = config.CurrentMaxVolume;
           }
        }

        protected override void OnStop()
        {
        }
    }
}
