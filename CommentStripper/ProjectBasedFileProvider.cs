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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommentStripper.Utilities;

namespace CommentStripper
{
  public class ProjectBasedFileProvider : IFileProvider
  {
    private readonly string _projectFile;

    public ProjectBasedFileProvider (string projectFile)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("projectFile", projectFile);


      _projectFile = projectFile;
    }

    /// <summary>
    /// Returns a list of all cs-source files of the project.
    /// </summary>
    public IEnumerable<string> ReadAllSourceFiles ()
    {
      var projectDirectory = Path.GetDirectoryName (_projectFile) ?? ".";
      try
      {
        return XDocument.Load (_projectFile)
            .Descendants()
            .Where (d => d.Name == NS.Project + "Compile")
            .Where (d => !IsLinkedSource (d))
            .Where (d => !IsGeneratedSource (d))
            .Attributes ("Include")
            .Select (n => Path.Combine (projectDirectory, n.Value))
            .OrderBy (f => f);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException (string.Format ("Unable to read included source files from project file '{0}'.", _projectFile), ex);
      }
    }

    private static bool IsLinkedSource (XElement compileElement)
    {
      return compileElement.Elements (NS.Project + "Link").Any();
    }

    private static bool IsGeneratedSource (XElement compileElement)
    {
      // heuristic approach
      bool isNamedDesigner = compileElement.Attributes ("Include")
          .Any (v => ((string) v).EndsWith (".Designer.cs", StringComparison.InvariantCultureIgnoreCase));
      bool isDependentFile = compileElement.Elements (NS.Project + "DependentUpon").Any ();
      return isNamedDesigner && isDependentFile;
    }
  }
}