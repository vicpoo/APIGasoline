using System;
using System.Collections.Generic;
using System.Text;

namespace APIGasoline.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}