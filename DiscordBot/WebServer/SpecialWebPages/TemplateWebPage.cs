using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWebServer;
using System.IO;

using System.Text.RegularExpressions;

namespace SimpleWebServer.SpecialWebPages {
    class TemplateWebPage : WebPage {

        private string tpl;
        private Func<string,string> rpl;
        private Regex stuff = new Regex("{{(?<name>.*?)}}");

        public TemplateWebPage(string file) {
            tpl = string.Join("\n", File.ReadAllLines(file));
        }

        public void SetReplacementMethod(Func<string, string> value) {
            rpl = value;
        }

        public override string GetContent(params KeyValuePair<string, string>[] param) {
            string val = stuff.Replace(tpl, (Match m) => {
                if (rpl == null)
                    return string.Empty;

                return rpl.Invoke(m.Groups["name"].Value);
            });

            return val;
        }
    }
}
