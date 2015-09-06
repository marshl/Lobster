using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobster
{
    public class MimeTypeList
    {
        public List<MimeType> mimeTypes;

        public class MimeType
        {
            public string name;
            public string prefix;
            public string extension;
        }
    }
}
