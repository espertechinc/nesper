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
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.lambda;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.datetime
{
    [TestFixture]
    public class TestDTBetween
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp() {
    
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportDateTime", typeof(SupportDateTime));
            config.AddEventType("SupportTimeStartEndA", typeof(SupportTimeStartEndA));
            config.AddImport(typeof(DateTimeHelper));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        /// <summary>
        /// Tests the include endpoints.
        /// </summary>
        [Test]
        public void TestIncludeEndpoints()
        {
            String startTime = "2002-05-30 09:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
    
            String[] fieldsCurrentTs = "val0,val1,val2,val3,val4,val5,val6".Split(',');
            String eplCurrentTS = "select " +
                    "current_timestamp.After(longdateStart) as val0, " +
                    "current_timestamp.Between(longdateStart, longdateEnd) as val1, " +
                    "current_timestamp.Between(utildateStart, caldateEnd) as val2, " +
                    "current_timestamp.Between(caldateStart, utildateEnd) as val3, " +
                    "current_timestamp.Between(utildateStart, utildateEnd) as val4, " +
                    "current_timestamp.Between(caldateStart, caldateEnd) as val5, " +
                    "current_timestamp.Between(caldateEnd, caldateStart) as val6 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtCurrentTs = _epService.EPAdministrator.CreateEPL(eplCurrentTS);
            stmtCurrentTs.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtCurrentTs.EventType, fieldsCurrentTs, typeof(bool?));
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 08:59:59.999", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{true, false, false, false, false, false, false});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 08:59:59.999", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{true, true, true, true, true, true, true});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 08:59:59.999", 100));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{true, true, true, true, true, true, true});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 09:00:00.000", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{false, true, true, true, true, true, true});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 09:00:00.000", 100));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{false, true, true, true, true, true, true});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 09:00:00.001", 100));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{false, false, false, false, false, false, false});
            stmtCurrentTs.Dispose();
    
            // test calendar field and constants
            _epService.EPAdministrator.Configuration.AddImport(typeof(DateTimeParser));
            String[] fieldsConstants = "val0,val1,val2,val3".Split(',');
            String eplConstants = "select " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000')) as val0, " +
                    "utildateStart.between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000')) as val1, " +
                    "caldateStart.between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000')) as val2, " +
                    "longdateStart.between(DateTimeParser.ParseDefault('2002-05-30 09:01:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:00:00.000')) as val3 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtConstants = _epService.EPAdministrator.CreateEPL(eplConstants);
            stmtConstants.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtConstants.EventType, fieldsConstants, typeof(bool?));
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 8:59:59.999", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsConstants, false);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:00:00.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:00:05.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:00:59.999", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:01:00.000", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsConstants, true);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:01:00.001", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsConstants, false);
    
            stmtConstants.Dispose();
        }
    
        [Test]
        public void TestExcludeEndpoints()
        {
            String startTime = "2002-05-30 9:00:00.000";
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(startTime)));
            _epService.EPAdministrator.CreateEPL("create variable boolean VAR_TRUE = true");
            _epService.EPAdministrator.CreateEPL("create variable boolean VAR_FALSE = false");
    
            String[] fieldsCurrentTs = "val0,val1,val2,val3,val4,val5,val6,val7".Split(',');
            String eplCurrentTS = "select " +
                    "current_timestamp.Between(longdateStart, longdateEnd, true, true) as val0, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, true, false) as val1, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, false, true) as val2, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, false, false) as val3, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, VAR_TRUE, VAR_TRUE) as val4, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, VAR_TRUE, VAR_FALSE) as val5, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, VAR_FALSE, VAR_TRUE) as val6, " +
                    "current_timestamp.Between(longdateStart, longdateEnd, VAR_FALSE, VAR_FALSE) as val7 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtCurrentTs = _epService.EPAdministrator.CreateEPL(eplCurrentTS);
            stmtCurrentTs.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtCurrentTs.EventType, fieldsCurrentTs, typeof(bool?));
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 08:59:59.999", 0));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, false);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 08:59:59.999", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{true, false, true, false, true, false, true, false});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 08:59:59.999", 2));
            EPAssertionUtil.AssertPropsAllValuesSame(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, true);
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 09:00:00.000", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsCurrentTs, new Object[]{true, true, false, false, true, true, false, false});
    
            stmtCurrentTs.Dispose();
    
            // test calendar field and constants
            _epService.EPAdministrator.Configuration.AddImport(typeof(DateTimeParser));
            String[] fieldsConstants = "val0,val1,val2,val3".Split(',');
            String eplConstants = "select " +
                    "longdateStart.Between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000'), true, true) as val0, " +
                    "longdateStart.Between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000'), true, false) as val1, " +
                    "longdateStart.Between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000'), false, true) as val2, " +
                    "longdateStart.Between(DateTimeParser.ParseDefault('2002-05-30 09:00:00.000'), DateTimeParser.ParseDefault('2002-05-30 09:01:00.000'), false, false) as val3 " +
                    "from SupportTimeStartEndA";
            EPStatement stmtConstants = _epService.EPAdministrator.CreateEPL(eplConstants);
            stmtConstants.Events += _listener.Update;
            LambdaAssertionUtil.AssertTypesAllSame(stmtConstants.EventType, fieldsConstants, typeof(bool?));
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E1", "2002-05-30 8:59:59.999", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsConstants, new Object[]{false, false, false, false});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:00:00.000", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsConstants, new Object[]{true, true, false, false});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:00:05.000", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsConstants, new Object[]{true, true, true, true});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:00:59.999", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsConstants, new Object[]{true, true, true, true});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:01:00.000", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsConstants, new Object[]{true, false, true, false});
    
            _epService.EPRuntime.SendEvent(SupportTimeStartEndA.Make("E2", "2002-05-30 9:01:00.001", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fieldsConstants, new Object[]{false, false, false, false});
    
            stmtConstants.Dispose();
        }
    }
}
