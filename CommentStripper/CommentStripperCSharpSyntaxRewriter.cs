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

    public override SyntaxToken VisitToken (SyntaxToken token)
    {
      ArgumentUtility.CheckNotNull ("token", token);

      bool isStartOfFile = token.FullSpan.Start == 0 && token.Parent != null && token.Parent.Parent is CompilationUnitSyntax;
      if (!isStartOfFile)
      {
        var leadingTrivia = token.LeadingTrivia;
        var newLeadingTrivia = RemoveSingleLineCommentsFromTriva (leadingTrivia, true);
        newLeadingTrivia = RemoveMultiLineCommentsFromTriva (newLeadingTrivia, true);
        token = token.WithLeadingTrivia (newLeadingTrivia);
      }

      var trailingTrivia = token.TrailingTrivia;
      var newTrailingTriva = RemoveSingleLineCommentsFromTriva (trailingTrivia, false);
      newTrailingTriva = RemoveMultiLineCommentsFromTriva (newTrailingTriva, false);
      token = token.WithTrailingTrivia (newTrailingTriva);

      return base.VisitToken (token);
    }

    private List<SyntaxTrivia> RemoveSingleLineCommentsFromTriva (IReadOnlyList<SyntaxTrivia> triviaList, bool removeEndOfLineTrivia)
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

    private List<SyntaxTrivia> RemoveMultiLineCommentsFromTriva (IReadOnlyList<SyntaxTrivia> triviaList, bool removeEndOfLineTrivia)
    {
      var newTriviaList = new List<SyntaxTrivia>();
      var triviaToKeep = new List<SyntaxTrivia>();
      for (int i = 0; i < triviaList.Count; i++)
      {
        var currentTrivia = triviaList[i];
        var nextTrivia = i + 1 < triviaList.Count ? triviaList[i + 1] : new SyntaxTrivia();

        var isWhitespaceThenMultiLineComment = currentTrivia.IsKind (SyntaxKind.WhitespaceTrivia)
                                               && nextTrivia.IsKind (SyntaxKind.MultiLineCommentTrivia);

        var isMultiLineCommentThenEndOfLine = currentTrivia.IsKind (SyntaxKind.MultiLineCommentTrivia)
                                              && nextTrivia.IsKind (SyntaxKind.EndOfLineTrivia);

        var isMultiLineCommentThenEndOfFile = currentTrivia.IsKind (SyntaxKind.MultiLineCommentTrivia)
                                              && nextTrivia.IsKind (SyntaxKind.None)
                                              && currentTrivia.Token.IsKind (SyntaxKind.EndOfFileToken);

        var isMultiLineComment = currentTrivia.IsKind (SyntaxKind.MultiLineCommentTrivia); // followed by non-removable syntax

        if (isWhitespaceThenMultiLineComment)
        {
          triviaToKeep.Add (currentTrivia);
          continue;
        }
        if (isMultiLineCommentThenEndOfLine && removeEndOfLineTrivia)
          i++;
        if (isMultiLineCommentThenEndOfLine)
          continue;
        if (isMultiLineCommentThenEndOfFile)
          continue;
        if (isMultiLineComment)
        {
          if (currentTrivia.ToString().Contains ("\n"))
            triviaToKeep.Insert (0, SyntaxFactory.LineFeed);
          if (currentTrivia.ToString().Contains ("\r"))
            triviaToKeep.Insert (0, SyntaxFactory.CarriageReturn);
          if (triviaToKeep.Any())
            newTriviaList.AddRange (triviaToKeep);
          continue;
        }

        newTriviaList.Add (currentTrivia);
      }
      return newTriviaList;
    }
  }
}