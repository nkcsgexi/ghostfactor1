using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.refactoring;
using warnings.resources;
using warnings.util;

namespace warnings.conditions
{
    partial class InlineMethodConditionCheckersList
    {
        private class ChangedVariableValuesChecker : InlineMethodConditionsChecker
        {
            public override ICodeIssueComputer CheckInlineMethodCondition(IDocument before, IDocument after, 
                IInlineMethodRefactoring refactoring)
            {
                // Get the out going symbols before the method is inlined.
                var writtenSymbolsBeforeInline = ConditionCheckersUtils.GetFlowOutData(GetStatementEnclosingInvocation
                    (refactoring.InlinedMethodInvocation), before);

                // Get the out going symbols after the method is inlined.
                var writtenSymbolsAfterInline = ConditionCheckersUtils.GetFlowOutData(refactoring.InlinedStatementsInMethodAfter, after);
                
                // Calculate the symbols that are added by inlining method.
                var addedSymbols = ConditionCheckersUtils.GetSymbolListExceptByName(writtenSymbolsAfterInline, 
                    writtenSymbolsBeforeInline);

                // Calculate the symbols that are removed by inlining method.
                var missingSymbols = ConditionCheckersUtils.GetSymbolListExceptByName(writtenSymbolsBeforeInline,
                    writtenSymbolsAfterInline);
                
                // If found any missing and additional symbols, return a code issue computer.
                if(addedSymbols.Any() || missingSymbols.Any())
                {
                    return new ModifiedFlowOutData(refactoring.CallerMethodAfter, refactoring.InlinedStatementsInMethodAfter, 
                        addedSymbols, missingSymbols);
                }
                return new NullCodeIssueComputer();
            }

            /* Get the statement that is enclosing the given method invocation. */
            private SyntaxNode GetStatementEnclosingInvocation(SyntaxNode invocation)
            {
                SyntaxNode parent;
                for (parent = invocation; parent != null && !(parent is StatementSyntax); parent = parent.Parent);
                return parent;
            }

          
            private class ModifiedFlowOutData : ICodeIssueComputer
            {
                private readonly IEnumerable<ISymbol> missingSymbols;
                private readonly IEnumerable<ISymbol> addedSymbols;
                private readonly SyntaxNode methodAfter;
                private readonly IEnumerable<SyntaxNode> inlinedStatements;

                internal ModifiedFlowOutData( SyntaxNode methodAfter, IEnumerable<SyntaxNode> inlinedStatements,
                    IEnumerable<ISymbol> addedSymbols, IEnumerable<ISymbol> missingSymbols)
                {
                    this.methodAfter = methodAfter;
                    this.inlinedStatements = inlinedStatements;
                    this.addedSymbols = addedSymbols;
                    this.missingSymbols = missingSymbols;
                }

                public bool Equals(ICodeIssueComputer o)
                {
                    if(o is ModifiedFlowOutData)
                    {
                        var other = (ModifiedFlowOutData) o;
                        if(ConditionCheckersUtils.CompareSymbolListByName(other.missingSymbols, this.missingSymbols))
                        {
                            if(ConditionCheckersUtils.CompareSymbolListByName(other.addedSymbols, this.addedSymbols))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                public RefactoringType RefactoringType
                {
                    get { return RefactoringType.INLINE_METHOD; }
                }

                public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
                {
                    // The node should be a statement instance and the document is correct.
                    if(node is StatementSyntax && IsDocumentRight(document))
                    {
                        // Get the method containing the node.
                        var method = GetContainingMethod(node);

                        // If the outside method can be found and the method has the same name with the inlined method.
                        if(method != null && ASTUtil.AreMethodsNameSame(method, methodAfter))
                        {
                            // Get the statements in the found method that map with detected inlined statements.
                            var statements = GetCurrentInlinedStatements(method);

                            // If current node is among these statemens, return a code issue at the node.
                            if(statements.Contains(node))
                            {
                                yield return new CodeIssue(CodeIssue.Severity.Error, node.Span, GetDescription(), 
                                    new ICodeAction[]{new ModifiedFlowOutDataFix(document, methodAfter, inlinedStatements, 
                                        addedSymbols, missingSymbols)});
                            }
                        }   
                    }
                }

                /* Get the description of the issue. */
                private string GetDescription()
                {
                    var sb = new StringBuilder();
                    if (addedSymbols.Any())
                    {
                        sb.AppendLine("Inlined method may change variable " +
                                      StringUtil.ConcatenateAll(",", addedSymbols.Select(s => s.Name)));
                    }
                    if (missingSymbols.Any())
                    {
                        sb.AppendLine("Inlined method may fail to change " +
                                      StringUtil.ConcatenateAll(",", missingSymbols.Select(s => s.Name)));
                    }
                    return sb.ToString();
                }

                /* Is the document where the inline method refactoring happened? */
                private bool IsDocumentRight(IDocument document)
                {
                    // Get the qualified name of the type that encloses the method.
                    var analyzer = AnalyzerFactory.GetQualifiedNameAnalyzer();
                    analyzer.SetSyntaxNode(methodAfter);
                    var containingMethodName = analyzer.GetOutsideTypeQualifiedName();

                    // Get the qualified names of types that are contained in the document.
                    analyzer.SetSyntaxNode((SyntaxNode)document.GetSyntaxRoot());
                    var documentContainedNames = analyzer.GetInsideQualifiedNames();

                    // If the type names in the document contains the name we want. 
                    return documentContainedNames.Contains(containingMethodName);
                }

                /* Get the method that encloses a syntax node. */
                private SyntaxNode GetContainingMethod(SyntaxNode node)
                {
                    SyntaxNode method;
                    for (method = node; method != null && method.Kind != SyntaxKind.MethodDeclaration; method = method.Parent) ;
                    return method;
                }

                /* Get statements in the current method that map with the previously detected inlined statements. */
                private IEnumerable<SyntaxNode> GetCurrentInlinedStatements(SyntaxNode method)
                {
                    var list = new List<SyntaxNode>();
                    
                    // Get statements in the current method. 
                    var analyzer = AnalyzerFactory.GetMethodDeclarationAnalyzer();
                    analyzer.SetMethodDeclaration(method);
                    var statements = analyzer.GetStatements();

                    // If any of the statements is same with the detected statement, add it to the list.
                    foreach (var statement in statements)
                    {
                        if(inlinedStatements.Any(i => ASTUtil.AreSyntaxNodesSame(i, statement)))
                        {
                            list.Add(statement);
                        }
                    }

                    // Get the longest group of sequential statements.
                    return GetSequentialStatements(list);
                }

                // Get the longest sequential statements.
                private IEnumerable<SyntaxNode> GetSequentialStatements(IEnumerable<SyntaxNode> list)
                {
                    var analyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();
                    analyzer.SetSyntaxNodes(list);
                    list = analyzer.RemoveSubNodes();
                    analyzer.SetSyntaxNodes(list);
                    return analyzer.GetLongestNeighborredNodesGroup();
                }


                private class ModifiedFlowOutDataFix : ICodeAction
                {
                    private readonly IDocument document;
                    private readonly SyntaxNode method;
                    private readonly IEnumerable<SyntaxNode> inlinedStatements;
                    private readonly IEnumerable<ISymbol> addedSymbols;
                    private readonly IEnumerable<ISymbol> missingSymbols;

                    internal ModifiedFlowOutDataFix(IDocument document, SyntaxNode method, IEnumerable<SyntaxNode> inlinedStatements, 
                        IEnumerable<ISymbol> addedSymbols, IEnumerable<ISymbol> missingSymbols)
                    {
                        this.document = document;
                        this.method = method;
                        this.inlinedStatements = inlinedStatements;
                        this.addedSymbols = addedSymbols;
                        this.missingSymbols = missingSymbols;
                    }

                    public CodeActionEdit GetEdit(CancellationToken cancellationToken = new CancellationToken())
                    {
                        var modifidStatements = inlinedStatements;

                        // If additional symbols are modified, add statements to fix the problem. 
                        if(addedSymbols.Any())
                        {
                            foreach (var s in addedSymbols)
                            {
                                modifidStatements = AddAddedSymbolsFixStatements(modifidStatements, s);
                            }
                        }

                        // If missing symbols that should be modified, add statement to fix this problem.
                        if(missingSymbols.Any())
                        {
                            foreach (var s in missingSymbols)
                            {
                                modifidStatements = AddMissingSymbolsFixStatements(modifidStatements, s);
                            }
                        }

                        // Update method by changing the inlined statements with updated statements. 
                        var updatedMethod = UpdateStatements(method, inlinedStatements, modifidStatements);

                        // Update root and document, return the code edition. 
                        var root = (SyntaxNode) document.GetSyntaxRoot();
                        var updatedRoot = root.ReplaceNodes(new[] {method}, (node1, node2) => updatedMethod);
                        return new CodeActionEdit(document.UpdateSyntaxRoot(updatedRoot));
                    }


                    private IEnumerable<SyntaxNode> AddMissingSymbolsFixStatements(IEnumerable<SyntaxNode> statements, 
                        ISymbol symbol)
                    {
                        throw new NotImplementedException();
                    }

                    private IEnumerable<SyntaxNode> AddAddedSymbolsFixStatements(IEnumerable<SyntaxNode> statements, ISymbol s)
                    {
                        // Temp local variable to save the original value.
                        var tempName = "original" + s.Name;

                        // Assign the additional symbol to the temp.
                        var assign = Syntax.ParseStatement("var "+ tempName + " = " + s.Name +";");

                        // Assign the temp variable back to the symbol.
                        var assignBack = Syntax.ParseStatement(s.Name + " = " + tempName + ";");

                        // Add the assignment and assignment back to the proper positions of these statements.
                        var list = new List<SyntaxNode>();
                        list.Add(assign);
                        list.AddRange(statements);
                        list.Add(assignBack);
                        return list;
                    }

                    private SyntaxNode UpdateStatements(SyntaxNode method, IEnumerable<SyntaxNode> originalStatements, 
                        IEnumerable<SyntaxNode> newStatements)
                    {
                        // Get the block and the statements in the block of the given method.
                        var analyzer = AnalyzerFactory.GetMethodDeclarationAnalyzer();
                        analyzer.SetMethodDeclaration(method);
                        var block = (BlockSyntax) analyzer.GetBlock();
                        var statements = block.Statements;
                        
                        // Get the start and end position of these inlined statements.
                        var start = statements.IndexOf((StatementSyntax) originalStatements.First());
                        var end = statements.IndexOf((StatementSyntax) originalStatements.Last());
                        
                        // New updated statements.
                        var updatedStatements = new SyntaxList<StatementSyntax>();
                        
                        // Copy the statements before inlined statements to the updated statements list.
                        for (int i = 0; i < start; i ++ )
                        {
                            updatedStatements.Add(statements.ElementAt(i));
                        }

                        // Copy the udpated inlined statements to the list.
                        updatedStatements.Add(newStatements.Select(s => (StatementSyntax)s).ToArray());
                        
                        // Copy the statements after the inlined statements back to the list.
                        for(int i = end + 1; i< statements.Count; i++ )
                        {
                            updatedStatements.Add(statements.ElementAt(i));
                        }

                        // Get a new block with the updated statement list.
                        var updatedBlock = block.Update(block.OpenBraceToken, updatedStatements, block.CloseBraceToken);

                        // Update the block in the method.
                        return new BlockRewriter(updatedBlock).Visit(method);
                    }

                    private class BlockRewriter : SyntaxRewriter
                    {
                        private readonly SyntaxNode updatedBlock;

                        internal BlockRewriter(SyntaxNode updatedBlock)
                        {
                            this.updatedBlock = updatedBlock;
                        }

                        public override SyntaxNode VisitBlock(BlockSyntax node)
                        {
                            return updatedBlock;
                        }
                    }

                    public ImageSource Icon
                    {
                        get { return ResourcePool.GetIcon(); }
                    }

                    public string Description
                    {
                        get { return "Fix the changed data by inlining method."; }
                    }
                }
            }
        }
    }
}
