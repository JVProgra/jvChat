using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClientSimulator.Core.Networking
{
    /// <summary>
    /// These are the different protocols our chat software will be able to use
    /// </summary>
    public enum ConnectionProtocol
    {
        Invalid = 0,
        Information,
        Audio,
        Video
    }
}
