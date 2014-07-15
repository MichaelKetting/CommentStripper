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
using CommentStripper;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace UnitTests
{
  [TestFixture]
  public class CommentStripperCSharpSyntaxRewriterTest
  {
    [Test]
    public void RemoveSingleLineCommentTrivia_WithCommentBeforeBody ()
    {
      var tree = CSharpSyntaxTree.ParseText (
          @"//Comment
    public void MyMethod ()
    {
    }
");

      const string expectedTreeString =
          @"public void MyMethod ()
    {
    }
";
    }

    [Test]
    public void RemoveSingleLineCommentTrivia_WithCommentInBody ()
    {
      var tree = CSharpSyntaxTree.ParseText (
    @"
    public void MyMethod ()
    {
      var i = 0;
      // Comment
      i++;
    }
");

      const string expectedTreeString =
          @"
    public void MyMethod ()
    {
      var i = 0;
      i++;
    }
";

      var rewriter = new CommentStripperCSharpSyntaxRewriter ();

      Assert.That (rewriter.Visit (tree.GetRoot ()).ToFullString (), Is.EqualTo (expectedTreeString));
    }

    [Test]
    public void RemoveSingleLineCommentTrivia_WithTwoCommentLinesInBody ()
    {
      var tree = CSharpSyntaxTree.ParseText (
    @"
    public void MyMethod ()
    {
      var i = 0; // Comment
      // Comment
      i++;
    }
");

      const string expectedTreeString =
          @"
    public void MyMethod ()
    {
      var i = 0;
      i++;
    }
";

      var rewriter = new CommentStripperCSharpSyntaxRewriter ();

      Assert.That (rewriter.Visit (tree.GetRoot ()).ToFullString (), Is.EqualTo (expectedTreeString));
    }

    [Test]
    public void RemoveSingleLineCommentTrivia_WithCommentAfterBodyAndEndOfLine ()
    {
      var tree = CSharpSyntaxTree.ParseText (
          @"public void MyMethod ()
    {
    }
//Comment
");

      const string expectedTreeString =
          @"public void MyMethod ()
    {
    }
";

      var rewriter = new CommentStripperCSharpSyntaxRewriter ();

      Assert.That (rewriter.Visit (tree.GetRoot ()).ToFullString (), Is.EqualTo (expectedTreeString));
    }

    [Test]
    public void RemoveSingleLineCommentTrivia_WithCommentAfterBodyAndNoEndOfLine ()
    {
      var tree = CSharpSyntaxTree.ParseText (
          @"public void MyMethod ()
    {
    }
//Comment");

      const string expectedTreeString =
          @"public void MyMethod ()
    {
    }
";

      var rewriter = new CommentStripperCSharpSyntaxRewriter ();

      Assert.That (rewriter.Visit (tree.GetRoot ()).ToFullString (), Is.EqualTo (expectedTreeString));
    }

    [Test]
    public void RemoveSingleLineCommentTrivia ()
    {
            var tree = CSharpSyntaxTree.ParseText (
          @"// Comment
namespace MyNamespace // Comment
{ // Comment
  // Comment
  public class MyClass // Comment
  { // Comment
    // Comment
    public void MyMethod () // Comment
    { // Comment
      var i = 0; // Comment
      // Comment
      i++;
    } // Comment
    // Comment
  } // Comment
  // Comment
} // Comment
// Comment");

      const string expectedTreeString =
          @"namespace MyNamespace
{
  public class MyClass
  {
    public void MyMethod ()
    {
      var i = 0;
      i++;
    }
  }
}
";

      var rewriter = new CommentStripperCSharpSyntaxRewriter();

      Assert.That (rewriter.Visit (tree.GetRoot()).ToFullString(), Is.EqualTo (expectedTreeString));
    }

    [Test]
    public void RemoveMultiLineCommentTrivia ()
    {
      var tree = CSharpSyntaxTree.ParseText (
    @"/* Comment
*
*/namespace MyNamespace /* Comment
*/{ /* Comment
*/public class MyClass /* Comment
*
*/{ /* Comment
*
**/ public void MyMethod () /* Comment
*
*/  { /* Comment
*
*/    var i = 0; /* Comment
*
*/    i++; /* Comment
*
*/ } /* Comment
*
*/ } /* Comment
*
*/ } /* Comment
*
*/");

      const string expectedTreeString =
          @"namespace MyNamespace { public class MyClass {  public void MyMethod ()   {     var i = 0;     i++;  }  }  } ";

      var rewriter = new CommentStripperCSharpSyntaxRewriter ();

      Assert.That (rewriter.Visit (tree.GetRoot ()).ToFullString (), Is.EqualTo (expectedTreeString));
    }
  }
}

