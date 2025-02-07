using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public static class DnsResponseParserFactory
    {
        private static readonly Dictionary<int, IDnsResponseParser> Parsers = new Dictionary<int, IDnsResponseParser>
        {
            { 1, new ARecordParser() },
            { 2, new NsRecordParser() },
            { 5, new CNameRecordParser() },
            { 6, new SoaRecordParser() },
            { 12, new PtrRecordParser() },
            { 15, new MxRecordParser() },
            { 16, new TxtRecordParser() },
            { 28, new AaaaRecordParser() },
            { 33, new SrvRecordParser() },
            { 35, new NaptrRecordParser() },
            { 46, new RrsigRecordParser() },
            { 47, new NsecRecordParser() },
            { 48, new DnskeyRecordParser() },
            { 50, new Nsec3RecordParser() },
            { 52, new TlsaRecordParser() },
            { 99, new SpfRecordParser() },
            { 257, new CaaRecordParser() }
        };

        public static IDnsResponseParser GetParser(int type)
        {
            if (Parsers.TryGetValue(type, out var parser))
            {
                return parser;
            }
            throw new ArgumentException($"Unknown response type: {type}");
        }
    }
}
