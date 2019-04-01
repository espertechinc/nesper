///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.type
{
	public class AnnotationAudit : AuditAttribute {
	    private readonly string value;

	    public AnnotationAudit(string value) {
	        this.value = value;
	    }

	    public override string Value {
	        get { return value; }
	        set { this.value = value; }
	    }

	    public Type AnnotationType() {
	        return typeof(Audit);
	    }
	}
} // end of namespace