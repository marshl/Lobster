using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    public class FileInsertException : Exception
    {
        public FileInsertException()
        {
        }

        public FileInsertException(string message) : base(message)
        {
        }

        public FileInsertException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileInsertException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}