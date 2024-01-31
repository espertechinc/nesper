///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.dataflow.core;

namespace com.espertech.esper.common.@internal.epl.dataflow.util
{
    public class DefaultSupportGraphOpProvider : EPDataFlowOperatorProvider
    {
        private readonly object[] ops;

        public DefaultSupportGraphOpProvider(object op)
        {
            ops = new object[] { op };
        }

        public DefaultSupportGraphOpProvider(params object[] ops)
        {
            this.ops = ops;
        }

        public object Provide(EPDataFlowOperatorProviderContext context)
        {
            foreach (var op in ops) {
                if (context.OperatorName.Equals(op.GetType().Name)) {
                    return op;
                }
            }

            return null;
        }
    }
} // end of namespace