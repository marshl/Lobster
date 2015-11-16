using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class ClobDirectoryNotFoundForFileException : Exception
    {
        public ClobDirectoryNotFoundForFileException()
        {
        }

        public ClobDirectoryNotFoundForFileException(string message) : base(message)
        {
        }

        public ClobDirectoryNotFoundForFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClobDirectoryNotFoundForFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}