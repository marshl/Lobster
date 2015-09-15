namespace Lobster
{
    /// <summary>
    /// 'Men multiply and the Firstborn decrease, and the two kindreds are estranged.'
    ///     -- Elrond
    /// 
    /// [ _The Lord of the Rings_, II/ii: "The Council of Elrond"]
    /// </summary>
    public class ClobFile
    {
        public ClobFile( ClobDirectory _parentClobDirectory )
        {
            this.parentClobDirectory = _parentClobDirectory;
        }
       
        public ClobDirectory parentClobDirectory;

        public DBClobFile dbClobFile;
        public LocalClobFile localClobFile;

        public bool IsLocalOnly { get { return this.dbClobFile == null && this.localClobFile != null; } }
        public bool IsDbOnly { get { return this.dbClobFile != null && this.localClobFile == null; } }
        public bool IsSynced { get { return this.dbClobFile != null && this.localClobFile != null; } }
        public bool IsEditable {  get { return !this.localClobFile.fileInfo.IsReadOnly; } }
    }
}
