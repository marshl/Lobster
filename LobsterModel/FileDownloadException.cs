using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class FileDownloadException : Exception
    {
        public FileDownloadException()
        {
        }

        public FileDownloadException(string message) : base(message)
        {
        }

        public FileDownloadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileDownloadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}