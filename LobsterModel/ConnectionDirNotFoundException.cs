using System;
using System.Runtime.Serialization;

namespace LobsterModel
{
    /// <summary>
    /// An exception for when the user refuses to select a Connection Directory
    /// </summary>
    /// TODO: Kill this sumbitch
    [Serializable]
    public class ConnectionDirNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
        /// </summary>
        public ConnectionDirNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The exception messaage.</param>
        public ConnectionDirNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ConnectionDirNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDirNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialisation info.</param>
        /// <param name="context">The streaming context.</param>
        protected ConnectionDirNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
