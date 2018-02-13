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
using com.espertech.esper.client.time;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.expr.datetime
{
    public class ExecDTProperty : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("SupportDateTime", typeof(SupportDateTime));
        }
    
        public override void Run(EPServiceProvider epService)
        {
            string startTime = "2002-05-30T09:01:02.003";   // use 2-digit hour, see https://bugs.openjdk.java.net/browse/JDK-8066806
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            string[] fields = "valmoh,valmoy,valdom,valdow,valdoy,valera,valhod,valmos,valsom,valwye,valyea,val1,val2,val3,val4,val5".Split(',');
            string eplFragment = "select " +
                    "current_timestamp.MinuteOfHour as valmoh," +
                    "current_timestamp.MonthOfYear as valmoy," +
                    "current_timestamp.DayOfMonth as valdom," +
                    "current_timestamp.DayOfWeek as valdow," +
                    "current_timestamp.DayOfYear as valdoy," +
                    "current_timestamp.Era as valera," +
                    "current_timestamp.hourOfDay as valhod," +
                    "current_timestamp.millisOfSecond  as valmos," +
                    "current_timestamp.secondOfMinute as valsom," +
                    "current_timestamp.weekyear as valwye," +
                    "current_timestamp.year as valyea," +
                    "utildate.hourOfDay as val1," +
                    "longdate.hourOfDay as val2," +
                    "caldate.hourOfDay as val3," +
                    "zoneddate.hourOfDay as val4," +
                    "localdate.hourOfDay as val5" +
                    " from SupportDateTime";
            EPStatement stmtFragment = epService.EPAdministrator.CreateEPL(eplFragment);
            var listener = new SupportUpdateListener();
            stmtFragment.AddListener(listener);
            foreach (string field in fields) {
                Assert.AreEqual(typeof(int?), stmtFragment.EventType.GetPropertyType(field));
            }
    
            epService.EPRuntime.SendEvent(SupportDateTime.Make(startTime));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[]{
                    1, 4, 30, 5, 150, 1, 9, 3, 2, 22, 2002, 9, 9, 9, 9, 9
            });
    
            // test Map inheritance via create-schema
            epService.EPAdministrator.CreateEPL("create schema ParentType as (startTS long, endTS long) starttimestamp startTS endtimestamp endTS");
            epService.EPAdministrator.CreateEPL("create schema ChildType as (foo string) inherits ParentType");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from ChildType dt where Dt.Before(Current_timestamp())");
            Assert.AreEqual("startTS", stmt.EventType.StartTimestampPropertyName);
            Assert.AreEqual("endTS", stmt.EventType.EndTimestampPropertyName);
    
            // test POJO inheritance via create-schema
            epService.EPAdministrator.CreateEPL("create schema InterfaceType as " + typeof(MyInterface).Name + " starttimestamp startTS endtimestamp endTS");
            epService.EPAdministrator.CreateEPL("create schema DerivedType as " + typeof(MyImplOne).Name);
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select * from DerivedType dt where Dt.Before(Current_timestamp())");
            Assert.AreEqual("startTS", stmtTwo.EventType.StartTimestampPropertyName);
            Assert.AreEqual("endTS", stmtTwo.EventType.EndTimestampPropertyName);
    
            // test incompatible
            epService.EPAdministrator.CreateEPL("create schema T1 as (startTS long, endTS long) starttimestamp startTS endtimestamp endTS");
            epService.EPAdministrator.CreateEPL("create schema T2 as (startTSOne long, endTSOne long) starttimestamp startTSOne endtimestamp endTSOne");
            try {
                epService.EPAdministrator.CreateEPL("create schema T12 as () inherits T1,T2");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Event type declares start timestamp as property 'startTS' however inherited event type 'T2' declares start timestamp as property 'startTSOne' [create schema T12 as () inherits T1,T2]", ex.Message);
            }
            try {
                epService.EPAdministrator.CreateEPL("create schema T12 as (startTSOne long, endTSXXX long) inherits T2 starttimestamp startTSOne endtimestamp endTSXXX");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Event type declares end timestamp as property 'endTSXXX' however inherited event type 'T2' declares end timestamp as property 'endTSOne' [create schema T12 as (startTSOne long, endTSXXX long) inherits T2 starttimestamp startTSOne endtimestamp endTSXXX]", ex.Message);
            }
        }
    
        public interface MyInterface
        {
            long StartTS { get; }
            long EndTS { get; }
        }
    
        public class MyImplOne : MyInterface
        {
            public MyImplOne(string datestr, long duration) {
                StartTS = DateTimeParser.ParseDefaultMSec(datestr);
                EndTS = StartTS + duration;
            }

            public long StartTS { get; }
            public long EndTS { get; }
        }
    }
} // end of namespace
