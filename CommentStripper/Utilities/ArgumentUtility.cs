#region copyright
// Copyright (c) Michael Ketting

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
#endregion

using System;

namespace CommentStripper.Utilities
{
  public static class ArgumentUtility
  {
    public static object CheckNotNull (string argumentName, object actualValue)
    {
      if (actualValue == null)
        throw new ArgumentNullException (argumentName);

      return actualValue;
    }

    public static string CheckNotNullOrEmpty (string argumentName, string actualValue)
    {
      CheckNotNull (argumentName, actualValue);

      if (actualValue.Length == 0)
        throw new ArgumentException (String.Format ("String cannot be empty. Parameter name: {0}", argumentName));

      return actualValue;
    }
  }
}