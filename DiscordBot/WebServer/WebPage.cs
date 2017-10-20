using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SimpleWebServer {
    class WebPage {

        public string template;

        public WebPage(string content = "") {
            template = content;
        }

        public static WebPage FromFile(string url) {
            return new WebPage(string.Join("\n", File.ReadAllLines(url)));
        }

        public virtual string GetContent(params KeyValuePair<string,string>[] param) {
            return template;
        }

    }
}
