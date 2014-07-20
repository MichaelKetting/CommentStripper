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
using System.Linq;
using CommentStripper.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CommentStripper
{
  public class CommentStripperCSharpSyntaxRewriter : CSharpSyntaxRewriter
  {
    public CommentStripperCSharpSyntaxRewriter ()
    {
    }

    public override SyntaxTrivia VisitTrivia (SyntaxTrivia trivia)
    {
      ArgumentUtility.CheckNotNull ("trivia", trivia);

      //if (trivia.IsKind (SyntaxKind.SingleLineCommentTrivia))
      //  return new SyntaxTrivia ();

      if (trivia.IsKind (SyntaxKind.MultiLineCommentTrivia))
        return new SyntaxTrivia ();

      return base.VisitTrivia (trivia);
    }

    public override SyntaxToken VisitToken (SyntaxToken token)
    {
      ArgumentUtility.CheckNotNull ("token", token);

      bool isStartOfFile = token.FullSpan.Start == 0 && token.Parent != null && token.Parent.Parent is CompilationUnitSyntax;
      if (!isStartOfFile)
      {
        var leadingTrivia = token.LeadingTrivia;
        var newLeadingTrivia = RemoveCommentsFromTriva (leadingTrivia, true);
        if (leadingTrivia.Count != newLeadingTrivia.Count)
          token = token.WithLeadingTrivia (newLeadingTrivia);
      }

      var trailingTrivia = token.TrailingTrivia;
      var newTrailingTriva = RemoveCommentsFromTriva (trailingTrivia, false);
      if (trailingTrivia.Count != newTrailingTriva.Count)
        token = token.WithTrailingTrivia (newTrailingTriva);

      return base.VisitToken (token);
    }

    private List<SyntaxTrivia> RemoveCommentsFromTriva (SyntaxTriviaList triviaList, bool removeEndOfLineTrivia)
    {
      var newTriviaList = new List<SyntaxTrivia>();
      for (int i = 0; i < triviaList.Count; i++)
      {
        var currentTrivia = triviaList[i];
        var nextTrivia = i + 1 < triviaList.Count ? triviaList[i + 1] : new SyntaxTrivia();

        var isWhitespaceThenSingleLineComment = currentTrivia.IsKind (SyntaxKind.WhitespaceTrivia)
                                                && nextTrivia.IsKind (SyntaxKind.SingleLineCommentTrivia);

        var isSingleLineCommentThenEndOfLine = currentTrivia.IsKind (SyntaxKind.SingleLineCommentTrivia)
                                               && nextTrivia.IsKind (SyntaxKind.EndOfLineTrivia);

        var isSingleLineCommentThenEndOfFile = currentTrivia.IsKind (SyntaxKind.SingleLineCommentTrivia)
                                               && nextTrivia.IsKind (SyntaxKind.None) 
                                               && currentTrivia.Token.IsKind (SyntaxKind.EndOfFileToken);

        if (isWhitespaceThenSingleLineComment)
          continue;
        if (isSingleLineCommentThenEndOfLine && removeEndOfLineTrivia)
          i++;
        if (isSingleLineCommentThenEndOfLine)
          continue;
        if (isSingleLineCommentThenEndOfFile)
          continue;

        newTriviaList.Add (currentTrivia);
      }
      return newTriviaList;
    }
  }
}