using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LobsterModel
{
    public interface IModelEventListener
    {
        void OnFileChange(string filename);
        void OnUpdateComplete(string filename);
        Table PromptForTable(string fullpath);
        string PromptForMimeType(string fullpath, Table tableToInsertInto);
        void OnFileInsertComplete(bool result);
    }
}
