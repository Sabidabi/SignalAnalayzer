using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SignalAnalayzer.Models
{
    public class BOSMeth
    {
        [XmlArray("Channels")]
        [XmlArrayItem("Channel")]
        public List<Channel> Channels { get; set; } = new();
    }
    public class Channel
    {
        [XmlAttribute("UnicNumber")]
        public int UnicNumber { get; set; }
        [XmlAttribute("SignalFileName")]
        public string SignalFileName { get; set; }

        [XmlAttribute("Type")]
        public int Type { get; set; }

        [XmlAttribute("EffectiveFd")]
        public double EffectiveFd { get; set; }
    }
}
