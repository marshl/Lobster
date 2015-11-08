using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobsterModel;


namespace LobsterWpf
{
    class ConnectionView
    {
        private DatabaseConnection connection;

        public ConnectionView( DatabaseConnection con)
        {
            this.connection = con;

            this.ClobTypes = new ObservableCollection<ClobTypeView>();
            foreach ( ClobDirectory clobDir in con.ClobDirectoryList)
            {
                this.ClobTypes.Add(new ClobTypeView(clobDir.ClobType));
            }
        }

        public ObservableCollection<ClobTypeView> ClobTypes { get; set; }
    }
}
