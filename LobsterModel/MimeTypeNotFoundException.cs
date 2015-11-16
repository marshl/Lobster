using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class MimeTypeNotFoundException : Exception
    {
        public MimeTypeNotFoundException()
        {
        }

        public MimeTypeNotFoundException(string message) : base(message)
        {
        }

        public MimeTypeNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MimeTypeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}