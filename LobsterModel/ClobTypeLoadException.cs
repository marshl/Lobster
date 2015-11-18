using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    [Serializable]
    public class ClobTypeLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeLoadException"/> class.
        /// </summary>
        public ClobTypeLoadException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeLoadException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ClobTypeLoadException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeLoadException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public ClobTypeLoadException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClobTypeLoadException"/> class with the serialied info.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information
        /// about the source or destination.</param>
        protected ClobTypeLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}