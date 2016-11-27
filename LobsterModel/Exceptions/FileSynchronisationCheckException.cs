namespace LobsterModel
{
    using System;

    /// <summary>
    /// The exception for when a file cannot be found in a clob directory.
    /// </summary>
    public class FileSynchronisationCheckException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSynchronisationCheckException"/> class.
        /// </summary>
        public FileSynchronisationCheckException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSynchronisationCheckException"/> class 
        /// with an error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public FileSynchronisationCheckException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSynchronisationCheckException"/> class with an error 
        /// message and th inner exception tha tcaused this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of this exception..</param>
        public FileSynchronisationCheckException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
