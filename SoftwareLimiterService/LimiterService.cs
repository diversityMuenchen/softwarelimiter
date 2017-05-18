using NAudio.CoreAudioApi;
using System;
using System.Linq;
using System.ServiceProcess;
using LimiterConfig;
using System.IO;
using System.Globalization;
using System.Threading;

namespace SoftwareLimiterService
{
    enum ConfigState { Hour, Minute, Volume, Comment, LineEndComment }
    public partial class LimiterService : ServiceBase
    {
        LimitConfig config;
        MMDevice outputDevice;

        public LimiterService()
        {
            InitializeComponent();

            config = ReadConfig();
        }

        private LimitConfig ReadConfig()
        {
            var lc = new LimitConfig();

            TextReader r = File.OpenText(@"C:\ProgramData\SoftwareLimiter\config.txt");
            // Config format
            // as simple as possible
            // hour:minute floatmax
            // bear in mind that it is not carried after midnight, as in if you want to limit to 35% between 22:00 and 8:00 _of the next day_,
            // your config would look like this:
            // 00:00 35.0
            // 08:00 100.0
            // 22:00 35.0
            // anything after # will be ignored.

            int hour = -1, minute = -1;
            float vol;
            string acc = "";
            ConfigState state = ConfigState.Hour;

            string toparse = r.ReadToEnd(); // I think the config files will be reasonably small.

            foreach (char c in toparse)
            {
                if (state == ConfigState.Hour)
                {
                    if (acc == "" && c == '#')
                    {
                        state = ConfigState.Comment;
                        continue;
                    }
                    if ("012345689".Contains(c))
                    {
                        acc += c;
                        continue;
                    }
                    else if (c == ':')
                    {
                        hour = int.Parse(acc);
                        state = ConfigState.Minute;
                        acc = "";
                        continue;
                    }
                    if(acc == "" && c == '\n')
                    {
                        // empty line, ignore.
                        continue;
                    }
                }
                else if (state == ConfigState.Minute)
                {
                    if ("012345689".Contains(c))
                    {
                        acc += c;
                        continue;
                    }
                    else if (c == ' ')
                    {
                        minute = int.Parse(acc);
                        state = ConfigState.Volume;
                        acc = "";
                        continue;
                    }
                }
                else if (state == ConfigState.Volume)
                {
                    if ("012345689.".Contains(c))
                    {
                        acc += c;
                        continue;
                    }
                    if(c == '#')
                    {
                        state = ConfigState.LineEndComment;
                        continue;
                    }
                }
                if(c == '\r')
                {
                    // carriage return will be ignored.
                    continue;
                }
                if (c == '\n' && (state == ConfigState.Volume || state == ConfigState.LineEndComment))
                {
                    vol = float.Parse(acc, CultureInfo.InvariantCulture);
                    TimeSpan d = new TimeSpan(hour, minute, 0);
                    lc.Mappings.Add(d, vol);
                    acc = "";
                    hour = -1;
                    minute = -1;
                    state = ConfigState.Hour;
                    continue;
                }
                if(c == '\n' && state == ConfigState.Comment)
                {
                    state = ConfigState.Hour;
                    continue;
                }
                if (state == ConfigState.Comment)
                {
                    continue;
                }
                throw new Exception("Parser error. Pls fix your config file :(");
            }
            return lc;
        }
        
        protected override void OnStart(string[] args)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            outputDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            this.EventLog.WriteEntry("Startup. Got Volume" + outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar.ToString("0.00") + ", Current max setting is " + (config.CurrentMaxVolume / 100.0f).ToString("0.00"));

            // if it's too high on startup, limit
            if (outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar > (config.CurrentMaxVolume / 100.0f))
            {
                outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar = config.CurrentMaxVolume / 100.0f;
            }

            outputDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
            ThreadPool.QueueUserWorkItem(LimitLoop);

        }

        private void LimitLoop(Object o)
        {
            while(true)
            {
                Thread.Sleep(30000);
                if (outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar > (config.CurrentMaxVolume / 100.0f))
                {
                    this.EventLog.WriteEntry("Volume too high! Got Volume" + outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar.ToString("0.00") + ", Current max setting is " + (config.CurrentMaxVolume / 100.0f).ToString("0.00"));
                    outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar = config.CurrentMaxVolume / 100.0f;
                }
            }
        }

        private void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar > (config.CurrentMaxVolume / 100.0f))
            {
                this.EventLog.WriteEntry("Volume too high! Got Volume" + outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar.ToString("0.00") + ", Current max setting is " + (config.CurrentMaxVolume / 100.0f).ToString("0.00"));
                outputDevice.AudioEndpointVolume.MasterVolumeLevelScalar = config.CurrentMaxVolume / 100.0f;
            }
        }

        protected override void OnStop()
        {
        }
    }
}
