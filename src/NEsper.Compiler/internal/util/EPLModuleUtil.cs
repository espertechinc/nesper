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
using System.Reflection;

using Antlr4.Runtime;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.@internal.parse;
using com.espertech.esper.grammar.@internal.generated;
using com.espertech.esper.grammar.@internal.util;

using Module = com.espertech.esper.common.client.module.Module;

namespace com.espertech.esper.compiler.@internal.util
{
    public class EPLModuleUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Newline character.
        /// </summary>
        public static readonly string NEWLINE = Environment.NewLine;

        public static Module ReadInternal(
            Stream stream,
            string resourceName,
            bool closeStreamOnCompletion)
        {
            var reader = new StreamReader(stream);
            var buffer = new StringWriter();
            string strLine;
            while ((strLine = reader.ReadLine()) != null) {
                buffer.Write(strLine);
                buffer.Write(NEWLINE);
            }

            if (closeStreamOnCompletion) {
                stream.Close();
            }

            return ParseInternal(buffer.ToString(), resourceName);
        }

        public static Module ParseInternal(
            string buffer,
            string resourceName)
        {
            var semicolonSegments = Parse(buffer);
            IList<ParseNode> nodes = new List<ParseNode>();
            foreach (var segment in semicolonSegments) {
                nodes.Add(GetModule(segment, resourceName));
            }

            string moduleName = null;
            var count = 0;
            foreach (var node in nodes) {
                if (node is ParseNodeComment) {
                    continue;
                }

                if (node is ParseNodeModule parseNodeModule) {
                    if (moduleName != null) {
                        throw new ParseException(
                            "Duplicate use of the 'module' keyword for resource '" + resourceName + "'");
                    }

                    if (count > 0) {
                        throw new ParseException(
                            "The 'module' keyword must be the first declaration in the module file for resource '" +
                            resourceName +
                            "'");
                    }

                    moduleName = parseNodeModule.ModuleName;
                }

                count++;
            }

            ISet<string> uses = new LinkedHashSet<string>();
            ISet<Import> imports = new HashSet<Import>();
            count = 0;
            foreach (var node in nodes) {
                if (node is ParseNodeComment || node is ParseNodeModule) {
                    continue;
                }

                var message =
                    "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration";
                if (node is ParseNodeUses parseNodeUses) {
                    if (count > 0) {
                        throw new ParseException(message);
                    }

                    uses.Add(parseNodeUses.Uses);
                    continue;
                }

                if (node is ParseNodeImport parseNodeImport) {
                    if (count > 0) {
                        throw new ParseException(message);
                    }

                    if (IsNamespaceWildcard(parseNodeImport.Imported)) {
                        imports.Add(
                            new ImportNamespace(
                                GetNamespace(parseNodeImport.Imported)));
                    }
                    else {
                        imports.Add(
                            new ImportType(
                                parseNodeImport.Imported.UnmaskTypeName()));
                    }

                    continue;
                }

                count++;
            }

            IList<ModuleItem> items = new List<ModuleItem>();
            foreach (var node in nodes) {
                if (node is ParseNodeComment || node is ParseNodeExpression) {
                    var isComments = node is ParseNodeComment;
                    items.Add(
                        new ModuleItem(
                            node.Item.Expression,
                            isComments,
                            node.Item.LineNum,
                            node.Item.StartChar,
                            node.Item.EndChar));
                }
            }

            return new Module(moduleName, resourceName, uses, imports, items, buffer);
        }

        public static ParseNode GetModule(
            EPLModuleParseItem item,
            string resourceName)
        {
            ICharStream input = new CaseInsensitiveInputStream(item.Expression);
            var lex = ParseHelper.NewLexer(input);
            var tokenStream = new CommonTokenStream(lex);
            tokenStream.Fill();

            var tokens = tokenStream.GetTokens();
            var beginIndex = 0;
            var isMeta = false;
            var isModule = false;
            var isUses = false;
            var isExpression = false;

            while (beginIndex < tokens.Count) {
                var t = tokens[beginIndex];
                if (t.Type == EsperEPL2GrammarParser.Eof) {
                    break;
                }

                if (t.Type == EsperEPL2GrammarParser.WS ||
                    t.Type == EsperEPL2GrammarParser.SL_COMMENT ||
                    t.Type == EsperEPL2GrammarParser.ML_COMMENT) {
                    beginIndex++;
                    continue;
                }

                var tokenText = t.Text.Trim().ToLowerInvariant();
                if (tokenText.Equals("module")) {
                    isModule = true;
                    isMeta = true;
                }
                else if (tokenText.Equals("uses")) {
                    isUses = true;
                    isMeta = true;
                }
                else if (tokenText.Equals("import")) {
                    isMeta = true;
                }
                else {
                    isExpression = true;
                    break;
                }

                beginIndex++;
                beginIndex++; // skip space
                break;
            }

            if (isExpression) {
                return new ParseNodeExpression(item);
            }

            if (!isMeta) {
                return new ParseNodeComment(item);
            }

            // check meta tag (module, uses, import)
            var buffer = new StringWriter();
            for (var i = beginIndex; i < tokens.Count; i++) {
                var t = tokens[i];
                if (t.Type == EsperEPL2GrammarParser.Eof) {
                    break;
                }

                if (t.Type != EsperEPL2GrammarParser.IDENT &&
                    t.Type != EsperEPL2GrammarParser.DOT &&
                    t.Type != EsperEPL2GrammarParser.STAR &&
                    !t.Text.Matches("[a-zA-Z]*")) {
                    throw GetMessage(isModule, isUses, resourceName, t.Type);
                }

                buffer.Write(t.Text.Trim());
            }

            var result = buffer.ToString().Trim();
            if (result.Length == 0) {
                throw GetMessage(isModule, isUses, resourceName, -1);
            }

            if (isModule) {
                return new ParseNodeModule(item, result);
            }

            if (isUses) {
                return new ParseNodeUses(item, result);
            }

            return new ParseNodeImport(item, result);
        }

        private static ParseException GetMessage(
            bool module,
            bool uses,
            string resourceName,
            int type)
        {
            var message = "Keyword '";
            if (module) {
                message += "module";
            }
            else if (uses) {
                message += "uses";
            }
            else {
                message += "import";
            }

            message += "' must be followed by a name or package name (set of names separated by dots) for resource '" +
                       resourceName +
                       "'";

            if (type != -1) {
                string tokenName = EsperEPL2GrammarParser.GetLexerTokenParaphrases().Get(type);
                if (tokenName == null) {
                    tokenName = EsperEPL2GrammarParser.GetParserTokenParaphrases().Get(type);
                }

                if (tokenName != null) {
                    message += ", unexpected reserved keyword " + tokenName + " was encountered as part of the name";
                }
            }

            return new ParseException(message);
        }

        public static IList<EPLModuleParseItem> Parse(string module)
        {
            var input = new CaseInsensitiveInputStream(module);
            var lex = ParseHelper.NewLexer(input);
            var tokens = new CommonTokenStream(lex);
            try {
                tokens.Fill();
            }
            catch (Exception ex) {
                var message = "Unexpected exception recognizing module text";
                if (ex is LexerNoViableAltException) {
                    if (ParseHelper.HasControlCharacters(module)) {
                        message = "Unrecognized control characters found in text, failed to parse text";
                    }
                    else {
                        message += ", recognition failed for " + ex;
                    }
                }
                else if (ex is RecognitionException) {
                    var recog = (RecognitionException) ex;
                    message += ", recognition failed for " + recog;
                }
                else if (ex.Message != null) {
                    message += ": " + ex.Message;
                }

                message += " [" + module + "]";
                Log.Error(message, ex);
                throw new ParseException(message);
            }

            IList<EPLModuleParseItem> statements = new List<EPLModuleParseItem>();
            var current = new StringWriter();
            int? lineNum = null;
            var charPosStart = 0;
            var charPos = 0;
            var tokenList = tokens.GetTokens();
            var skippedSemicolonIndexes = GetSkippedSemicolons(tokenList);
            var index = -1;
            // Call getTokens first before invoking tokens.size! ANTLR problem
            foreach (var token in tokenList) {
                index++;
                var t = token;
                var semi = t.Type == EsperEPL2GrammarLexer.SEMI && !skippedSemicolonIndexes.Contains(index);
                if (semi) {
                    if (current.ToString().Trim().Length > 0) {
                        statements.Add(
                            new EPLModuleParseItem(current.ToString().Trim(), lineNum ?? 0, charPosStart, charPos));
                        lineNum = null;
                    }

                    current = new StringWriter();
                }
                else {
                    if (lineNum == null && t.Type != EsperEPL2GrammarParser.WS) {
                        lineNum = t.Line;
                        charPosStart = charPos;
                    }

                    if (t.Type != EsperEPL2GrammarLexer.Eof) {
                        current.Write(t.Text);
                        charPos += t.Text.Length;
                    }
                }
            }

            if (current.ToString().Trim().Length > 0) {
                statements.Add(new EPLModuleParseItem(current.ToString().Trim(), lineNum ?? 0, 0, 0));
            }

            return statements;
        }

        public static Module ReadFile(FileInfo file)
        {
            using (var inputStream = file.OpenRead()) {
                return EPLModuleUtil.ReadInternal(inputStream, file.FullName, false);
            }
        }

        public static Module ReadResource(
            string resource,
            IResourceManager resourceManager)
        {
            var stripped = resource.StartsWith("/") ? resource.Substring(1) : resource;

            var stream = resourceManager.GetResourceAsStream(stripped);
            if (stream == null) {
                throw new IOException("Failed to find resource '" + resource + "' in classpath");
            }

            using (stream) {
                return ReadInternal(stream, resource, false);
            }
        }

        /// <summary>
        ///     Find expression declarations and skip semicolon content between square brackets for scripts
        /// </summary>
        private static ISet<int> GetSkippedSemicolons(IList<IToken> tokens)
        {
            ISet<int> result = null;

            int index = 0;
            while (index < tokens.Count) {
                var t = tokens[index];
                if (t.Type == EsperEPL2GrammarParser.EXPRESSIONDECL) {
                    if (result == null) {
                        result = new HashSet<int>();
                    }

                    index = GetSkippedSemicolonsBetweenSquareBrackets(index, tokens, result);
                }

                if (t.Type == EsperEPL2GrammarParser.CLASSDECL) {
                    if (result == null) {
                        result = new HashSet<int>();
                    }

                    index = GetSkippedSemicolonsBetweenTripleQuotes(index, tokens, result);
                }

                index++;
            }

            return result ?? new EmptySet<int>();
        }

        /// <summary>
        ///     Find content between square brackets
        /// </summary>
        private static int GetSkippedSemicolonsBetweenSquareBrackets(
            int index,
            IList<IToken> tokens,
            ICollection<int> result)
        {
            // Handle EPL expression "{text}" and script expression "[text]"
            var indexFirstCurly = IndexFirstToken(index, tokens, EsperEPL2GrammarParser.LCURLY);
            var indexFirstSquare = IndexFirstToken(index, tokens, EsperEPL2GrammarParser.LBRACK);
            if (indexFirstSquare == -1) {
                return index;
            }

            if (indexFirstCurly != -1 && indexFirstCurly < indexFirstSquare) {
                return index;
            }

            var indexCloseSquare = FindEndSquareBrackets(indexFirstSquare, tokens);
            if (indexCloseSquare == -1) {
                return index;
            }

            if (indexFirstSquare == indexCloseSquare - 1) {
                GetSkippedSemicolonsBetweenSquareBrackets(indexCloseSquare, tokens, result);
            }
            else {
                GetSkippedSemicolonsBetweenIndexes(indexFirstSquare, indexCloseSquare, tokens, result);
            }
            return indexCloseSquare;
        }
        
        private static int FindEndSquareBrackets(
            int startIndex,
            IList<IToken> tokens)
        {
            var index = startIndex + 1;
            var squareBracketDepth = 0;
            while (index < tokens.Count) {
                var t = tokens[index];
                if (t.Type == EsperEPL2GrammarParser.RBRACK) {
                    if (squareBracketDepth == 0) {
                        return index;
                    }

                    squareBracketDepth--;
                }

                if (t.Type == EsperEPL2GrammarParser.LBRACK) {
                    squareBracketDepth++;
                }

                index++;
            }

            return -1;
        }

        private static int IndexFirstToken(
            int startIndex,
            IList<IToken> tokens,
            int tokenType)
        {
            var index = startIndex;
            while (index < tokens.Count) {
                var t = tokens[index];
                if (t.Type == tokenType) {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public static string GetNamespace(string importName)
        {
            return importName.Substring(0, importName.Length - 2);
        }

        public static bool IsNamespaceWildcard(string importName)
        {
            var classNameRegEx = "(\\w+\\.)+\\*";
            return importName.Matches(classNameRegEx);
        }

        /// <summary>
        /// Find content between triple quotes
        /// </summary>
        private static int GetSkippedSemicolonsBetweenTripleQuotes(
            int index,
            IList<IToken> tokens,
            ICollection<int> result)
        {
            // Handle class """{text}"""
            int indexFirstTriple = IndexFirstToken(index, tokens, EsperEPL2GrammarParser.TRIPLEQUOTE);
            if (indexFirstTriple == -1) {
                return index;
            }

            int indexCloseTriple = IndexFirstToken(indexFirstTriple + 1, tokens, EsperEPL2GrammarParser.TRIPLEQUOTE);
            if (indexCloseTriple == -1) {
                return index;
            }

            GetSkippedSemicolonsBetweenIndexes(indexFirstTriple, indexCloseTriple, tokens, result);
            return indexCloseTriple;
        }

        private static void GetSkippedSemicolonsBetweenIndexes(
            int indexOpen,
            int indexClose,
            IList<IToken> tokens,
            ICollection<int> result)
        {
            int current = indexOpen;
            while (current < indexClose) {
                var t = tokens[current];
                if (t.Type == EsperEPL2GrammarParser.SEMI) {
                    result.Add(current);
                }

                current++;
            }
        }
    }
} // end of namespace