using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class SimpleUri
    {
        private string m_Str;
        public string m_Host;
        public string PathAndQuery { get; private set; }

        public SimpleUri(string url)
        {
            if (url != "")
            {
                Uri uri = new Uri(url);
                m_Str = uri.ToString();
                PathAndQuery = uri.PathAndQuery;
                m_Host = uri.Authority;
            }
            else
            {
                m_Str = "";
                PathAndQuery = "";
                m_Host = "";
            }
        }

        public override string ToString()
        {
            return m_Str;
        }

        public bool HostEquals(SimpleUri that)
        {
            return this.m_Host != "" && string.Compare(this.m_Host, that.m_Host, true) == 0;
        }

        public string GetShortUrl(SimpleUri request)
        {
            if (this.HostEquals(request))
                return PathAndQuery;
            else
                return m_Str;
        }
    }
}
