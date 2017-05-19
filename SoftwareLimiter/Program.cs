using LimiterConfig;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareLimiter
{
    enum ConfigState { Hour, Minute, Volume, Comment, LineEndComment }
    class Program
    {
        static void Main(string[] args)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice dev = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            dev.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;

            //dev.AudioEndpointVolume.MasterVolumeLevelScalar = 45.0f / 100.0f;

            LimitConfig c = ReadConfig();

            Console.WriteLine(c.CurrentMaxVolume);

            Console.ReadLine();
        }

        private static void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            
        }

        private static LimitConfig ReadConfig()
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
                    if (acc == "" && c == '\n')
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
                    if (c == '#')
                    {
                        state = ConfigState.LineEndComment;
                        continue;
                    }
                }
                if (c == '\r')
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
                if (c == '\n' && state == ConfigState.Comment)
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
            // no newline at end?
            if(hour != -1 && minute != -1)
            {
                vol = float.Parse(acc, CultureInfo.InvariantCulture);
                TimeSpan d = new TimeSpan(hour, minute, 0);
                lc.Mappings.Add(d, vol);
            }
            return lc;
        }
    }
}
