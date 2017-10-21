using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

using System.Text.RegularExpressions;

namespace SimpleWebServer {
    class WebServer {

        private HttpListener listener = new HttpListener();
        private Dictionary<string, WebPage> pages = new Dictionary<string, WebPage>();
        private WebPage notFoundPage = new WebPage("<h1>404: Page Not Found</h1> This page does not exist on this server");

        private static Regex parameters = new Regex("\\?(.+?=[^&]+)(?:&(.+?=[^&]+))*$");
        private DiscordBot.Logger logger;

        public WebServer(DiscordBot.Logger logger, string prefix = "http://localhost", uint port = 8080, WebPage index = null) {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");

            string url = prefix + ":" + port + "/";

            listener.Prefixes.Add(url);

            this.logger = logger;

            if (index != null)
                AddPage("index", index);
        }

        public void AddPage(string name, WebPage page) {
            pages[name] = page;
        }

        public void RemovePage(string name) {
            pages.Remove(name);
        }

        public void Start() {
            listener.Start();

            ThreadPool.QueueUserWorkItem((o) => {
                logger.Log("WebServer", "started");
                try {
                    while (listener.IsListening) {
                        ThreadPool.QueueUserWorkItem((c) => {
                            HttpListenerContext ctx = c as HttpListenerContext;
                            try {
                                //Get the page path
                                string local = ctx.Request.Url.LocalPath.Trim().Remove(0,1);
                                string qry = ctx.Request.Url.Query;
                                string path = local;
                                if (path == string.Empty)
                                    path = "index";

                                //Get the parameters as key value pairs
                                List<KeyValuePair<string, string>> p = new List<KeyValuePair<string, string>>();
                                foreach (Match match in parameters.Matches(qry)) {
                                    for (int i = 1; i < match.Groups.Count; i++) {
                                        string[] split = match.Groups[i].Value.Split('=');
                                        if (split.Length == 2)
                                            p.Add(new KeyValuePair<string, string>(split[0].Trim(), split[1].Trim()));
                                    }
                                }

                                //Get the response message
                                string response = this.notFoundPage.GetContent();
                                if (pages.ContainsKey(path)) {
                                    response = pages[path].GetContent(p.ToArray());
                                }

                                //Send the message
                                byte[] buffer = Encoding.UTF8.GetBytes(response);
                                ctx.Response.ContentLength64 = buffer.Length;
                                ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            }
                            catch (Exception e2) { }
                            finally {
                                ctx.Response.OutputStream.Close();
                            }
                        }, listener.GetContext());
                    }
                }
                catch (Exception e) {

                }
                logger.Log("WebServer", "terminated");
            });
        }

        public void Stop() {
            listener.Stop();
        }

    }
}
