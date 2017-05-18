using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimiterConfig
{
    public class LimitConfig
    {
        public Dictionary<DateTime, float> Mappings { get; private set; }
        
        public LimitConfig()
        {
            Mappings = new Dictionary<DateTime, float>();
        }

        public float CurrentMaxVolume
        {
            get
            {
                return (from s in Mappings
                       where s.Key.TimeOfDay <= DateTime.Now.TimeOfDay
                       orderby s.Key.TimeOfDay
                       select s.Value).First();
            }
        }

        public float GetMaxVolumeForTime(DateTime d)
        {
            return (from s in Mappings
                    where s.Key.TimeOfDay <= d.TimeOfDay
                    orderby s.Key.TimeOfDay
                    select s.Value).First();
        }
    }
}
