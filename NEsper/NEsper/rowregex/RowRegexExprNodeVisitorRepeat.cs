///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.rowregex
{
	public class RowRegexExprNodeVisitorRepeat : RowRegexExprNodeVisitor
    {
	    private IList<Pair<RowRegexExprNodeAtom, RowRegexExprNode>> _atoms;
	    private IList<RowRegexNestedDesc> _nesteds;
	    private IList<RowRegexPermuteDesc> _permutes;

	    public void Visit(RowRegexExprNode node, RowRegexExprNode optionalParent, int level)
        {
	        if (node is RowRegexExprNodeAtom) {
	            var atom = (RowRegexExprNodeAtom) node;
	            if (atom.OptionalRepeat != null) {
	                if (_atoms == null) {
	                    _atoms = new List<Pair<RowRegexExprNodeAtom, RowRegexExprNode>>();
	                }
	                _atoms.Add(new Pair<RowRegexExprNodeAtom, RowRegexExprNode>(atom, optionalParent));
	            }
	        }
	        if (node is RowRegexExprNodeNested) {
	            var nested = (RowRegexExprNodeNested) node;
	            if (nested.OptionalRepeat != null) {
	                if (_nesteds == null) {
	                    _nesteds = new List<RowRegexNestedDesc>();
	                }
	                _nesteds.Add(new RowRegexNestedDesc(nested, optionalParent, level));
	            }
	        }
	        if (node is RowRegexExprNodePermute) {
	            var permute = (RowRegexExprNodePermute) node;
	            if (_permutes == null) {
	                _permutes = new List<RowRegexPermuteDesc>();
	            }
	            _permutes.Add(new RowRegexPermuteDesc(permute, optionalParent, level));
	        }
	    }

	    public IList<Pair<RowRegexExprNodeAtom, RowRegexExprNode>> Atoms
	    {
	        get
	        {
	            if (_atoms == null)
	            {
                    return Collections.GetEmptyList<Pair<RowRegexExprNodeAtom, RowRegexExprNode>>();
	            }
	            return _atoms;
	        }
	    }

	    public IList<RowRegexNestedDesc> Nesteds
	    {
	        get
	        {
	            if (_nesteds == null)
	            {
                    return Collections.GetEmptyList<RowRegexNestedDesc>();
	            }
	            return _nesteds;
	        }
	    }

	    public IList<RowRegexPermuteDesc> Permutes
	    {
	        get
	        {
	            if (_permutes == null)
	            {
                    return Collections.GetEmptyList<RowRegexPermuteDesc>();
	            }
	            return _permutes;
	        }
	    }

	    public struct RowRegexPermuteDesc
        {
	        public RowRegexPermuteDesc(RowRegexExprNodePermute permute, RowRegexExprNode optionalParent, int level)
            {
	            Permute = permute;
	            OptionalParent = optionalParent;
	            Level = level;
	        }

	        public RowRegexExprNodePermute Permute;
	        public RowRegexExprNode OptionalParent;
	        public int Level;
        }

	    public struct RowRegexNestedDesc
        {
	        public RowRegexNestedDesc(RowRegexExprNodeNested nested, RowRegexExprNode optionalParent, int level)
            {
	            Nested = nested;
	            OptionalParent = optionalParent;
	            Level = level;
	        }

	        public RowRegexExprNodeNested Nested;
	        public RowRegexExprNode OptionalParent;
	        public int Level;
        }
	}
} // end of namespace
