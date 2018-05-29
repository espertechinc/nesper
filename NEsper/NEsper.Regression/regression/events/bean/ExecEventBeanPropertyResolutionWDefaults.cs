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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    using Map = IDictionary<string, object>;

    public class ExecEventBeanPropertyResolutionWDefaults : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_SENSITIVE;
        }

        public override void Run(EPServiceProvider epService)
        {
            RunAssertionReservedKeywordEscape(epService);
            RunAssertionWriteOnly(epService);
            RunAssertionCaseSensitive(epService);
        }

        private void RunAssertionReservedKeywordEscape(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType("SomeKeywords", typeof(SupportBeanReservedKeyword));
            epService.EPAdministrator.Configuration.AddEventType("Order", typeof(SupportBeanReservedKeyword));
            var listener = new SupportUpdateListener();

            EPStatement stmt = epService.EPAdministrator.CreateEPL("select `seconds`, `order` from SomeKeywords");
            stmt.Events += listener.Update;

            var theEvent = new SupportBeanReservedKeyword(1, 2);
            epService.EPRuntime.SendEvent(theEvent);
            EventBean eventBean = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, eventBean.Get("seconds"));
            Assert.AreEqual(2, eventBean.Get("order"));

            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select * from `Order`");
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(theEvent);
            eventBean = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, eventBean.Get("seconds"));
            Assert.AreEqual(2, eventBean.Get("order"));

            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select timestamp.`hour` as val from SomeKeywords");
            stmt.Events += listener.Update;

            var bean = new SupportBeanReservedKeyword(1, 2);
            bean.Timestamp = new SupportBeanReservedKeyword.Inner();
            bean.Timestamp.Hour = 10;
            epService.EPRuntime.SendEvent(bean);
            eventBean = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(10, eventBean.Get("val"));

            // test back-tick with spaces etc
            var defType = new Dictionary<string, Object>();
            defType.Put("candidate book", typeof(string));
            defType.Put("XML Message Type", typeof(string));
            defType.Put("select", typeof(int));
            defType.Put("children's books", typeof(int[]));
            defType.Put("my <> map", typeof(Map));
            epService.EPAdministrator.Configuration.AddEventType("MyType", defType);
            epService.EPAdministrator.CreateEPL(
                    "select `candidate book` as c0, `XML Message Type` as c1, `select` as c2, `children's books`[0] as c3, `my <> map`('xx') as c4 from MyType")
                .Events += listener.Update;

            var defValues = new Dictionary<string, Object>();
            defValues.Put("candidate book", "Enders Game");
            defValues.Put("XML Message Type", "book");
            defValues.Put("select", 100);
            defValues.Put("children's books", new[] {50, 51});
            defValues.Put("my <> map", Collections.SingletonMap("xx", "abc"));
            epService.EPRuntime.SendEvent(defValues, "MyType");
            EPAssertionUtil.AssertProps(
                listener.AssertOneGetNewAndReset(), "c0,c1,c2,c3,c4".Split(','),
                new object[] {"Enders Game", "book", 100, 50, "abc"});

            try
            {
                epService.EPAdministrator.CreateEPL("select `select` from " + typeof(SupportBean).FullName);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex,
                    "Error starting statement: Failed to validate select-clause expression 'select': Property named 'select' is not valid in any stream [");
            }

            try
            {
                epService.EPAdministrator.CreateEPL("select `ab cd` from " + typeof(SupportBean).FullName);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex,
                    "Error starting statement: Failed to validate select-clause expression 'ab cd': Property named 'ab cd' is not valid in any stream [");
            }

            // test resolution as nested property
            epService.EPAdministrator.CreateEPL("create schema MyEvent as (customer string, `from` string)");
            epService.EPAdministrator.CreateEPL("insert into DerivedStream select customer,`from` from MyEvent");
            epService.EPAdministrator.CreateEPL(
                "create window TheWindow#firstunique(customer,`from`) as DerivedStream");
            epService.EPAdministrator.CreateEPL(
                "on pattern [a=TheWindow -> timer:interval(12 hours)] as s0 delete from TheWindow as s1 where s0.a.`from`=s1.`from`");

            // test escape in column name
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(
                "select TheString as `order`, TheString as `price.for.goods` from SupportBean");
            stmtTwo.Events += listener.Update;
            Assert.AreEqual(typeof(string), stmtTwo.EventType.GetPropertyType("order"));
            Assert.AreEqual("price.for.goods", stmtTwo.EventType.PropertyDescriptors[1].PropertyName);

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            IDictionary<string, Object> @out = (IDictionary<string, Object>) listener.AssertOneGetNew().Underlying;
            Assert.AreEqual("E1", @out.Get("order"));
            Assert.AreEqual("E1", @out.Get("price.for.goods"));

            // try control character
            TryInvalidControlCharacter(listener.AssertOneGetNew());

            // try enum with keyword
            TryEnumWithKeyword(epService);

            TryEnumItselfReserved(epService);

            epService.EPAdministrator.DestroyAllStatements();
        }

        private void RunAssertionWriteOnly(EPServiceProvider epService)
        {
            EPStatement stmt =
                epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBeanWriteOnly).FullName);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            var theEvent = new SupportBeanWriteOnly();
            epService.EPRuntime.SendEvent(theEvent);
            EventBean eventBean = listener.AssertOneGetNewAndReset();
            Assert.AreSame(theEvent, eventBean.Underlying);

            EventType type = stmt.EventType;
            Assert.AreEqual(0, type.PropertyNames.Length);

            stmt.Dispose();
        }

        private void RunAssertionCaseSensitive(EPServiceProvider epService)
        {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                "select MYPROPERTY, myproperty, myProperty from " + typeof(SupportBeanDupProperty).FullName);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBeanDupProperty("lowercamel", "uppercamel", "upper", "lower"));
            EventBean result = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("upper", result.Get("MYPROPERTY"));
            Assert.AreEqual("lower", result.Get("myproperty"));
            Assert.IsTrue(
                result.Get("myProperty").Equals("lowercamel") ||
                result.Get("myProperty").Equals("uppercamel")); // JDK6 versus JDK7 JavaBean inspector

            stmt.Dispose();
            try
            {
                epService.EPAdministrator.CreateEPL(
                    "select MyProPerty from " + typeof(SupportBeanDupProperty).FullName);
                Assert.Fail();
            }
            catch (EPException)
            {
                // expected
            }
        }

        private void TryEnumWithKeyword(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(LocalEventWithEnum));
            epService.EPAdministrator.Configuration.AddImport(typeof(LocalEventEnum));
            epService.EPAdministrator.CreateEPL(
                "select * from LocalEventWithEnum(LocalEventEnum=LocalEventEnum.`NEW`)");
        }

        private void TryInvalidControlCharacter(EventBean eventBean)
        {
            try
            {
                eventBean.Get("a\u008F");
                Assert.Fail();
            }
            catch (PropertyAccessException ex)
            {
                SupportMessageAssertUtil.AssertMessage(
                    ex, "Property named 'a\u008F' is not a valid property name for this type");
            }
        }

        private void TryEnumItselfReserved(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(LocalEventWithGroup));
            epService.EPAdministrator.Configuration.AddImport(typeof(GROUP));
            epService.EPAdministrator.CreateEPL("select * from LocalEventWithGroup(`GROUP`=`GROUP`.FOO)");
        }

        public class LocalEventWithEnum
        {
            public LocalEventWithEnum(LocalEventEnum localEventEnum)
            {
                LocalEventEnum = localEventEnum;
            }

            public LocalEventEnum LocalEventEnum { get; }
        }

        public enum LocalEventEnum
        {
            NEW
        }

        public class LocalEventWithGroup
        {
            public GROUP GROUP { get; }
            public LocalEventWithGroup(GROUP group)
            {
                GROUP = group;
            }
        }

        public enum GROUP
        {
            FOO,
            BAR
        }
    }
} // end of namespace
