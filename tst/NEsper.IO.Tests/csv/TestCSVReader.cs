///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esperio.support.util;

using NUnit.Framework;

namespace com.espertech.esperio.csv
{
    [TestFixture]
    public class TestCSVReader
    {
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();
        }

        [Test]
        public void TestClose()
        {
            var path = "regression/parseTests.csv";
            var reader = new CSVReader(new AdapterInputSource(_container, path));

            reader.Close();
            try {
                reader.GetNextRecord();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
            try {
                reader.Close();
                Assert.Fail();
            }
            catch (EPException) {
                // Expected
            }
        }

        [Test]
        public void TestLooping()
        {
            AssertLooping("regression/endOnNewline.csv");
            AssertLooping("regression/endOnEOF.csv");
            AssertLooping("regression/endOnCommentedEOF.csv");
        }

        [Test]
        public void TestNonLooping()
        {
            AssertNonLooping("regression/endOnNewline.csv");
            AssertNonLooping("regression/endOnEOF.csv");
            AssertNonLooping("regression/endOnCommentedEOF.csv");
        }

        [Test]
        public void TestParsing()
        {
            var path = "regression/parseTests.csv";
            var reader = new CSVReader(new AdapterInputSource(_container, path));

            var nextRecord = reader.GetNextRecord();
            var expected = new[] {"8", "8.0", "c", "'c'", "string", "string"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"", "string", "", "string", "", "", ""};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"leading spaces", "trailing spaces", ""};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"unquoted value 1", "unquoted value 2", ""};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"value with embedded \"\" quotes"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {string.Format("value{0}with newline", Environment.NewLine)};//"\n")};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"value after empty lines"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"value after comments"};
            Assert.AreEqual(expected, nextRecord);

            try {
                reader.GetNextRecord();
                Assert.Fail();
            }
            catch (EndOfStreamException) {
                // Expected
            }
        }

        [Test]
        public void TestReset()
        {
            var reader = new CSVReader(new AdapterInputSource(_container, "regression/endOnNewline.csv"));

            var nextRecord = reader.GetNextRecord();
            var expected = new[] {"first line", "1"};
            Assert.AreEqual(expected, nextRecord);

            reader.Reset();

            nextRecord = reader.GetNextRecord();
            Assert.AreEqual(expected, nextRecord);

            reader.Reset();

            nextRecord = reader.GetNextRecord();
            Assert.AreEqual(expected, nextRecord);
        }

        [Test]
        public void TestTitleRow()
        {
            var reader = new CSVReader(new AdapterInputSource(_container, "regression/titleRow.csv"));
            reader.Looping = true;

            // isUsingTitleRow is false by default, so get the title row
            var nextRecord = reader.GetNextRecord();
            var expected = new[] {"MyString", "MyInt", "timestamp", "MyDouble"};
            Assert.AreEqual(expected, nextRecord);

            // Acknowledge the title row and reset the file afterwards
            reader.IsUsingTitleRow = true;
            reader.Reset();

            // First time through the file
            nextRecord = reader.GetNextRecord();
            expected = new[] {"one", "1", "100", "1.1"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"three", "3", "300", "3.3"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"five", "5", "500", "5.5"};
            Assert.AreEqual(expected, nextRecord);

            // Second time through the file
            nextRecord = reader.GetNextRecord();
            expected = new[] {"one", "1", "100", "1.1"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"three", "3", "300", "3.3"};
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] {"five", "5", "500", "5.5"};
            Assert.AreEqual(expected, nextRecord);

            // Pretend no title row again
            reader.IsUsingTitleRow = false;

            nextRecord = reader.GetNextRecord();
            expected = new[] {"MyString", "MyInt", "timestamp", "MyDouble"};
            Assert.AreEqual(expected, nextRecord);

            reader.Reset();

            nextRecord = reader.GetNextRecord();
            expected = new[] {"MyString", "MyInt", "timestamp", "MyDouble"};
            Assert.AreEqual(expected, nextRecord);
        }

        [Test]
        public void TestNestedProperties()
        {
            var container = ContainerExtensions.CreateDefaultContainer();

            var configuration = new Configuration(container);
            configuration.Common.AddEventType(typeof(Figure));

            var runtimeProvider = new EPRuntimeProvider();
            var runtime = runtimeProvider.GetRuntimeInstance("testNestedProperties", configuration);
            var ul = new SupportUpdateListener();

            var stmt = CompileUtil.CompileDeploy(runtime, "select * from Figure").Statements[0];
            stmt.Events += ul.Update;

            var source = new AdapterInputSource(_container, "regression/nestedProperties.csv");
            var spec = new CSVInputAdapterSpec(source, "Figure");
            var adapter = new CSVInputAdapter(runtime, spec);
            adapter.Start();

            Assert.IsTrue(ul.IsInvoked);
            var e = ul.AssertOneGetNewAndReset();
            var f = (Figure) e.Underlying;
            Assert.AreEqual(1, f.Point.X);
        }

        [Test]
        public void TestNestedMapProperties()
        {
            var configuration = new Configuration(_container);
            var point = new Dictionary<string, object>();
            point.Put("X", typeof (int));
            point.Put("Y", typeof (int));

            var figure = new Dictionary<string, object>();
            figure.Put("Name", typeof (string));
            figure.Put("Point", point);

            configuration.Common.AddEventType("Figure", figure);
            
            var runtimeProvider = new EPRuntimeProvider();
            var runtime = runtimeProvider.GetRuntimeInstance("testNestedMapProperties", configuration);
            var ul = new SupportUpdateListener();
            var stmt = CompileUtil.CompileDeploy(runtime, "select * from Figure").Statements[0];
            stmt.Events += ul.Update;

            var source = new AdapterInputSource(_container, "regression/nestedProperties.csv");
            var spec = new CSVInputAdapterSpec(source, "Figure");
            var adapter = new CSVInputAdapter(runtime, spec);
            adapter.Start();

            Assert.IsTrue(ul.IsInvoked);
            var e = ul.AssertOneGetNewAndReset();
            Assert.AreEqual(1, e.Get("Point.X"));
        }

        private void AssertLooping(String path)
        {
            var reader = new CSVReader(new AdapterInputSource(_container, path));
            reader.Looping = true;

            var nextRecord = reader.GetNextRecord();
            var expected = new[] { "first line", "1" };
            Assert.AreEqual(expected, nextRecord);

            Assert.IsTrue(reader.GetAndClearIsReset());

            nextRecord = reader.GetNextRecord();
            expected = new[] { "second line", "2" };
            Assert.AreEqual(expected, nextRecord);

            Assert.IsFalse(reader.GetAndClearIsReset());

            nextRecord = reader.GetNextRecord();
            expected = new[] { "first line", "1" };
            Assert.AreEqual(expected, nextRecord);

            Assert.IsTrue(reader.GetAndClearIsReset());

            nextRecord = reader.GetNextRecord();
            expected = new[] { "second line", "2" };
            Assert.AreEqual(expected, nextRecord);

            Assert.IsFalse(reader.GetAndClearIsReset());

            nextRecord = reader.GetNextRecord();
            expected = new[] { "first line", "1" };
            Assert.AreEqual(expected, nextRecord);

            Assert.IsTrue(reader.GetAndClearIsReset());

            reader.Close();
        }

        private void AssertNonLooping(String path)
        {
            var reader = new CSVReader(new AdapterInputSource(_container, path));

            var nextRecord = reader.GetNextRecord();
            var expected = new[] { "first line", "1" };
            Assert.AreEqual(expected, nextRecord);

            nextRecord = reader.GetNextRecord();
            expected = new[] { "second line", "2" };
            Assert.AreEqual(expected, nextRecord);

            try
            {
                reader.GetNextRecord();
                Assert.Fail();
            }
            catch (EndOfStreamException)
            {
                // Expected
            }

            reader.Close();
        }
    }

    public class Figure
    {
        public string Name { get; set; }
        public Point Point { get; set; }
	}

	public class Point {
        public int X { get; set; }
        public int Y { get; set; }
    }
}