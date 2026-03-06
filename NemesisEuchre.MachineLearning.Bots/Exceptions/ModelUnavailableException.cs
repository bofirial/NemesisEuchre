namespace NemesisEuchre.MachineLearning.Bots.Exceptions;

public class ModelUnavailableException : InvalidOperationException
{
    public ModelUnavailableException()
    {
    }

    public ModelUnavailableException(string message)
        : base(message)
    {
    }

    public ModelUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
