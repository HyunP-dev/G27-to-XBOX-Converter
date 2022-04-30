using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G27_to_XBOX_Converter
{
    internal class Safety
    {
        public static short SafeShort(long v)
        {
            if (v > short.MaxValue)
            {
                return short.MaxValue;
            }

            if (v < short.MinValue)
            {
                return short.MinValue;
            }
            return (short)v;
        }
    }
}
