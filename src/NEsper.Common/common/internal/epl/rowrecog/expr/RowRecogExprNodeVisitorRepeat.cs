///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.expr
{
    public class RowRecogExprNodeVisitorRepeat : RowRecogExprNodeVisitor
    {
        private IList<Pair<RowRecogExprNodeAtom, RowRecogExprNode>> _atoms;
        private IList<RowRegexNestedDesc> _nesteds;
        private IList<RowRegexPermuteDesc> _permutes;

        public IList<Pair<RowRecogExprNodeAtom, RowRecogExprNode>> Atoms {
            get {
                if (_atoms == null) {
                    return Collections.GetEmptyList<Pair<RowRecogExprNodeAtom, RowRecogExprNode>>();
                }

                return _atoms;
            }
        }

        public IList<RowRegexNestedDesc> Nesteds {
            get {
                if (_nesteds == null) {
                    return Collections.GetEmptyList<RowRegexNestedDesc>();
                }

                return _nesteds;
            }
        }

        public IList<RowRegexPermuteDesc> Permutes {
            get {
                if (_permutes == null) {
                    return Collections.GetEmptyList<RowRegexPermuteDesc>();
                }

                return _permutes;
            }
        }

        public void Visit(
            RowRecogExprNode node,
            RowRecogExprNode optionalParent,
            int level)
        {
            var atom = node as RowRecogExprNodeAtom;
            if (atom?.OptionalRepeat != null) {
                if (_atoms == null) {
                    _atoms = new List<Pair<RowRecogExprNodeAtom, RowRecogExprNode>>();
                }

                _atoms.Add(new Pair<RowRecogExprNodeAtom, RowRecogExprNode>(atom, optionalParent));
            }

            var nested = node as RowRecogExprNodeNested;
            if (nested?.OptionalRepeat != null) {
                if (_nesteds == null) {
                    _nesteds = new List<RowRegexNestedDesc>();
                }

                _nesteds.Add(new RowRegexNestedDesc(nested, optionalParent, level));
            }

            if (node is RowRecogExprNodePermute permute) {
                if (_permutes == null) {
                    _permutes = new List<RowRegexPermuteDesc>();
                }

                _permutes.Add(new RowRegexPermuteDesc(permute, optionalParent, level));
            }
        }

        public class RowRegexPermuteDesc
        {
            public RowRegexPermuteDesc(
                RowRecogExprNodePermute permute,
                RowRecogExprNode optionalParent,
                int level)
            {
                Permute = permute;
                OptionalParent = optionalParent;
                Level = level;
            }

            public RowRecogExprNodePermute Permute { get; }

            public RowRecogExprNode OptionalParent { get; }

            public int Level { get; }
        }

        public class RowRegexNestedDesc
        {
            public RowRegexNestedDesc(
                RowRecogExprNodeNested nested,
                RowRecogExprNode optionalParent,
                int level)
            {
                Nested = nested;
                OptionalParent = optionalParent;
                Level = level;
            }

            public RowRecogExprNodeNested Nested { get; }

            public RowRecogExprNode OptionalParent { get; }

            public int Level { get; }
        }
    }
} // end of namespace