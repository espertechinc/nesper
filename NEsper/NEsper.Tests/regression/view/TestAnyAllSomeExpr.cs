///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAnyAllSomeExpr 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("ArrayBean", typeof(SupportBeanArrayCollMap));
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestEqualsAll()
        {
            String[] fields = "eq,neq,sqlneq,nneq".Split(',');
            String stmtText = "select " +
                              "IntPrimitive=all (1,IntBoxed) as eq, " +
                              "IntPrimitive!=all (1,IntBoxed) as neq, " +
                              "IntPrimitive<>all (1,IntBoxed) as sqlneq, " +
                              "not IntPrimitive=all (1,IntBoxed) as nneq " +
                              "from SupportBean(TheString like \"E%\")";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // in the format intPrimitive, intBoxed
            int[][] testdata = {
                    new[] {1, 1},
                    new[] {1, 2},
                    new[] {2, 2},
                    new[] {2, 1},
            };
    
            Object[][] result = {
                    new Object[] {true, false, false, false}, // 1, 1
                    new Object[] {false, false, false, true}, // 1, 2
                    new Object[] {false, false, false, true}, // 2, 2
                    new Object[] {false, true, true, true}    // 2, 1
                    };
    
            for (int i = 0; i < testdata.Length; i++)
            {
                SupportBean bean = new SupportBean("E", testdata[i][0]);
                bean.IntBoxed = testdata[i][1];
                _epService.EPRuntime.SendEvent(bean);
                //Console.Out.WriteLine("line " + i);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
            
            // test OM
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText.Replace("<>", "!="), model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            for (int i = 0; i < testdata.Length; i++)
            {
                SupportBean bean = new SupportBean("E", testdata[i][0]);
                bean.IntBoxed = testdata[i][1];
                _epService.EPRuntime.SendEvent(bean);
                //Console.Out.WriteLine("line " + i);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
        }
    
        [Test]
        public void TestEqualsAllArray()
        {
            String[] fields = "e,ne".Split(',');
            String stmtText = "select " +
                              "LongBoxed = all ({1, 1}, intArr, longCol) as e, " +
                              "LongBoxed != all ({1, 1}, intArr, longCol) as ne " +
                              "from ArrayBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SupportBeanArrayCollMap arrayBean = new SupportBeanArrayCollMap(new int[] {1, 1});
            arrayBean.LongCol = new List<long?> {1L, 1L};
            arrayBean.LongBoxed = 1L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false});

            arrayBean.IntArr = new int[] {1, 1, 0};
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false});
    
            arrayBean.LongBoxed = 2L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
        }
    
        [Test]
        public void TestEqualsAnyArray()
        {
            String[] fields = "e,ne".Split(',');
            String stmtText = "select " +
                              "LongBoxed = any ({1, 1}, intArr, longCol) as e, " +
                              "LongBoxed != any ({1, 1}, intArr, longCol) as ne " +
                              "from ArrayBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SupportBeanArrayCollMap arrayBean = new SupportBeanArrayCollMap(new int[] {1, 1});
            arrayBean.LongCol = new List<long?>() {1L, 1L};
            arrayBean.LongBoxed = 1L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, false});
    
            arrayBean.IntArr = new int[] {1, 1, 0};
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true});
    
            arrayBean.LongBoxed = 2L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
        }
    
        [Test]
        public void TestRelationalOpAllArray()
        {
            String[] fields = "g,ge".Split(',');
            String stmtText = "select " +
                              "LongBoxed>all ({1,2},intArr,intCol) as g, " +
                              "LongBoxed>=all ({1,2},intArr,intCol) as ge " +
                              "from ArrayBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SupportBeanArrayCollMap arrayBean = new SupportBeanArrayCollMap(new int[] {1, 2});
            arrayBean.IntCol = new int[] {1, 2};
            arrayBean.LongBoxed = 3L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true});
    
            arrayBean.LongBoxed = 2L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[] {1, 3});
            arrayBean.IntCol = new[] {1, 2};
            arrayBean.LongBoxed = 3L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[] {1, 2});
            arrayBean.IntCol = new[] { 1, 3 };
            arrayBean.LongBoxed = 3L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
    
            // test OM
            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            Assert.AreEqual(stmtText.Replace("<>", "!="), model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            stmt.Events += _listener.Update;
    
            arrayBean = new SupportBeanArrayCollMap(new int[] {1, 2});
            arrayBean.IntCol = new[] { 1, 2 };
            arrayBean.LongBoxed = 3L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true});
        }
    
        [Test]
        public void TestRelationalOpNullOrNoRows()
        {
            // test array
            String[] fields = "vall,vany".Split(',');
            String stmtText = "select " +
                "IntBoxed >= all ({DoubleBoxed, LongBoxed}) as vall, " +
                "IntBoxed >= any ({DoubleBoxed, LongBoxed}) as vany " +
                " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEvent("E3", null, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
            SendEvent("E4", 1, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
    
            SendEvent("E5", null, 1d, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
            SendEvent("E6", 1, 1d, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, true});
            SendEvent("E7", 0, 1d, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false});
    
            // test fields
            stmt.Dispose();
            fields = "vall,vany".Split(',');
            stmtText = "select " +
                "IntBoxed >= all (DoubleBoxed, LongBoxed) as vall, " +
                "IntBoxed >= any (DoubleBoxed, LongBoxed) as vany " +
                " from SupportBean(TheString like 'E%')";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEvent("E3", null, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
            SendEvent("E4", 1, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
    
            SendEvent("E5", null, 1d, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null});
            SendEvent("E6", 1, 1d, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, true});
            SendEvent("E7", 0, 1d, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false});
        }
    
        [Test]
        public void TestRelationalOpAnyArray()
        {
            String[] fields = "g,ge".Split(',');
            String stmtText = "select " +
                              "LongBoxed > any ({1, 2}, intArr, intCol) as g, " +
                              "LongBoxed >= any ({1, 2}, intArr, intCol) as ge " +
                              "from ArrayBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SupportBeanArrayCollMap arrayBean = new SupportBeanArrayCollMap(new int[] {1, 2});
            arrayBean.IntCol = new[] { 1, 2 };
            arrayBean.LongBoxed = 1L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
    
            arrayBean.LongBoxed = 2L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {true, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[] {2, 2});
            arrayBean.IntCol = new[] { 2, 1 };
            arrayBean.LongBoxed = 1L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, true});
    
            arrayBean = new SupportBeanArrayCollMap(new int[] {1, 1});
            arrayBean.IntCol = new[] { 1, 1 };
            arrayBean.LongBoxed = 0L;
            _epService.EPRuntime.SendEvent(arrayBean);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, false});
        }
    
        [Test]
        public void TestEqualsAny()
        {
            String[] fields = "eq,neq,sqlneq,nneq".Split(',');
            String stmtText = "select " +
                              "IntPrimitive = any (1, IntBoxed) as eq, " +
                              "IntPrimitive != any (1, IntBoxed) as neq, " +
                              "IntPrimitive <> any (1, IntBoxed) as sqlneq, " +
                              "not IntPrimitive = any (1, IntBoxed) as nneq " +
                              " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // in the format intPrimitive, intBoxed
            int[][] testdata = {
                    new[] {1, 1},
                    new[] {1, 2},
                    new[] {2, 2},
                    new[] {2, 1},
            };
    
            Object[][] result = {
                    new Object[] {true, false, false, false}, // 1, 1
                    new Object[] {true, true, true, false}, // 1, 2
                    new Object[] {true, true, true, false}, // 2, 2
                    new Object[] {false, true, true, true} // 2, 1
                    };
    
            for (int i = 0; i < testdata.Length; i++)
            {
                SupportBean bean = new SupportBean("E", testdata[i][0]);
                bean.IntBoxed = testdata[i][1];
                _epService.EPRuntime.SendEvent(bean);
                //Console.Out.WriteLine("line " + i);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
        }
    
        [Test]
        public void TestRelationalOpAll()
        {
            String[] fields = "g,ge,l,le".Split(',');
            String stmtText = "select " +
                "IntPrimitive > all (1, 3, 4) as g, " +
                "IntPrimitive >= all (1, 3, 4) as ge, " +
                "IntPrimitive < all (1, 3, 4) as l, " +
                "IntPrimitive <= all (1, 3, 4) as le " +
                " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            Object[][] result = {
                    new Object[] {false, false, true, true},
                    new Object[] {false, false, false, true},
                    new Object[] {false, false, false, false},
                    new Object[] {false, false, false, false},
                    new Object[] {false, true, false, false},
                    new Object[] {true, true, false, false}
                    };
            
            for (int i = 0; i < 6; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", i));
                //Console.Out.WriteLine("line " + i);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
        }
    
        [Test]
        public void TestRelationalOpAny()
        {
            String[] fields = "g,ge,l,le".Split(',');
            String stmtText = "select " +
                "IntPrimitive > any (1, 3, 4) as g, " +
                "IntPrimitive >= some (1, 3, 4) as ge, " +
                "IntPrimitive < any (1, 3, 4) as l, " +
                "IntPrimitive <= some (1, 3, 4) as le " +
                " from SupportBean(TheString like 'E%')";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            Object[][] result = {
                    new Object[] {false, false, true, true},
                    new Object[] {false, true, true, true},
                    new Object[] {true, true, true, true},
                    new Object[] {true, true, true, true},
                    new Object[] {true, true, false, true},
                    new Object[] {true, true, false, false}
                    };
    
            for (int i = 0; i < 6; i++)
            {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", i));
                //Console.Out.WriteLine("line " + i);
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, result[i]);
            }
        }
    
        [Test]
        public void TestEqualsInNullOrNoRows()
        {
            // test fixed array case
            String[] fields = "eall,eany,neall,neany,isin".Split(',');
            String stmtText = "select " +
                "IntBoxed = all ({DoubleBoxed, LongBoxed}) as eall, " +
                "IntBoxed = any ({DoubleBoxed, LongBoxed}) as eany, " +
                "IntBoxed != all ({DoubleBoxed, LongBoxed}) as neall, " +
                "IntBoxed != any ({DoubleBoxed, LongBoxed}) as neany, " +
                "IntBoxed in ({DoubleBoxed, LongBoxed}) as isin " +
                " from SupportBean";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEvent("E3", null, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
            SendEvent("E4", 1, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
    
            SendEvent("E5", null, null, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
            SendEvent("E6", 1, null, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, true, false, null, true});
            SendEvent("E7", 0, null, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, null,  null, true, null});
    
            // test non-array case
            stmt.Dispose();
            fields = "eall,eany,neall,neany,isin".Split(',');
            stmtText = "select " +
                "IntBoxed = all (DoubleBoxed, LongBoxed) as eall, " +
                "IntBoxed = any (DoubleBoxed, LongBoxed) as eany, " +
                "IntBoxed != all (DoubleBoxed, LongBoxed) as neall, " +
                "IntBoxed != any (DoubleBoxed, LongBoxed) as neany, " +
                "IntBoxed in (DoubleBoxed, LongBoxed) as isin " +
                " from SupportBean";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEvent("E3", null, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
            SendEvent("E4", 1, null, null);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
    
            SendEvent("E5", null, null, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, null, null, null, null});
            SendEvent("E6", 1, null, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {null, true, false, null, true});
            SendEvent("E7", 0, null, 1L);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {false, null,  null, true, null});
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                String stmtText = "select intArr = all (1, 2, 3) as r1 from ArrayBean";
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'intArr=all(1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr = all (1, 2, 3) as r1 from ArrayBean]", ex.Message);
            }
    
            try
            {
                String stmtText = "select intArr > all (1, 2, 3) as r1 from ArrayBean";
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Failed to validate select-clause expression 'intArr>all(1,2,3)': Collection or array comparison is not allowed for the IN, ANY, SOME or ALL keywords [select intArr > all (1, 2, 3) as r1 from ArrayBean]", ex.Message);
            }
        }
    
        public void SendEvent(string stringValue, int? intBoxed, double? doubleBoxed, long? longBoxed)
        {
            SupportBean bean = new SupportBean(stringValue, -1);
            bean.IntBoxed = intBoxed;
            bean.DoubleBoxed = doubleBoxed;
            bean.LongBoxed = longBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
