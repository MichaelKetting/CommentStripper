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
using System.Diagnostics;
using System.IO;

namespace CommentStripper
{
  internal class Program
  {
    private static int Main (string[] args)
    {
      if (args.Length != 1)
      {
        Console.WriteLine ("Please specify file with one of the following extensions: SLN, CSPROJ, CS");
        return -1;
      }

      var file = args[0];
      if (!File.Exists (file))
      {
        Console.WriteLine ("Please specify a valid file path.");
        return -2;
      }
      try
      {
        var stopwatch = Stopwatch.StartNew();
        var projectFileHandler = CreateSourceFileProvider (file);
        var sourceFileHandler = new CSharpSourceFileHandler();
        var syntaxTreeHandler = new SyntaxNodeHandler (new CommentStripperCSharpSyntaxRewriter());

        Console.WriteLine ("Processing source files for '{0}'...", file);
        foreach (var sourceFile in projectFileHandler.ReadAllSourceFiles())
          sourceFileHandler.ApplySyntaxTreeTransformation (sourceFile, syntaxTreeHandler);

        stopwatch.Stop();
        Console.WriteLine ("Finished processing source files for '{0}'. Time taken: {1} seconds", file, stopwatch.Elapsed.TotalSeconds);
        return 0;
      }
      catch (Exception ex)
      {
        Console.WriteLine ("Error processing sources for file '{0}'.", file);
        Console.WriteLine (ex);
        return -2;
      }
    }

    private static IFileProvider CreateSourceFileProvider (string file)
    {
      var extension = (Path.GetExtension (file) ?? "").ToLower().TrimStart ('.');
      switch (extension)
      {
        case "sln":
          return new SolutionBasedFileProvider (file);
        case "csproj":
          return new ProjectBasedFileProvider (file);
        case "cs":
          return new SourceFileBasedFileProvider (file);
        default:
          throw new ArgumentException ("Invalid file type. Supported extensions: SLN, CSPROJ, CS");
      }
    }
  }
}