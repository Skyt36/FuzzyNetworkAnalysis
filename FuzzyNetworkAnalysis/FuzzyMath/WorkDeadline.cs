using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyNetworkAnalysis.FuzzyMath
{
    internal class WorkDeadline
    {
        public int work_id {  get; }
        public FuzzyInterval interval { get; set; }
        public WorkDeadline(int work_id)
        {
            this.work_id = work_id;
            interval = new FuzzyInterval();
        }
        public WorkDeadline(int work_id, FuzzyInterval interval) : this(work_id)
        {
            this.interval = interval;
        }
    }
}
