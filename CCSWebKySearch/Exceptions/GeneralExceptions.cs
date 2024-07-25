namespace CCSWebKySearch.Exceptions
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

}