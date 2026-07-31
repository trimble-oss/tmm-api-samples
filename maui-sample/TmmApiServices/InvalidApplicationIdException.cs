using System;
using System.Collections.Generic;
using System.Text;

namespace MauiSample;

internal class InvalidApplicationIdException : Exception
{
  public InvalidApplicationIdException(string message) : base(message)
  {
  }
}
