///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.util;

namespace com.espertech.esper.common.@internal.epl.join.hint
{
    public class IndexHint
    {
        private readonly IList<SelectorInstructionPair> pairs;

        public IndexHint(IList<SelectorInstructionPair> pairs)
        {
            this.pairs = pairs;
        }

        public static IndexHint GetIndexHint(Attribute[] annotations)
        {
            if (annotations == null) {
                return null;
            }

            var hints = HintEnum.INDEX.GetHintAssignedValues(annotations);
            if (hints == null) {
                return null;
            }

            IList<SelectorInstructionPair> pairs = new List<SelectorInstructionPair>();
            foreach (var hint in hints) {
                var hintAtoms = HintEnumExtensions.SplitCommaUnlessInParen(hint);
                IList<IndexHintSelector> selectors = new List<IndexHintSelector>();
                IList<IndexHintInstruction> instructions = new List<IndexHintInstruction>();
                for (var i = 0; i < hintAtoms.Length; i++) {
                    var hintAtom = hintAtoms[i];
                    if (hintAtom.ToLowerInvariant().Trim().Equals("bust")) {
                        instructions.Add(new IndexHintInstructionBust());
                    }
                    else if (hintAtom.ToLowerInvariant().Trim().Equals("explicit")) {
                        instructions.Add(new IndexHintInstructionExplicit());
                    }
                    else if (CheckValueInParen("subquery", hintAtom.ToLowerInvariant())) {
                        var subqueryNum = ExtractValueParen(hintAtom);
                        selectors.Add(new IndexHintSelectorSubquery(subqueryNum));
                    }
                    else {
                        instructions.Add(new IndexHintInstructionIndexName(hintAtom.Trim()));
                    }
                }

                pairs.Add(new SelectorInstructionPair(selectors, instructions));
            }

            return new IndexHint(pairs);
        }

        public IList<IndexHintInstruction> GetInstructionsSubquery(int subqueryNumber)
        {
            foreach (var pair in pairs) {
                if (pair.Selector.IsEmpty()) { // empty selector mean hint applies to all
                    return pair.Instructions;
                }

                foreach (var selector in pair.Selector) {
                    if (selector.MatchesSubquery(subqueryNumber)) {
                        return pair.Instructions;
                    }
                }
            }

            return Collections.GetEmptyList<IndexHintInstruction>();
        }

        public IList<IndexHintInstruction> GetInstructionsFireAndForget()
        {
            foreach (var pair in pairs) {
                if (pair.Selector.IsEmpty()) { // empty selector mean hint applies to all
                    return pair.Instructions;
                }
            }

            return Collections.GetEmptyList<IndexHintInstruction>();
        }

        protected internal static bool CheckValueInParen(
            string type,
            string value)
        {
            var indexOpen = value.IndexOf('(');
            if (indexOpen != -1) {
                var noparen = value.Substring(0, indexOpen).Trim().ToLowerInvariant();
                if (type.Equals(noparen)) {
                    return true;
                }
            }

            return false;
        }

        protected internal static bool CheckValueAssignment(
            string type,
            string value)
        {
            var indexEquals = value.IndexOf('=');
            if (indexEquals != -1) {
                var noequals = value.Substring(0, indexEquals).Trim().ToLowerInvariant();
                if (type.Equals(noequals)) {
                    return true;
                }
            }

            return false;
        }

        protected internal static int ExtractValueParen(string text)
        {
            var indexOpen = text.IndexOf('(');
            var indexClosed = text.LastIndexOf(')');
            if (indexOpen != -1) {
                var value = text.Substring(indexOpen + 1, indexClosed - indexOpen).Trim();
                try {
                    return int.Parse(value);
                }
                catch (Exception) {
                    throw new EPException("Failed to parse '" + value + "' as an index hint integer value");
                }
            }

            throw new IllegalStateException("Not a parentheses value");
        }

        protected internal static object ExtractValueEqualsStringOrInt(string text)
        {
            var value = ExtractValueEquals(text);
            try {
                return SimpleTypeParserFunctions.ParseInt32(value);
            }
            catch (Exception) {
                return value;
            }
        }

        protected internal static string ExtractValueEquals(string text)
        {
            var indexEquals = text.IndexOf('=');
            if (indexEquals != -1) {
                return text.Substring(indexEquals + 1).Trim();
            }

            throw new IllegalStateException("Not a parentheses value");
        }
    }
} // end of namespace