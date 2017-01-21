///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime.Tree;

using com.espertech.esper.epl.generated;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.parse
{
    /// <summary>
    /// Parses constant strings and returns the constant Object.
    /// </summary>
    public class ASTConstantHelper
    {
        /// <summary>
        /// Remove tick '`' character from a string start and end.
        /// </summary>
        /// <param name="tickedString">delimited string</param>
        /// <returns>delimited string with ticks removed, if starting and ending with tick</returns>
        public static String RemoveTicks(String tickedString)
        {
            var indexFirst = tickedString.IndexOf('`');
            var indexLast = tickedString.LastIndexOf('`');
            if ((indexFirst != indexLast) && (indexFirst != -1) && (indexLast != -1))
            {
                return tickedString.Substring(indexFirst+1, indexLast - indexFirst - 1);
            }
            return tickedString;
        }
    
        /// <summary>
        /// Parse the AST constant node and return Object value.
        /// </summary>
        /// <param name="node">parse node for which to parse the string value</param>
        /// <returns>value matching AST node type</returns>
        public static Object Parse(IParseTree node)
        {
            if (node is ITerminalNode) {
                var terminal = (ITerminalNode) node;
                switch(terminal.Symbol.Type)
                {
                    case EsperEPL2GrammarParser.BOOLEAN_TRUE:  return BoolValue.ParseString(terminal.GetText());
                    case EsperEPL2GrammarParser.BOOLEAN_FALSE: return BoolValue.ParseString(terminal.GetText());
                    case EsperEPL2GrammarParser.VALUE_NULL:    return null;
                    default:
                        throw ASTWalkException.From("Encountered unexpected constant type " + terminal.Symbol.Type, terminal.Symbol);
                }
            }
            else {
                var ruleNode = (IRuleNode)node;
                var ruleIndex = ruleNode.RuleContext.RuleIndex;
                if (ruleIndex == EsperEPL2GrammarParser.RULE_number) {
                    return ParseNumber(ruleNode, 1);
                }
                else if (ruleIndex == EsperEPL2GrammarParser.RULE_numberconstant) {
                    var number = FindChildRuleByType(ruleNode, EsperEPL2GrammarParser.RULE_number);
                    if (ruleNode.ChildCount > 1) {
                        if (ASTUtil.IsTerminatedOfType(ruleNode.GetChild(0), EsperEPL2GrammarLexer.MINUS)) {
                            return ParseNumber(number, -1);
                        }
                        return ParseNumber(number, 1);
                    }
                    else {
                        return ParseNumber(number, 1);
                    }
                }
                else if (ruleIndex == EsperEPL2GrammarParser.RULE_stringconstant) {
                    return StringValue.ParseString(node.GetText());
                }
                else if (ruleIndex == EsperEPL2GrammarParser.RULE_constant) {
                    return Parse(ruleNode.GetChild(0));
                }
                throw ASTWalkException.From("Encountered unrecognized constant", node.GetText());
            }
        }
    
        private static Object ParseNumber(IRuleNode number, int factor) {
            var tokenType = GetSingleChildTokenType(number);
            if (tokenType == EsperEPL2GrammarLexer.IntegerLiteral) {
                return ParseIntLongByte(number.GetText(), factor);
            }
    
            else if (tokenType == EsperEPL2GrammarLexer.FloatingPointLiteral) {
                var numberText = number.GetText();
                if (numberText.EndsWith("f") || numberText.EndsWith("F")) {
                    return FloatValue.ParseString(number.GetText()) * factor;
                }
                else {
                    return DoubleValue.ParseString(number.GetText()) * factor;
                }
            }
            throw ASTWalkException.From("Encountered unrecognized constant", number.GetText());
        }
    
        private static Object ParseIntLongByte(String arg, int factor)
        {
            // try to parse as an int first, else try to parse as a long
            try
            {
                return IntValue.ParseString(arg)*factor;
            }
            catch (OverflowException e1)
            {
                try
                {
                    return (Int64) (LongValue.ParseString(arg)*factor);
                }
                catch
                {
                }

                throw;
            }
            catch (FormatException e1)
            {
                try
                {
                    return (Int64) (LongValue.ParseString(arg)*factor);
                }
                catch (Exception)
                {
                    try
                    {
                        return (Byte) (ByteValue.ParseString(arg)*factor);
                    }
                    catch (Exception)
                    {
                        throw e1;
                    }
                }
            }
        }

        private static IRuleNode FindChildRuleByType(ITree node, int ruleNum)
        {
            for (var i = 0; i < node.ChildCount; i++) {
                var child = node.GetChild(i);
                if (IsRuleOfType(child, ruleNum)) {
                    return (IRuleNode) child;
                }
            }
            return null;
        }
    
        private static bool IsRuleOfType(ITree child, int ruleNum) {
            if (!(child is IRuleNode))
            {
                return false;
            }
            var ruleNode = (IRuleNode)child;
            return ruleNode.RuleContext.RuleIndex == ruleNum;
        }

        private static int GetSingleChildTokenType(IRuleNode node)
        {
            return ((ITerminalNode) node.GetChild(0)).Symbol.Type;
        }
    }
}
