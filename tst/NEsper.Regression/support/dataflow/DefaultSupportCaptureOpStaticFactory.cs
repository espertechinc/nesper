///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.dataflow.interfaces;

namespace com.espertech.esper.regressionlib.support.dataflow
{
    public class DefaultSupportCaptureOpStaticFactory<T> : DataFlowOperatorFactory
    {
        public void InitializeFactory(DataFlowOpFactoryInitializeContext context)
        {
        }

        public DataFlowOperator Operator(DataFlowOpInitializeContext context)
        {
            return new DefaultSupportCaptureOpStatic<T>();
        }
    }
} // end of namespace