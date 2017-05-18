using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimiterConfig
{
    public class LimitConfig
    {
        public Dictionary<TimeSpan, float> Mappings { get; private set; }
        
        public LimitConfig()
        {
            Mappings = new Dictionary<TimeSpan, float>();
        }

        public float CurrentMaxVolume
        {
            get
            {
                return (from s in Mappings
                       where s.Key <= DateTime.Now.TimeOfDay
                       orderby s.Key
                       select s.Value).First();
            }
        }

        public float GetMaxVolumeForTime(TimeSpan d)
        {
            return (from s in Mappings
                    where s.Key <= d
                    orderby s.Key
                    select s.Value).First();
        }
    }
}
