///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using com.espertech.esper.client.dataflow;

namespace com.espertech.esper.dataflow.util
{
    public class DefaultSupportGraphOpProvider : EPDataFlowOperatorProvider
    {
        private readonly Object[] _ops;

        public DefaultSupportGraphOpProvider(Object op)
        {
            _ops = new Object[] { op };
        }

        public DefaultSupportGraphOpProvider(params Object[] ops)
        {
            _ops = ops;
        }

        public Object Provide(EPDataFlowOperatorProviderContext context)
        {
            return _ops.FirstOrDefault(op => context.OperatorName == op.GetType().Name);
        }
    }
}
