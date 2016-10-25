using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobsterModel;

namespace LobsterWpf.ViewModels
{
    public class WatchedNodeView
    {
        public WatchedNode BaseNode { get; }

        public WatchedNodeView(WatchedNode node)
        {
            this.BaseNode = node;
        }
    }
}
