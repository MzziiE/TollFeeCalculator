namespace TollFeeCalculator.Exceptions;

public class TollCalculatorException : Exception
{
    public TollCalculatorException(string message) : base(message) { }
}

public class InvalidPassageDatesException : TollCalculatorException
{
    public InvalidPassageDatesException(string message) : base(message) { }
}

public class MultipleDaysException : TollCalculatorException
{
    public MultipleDaysException(string message) : base(message) { }
}