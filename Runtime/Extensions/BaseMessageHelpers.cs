using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Reflection;

namespace Google.Protobuf
{
    public abstract class BaseMessageHelpers
    {
        protected class CmdInfo
        {
            public string Name;
            public MessageParser Parser;
            public Func<MessageDescriptor> Descriptor;
        }

        protected readonly Dictionary<ushort, CmdInfo> _cmdInfoMap = new();
        protected readonly Dictionary<string, ushort> _cmdIdMap = new();

        protected virtual void Register(ushort cmdId, string cmdName, MessageParser parser, Func<MessageDescriptor> descriptor)
        {
            _cmdInfoMap[cmdId] = new CmdInfo
            {
                Name = cmdName,
                Parser = parser,
                Descriptor = descriptor
            };
            _cmdIdMap[cmdName] = cmdId;
        }

        public MessageParser GetMessageParserByCmdId(ushort cmdId)
        {
            return _cmdInfoMap.GetValueOrDefault(cmdId)?.Parser;
        }

        public MessageDescriptor GetMessageDescriptorByCmdId(ushort cmdId)
        {
            return _cmdInfoMap.GetValueOrDefault(cmdId)?.Descriptor();
        }

        public int CmdCount => _cmdInfoMap.Count;

        public ushort[] AllCmdIds => _cmdInfoMap.Keys.ToArray();

        public string GetCmdName(ushort cmdId)
        {
            return _cmdInfoMap.GetValueOrDefault(cmdId)?.Name;
        }

        public ushort? GetCmdId(string cmdName)
        {
            if (!_cmdIdMap.TryGetValue(cmdName, out ushort cmdId))
            {
                return null;
            }
            return cmdId;
        }
    }
}
