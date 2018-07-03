///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.client.deploy;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.generated;
using com.espertech.esper.epl.parse;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.deploy
{
    using Module = client.deploy.Module;

    public class EPLModuleUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Newline character. </summary>
        public static String Newline = Environment.NewLine;

        public static Module ReadInternal(Stream stream, String resourceName)
        {
            var reader = new StreamReader(stream);
            var writer = new StringWriter();

            string strLine;
            while ((strLine = reader.ReadLine()) != null)
            {
                writer.WriteLine(strLine);
            }
            stream.Close();

            return ParseInternal(writer.ToString(), resourceName);
        }

        public static Module ParseInternal(String buffer, String resourceName)
        {
            var semicolonSegments = Parse(buffer);
            var nodes = new List<ParseNode>();
            foreach (EPLModuleParseItem segment in semicolonSegments)
            {
                nodes.Add(GetModule(segment, resourceName));
            }

            String moduleName = null;
            int count = 0;
            foreach (ParseNode node in nodes)
            {
                if (node is ParseNodeComment)
                {
                    continue;
                }
                if (node is ParseNodeModule)
                {
                    if (moduleName != null)
                    {
                        throw new ParseException(
                            "Duplicate use of the 'module' keyword for resource '" + resourceName + "'");
                    }
                    if (count > 0)
                    {
                        throw new ParseException(
                            "The 'module' keyword must be the first declaration in the module file for resource '" +
                            resourceName + "'");
                    }
                    moduleName = ((ParseNodeModule)node).ModuleName;
                }
                count++;
            }

            ICollection<String> uses = new LinkedHashSet<String>();
            ICollection<String> imports = new LinkedHashSet<String>();
            count = 0;
            foreach (ParseNode node in nodes)
            {
                if ((node is ParseNodeComment) || (node is ParseNodeModule))
                {
                    continue;
                }
                const string message = "The 'uses' and 'import' keywords must be the first declaration in the module file or follow the 'module' declaration";
                if (node is ParseNodeUses)
                {
                    if (count > 0)
                    {
                        throw new ParseException(message);
                    }
                    uses.Add(((ParseNodeUses)node).Uses);
                    continue;
                }
                if (node is ParseNodeImport)
                {
                    if (count > 0)
                    {
                        throw new ParseException(message);
                    }
                    imports.Add(((ParseNodeImport)node).Imported);
                    continue;
                }
                count++;
            }

            var items = new List<ModuleItem>();
            foreach (ParseNode node in nodes)
            {
                if ((node is ParseNodeComment) || (node is ParseNodeExpression))
                {
                    bool isComments = (node is ParseNodeComment);
                    items.Add(new ModuleItem(node.Item.Expression, isComments, node.Item.LineNum, node.Item.StartChar, node.Item.EndChar));
                }
            }

            return new Module(moduleName, resourceName, uses, imports, items, buffer);
        }

        public static IList<EventType> UndeployTypes(ICollection<String> referencedTypes,
                                                     StatementEventTypeRef statementEventTypeRef,
                                                     EventAdapterService eventAdapterService,
                                                     FilterService filterService)
        {
            var undeployedTypes = new List<EventType>();
            foreach (String typeName in referencedTypes)
            {
                bool typeInUse = statementEventTypeRef.IsInUse(typeName);
                if (typeInUse)
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("Event type '" + typeName + "' is in use, not removing type");
                    }
                    continue;
                }

                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Event type '" + typeName + "' is no longer in use, removing type");
                }
                var type = eventAdapterService.GetEventTypeByName(typeName);
                if (type != null)
                {
                    var spi = (EventTypeSPI)type;
                    if (!spi.Metadata.IsApplicationPreConfigured)
                    {
                        eventAdapterService.RemoveType(typeName);
                        undeployedTypes.Add(spi);
                        filterService.RemoveType(spi);
                    }
                }
            }
            return undeployedTypes;
        }

        public static ParseNode GetModule(EPLModuleParseItem item, String resourceName)
        {
            var input = new NoCaseSensitiveStream(item.Expression);

            var lex = ParseHelper.NewLexer(input);
            var tokenStream = new CommonTokenStream(lex);
            tokenStream.Fill();

            var tokens = tokenStream.GetTokens();
            var beginIndex = 0;
            var isMeta = false;
            var isModule = false;
            var isUses = false;
            var isExpression = false;

            while (beginIndex < tokens.Count)
            {
                var t = tokens[beginIndex];
                if (t.Type == EsperEPL2GrammarParser.Eof)
                {
                    break;
                }

                if ((t.Type == EsperEPL2GrammarParser.WS) ||
                    (t.Type == EsperEPL2GrammarParser.SL_COMMENT) ||
                    (t.Type == EsperEPL2GrammarParser.ML_COMMENT))
                {
                    beginIndex++;
                    continue;
                }
                var tokenText = t.Text.Trim().ToLower();
                switch (tokenText)
                {
                    case "module":
                        isModule = true;
                        isMeta = true;
                        break;
                    case "uses":
                        isUses = true;
                        isMeta = true;
                        break;
                    case "import":
                        isMeta = true;
                        break;
                    default:
                        isExpression = true;
                        break;
                }
                beginIndex++;
                beginIndex++; // skip space
                break;
            }

            if (isExpression)
            {
                return new ParseNodeExpression(item);
            }
            if (!isMeta)
            {
                return new ParseNodeComment(item);
            }

            // check meta tag (module, uses, import)
            var buffer = new StringWriter();
            for (int i = beginIndex; i < tokens.Count; i++)
            {
                var t = tokens[i];
                if (t.Type == EsperEPL2GrammarParser.Eof)
                {
                    break;
                }

                if ((t.Type != EsperEPL2GrammarParser.IDENT) &&
                    (t.Type != EsperEPL2GrammarParser.DOT) &&
                    (t.Type != EsperEPL2GrammarParser.STAR) &&
                    (!t.Text.Matches("[a-zA-Z]*")))
                {
                    throw GetMessage(isModule, isUses, resourceName, t.Type);
                }
                buffer.Write(t.Text.Trim());
            }

            String result = buffer.ToString().Trim();
            if (result.Length == 0)
            {
                throw GetMessage(isModule, isUses, resourceName, -1);
            }

            if (isModule)
            {
                return new ParseNodeModule(item, result);
            }
            else if (isUses)
            {
                return new ParseNodeUses(item, result);
            }
            return new ParseNodeImport(item, result);
        }

        private static ParseException GetMessage(bool module, bool uses, String resourceName, int type)
        {
            String message = "Keyword '";
            if (module)
            {
                message += "module";
            }
            else if (uses)
            {
                message += "uses";
            }
            else
            {
                message += "import";
            }
            message += "' must be followed by a name or package name (set of names separated by dots) for resource '" +
                       resourceName + "'";

            if (type != -1)
            {
                String tokenName = EsperEPL2GrammarParser.GetLexerTokenParaphrases().Get(type);
                if (tokenName == null)
                {
                    tokenName = EsperEPL2GrammarParser.GetParserTokenParaphrases().Get(type);
                }
                if (tokenName != null)
                {
                    message += ", unexpected reserved keyword " + tokenName + " was encountered as part of the name";
                }
            }
            return new ParseException(message);
        }

        public static IList<EPLModuleParseItem> Parse(String module)
        {
            ICharStream input;
            try
            {
                input = new NoCaseSensitiveStream(module);
            }
            catch (IOException ex)
            {
                Log.Error("Exception reading module expression: " + ex.Message, ex);
                return null;
            }

            var lex = ParseHelper.NewLexer(input);
            var tokens = new CommonTokenStream(lex);

            try
            {
                tokens.Fill();
            }
            catch (Exception ex)
            {
                String message = "Unexpected exception recognizing module text";
                if (ex is LexerNoViableAltException)
                {
                    if (ParseHelper.HasControlCharacters(module))
                    {
                        message = "Unrecognized control characters found in text, failed to parse text";
                    }
                    else
                    {
                        message += ", recognition failed for " + ex;
                    }
                }
                else if (ex is RecognitionException)
                {
                    var recog = (RecognitionException)ex;
                    message += ", recognition failed for " + recog;
                }
                else if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    message += ": " + ex.Message;
                }
                message += " [" + module + "]";
                Log.Error(message, ex);
                throw new ParseException(message);
            }

            var statements = new List<EPLModuleParseItem>();
            var current = new StringWriter();
            int? lineNum = null;
            int charPosStart = 0;
            int charPos = 0;
            var tokenList = tokens.GetTokens();
            var skippedSemicolonIndexes = GetSkippedSemicolons(tokenList);
            int index = -1;

            foreach (var token in tokenList.TakeWhile(t => t.Type != EsperEPL2GrammarParser.Eof))
            {
                index++;
                var t = token;
                bool semi = t.Type == EsperEPL2GrammarParser.SEMI && !skippedSemicolonIndexes.Contains(index);
                if (semi)
                {
                    if (current.ToString().Trim().Length > 0)
                    {
                        statements.Add(
                            new EPLModuleParseItem(
                                current.ToString().Trim(), lineNum ?? 0, charPosStart, charPos));
                        lineNum = null;
                    }
                    current = new StringWriter();
                }
                else
                {
                    if ((lineNum == null) && (t.Type != EsperEPL2GrammarParser.WS))
                    {
                        lineNum = t.Line;
                        charPosStart = charPos;
                    }
                    if (t.Type != EsperEPL2GrammarLexer.Eof)
                    {
                        current.Write(t.Text);
                        charPos += t.Text.Length;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(current.ToString()))
            {
                statements.Add(new EPLModuleParseItem(current.ToString().Trim(), lineNum ?? 0, 0, 0));
            }
            return statements;
        }

        public static Module ReadFile(FileInfo file)
        {
            using (var stream = File.OpenRead(file.ToString()))
            {
                return ReadInternal(stream, Path.GetFullPath(file.ToString()));
            }
        }

        public static Module ReadResource(
            String resource, 
            EngineImportService engineImportService, 
            IResourceManager resourceManager)
        {
            Stream stream = null;

            var stripped = resource.StartsWith("/") ? resource.Substring(1) : resource;
            var classLoader = engineImportService.GetClassLoader();
            if (classLoader != null)
            {
                stream = classLoader.GetResourceAsStream(stripped);
            }
            if (stream == null)
            {
                stream = resourceManager.GetResourceAsStream(stripped);
            }
            if (stream == null)
            {
                throw new IOException("Failed to find resource '" + resource + "' in classpath");
            }

            try
            {
                return ReadInternal(stream, resource);
            }
            finally
            {
                try
                {
                    stream.Close();
                }
                catch (IOException e)
                {
                    Log.Debug("Error closing input stream", e);
                }
            }
        }

        /// <summary>Find expression declarations and skip semicolon content between square brackets for scripts </summary>
        private static ICollection<int?> GetSkippedSemicolons(IList<IToken> tokens)
        {
            ICollection<int?> result = null;

            int index = -1;
            foreach (var token in tokens)
            {
                index++;
                var t = (IToken)token;
                if (t.Type == EsperEPL2GrammarParser.EXPRESSIONDECL)
                {
                    if (result == null)
                    {
                        result = new HashSet<int?>();
                    }
                    GetSkippedSemicolonsBetweenSquareBrackets(index, tokens, result);
                }
            }

            return result ?? Collections.GetEmptySet<int?>();
        }

        /// <summary>
        /// Find content between square brackets
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="tokens">The tokens.</param>
        /// <param name="result">The result.</param>
        private static void GetSkippedSemicolonsBetweenSquareBrackets(int index, IList<IToken> tokens, ICollection<int?> result)
        {
            // Handle EPL expression "{text}" and script expression "[text]"
            int indexFirstCurly = IndexFirstToken(index, tokens, EsperEPL2GrammarParser.LCURLY);
            int indexFirstSquare = IndexFirstToken(index, tokens, EsperEPL2GrammarParser.LBRACK);
            if (indexFirstSquare == -1)
            {
                return;
            }
            if (indexFirstCurly != -1 && indexFirstCurly < indexFirstSquare)
            {
                return;
            }
            int indexCloseSquare = FindEndSquareBrackets(indexFirstSquare, tokens);
            if (indexCloseSquare == -1)
            {
                return;
            }

            int current = indexFirstSquare;
            while (current < indexCloseSquare)
            {
                IToken t = tokens[current];
                if (t.Type == EsperEPL2GrammarParser.SEMI)
                {
                    result.Add(current);
                }
                current++;
            }
        }

        private static int FindEndSquareBrackets(int startIndex, IList<IToken> tokens)
        {
            int index = startIndex + 1;
            int squareBracketDepth = 0;
            while (index < tokens.Count)
            {
                IToken t = tokens[index];
                if (t.Type == EsperEPL2GrammarParser.RBRACK)
                {
                    if (squareBracketDepth == 0)
                    {
                        return index;
                    }
                    squareBracketDepth--;
                }
                if (t.Type == EsperEPL2GrammarParser.LBRACK)
                {
                    squareBracketDepth++;
                }
                index++;
            }
            return -1;
        }

        private static int IndexFirstToken(int startIndex, IList<IToken> tokens, int tokenType)
        {
            int index = startIndex;
            while (index < tokens.Count)
            {
                IToken t = tokens[index];
                if (t.Type == tokenType)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }
    }
}