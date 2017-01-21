///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.espertech.esper.dataflow.util
{
    public class OperatorPortRepo
    {
        public OperatorPortRepo()
        {
            InputPorts = new List<MethodInfo>();
            OutputPorts = new List<MethodInfo>();
        }

        public List<MethodInfo> InputPorts { get; private set; }

        public List<MethodInfo> OutputPorts { get; private set; }

        public override String ToString()
        {
            return "OperatorPorts{" +
                   "inputPorts=" + InputPorts +
                   ", outputPorts=" + OutputPorts +
                   '}';
        }
    }
}