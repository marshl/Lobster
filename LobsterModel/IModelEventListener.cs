namespace LobsterModel
{
    public interface IModelEventListener
    {
        void OnFileChange(string filename);

        void OnAutoUpdateComplete(string filename);

        Table PromptForTable(string fullpath);

        string PromptForMimeType(string fullpath, Table tableToInsertInto);
    }
}
