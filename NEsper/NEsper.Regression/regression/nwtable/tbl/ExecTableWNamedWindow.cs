///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableWNamedWindow : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[]{typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('var') create table varagg (key string primary key, total sum(int))");
            epService.EPAdministrator.CreateEPL("@Name('win') create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('populate') into table varagg select sum(IntPrimitive) as total from MyWindow group by TheString");
            epService.EPAdministrator.CreateEPL("@Name('select') on SupportBean_S0 select TheString, varagg[p00].total as c0 from MyWindow where TheString = p00").Events += listener.Update;
            string[] fields = "TheString,c0".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 10});
        }
    }
} // end of namespace
