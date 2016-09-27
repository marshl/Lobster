using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobsterModel
{
    class CodeSourceController
    {
        public DatabaseConnection Connection { get; private set; }

        public CodeSourceConfig Config { get; private set; }
    }
}
