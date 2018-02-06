///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.epl.expression.core
{
	public class ExprNodePropOrStreamSet
    {
	    private ISet<ExprNodePropOrStreamPropDesc> _properties;
	    private IList<ExprNodePropOrStreamExprDesc> _expressions;

	    public ExprNodePropOrStreamSet()
        {
	    }

	    public void Add(ExprNodePropOrStreamDesc desc)
        {
	        if (desc is ExprNodePropOrStreamPropDesc) {
	            AllocateProperties();
	            _properties.Add((ExprNodePropOrStreamPropDesc) desc);
	        }
	        else if (desc is ExprNodePropOrStreamExprDesc) {
	            AllocateExpressions();
	            _expressions.Add((ExprNodePropOrStreamExprDesc) desc);
	        }
	    }

	    public void AddAll(IList<ExprNodePropOrStreamDesc> propertiesNode)
        {
	        foreach (ExprNodePropOrStreamDesc desc in propertiesNode) {
	            Add(desc);
	        }
	    }

	    public void AddAll(ExprNodePropOrStreamSet other) {
	        if (other._properties != null) {
	            AllocateProperties();
	            _properties.AddAll(other._properties);
	        }
	        if (other._expressions != null) {
	            AllocateExpressions();
	            _expressions.AddAll(other._expressions);
	        }
	    }

	    public bool IsEmpty() {
	        return (_properties == null || _properties.IsEmpty()) &&
	                (_expressions == null || _expressions.IsEmpty());
	    }

	    /// <summary>
	    /// Remove from the provided list those that are matching any of the contained-herein
	    /// </summary>
	    /// <param name="items">target list</param>
	    public void RemoveFromList(IList<ExprNodePropOrStreamDesc> items)
	    {
	        items.RemoveWhere(FindItem);
	    }

	    public string NotContainsAll(ExprNodePropOrStreamSet other) {
	        if (other._properties != null) {
	            foreach (ExprNodePropOrStreamPropDesc otherProp in other._properties) {
	                bool found = FindItem(otherProp);
	                if (!found) {
	                    return otherProp.Textual;
	                }
	            }
	        }
	        if (other._expressions != null) {
	            foreach (ExprNodePropOrStreamExprDesc otherExpr in other._expressions) {
	                bool found = FindItem(otherExpr);
	                if (!found) {
	                    return otherExpr.Textual;
	                }
	            }
	        }
	        return null;
	    }

	    public ICollection<ExprNodePropOrStreamPropDesc> Properties
	    {
	        get
	        {
	            if (_properties == null)
	            {
	                return Collections.GetEmptyList<ExprNodePropOrStreamPropDesc>();
	            }
	            return _properties;
	        }
	    }

	    public ExprNodePropOrStreamExprDesc FirstExpression
	    {
	        get { return _expressions != null ? _expressions.FirstOrDefault() : null; }
	    }

	    public ExprNodePropOrStreamDesc FirstWithStreamNumNotZero
	    {
	        get
	        {
	            if (_properties != null)
	            {
	                foreach (ExprNodePropOrStreamPropDesc prop in _properties)
	                {
	                    if (prop.StreamNum != 0)
	                    {
	                        return prop;
	                    }
	                }
	            }
	            if (_expressions != null)
	            {
	                foreach (ExprNodePropOrStreamExprDesc expr in _expressions)
	                {
	                    if (expr.StreamNum != 0)
	                    {
	                        return expr;
	                    }
	                }
	            }
	            return null;
	        }
	    }

	    private void AllocateProperties()
        {
	        if (_properties == null) {
	            _properties = new HashSet<ExprNodePropOrStreamPropDesc>();
	        }
	    }

	    private void AllocateExpressions()
        {
	        if (_expressions == null) {
	            _expressions = new List<ExprNodePropOrStreamExprDesc>(4);
	        }
	    }

	    private bool FindItem(ExprNodePropOrStreamDesc item)
        {
	        if (item is ExprNodePropOrStreamPropDesc) {
	            return _properties != null && _properties.Contains(item);
	        }
	        if (_expressions == null) {
	            return false;
	        }
	        var exprItem = (ExprNodePropOrStreamExprDesc) item;
	        foreach (ExprNodePropOrStreamExprDesc expression in _expressions) {
	            if (expression.StreamNum != exprItem.StreamNum) {
	                continue;
	            }
	            if (ExprNodeUtility.DeepEquals(expression.Originator, exprItem.Originator, false)) {
	                return true;
	            }
	        }
	        return false;
	    }
	}
} // end of namespace
