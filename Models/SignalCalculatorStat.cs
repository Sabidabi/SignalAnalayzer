using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalAnalayzer.Models
{
    public class SignalCalculatorStat
    {
        private double _sum;
        private int _count;
        private double _max=double.MinValue;
        private double _min=double.MaxValue;
        public double Min { get { return _min; } }
        public double Max { get { return _max; } }
        public double ExpectMate { get { return _count == 0 ? 0 : _sum / _count;  } }
        public void CalculateNewBlock(List<double> data)
        {
            foreach (var item in data)
            {
                _sum += item;
                _count++;
                if (item > _max)
                {
                    _max = item;
                }
                if (item < _min)
                {
                    _min = item;
                }
            }

        }
    }
}
