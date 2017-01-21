///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.dataflow.util;

namespace com.espertech.esper.dataflow.core
{
    public class ObjectBindingPair
    {
        public ObjectBindingPair(Object target, String operatorPrettyPrint, LogicalChannelBinding binding)
        {
            Target = target;
            OperatorPrettyPrint = operatorPrettyPrint;
            Binding = binding;
        }

        public string OperatorPrettyPrint { get; private set; }

        public object Target { get; private set; }

        public LogicalChannelBinding Binding { get; private set; }
    }
}