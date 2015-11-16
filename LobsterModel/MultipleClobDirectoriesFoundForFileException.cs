using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class MultipleClobDirectoriesFoundForFileException : Exception
    {
        public MultipleClobDirectoriesFoundForFileException()
        {
        }

        public MultipleClobDirectoriesFoundForFileException(string message) : base(message)
        {
        }

        public MultipleClobDirectoriesFoundForFileException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MultipleClobDirectoriesFoundForFileException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}