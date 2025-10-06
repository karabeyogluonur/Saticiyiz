namespace ST.Application.Exceptions
{
    public class ForbiddenAccessException : ApplicationException
    {
        public ForbiddenAccessException(string message)
            : base("Forbidden Access", message)
        {
        }

        public ForbiddenAccessException()
            : base("Forbidden Access", "You do not have permission to access this resource.")
        {
        }
    }
}