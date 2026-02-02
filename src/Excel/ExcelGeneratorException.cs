using System;

namespace Maestria.TypeProviders.Excel;

internal class ExcelGeneratorException : Exception
{
    public ExcelGeneratorException()
    {
    }

    public ExcelGeneratorException(string message) : base(message)
    {
    }

    public ExcelGeneratorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

internal class ExcelArgumentOutOfRangeException : ArgumentOutOfRangeException
{
    public ExcelArgumentOutOfRangeException()
    {
    }
    
    public ExcelArgumentOutOfRangeException(string paramName) : base(paramName)
    {
    }

    public ExcelArgumentOutOfRangeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ExcelArgumentOutOfRangeException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
    {
    }

    public ExcelArgumentOutOfRangeException(string paramName, string message) : base(paramName, message)
    {
    }
}

internal class ExcelArgumentNullException : ArgumentNullException
{
    public ExcelArgumentNullException()
    {
    }

    public ExcelArgumentNullException(string paramName) : base(paramName)
    {
    }

    public ExcelArgumentNullException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ExcelArgumentNullException(string paramName, string message) : base(paramName, message)
    {
    }
}