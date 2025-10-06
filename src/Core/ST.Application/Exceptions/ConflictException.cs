namespace ST.Application.Exceptions
{
    public class ConflictException : ApplicationException
    {
        public ConflictException(string message)
            : base("Conflict", message)
        {
        }
    }
}