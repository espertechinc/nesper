///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.script.core
{
	public class NameAndParamNum {
	    private readonly static NameAndParamNum[] EMPTY_ARRAY = new NameAndParamNum[0];

	    private readonly string name;
	    private readonly int paramNum;

	    public NameAndParamNum(string name, int paramNum) {
	        this.name = name;
	        this.paramNum = paramNum;
	    }

	    public string Name {
	        get => name;
	    }

	    public int ParamNum {
	        get => paramNum;
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;

	        NameAndParamNum that = (NameAndParamNum) o;

	        if (paramNum != that.paramNum) return false;
	        return name.Equals(that.name);
	    }

	    public override int GetHashCode() {
	        int result = name.GetHashCode();
	        result = 31 * result + paramNum;
	        return result;
	    }

	    public static NameAndParamNum[] ToArray(IList<NameAndParamNum> pathScripts) {
	        if (pathScripts.IsEmpty()) {
	            return EMPTY_ARRAY;
	        }
	        return pathScripts.ToArray();
	    }

	    public override string ToString() {
	        return name + " (" + paramNum + " parameters)";
	    }
	}
} // end of namespace