using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class FileListRetrievalException : Exception
    {
        public FileListRetrievalException()
        {
        }

        public FileListRetrievalException(string message) : base(message)
        {
        }

        public FileListRetrievalException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileListRetrievalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}