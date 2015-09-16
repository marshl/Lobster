using System.IO;

namespace Lobster
{
    /// <summary>
    /// 'My advice is: stay there! And don't get mixed up with these outlandish folk.'
    ///     -- Farmer Maggot, to Frodo
    /// 
    /// [ _The Lord of the Rings_, I/iv: "A Shortcut to Mushrooms"]
    /// </summary>
    public class LocalClobFile
    {
        public LocalClobFile(FileInfo fileInfo)
        {
            this.FileInfo = fileInfo;
        }

        public FileInfo FileInfo { get; private set; }
    }
}
