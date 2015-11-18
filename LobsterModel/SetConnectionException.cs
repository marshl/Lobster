using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    public class SetConnectionException : Exception
    {
        public SetConnectionException()
        {
        }

        public SetConnectionException(string message) : base(message)
        {
        }

        public SetConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SetConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}