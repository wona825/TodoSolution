using System.Net;

namespace Infrasfructure.Error
{
    [Serializable]
    public class CustomException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public CustomException(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
        }

        public CustomException(HttpStatusCode statusCode, string? message) : base(message)
        {
            StatusCode = statusCode;
        }

        public CustomException(HttpStatusCode statusCode, string? message, Exception? innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}