///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.grammar.@internal.generated;

using static com.espertech.esper.common.@internal.util.StringValue;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    /// Utility class for AST node handling.
    /// </summary>
    public class ASTUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string PROPERTY_ENABLED_AST_DUMP = "ENABLE_AST_DUMP";

        public static IList<string> GetIdentList(EsperEPL2GrammarParser.ColumnListContext ctx)
        {
            if (ctx == null || ctx.IsEmpty)
            {
                return new EmptyList<string>();
            }

            IList<ITerminalNode> idents = ctx.IDENT();
            IList<string> parameters = new List<string>(idents.Count);
            foreach (var ident in idents)
            {
                parameters.Add(ident.GetText());
            }

            return parameters;
        }

        public static bool IsTerminatedOfType(
            ITree child,
            int tokenType)
        {
            if (!(child is ITerminalNode))
            {
                return false;
            }

            var termNode = (ITerminalNode) child;
            return termNode.Symbol.Type == tokenType;
        }

        public static int GetRuleIndexIfProvided(IParseTree tree)
        {
            if (!(tree is IRuleNode))
            {
                return -1;
            }

            var ruleNode = (IRuleNode) tree;
            return ruleNode.RuleContext.RuleIndex;
        }

        public static int GetAssertTerminatedTokenType(IParseTree child)
        {
            if (!(child is ITerminalNode))
            {
                throw ASTWalkException.From("Unexpected exception walking AST, expected terminal node", child.GetText());
            }

            var term = (ITerminalNode) child;
            return term.Symbol.Type;
        }

        public static string PrintNode(ITree node)
        {
            var writer = new StringWriter();
            ASTUtil.DumpAST(writer, node, 0);
            return writer.ToString();
        }

        public static bool IsRecursiveParentRule(
            RuleContext ctx,
            ISet<int> rulesIds)
        {
            var parent = ctx.Parent;
            if (parent == null)
            {
                return false;
            }

            return rulesIds.Contains(parent.RuleIndex) || IsRecursiveParentRule(parent, rulesIds);
        }

        /// <summary>
        /// Dump the AST node to system.out.
        /// </summary>
        /// <param name="ast">to dump</param>
        public static void DumpAST(ITree ast)
        {
            if (Environment.GetEnvironmentVariable(PROPERTY_ENABLED_AST_DUMP) != null)
            {
                var writer = new StringWriter();

                RenderNode(new char[0], ast, writer);
                DumpAST(writer, ast, 2);

                Log.Info(".dumpAST ANTLR Tree dump follows...\n" + writer.ToString());
            }
        }

        public static void DumpAST(
            TextWriter textWriter,
            ITree ast,
            int ident)
        {
            var identChars = new char[ident];
            CompatExtensions.Fill(identChars, ' ');

            if (ast == null)
            {
                RenderNode(identChars, null, textWriter);
                return;
            }

            for (var i = 0; i < ast.ChildCount; i++)
            {
                var node = ast.GetChild(i);
                if (node == null)
                {
                    throw new NullReferenceException("Null AST node");
                }

                RenderNode(identChars, node, textWriter);
                DumpAST(textWriter, node, ident + 2);
            }
        }

        /// <summary>
        /// Print the token stream to the logger.
        /// </summary>
        /// <param name="tokens">to print</param>
        public static void PrintTokens(CommonTokenStream tokens)
        {
            if (Log.IsDebugEnabled)
            {
                var tokenList = tokens.GetTokens();
                var writer = new StringWriter();
                for (var i = 0; i < tokens.Size; i++)
                {
                    var t = (IToken) tokenList[i];
                    var text = t.Text;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        writer.Write("'" + text + "'");
                    }
                    else
                    {
                        writer.Write(text);
                    }

                    writer.Write('[');
                    writer.Write(t.Type);
                    writer.Write(']');
                    writer.Write(" ");
                }

                writer.WriteLine();
                Log.Debug("Tokens: " + writer.ToString());
            }
        }

        private static void RenderNode(
            char[] ident,
            ITree node,
            TextWriter printer)
        {
            printer.Write(ident);
            if (node == null)
            {
                printer.Write("NULL NODE");
            }
            else
            {
                if (node is ParserRuleContext)
                {
                    var ctx = (ParserRuleContext) node;
                    var ruleIndex = ctx.RuleIndex;
                    var ruleName = EsperEPL2GrammarParser.ruleNames[ruleIndex];
                    printer.Write(ruleName);
                }
                else
                {
                    var terminal = (ITerminalNode) node;
                    printer.Write(terminal.Symbol.Text);
                    printer.Write(" [");
                    printer.Write(terminal.Symbol.Type);
                    printer.Write("]");
                }

                if (node is IParseTree parseTree)
                {
                    var parseTreeText = parseTree.GetText();
                    if (parseTreeText == null)
                    {
                        printer.Write(" (null value in text)");
                    }
                    else if (parseTreeText.Contains("\\"))
                    {
                        var count = 0;
                        for (var i = 0; i < parseTreeText.Length; i++)
                        {
                            if (parseTreeText[i] == '\\')
                            {
                                count++;
                            }
                        }

                        printer.Write(" (" + count + " backlashes)");
                    }
                }
            }

            printer.WriteLine();
        }

        /// <summary>
        /// Escape all unescape dot characters in the text (identifier only) passed in.
        /// </summary>
        /// <param name="identifierToEscape">text to escape</param>
        /// <returns>text where dots are escaped</returns>
        internal static string EscapeDot(string identifierToEscape)
        {
            var indexof = identifierToEscape.IndexOf(".");
            if (indexof == -1)
            {
                return identifierToEscape;
            }

            var builder = new StringBuilder();
            for (var i = 0; i < identifierToEscape.Length; i++)
            {
                var c = identifierToEscape[i];
                if (c != '.')
                {
                    builder.Append(c);
                    continue;
                }

                if (i > 0)
                {
                    if (identifierToEscape[i - 1] == '\\')
                    {
                        builder.Append('.');
                        continue;
                    }
                }

                builder.Append('\\');
                builder.Append('.');
            }

            return builder.ToString();
        }

        /// <summary>
        /// Un-Escape all escaped dot characters in the text (identifier only) passed in.
        /// </summary>
        /// <param name="identifierToUnescape">text to un-escape</param>
        /// <returns>string</returns>
        public static string UnescapeDot(string identifierToUnescape)
        {
            var indexof = identifierToUnescape.IndexOf(".");
            if (indexof == -1)
            {
                return identifierToUnescape;
            }

            indexof = identifierToUnescape.IndexOf("\\");
            if (indexof == -1)
            {
                return identifierToUnescape;
            }

            var builder = new StringBuilder();
            var index = -1;
            var max = identifierToUnescape.Length - 1;
            do
            {
                index++;
                var c = identifierToUnescape[index];
                if (c != '\\')
                {
                    builder.Append(c);
                    continue;
                }

                if (index < identifierToUnescape.Length - 1)
                {
                    if (identifierToUnescape[index + 1] == '.')
                    {
                        builder.Append('.');
                        index++;
                    }
                }
            } while (index < max);

            return builder.ToString();
        }

        public static string GetPropertyName(
            EsperEPL2GrammarParser.EventPropertyContext ctx,
            int startNode)
        {
            var buf = new StringBuilder();
            for (var i = startNode; i < ctx.ChildCount; i++)
            {
                var tree = ctx.GetChild(i);
                buf.Append(tree.GetText());
            }

            return buf.ToString();
        }

        public static string UnescapeClassIdent(EsperEPL2GrammarParser.ClassIdentifierContext classIdentCtx)
        {
            var unescapeEscapableResult = UnescapeEscapableStr(classIdentCtx.escapableStr(), ".");
            
#if NOT_FUNCTIONAL
            var genericArgsList = classIdentCtx.classIdentifierGenericArgs();
            if (genericArgsList != null) {
                var genericArgRender = new StringBuilder();
                var genericArgDelimiter = "";
                genericArgRender.Append('<');

                // Render all of the generic argument elements.
                var genericArgElements = genericArgsList.classIdentifierGenericArgsList().classIdentifier();
                foreach (var genericArgElement in genericArgElements) {
                    var genericArgResult = UnescapeClassIdent(genericArgElement);
                    genericArgRender.Append(genericArgDelimiter);
                    genericArgRender.Append(genericArgResult);
                    genericArgDelimiter = ",";
                }
                
                genericArgRender.Append('>');

                unescapeEscapableResult += genericArgRender.ToString();
            }
#endif

            return unescapeEscapableResult;
        }

        public static string UnescapeSlashIdentifier(EsperEPL2GrammarParser.SubstitutionSlashIdentContext ctx)
        {
            var name = UnescapeEscapableStr(ctx.escapableStr(), "/");
            if (ctx.d != null)
            {
                name = "/" + name;
            }

            return name;
        }

        private static string UnescapeEscapableStr(
            IList<EsperEPL2GrammarParser.EscapableStrContext> ctxs,
            string delimiterConst)
        {
            if (ctxs.Count == 1)
            {
                return UnescapeBacktick(UnescapeDot(ctxs[0].GetText()));
            }

            var writer = new StringWriter();
            var delimiter = "";
            foreach (var ctx in ctxs)
            {
                writer.Write(delimiter);
                writer.Write(UnescapeBacktick(UnescapeDot(ctx.GetText())));
                delimiter = delimiterConst;
            }

            return writer.ToString();
        }

        public static string GetStreamNameUnescapedOptional(EsperEPL2GrammarParser.IdentOrTickedContext ctx)
        {
            return ctx != null ? StringValue.UnescapeBacktick(ctx.GetText()) : null;
        }
    }
} // end of namespace