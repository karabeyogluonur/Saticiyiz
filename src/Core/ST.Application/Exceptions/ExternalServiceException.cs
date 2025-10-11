namespace ST.Application.Exceptions
{
    public class ExternalServiceException : ApplicationException
    {
        public ExternalServiceException(string serviceName, string message)
            : base($"Error from {serviceName}", message)
        {
        }
    }
}
