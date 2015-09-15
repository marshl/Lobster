using System.Collections.Generic;

namespace Lobster
{
    /// <summary>
    /// 'What does the writing say?' asked Frodo, who was trying to decipher
    /// the inscription on the arch. 'I thought I knew the elf-letters but I cannot read these.'
    /// 
    /// [ _The Lord of the Rings_, II/iv: "A Journey in the Dark"]
    /// </summary>
    public class MimeTypeList
    {
        public List<MimeType> mimeTypes;

        public class MimeType
        {
            public string name;
            public string prefix;
            public string extension;
        }
    }
}
