using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class FileUpdateFailedException : Exception
    {
        private ClobColumnNotFoundException e;

        public FileUpdateFailedException()
        {
        }

        public FileUpdateFailedException(string message) : base(message)
        {
        }

        public FileUpdateFailedException(ClobColumnNotFoundException e)
        {
            this.e = e;
        }

        public FileUpdateFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected FileUpdateFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}