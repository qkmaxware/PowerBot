using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SimpleWebServer;

namespace SimpleWebServer.SpecialWebPages {
    class ApiCallPage : WebPage {

        private Func<Dictionary<string, string>, string> call;

        public ApiCallPage(Func<Dictionary<string, string>, string> call) {
            this.call = call;
        }

        public override string GetContent(params KeyValuePair<string, string>[] param) {
            Dictionary<string, string> ps = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in param) {
                ps[pair.Key] = pair.Value;
            }

            return call.Invoke(ps);
        }

    }
}
