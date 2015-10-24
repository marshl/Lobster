using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lobster
{
    public interface IModelEventListener
    {
        void OnFileChange();
        void OnUpdateComplete();
    }
}
