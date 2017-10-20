﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace DiscordBot.Bot.Modules {
    class xtype {
        private static Regex rules = new Regex("[a-z]+");

        public string Value {
            get;
            private set;   
        }

        private xtype() {}

        public static xtype Register(string xtype) {
            string match = rules.Match(xtype).Value.Trim();
            if (match == string.Empty)
                throw new Exception("Invalid xType name");
            xtype type = new Modules.xtype();
            type.Value = match;
            return type;
        }

        public override bool Equals(object obj) {
            return obj is xtype ? ((xtype)obj).Value == Value : false;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

    }
}