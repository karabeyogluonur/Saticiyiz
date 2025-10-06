namespace ST.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string message)
            : base("Not Found", message)
        {
        }

        public NotFoundException(string entityName, object key)
            : base("Not Found", $"Entity \"{entityName}\" ({key}) was not found.")
        {
        }
    }
}