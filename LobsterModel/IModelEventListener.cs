using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobsterModel
{
    public interface IModelEventListener
    {
        void OnFileChange();
        void OnUpdateComplete();
    }
}
