///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.map
{
    public class ExecEventMapPropertyConfig : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var properties = new Properties();
            properties.Put("myInt", typeof(int).Name);
            properties.Put("byteArr", typeof(byte[]).Name);
            properties.Put("myInt2", "int");
            properties.Put("double", "double");
            properties.Put("bool", "bool");
            properties.Put("long", "long");
            properties.Put("astring", "string");
            configuration.AddEventType("MyPrimMapEvent", properties);
        }
    
        public override void Run(EPServiceProvider epService) {
            // no assertions required
        }
    }
} // end of namespace
