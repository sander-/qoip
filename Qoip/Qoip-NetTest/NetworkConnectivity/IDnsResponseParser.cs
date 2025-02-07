using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public interface IDnsResponseParser
    {
        IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails);
    }

}
