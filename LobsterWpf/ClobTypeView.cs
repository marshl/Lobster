using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobsterModel;


namespace LobsterWpf
{
    class ClobTypeView
    {
        private ClobType clobType;

        public ClobTypeView(ClobType ct)
        {
            this.clobType = ct;
        }

        public string Name
        {
            get
            {
                return this.clobType.Name;
            }
        }
    }
}
