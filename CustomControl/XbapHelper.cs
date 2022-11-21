using System.Collections.Specialized;
using System.Deployment.Application;
using System.Windows.Interop;
using System.Text;

namespace Ecm.CustomControl
{
    public class XbapHelper
    {
        public static NameValueCollection Params
        {
            get
            {
                if (_params == null)
                {
                    _params = new NameValueCollection();
                }

                return _params;
            }
            set
            {
                _params = value;
            }
        }

        public static void Configurate()
        {
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                string query = string.Empty;
                string url = BrowserInteropHelper.Source.ToString();
               
                int index = url.IndexOf("?");

                if (index > 0)
                {
                    query = url.Substring(index);
                }

                _params = ParseQueryString(query);
            }
        }

        public static NameValueCollection ParseQueryString(string query)
        {
            NameValueCollection queryParameters = new NameValueCollection();
            query = query + string.Empty;
            if (query.Length > 0)
            {
                if (query[0] == '?')
                {
                    query = query.Substring(1);
                }

                string[] querySegments = query.Split('&');
                foreach (string segment in querySegments)
                {
                    if ((segment + string.Empty).Trim() != string.Empty)
                    {
                        string[] parts = segment.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim().ToLower();
                            string val = parts[1].Trim();

                            queryParameters.Add(key, val);
                        }
                        else if (parts.Length > 2)
                        {
                            string key = parts[0].Trim().ToLower();
                            StringBuilder val = new StringBuilder(parts[1].Trim());
                            for (int nCnt = 2; nCnt < parts.Length; nCnt++)
                            {
                                val.Append("=");
                                val.Append(parts[nCnt].Trim());
                            }
                            queryParameters.Add(key, val.ToString());
                        }
                    }
                }
            }

            return queryParameters;
        }

        private static NameValueCollection _params;
    }
}