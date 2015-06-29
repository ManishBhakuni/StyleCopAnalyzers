﻿namespace StyleCop.Analyzers.SpacingRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// An opening square bracket within a C# statement is not spaced correctly.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when an opening square bracket within a statement is preceded or followed
    /// by whitespace.</para>
    ///
    /// <para>An opening square bracket must never be preceded by whitespace, unless it is the first character on the
    /// line, and an opening square must never be followed by whitespace, unless it is the last character on the
    /// line.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1010OpeningSquareBracketsMustBeSpacedCorrectly : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1010OpeningSquareBracketsMustBeSpacedCorrectly"/>
        /// analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1010";
        private const string Title = "Opening square brackets must be spaced correctly";
        private const string MessageFormat = "Opening square brackets must {0} by a space.";
        private const string Category = "StyleCop.CSharp.SpacingRules";
        private const string Description = "An opening square bracket within a C# statement is not spaced correctly.";
        private const string HelpLink = "http://www.stylecop.com/docs/SA1010.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeActionHonorExclusions(HandleSyntaxTree);
        }

        private static void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            foreach (var token in root.DescendantTokens())
            {
                if (token.IsKind(SyntaxKind.OpenBracketToken) && !token.IsMissing)
                {
                    // attribute brackets are handled separately
                    if (!token.Parent.IsKind(SyntaxKind.AttributeList))
                    {
                        HandleOpenBracketToken(context, token);
                    }
                }
            }
        }

        private static void HandleOpenBracketToken(SyntaxTreeAnalysisContext context, SyntaxToken token)
        {
            bool precededBySpace;
            bool firstInLine;
            bool ignorePrecedingSpaceProblem = false;

            firstInLine = token.HasLeadingTrivia || token.GetLocation()?.GetMappedLineSpan().StartLinePosition.Character == 0;
            if (firstInLine)
            {
                precededBySpace = true;
            }
            else
            {
                SyntaxToken precedingToken = token.GetPreviousToken();
                precededBySpace = precedingToken.HasTrailingTrivia;

                // ignore if handled by SA1026
                ignorePrecedingSpaceProblem = precededBySpace && precedingToken.IsKind(SyntaxKind.NewKeyword);
            }

            bool followedBySpace = token.HasTrailingTrivia;
            bool lastInLine = followedBySpace && token.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);

            if (!firstInLine && precededBySpace && !ignorePrecedingSpaceProblem && !lastInLine && followedBySpace)
            {
                // Opening square bracket must {neither preceded nor followed} by a space.
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation(), "neither preceded nor followed"));
            }
            else if (!firstInLine && precededBySpace && !ignorePrecedingSpaceProblem)
            {
                // Opening square bracket must {not be preceded} by a space.
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation(), "not be preceded"));
            }
            else if (!lastInLine && followedBySpace)
            {
                // Opening square bracket must {not be followed} by a space.
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation(), "not be followed"));
            }
        }
    }
}
