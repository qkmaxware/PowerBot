﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Modules {
    public interface ICommandContext {
        string GetMessage();
        string GetSenderName();
        string GetSenderId();
        string GetSenderMention();
        void ReplyToChannel(string msg);
        void ReplyToUser(string msg);
    }
}
