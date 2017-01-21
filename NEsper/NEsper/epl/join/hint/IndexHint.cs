///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.join.hint
{
    public class IndexHint
    {
        private readonly List<SelectorInstructionPair> _pairs;

        public IndexHint(List<SelectorInstructionPair> pairs)
        {
            _pairs = pairs;
        }

        public static IndexHint GetIndexHint(Attribute[] annotations)
        {
            IList<string> hints = HintEnum.INDEX.GetHintAssignedValues(annotations);
            if (hints == null)
            {
                return null;
            }

            var pairs = new List<SelectorInstructionPair>();
            foreach (String hint in hints)
            {
                String[] hintAtoms = HintEnumExtensions.SplitCommaUnlessInParen(hint);
                var selectors = new List<IndexHintSelector>();
                var instructions = new List<IndexHintInstruction>();
                for (int i = 0; i < hintAtoms.Length; i++)
                {
                    String hintAtom = hintAtoms[i];
                    if (hintAtom.ToLower().Trim() == "bust")
                    {
                        instructions.Add(new IndexHintInstructionBust());
                    }
                    else if (hintAtom.ToLower().Trim() == "explicit")
                    {
                        instructions.Add(new IndexHintInstructionExplicit());
                    }
                    else if (CheckValueInParen("subquery", hintAtom.ToLower()))
                    {
                        int subqueryNum = ExtractValueParen(hintAtom);
                        selectors.Add(new IndexHintSelectorSubquery(subqueryNum));
                    }
                    else
                    {
                        instructions.Add(new IndexHintInstructionIndexName(hintAtom.Trim()));
                    }
                }
                pairs.Add(new SelectorInstructionPair(selectors, instructions));
            }
            return new IndexHint(pairs);
        }

        public IList<IndexHintInstruction> GetInstructionsSubquery(int subqueryNumber)
        {
            foreach (SelectorInstructionPair pair in _pairs)
            {
                if (pair.Selector.IsEmpty())
                {
                    // empty selector mean hint applies to all
                    return pair.Instructions;
                }
                foreach (IndexHintSelector selector in pair.Selector)
                {
                    if (selector.MatchesSubquery(subqueryNumber))
                    {
                        return pair.Instructions;
                    }
                }
            }
            return Collections.GetEmptyList<IndexHintInstruction>();
        }

        public IList<IndexHintInstruction> InstructionsFireAndForget
        {
            get
            {
                foreach (SelectorInstructionPair pair in _pairs)
                {
                    if (pair.Selector.IsEmpty())
                    {
                        // empty selector mean hint applies to all
                        return pair.Instructions;
                    }
                }
                return Collections.GetEmptyList<IndexHintInstruction>();
            }
        }

        private static bool CheckValueInParen(String type, String value)
        {
            int indexOpen = value.IndexOf('(');
            if (indexOpen != -1)
            {
                String noparen = value.Substring(0, indexOpen).Trim().ToLower();
                if (type.Equals(noparen))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool CheckValueAssignment(String type, String value)
        {
            int indexEquals = value.IndexOf('=');
            if (indexEquals != -1)
            {
                var noequals = value.Substring(0, indexEquals).Trim().ToLower();
                if (type.Equals(noequals))
                {
                    return true;
                }
            }
            return false;
        }

        private static int ExtractValueParen(String text)
        {
            int indexOpen = text.IndexOf('(');
            int indexClosed = text.LastIndexOf(')');
            if (indexOpen != -1)
            {
                string value = text.Substring(indexOpen + 1, indexClosed - indexOpen - 1).Trim();
                try
                {
                    return int.Parse(value);
                }
                catch (Exception)
                {
                    throw new EPException("Failed to parse '" + value + "' as an index hint integer value");
                }
            }

            throw new IllegalStateException("Not a parentheses value");
        }

        internal static Object ExtractValueEqualsStringOrInt(String text)
        {
            String value = ExtractValueEquals(text);
            try
            {
                return int.Parse(value);
            }
            catch (Exception)
            {
                return value;
            }
        }

        internal static String ExtractValueEquals(String text)
        {
            int indexEquals = text.IndexOf('=');
            if (indexEquals != -1)
            {
                return text.Substring(indexEquals + 1).Trim();
            }
            throw new IllegalStateException("Not a parentheses value");
        }
    }
}