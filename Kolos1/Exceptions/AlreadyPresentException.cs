namespace Kolos1.Exceptions;

public class AlreadyPresentException : Exception
{
    public AlreadyPresentException()
    {
    }
    public AlreadyPresentException(string? message) : base(message)
    {
    }

    public AlreadyPresentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}