using System.Collections.Generic;
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

        public void GetWorkingFiles( ref List<ClobFile> _workingFiles )
        {
            this.rootClobNode.GetWorkingFiles( ref _workingFiles );
        }
    }
}
