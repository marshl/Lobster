using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class FileInsertFailedException : Exception
    {
        public FileInsertFailedException()
        {
        }

        public FileInsertFailedException(string message) : base(message)
        {
        }

        public FileInsertFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileInsertFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}