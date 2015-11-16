using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    internal class DatabaseConnectionFailedException : Exception
    {
        private Exception e;

        public DatabaseConnectionFailedException()
        {
        }

        public DatabaseConnectionFailedException(string message) : base(message)
        {
        }

        public DatabaseConnectionFailedException(Exception e)
        {
            this.e = e;
        }

        public DatabaseConnectionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DatabaseConnectionFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}