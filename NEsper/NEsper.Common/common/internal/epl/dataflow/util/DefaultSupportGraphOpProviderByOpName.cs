///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
	public class DefaultSupportGraphOpProviderByOpName : EPDataFlowOperatorProvider {
	    private readonly IDictionary<string, object> names;

	    public DefaultSupportGraphOpProviderByOpName(IDictionary<string, object> names) {
	        this.names = names;
	    }

	    public object Provide(EPDataFlowOperatorProviderContext context) {
	        if (names.ContainsKey(context.OperatorName)) {
	            return names.Get(context.OperatorName);
	        }
	        if (context.Factory is DefaultSupportSourceOpFactory) {
	            DefaultSupportSourceOpFactory factory = (DefaultSupportSourceOpFactory) context.Factory;
	            if (factory.Name != null && names.ContainsKey(factory.Name)) {
	                return names.Get(factory.Name);
	            }
	        }
	        if (context.Factory is DefaultSupportCaptureOpFactory) {
	            DefaultSupportCaptureOpFactory factory = (DefaultSupportCaptureOpFactory) context.Factory;
	            if (factory.Name != null && names.ContainsKey(factory.Name)) {
	                return names.Get(factory.Name);
	            }
	        }
	        return null;
	    }
	}
} // end of namespace