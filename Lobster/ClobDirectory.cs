using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lobster
{
    public class ClobDirectory
    {
        public ClobType clobType;

        public LobsterModel parentModel;
        public Dictionary<string, ClobFile> fullpathClobMap = new Dictionary<string, ClobFile>();
        public DataGridView dataGridView;
        public ClobNode rootClobNode;
    }
}
