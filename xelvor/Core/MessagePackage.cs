using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xelvor.Core
{
    public enum MessageType
    {
        Message,
        Error
    }

    class MessagePackage
    {
        public string Message { get; set; }
        public MessageType Type { get; set; }
    }
}
