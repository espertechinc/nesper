///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DefaultSupportGraphOpProviderByOpName : EPDataFlowOperatorProvider
    {
        private readonly IDictionary<string, object> names;

        public DefaultSupportGraphOpProviderByOpName(IDictionary<string, object> names)
        {
            this.names = names;
        }

        public object Provide(EPDataFlowOperatorProviderContext context)
        {
            if (names.ContainsKey(context.OperatorName)) {
                return names.Get(context.OperatorName);
            }

            if (context.Factory is DefaultSupportSourceOpFactory sourceOpFactory) {
                if (sourceOpFactory.Name != null && names.ContainsKey(sourceOpFactory.Name)) {
                    return names.Get(sourceOpFactory.Name);
                }
            }

            if (context.Factory is DefaultSupportCaptureOpFactory<object> captureOpFactory) {
                if (captureOpFactory.Name != null && names.ContainsKey(captureOpFactory.Name)) {
                    return names.Get(captureOpFactory.Name);
                }
            }

            return null;
        }
    }
} // end of namespace