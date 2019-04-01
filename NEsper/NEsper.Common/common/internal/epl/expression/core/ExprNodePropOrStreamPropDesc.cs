///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
	public class ExprNodePropOrStreamPropDesc : ExprNodePropOrStreamDesc
    {
	    public ExprNodePropOrStreamPropDesc(int streamNum, string propertyName) {
	        this.StreamNum = streamNum;
	        this.PropertyName = propertyName;
	        if (propertyName == null) {
	            throw new ArgumentException("Property name is null");
	        }
	    }

	    public string PropertyName { get; private set; }

	    public int StreamNum { get; private set; }

	    public string Textual
	    {
	        get { return "property '" + PropertyName + "'"; }
	    }

	    protected bool Equals(ExprNodePropOrStreamPropDesc other)
	    {
	        return StreamNum == other.StreamNum && string.Equals(PropertyName, other.PropertyName);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj))
	            return false;
	        if (ReferenceEquals(this, obj))
	            return true;
	        if (obj.GetType() != this.GetType())
	            return false;
	        return Equals((ExprNodePropOrStreamPropDesc) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return ((PropertyName != null ? PropertyName.GetHashCode() : 0)*397) ^ StreamNum;
	        }
	    }
    }
} // end of namespace