using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace warnings.analyzer
{
    /* Analyzer for getting information of a method invocation. */
    public interface IMethodInvocationAnalyzer
    {
        void SetMethodInvocation(SyntaxNode invocation);
        SyntaxNode GetMethodName();
        IEnumerable<SyntaxNode> GetArguments();
        bool HasArguments();
    }

    internal class MethodInvocationAnalyzer : IMethodInvocationAnalyzer
    {

        private static int ANALYZER_COUNT = 0;

        public static int GetCount()
        {
            return ANALYZER_COUNT;
        }

        internal MethodInvocationAnalyzer()
        {
            ANALYZER_COUNT++;
        }

        ~MethodInvocationAnalyzer()
        {
            ANALYZER_COUNT--;
        }

        private InvocationExpressionSyntax invocation;

        private Logger logger = NLoggerUtil.getNLogger(typeof (MethodInvocationAnalyzer));

        public void SetMethodInvocation(SyntaxNode invocation)
        {
            this.invocation = (InvocationExpressionSyntax) invocation;
        }

        public SyntaxNode GetMethodName()
        {
            logger.Info(invocation);
            logger.Info(invocation.DescendantNodes().Count());

            // The left parentheses is the rightmost position for the method name.
            int rightMost = invocation.ArgumentList.OpenParenToken.Span.Start;

            // Order the decendents by its span
            var orderedDecendents = invocation.DescendantNodes().OrderBy(n => n.Span.Length);

            // The decendent whose length is the most and end is before ( should be method name node.
            var name = orderedDecendents.Last(n => n.Span.End <= rightMost);
            logger.Info(name.Kind.ToString);
            return name;
        }

        public IEnumerable<SyntaxNode> GetArguments()
        {
            return invocation.ArgumentList.Arguments;
        }

        public bool HasArguments()
        {
            return invocation.ArgumentList.Arguments.Any();
        }
    }

}
