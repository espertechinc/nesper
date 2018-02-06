///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.virtualdw;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.virtualdw;

namespace com.espertech.esper.regression.client
{
    public class ExecClientVirtualDataWindowToLookup : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory));
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
        }
    
        public override void Run(EPServiceProvider epService) {
            // client-side
            epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("E1", 100);
            window.Data = Collections.SingletonSet<object>(supportBean);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select (select sum(IntPrimitive) from MyVDW vdw where vdw.TheString = s0.p00) from SupportBean_S0 s0");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            VirtualDataWindowLookupContextSPI spiContext = (VirtualDataWindowLookupContextSPI) window.LastRequestedIndex;
    
            // CM side
            epService.EPAdministrator.CreateEPL("create window MyWin#unique(TheString) as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWin select * from SupportBean");
        }
    
        private VirtualDataWindow GetFromContext(EPServiceProvider epService, string name) {
            return (VirtualDataWindow) epService.Directory.Lookup(name);
        }
    }
} // end of namespace
