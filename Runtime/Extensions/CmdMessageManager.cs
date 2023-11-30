using System.Collections.Generic;
using System.Linq;

namespace Google.Protobuf
{
    public static class CmdMessageManager
    {
        private readonly struct CmdInfo
        {
            public readonly string Name;
            public readonly MessageParser Parser;

            public CmdInfo(string name, MessageParser parser)
            {
                Name = name;
                Parser = parser;
            }
        }

        private static readonly Dictionary<ushort, CmdInfo> _cmdMap = new();
        private static readonly HashSet<ushort> _dupCmdIdSet = new();

        public static int CmdCount => _cmdMap.Count;

        public static bool HasDuplicatedCmdId => _dupCmdIdSet.Count > 0;

        public static ushort[] DuplicatedCmdIds => _dupCmdIdSet.ToArray();

        public static MessageParser GetCmdParserById(ushort cmdId)
        {
            if (!_cmdMap.TryGetValue(cmdId, out CmdInfo cmdInfo))
            {
                return null;
            }

            return cmdInfo.Parser;
        }

        public static string GetCmdNameById(ushort cmdId)
        {
            if (!_cmdMap.TryGetValue(cmdId, out CmdInfo cmdInfo))
            {
                return null;
            }

            return cmdInfo.Name;
        }

        public static void RegisterCmd(ushort cmdId, string cmdName, MessageParser parser)
        {
            if (_cmdMap.ContainsKey(cmdId))
            {
                _dupCmdIdSet.Add(cmdId);
            }
            _cmdMap[cmdId] = new CmdInfo(cmdName, parser);
        }

        public static void UnregisterCmd(ushort cmdId)
        {
            _cmdMap.Remove(cmdId);
            _dupCmdIdSet.Remove(cmdId);
        }
    }
}
