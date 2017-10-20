using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWebServer;
using System.IO;

namespace SimpleWebServer.SpecialWebPages {
    class TemplateWebPage : WebPage {

        private string tpl;

        public TemplateWebPage(string file) {
            tpl = string.Join("\n", File.ReadAllLines(file));
        }

        public override string GetContent(params KeyValuePair<string, string>[] param) {
            return tpl;
        }
    }
}
