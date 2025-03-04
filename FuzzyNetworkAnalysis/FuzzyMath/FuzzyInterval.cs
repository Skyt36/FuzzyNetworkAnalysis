using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyNetworkAnalysis.FuzzyMath
{
    internal class FuzzyInterval
    {
        public double L {  get; set; }
        public double R { get; set; }
        public FuzzyInterval(double l, double r)
        {
            L = l;
            R = r;
        }
        public FuzzyInterval()
        {
            L = R = 0;
        }
    }
}
