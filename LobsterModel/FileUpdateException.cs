using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    public class FileUpdateException : Exception
    {
        public FileUpdateException()
        {
        }

        public FileUpdateException(string message) : base(message)
        {
        }

        public FileUpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileUpdateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}