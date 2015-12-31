using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace LobsterModel
{
    public abstract class SerializableObject
    {
        public SerializableObject()
        {

        }
        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                return this.ErrorList.Count == 0;
            }
        }

        [XmlIgnore]
        public List<string> ErrorList = new List<string>();
    }
}
