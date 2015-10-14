using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer.Firefox
{
    public class TopLevelCall
    {
        public Call Root { get; set; }
        public Call FilteredRoot { get; set; }
        public bool IsHighlighted { get; set; }

        public TopLevelCall(Call root)
        {
            Root = root;
            FilteredRoot = root.FilterDojoJQuery(false, null);
        }

        public Call GetRoot(bool filter)
        {
            return filter ? FilteredRoot : Root;
        }

        public bool HasFilteredVersion
        {
            get { return FilteredRoot != null; }
        }
    }
}
