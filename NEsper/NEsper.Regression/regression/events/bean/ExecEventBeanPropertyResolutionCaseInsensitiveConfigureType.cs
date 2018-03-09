///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanPropertyResolutionCaseInsensitiveConfigureType : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var legacyDef = new ConfigurationEventTypeLegacy();
            legacyDef.PropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;
            configuration.AddEventType("Bean", typeof(SupportBean).AssemblyQualifiedName, legacyDef);
        }
    
        public override void Run(EPServiceProvider epService) {
            ExecEventBeanPropertyResolutionCaseInsensitiveEngineDefault.TryCaseInsensitive(epService, "select THESTRING, INTPRIMITIVE from Bean where THESTRING='A'", "THESTRING", "INTPRIMITIVE");
            ExecEventBeanPropertyResolutionCaseInsensitiveEngineDefault.TryCaseInsensitive(epService, "select ThEsTrInG, INTprimitIVE from Bean where THESTRing='A'", "ThEsTrInG", "INTprimitIVE");
        }
    }
} // end of namespace
