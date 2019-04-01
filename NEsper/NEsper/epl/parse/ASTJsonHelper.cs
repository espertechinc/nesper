///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.generated;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Walker to annotation stuctures.
    /// </summary>
    public class ASTJsonHelper
    {
        /// <summary>
        /// Walk an annotation root name or child node (nested annotations).
        /// </summary>
        /// <param name="tokenStream">The token stream.</param>
        /// <param name="node">annotation walk node</param>
        /// <returns>
        /// annotation descriptor
        /// </returns>
        /// <throws>com.espertech.esper.epl.parse.ASTWalkException if the walk failed</throws>
        public static Object Walk(CommonTokenStream tokenStream, EsperEPL2GrammarParser.JsonvalueContext node)
        {
            if (node.constant() != null)
            {
                EsperEPL2GrammarParser.ConstantContext constCtx = node.constant();
                if (constCtx.stringconstant() != null)
                {
                    return ExtractString(constCtx.stringconstant().GetText());
                }
                else
                {
                    return ASTConstantHelper.Parse(constCtx.GetChild(0));
                }
            }
            else if (node.jsonobject() != null)
            {
                return WalkObject(tokenStream, node.jsonobject());
            }
            else if (node.jsonarray() != null)
            {
                return WalkArray(tokenStream, node.jsonarray());
            }
            throw ASTWalkException.From("Encountered unexpected node type in json tree", tokenStream, node);
        }

        public static IDictionary<String, Object> WalkObject(CommonTokenStream tokenStream, EsperEPL2GrammarParser.JsonobjectContext ctx)
        {
            IDictionary<String, Object> map = new LinkedHashMap<String, Object>();
            IList<EsperEPL2GrammarParser.JsonpairContext> pairs = ctx.jsonmembers().jsonpair();
            foreach (EsperEPL2GrammarParser.JsonpairContext pair in pairs)
            {
                Pair<String, Object> value = WalkJSONField(tokenStream, pair);
                map.Put(value.First, value.Second);
            }
            return map;
        }

        public static IList<Object> WalkArray(CommonTokenStream tokenStream, EsperEPL2GrammarParser.JsonarrayContext ctx)
        {
            IList<Object> list = new List<Object>();
            if (ctx.jsonelements() == null)
            {
                return list;
            }
            IList<EsperEPL2GrammarParser.JsonvalueContext> values = ctx.jsonelements().jsonvalue();
            foreach (EsperEPL2GrammarParser.JsonvalueContext value in values)
            {
                Object val = Walk(tokenStream, value);
                list.Add(val);
            }
            return list;
        }

        private static Pair<String, Object> WalkJSONField(CommonTokenStream tokenStream, EsperEPL2GrammarParser.JsonpairContext ctx)
        {
            String label;
            if (ctx.stringconstant() != null)
            {
                label = ExtractString(ctx.stringconstant().GetText());
            }
            else
            {
                label = ctx.keywordAllowedIdent().GetText();
            }
            Object value = Walk(tokenStream, ctx.jsonvalue());
            return new Pair<String, Object>(label, value);
        }

        private static String ExtractString(String text)
        {
            var sb = new StringBuilder(text);
            int startPoint = 1;
            for (; ; )
            {
                int slashIndex = sb.IndexOf("\\", startPoint);
                if (slashIndex == -1)
                {
                    break;
                }
                char escapeType = sb[slashIndex + 1];
                switch (escapeType)
                {
                    case 'u':
                        String unicode = ExtractUnicode(sb, slashIndex);
                        sb.Remove(slashIndex, 6);
                        sb.Insert(slashIndex, unicode);
                        //sb.Replace(slashIndex, slashIndex + 6, unicode); // backspace
                        break; // back to the loop

                    // note: C#'s character escapes match JSON's, which is why it looks like we're replacing
                    // "\b" with "\b". We're actually replacing 2 characters (slash-b) with one (backspace).
                    case 'b':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\b");
                        //sb.Replace(slashIndex, slashIndex + 2, "\b");
                        break;
                    case 't':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\t");
                        //sb.Replace(slashIndex, slashIndex + 2, "\t");
                        break;
                    case 'n':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\n");
                        //sb.Replace(slashIndex, slashIndex + 2, "\n");
                        break;
                    case 'f':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\f");
                        //sb.Replace(slashIndex, slashIndex + 2, "\f");
                        break;
                    case 'r':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\r");
                        //sb.Replace(slashIndex, slashIndex + 2, "\r");
                        break;
                    case '\'':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\'");
                        //sb.Replace(slashIndex, slashIndex + 2, "\'");
                        break;
                    case '\"':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\"");
                        //sb.Replace(slashIndex, slashIndex + 2, "\"");
                        break;
                    case '\\':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "\\");
                        //sb.Replace(slashIndex, slashIndex + 2, "\\");
                        break;
                    case '/':
                        sb.Remove(slashIndex, 2);
                        sb.Insert(slashIndex, "/");
                        //sb.Replace(slashIndex, slashIndex + 2, "/");
                        break;
                    default:
                        break;
                }
                startPoint = slashIndex + 1;
            }
            sb.Remove(0, 1);
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private static String ExtractUnicode(StringBuilder sb, int slashIndex)
        {
            String code = sb.ToString().Substring(slashIndex + 2, 4);
            int ordinal = int.Parse(code, NumberStyles.HexNumber); // hex to integer
            if (ordinal < 0x10000)
                return ((char)ordinal).ToString();

            return string.Format(
                "{0}{1}",
                (char)((ordinal - 0x10000) >> 10 + 0xD800),
                (char)((ordinal - 0x10000) & 0x3FF + 0xDC00));
        }
    }
}
