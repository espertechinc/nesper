///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Antlr4.Runtime;

using com.espertech.esper.common.client.module;
using com.espertech.esper.compat.collections;
using com.espertech.esper.grammar.@internal.generated;

namespace com.espertech.esper.compiler.@internal.parse
{
    /// <summary>
    /// Walker to annotation stuctures.
    /// </summary>
    public class ASTJsonHelper
    {
        public static object Walk(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.JsonvalueContext node)
        {
            if (node.constant() != null) {
                EsperEPL2GrammarParser.ConstantContext constCtx = node.constant();
                if (constCtx.stringconstant() != null) {
                    return ExtractString(constCtx.stringconstant().GetText());
                }
                else {
                    return ASTConstantHelper.Parse(constCtx.GetChild(0));
                }
            }
            else if (node.jsonobject() != null) {
                return WalkObject(tokenStream, node.jsonobject());
            }
            else if (node.jsonarray() != null) {
                return WalkArray(tokenStream, node.jsonarray());
            }

            throw ASTWalkException.From("Encountered unexpected node type in json tree", tokenStream, node);
        }

        public static IDictionary<string, object> WalkObject(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.JsonobjectContext ctx)
        {
            IDictionary<string, object> map = new LinkedHashMap<string, object>();
            IList<EsperEPL2GrammarParser.JsonpairContext> pairs = ctx.jsonmembers().jsonpair();
            foreach (EsperEPL2GrammarParser.JsonpairContext pair in pairs)
            {
                Pair<string, object> value = WalkJSONField(tokenStream, pair);
                map.Put(value.First, value.Second);
            }

            return map;
        }

        public static IList<object> WalkArray(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.JsonarrayContext ctx)
        {
            IList<object> list = new List<object>();
            if (ctx.jsonelements() == null)
            {
                return list;
            }

            foreach (EsperEPL2GrammarParser.JsonvalueContext value in ctx.jsonelements().jsonvalue())
            {
                list.Add(Walk(tokenStream, value));
            }

            return list;
        }

        private static Pair<string, object> WalkJSONField(
            CommonTokenStream tokenStream,
            EsperEPL2GrammarParser.JsonpairContext ctx)
        {
            string label;
            if (ctx.stringconstant() != null)
            {
                label = ExtractString(ctx.stringconstant().GetText());
            }
            else
            {
                label = ctx.keywordAllowedIdent().GetText();
            }

            object value = Walk(tokenStream, ctx.jsonvalue());
            return new Pair<string, object>(label, value);
        }

        /// <summary>
        /// Extracts a unicode character using the using the 16-bit encoding supported by .NET.  A unicode character
        /// in the range U+10000 to U+10FFFFF is not permitted in a character literal and is represented using a
        /// Unicode surrogate pair in a string literal.
        /// </summary>
        private static string ExtractUnicode32(
            string text,
            int index)
        {
            int codePoint = Int32.Parse(text.Substring(index, 4), NumberStyles.HexNumber);
            return Char.ConvertFromUtf32(codePoint);
        }

        private static string ExtractString(string text)
        {
            var builder = new StringBuilder();
            var index = 1;
            var lastIndex = text.Length - 1;
            while (index < lastIndex)
            {
                var nextCharacter = text[index++];
                if (nextCharacter != '\\')
                {
                    builder.Append(nextCharacter);
                    continue;
                }

                // Escape sequences follow
                var escapeSequence = text[index++];
                switch (escapeSequence)
                {
                    case 'u':
                        builder.Append(ExtractUnicode32(text, index));
                        index += 4;
                        break; // back to the loop

                    case 'b':
                        builder.Append('\b');
                        break;

                    case 't':
                        builder.Append('\t');
                        break;

                    case 'n':
                        builder.Append('\n');
                        break;

                    case 'f':
                        builder.Append('\f');
                        break;

                    case 'r':
                        builder.Append('\r');
                        break;

                    case '\'':
                        builder.Append('\'');
                        break;

                    case '\"':
                        builder.Append('"');
                        break;

                    case '\\':
                        builder.Append('\\');
                        break;

                    case '/':
                        builder.Append('/');
                        break;

                    default:
                        // This represents an escape sequence we are unfamiliar with. We can handle this
                        // by throwing an exception or allowing the content to pass uninterrupted.  For now
                        // we will treat this as an error as the slash indicates an expectation to escape.
                        throw new ParseException("unable to parse escape sequence");
                }
            }

            return builder.ToString();
        }

#if false
        private static string ExtractString (string text) {
            var sb = new StringBuilder (text);
            int startPoint = 1;
            for (;;) {
                int slashIndex = sb.IndexOf ("\\", startPoint);
                if (slashIndex == -1) {
                    break;
                }
                char escapeType = sb[slashIndex + 1];
                switch (escapeType) {
                    case 'u':

                        string unicode = ExtractUnicode (sb, slashIndex);
                        sb.Replace (slashIndex, slashIndex + 6, unicode); // backspace
                        break; // back to the loop

                    case 'b':
                        sb.Replace (slashIndex, slashIndex + 2, "\b");
                        break;

                    case 't':
                        sb.Replace (slashIndex, slashIndex + 2, "\t");
                        break;

                    case 'n':
                        sb.Replace (slashIndex, slashIndex + 2, "\n");
                        break;

                    case 'f':
                        sb.Replace (slashIndex, slashIndex + 2, "\f");
                        break;

                    case 'r':
                        sb.Replace (slashIndex, slashIndex + 2, "\r");
                        break;

                    case '\'':
                        sb.Replace (slashIndex, slashIndex + 2, "\'");
                        break;

                    case '\"':
                        sb.Replace (slashIndex, slashIndex + 2, "\"");
                        break;

                    case '\\':
                        sb.Replace (slashIndex, slashIndex + 2, "\\");
                        break;

                    case '/':
                        sb.Replace (slashIndex, slashIndex + 2, "/");
                        break;

                    default:
                        break;
                }
                startPoint = slashIndex + 1;
            }
            sb.DeleteCharAt (0);
            sb.DeleteCharAt (sb.Length - 1);
            return sb.ToString ();
        }

        private static string ExtractUnicode (StringBuilder sb, int slashIndex) {
            string result;
            string code = sb.Substring (slashIndex + 2, slashIndex + 6);
            int charNum = Int32.Parse (code, 16); // hex to integer
            try {
                ByteArrayOutputStream baos = new ByteArrayOutputStream ();
                OutputStreamWriter osw = new OutputStreamWriter (baos, "UTF-8");
                osw.Write (charNum);
                osw.Flush ();
                result = baos.ToString ("UTF-8");
            } catch (Exception e) {
                throw ASTWalkException.From ("Failed to obtain for unicode '" + charNum + "'", e);
            }
            return result;
        }
#endif
    }
} // end of namespace