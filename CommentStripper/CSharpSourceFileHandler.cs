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
using System.IO;
using System.Text;
using CommentStripper.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CommentStripper
{
  public class CSharpSourceFileHandler
  {
    private static readonly Encoding s_utf8NoBom = new UTF8Encoding (false, true);

    public void ApplySyntaxTreeTransformation (string filePath, ISyntaxNodeHandler syntaxTreeHandler)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("sourceFile", filePath);
      ArgumentUtility.CheckNotNull ("syntaxTreeHandler", syntaxTreeHandler);

      var sourceFileData = ParseSourceFile (filePath);
      var encoding = sourceFileData.Item2;
      var syntaxTree = sourceFileData.Item1;
      var syntaxNode = syntaxTree.GetRoot();

      syntaxNode = syntaxTreeHandler.Apply (syntaxNode);

      WriteTreeToSourceFile (filePath, encoding, syntaxNode);
    }

    private Tuple<SyntaxTree, Encoding> ParseSourceFile (string filePath)
    {
      try
      {
        var encoding = GetEncoding (filePath);
        var syntaxTree = CSharpSyntaxTree.ParseFile (filePath);
        return Tuple.Create (syntaxTree, encoding);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException (string.Format ("Unable to parse source file '{0}'.", filePath), ex);
      }
    }

    private void WriteTreeToSourceFile (string filePath, Encoding encoding, SyntaxNode rootNode)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("filePath", filePath);
      ArgumentUtility.CheckNotNull ("rootNode", rootNode);

      try
      {
        using (var fileStream = new FileStream (filePath, FileMode.Truncate))
        {
          using (var writer = new StreamWriter (fileStream, encoding))
          {
            rootNode.WriteTo (writer);
          }
        }
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException (string.Format ("Unable to write source file '{0}'.", filePath), ex);
      }
    }

    private Encoding GetEncoding (string filePath)
    {
      var encodings = new[] { Encoding.Unicode, Encoding.UTF8 };
      byte[] data = new byte[10];
      using (var file = File.Open (filePath, FileMode.Open))
      {
        file.Read (data, 0, data.Length);
      }

      foreach (Encoding encoding in encodings)
      {
        if (MatchByteOrderMark (data, encoding))
          return encoding;
      }

      //Fallback for missing byte order mark.
      return s_utf8NoBom;
    }

    private bool MatchByteOrderMark (byte[] data, Encoding encoding)
    {
      byte[] byteOrderMark = encoding.GetPreamble();

      if (byteOrderMark.Length > data.Length)
        return false;

      for (int i = 0; i < byteOrderMark.Length; i++)
      {
        if (byteOrderMark[i] != data[i])
          return false;
      }

      return true;
    }
  }
}