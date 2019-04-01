///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
	public class ContextControllerKeyedCompositeKey {
	    private readonly IntSeqKey path;
	    private readonly object key;

	    public ContextControllerKeyedCompositeKey(IntSeqKey path, object key) {
	        this.path = path;
	        this.key = key;
	    }

	    public IntSeqKey Path {
	        get => path;
	    }

	    public object Key {
	        get => key;
	    }

	    public override bool Equals(object o) {
	        if (this == o) return true;
	        if (o == null || GetType() != o.GetType()) return false;

	        ContextControllerKeyedCompositeKey that = (ContextControllerKeyedCompositeKey) o;

	        if (!path.Equals(that.path)) return false;
	        return key != null ? key.Equals(that.key) : that.key == null;
	    }

	    public override int GetHashCode() {
	        int result = path.GetHashCode();
	        result = 31 * result + (key != null ? key.GetHashCode() : 0);
	        return result;
	    }
	}
} // end of namespace