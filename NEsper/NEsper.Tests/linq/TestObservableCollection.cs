///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.util;
using NUnit.Framework;

namespace com.espertech.esper.linq
{
    [TestFixture]
    public class TestObservableCollection
    {
        private EPServiceProvider _serviceProvider;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            var configuration = new Configuration(_container);
            configuration.AddEventType<MiniBean>();

            _serviceProvider = EPServiceProviderManager.GetDefaultProvider(configuration);
            _serviceProvider.Initialize();
        }

        [Test]
        public void TestSimpleObservableCollection()
        {
            using (var statement = _serviceProvider.EPAdministrator.CreateEPL("@IterableUnbound select * from MiniBean"))
            {
                var observableCollection = statement.AsObservableCollection<MiniBean>();
                Assert.IsNotNull(observableCollection);

                var eventA = SendMiniBean("A", 1);

                Assert.AreEqual(observableCollection.Count, 1);
                Assert.AreEqual(observableCollection[0], eventA);
                Assert.AreSame(observableCollection[0], eventA);
            }

            using (var statement = _serviceProvider.EPAdministrator.CreateEPL("@IterableUnbound select * from MiniBean#length(10)"))
            {
                var observableCollectionA = statement.AsObservableCollection<MiniBean>();
                var observableCollectionB = statement.AsObservableCollection<SupportBean>();

                Assert.IsNotNull(observableCollectionA);
                Assert.IsNotNull(observableCollectionB);

                for (var ii = 0; ii < 100; ii++)
                {
                    SendMiniBean("A", ii);
                }

                SendMiniBean("A", 100);

                Assert.AreEqual(10, observableCollectionA.Count);
                Assert.IsInstanceOf(typeof(MiniBean), observableCollectionA[0]);
                Assert.AreEqual(observableCollectionA[0].IntPrimitive, 91);
                Assert.AreEqual(observableCollectionA[1].IntPrimitive, 92);
                Assert.AreEqual(observableCollectionA[2].IntPrimitive, 93);
                Assert.AreEqual(observableCollectionA[3].IntPrimitive, 94);
                Assert.AreEqual(observableCollectionA[4].IntPrimitive, 95);
                Assert.AreEqual(observableCollectionA[5].IntPrimitive, 96);
                Assert.AreEqual(observableCollectionA[6].IntPrimitive, 97);
                Assert.AreEqual(observableCollectionA[7].IntPrimitive, 98);
                Assert.AreEqual(observableCollectionA[8].IntPrimitive, 99);
                Assert.AreEqual(observableCollectionA[9].IntPrimitive, 100);

                Assert.AreEqual(10, observableCollectionB.Count);
                Assert.IsInstanceOf(typeof(SupportBean), observableCollectionB[0]);
                Assert.AreEqual(observableCollectionB[0].IntPrimitive, 91);
                Assert.AreEqual(observableCollectionB[1].IntPrimitive, 92);
                Assert.AreEqual(observableCollectionB[2].IntPrimitive, 93);
                Assert.AreEqual(observableCollectionB[3].IntPrimitive, 94);
                Assert.AreEqual(observableCollectionB[4].IntPrimitive, 95);
                Assert.AreEqual(observableCollectionB[5].IntPrimitive, 96);
                Assert.AreEqual(observableCollectionB[6].IntPrimitive, 97);
                Assert.AreEqual(observableCollectionB[7].IntPrimitive, 98);
                Assert.AreEqual(observableCollectionB[8].IntPrimitive, 99);
                Assert.AreEqual(observableCollectionB[9].IntPrimitive, 100);
            }
        }

        [Test]
        public void TestMappedObservableCollection()
        {
            var pseudoEventType = new Dictionary<string, object>();
            pseudoEventType["TheString"] = typeof(string);
            pseudoEventType["IntPrimitive"] = typeof(int);

            _serviceProvider.EPAdministrator.Configuration.AddEventType("MapBean", pseudoEventType);

            using (var statement = _serviceProvider.EPAdministrator.CreateEPL("@IterableUnbound select * from MapBean"))
            {
                var observableCollection = statement.AsObservableCollection<MiniBean>();
                Assert.IsNotNull(observableCollection);

                SendMapBean("A", 1);
                Assert.AreEqual(observableCollection.Count, 1);
                Assert.IsInstanceOf(typeof(MiniBean), observableCollection[0]);
                Assert.AreEqual(observableCollection[0].IntPrimitive, 1);
                Assert.AreEqual(observableCollection[0].TheString, "A");

                SendMapBean("B", 2);
                Assert.AreEqual(1, observableCollection.Count);
                Assert.IsInstanceOf(typeof(MiniBean), observableCollection[0]);
                Assert.AreEqual(observableCollection[0].IntPrimitive, 2);
                Assert.AreEqual(observableCollection[0].TheString, "B");
            }

            using (var statement = _serviceProvider.EPAdministrator.CreateEPL("@IterableUnbound select * from MapBean#length(10)"))
            {
                var observableCollectionA = statement.AsObservableCollection<MiniBean>();
                var observableCollectionB = statement.AsObservableCollection<SupportBean>();

                Assert.IsNotNull(observableCollectionA);
                Assert.IsNotNull(observableCollectionB);

                for (var ii = 0; ii < 100; ii++)
                {
                    SendMapBean("A", ii);
                }

                SendMapBean("A", 100);

                Assert.AreEqual(10, observableCollectionA.Count);
                Assert.IsInstanceOf(typeof(MiniBean), observableCollectionA[0]);
                Assert.AreEqual(observableCollectionA[0].IntPrimitive, 91);
                Assert.AreEqual(observableCollectionA[1].IntPrimitive, 92);
                Assert.AreEqual(observableCollectionA[2].IntPrimitive, 93);
                Assert.AreEqual(observableCollectionA[3].IntPrimitive, 94);
                Assert.AreEqual(observableCollectionA[4].IntPrimitive, 95);
                Assert.AreEqual(observableCollectionA[5].IntPrimitive, 96);
                Assert.AreEqual(observableCollectionA[6].IntPrimitive, 97);
                Assert.AreEqual(observableCollectionA[7].IntPrimitive, 98);
                Assert.AreEqual(observableCollectionA[8].IntPrimitive, 99);
                Assert.AreEqual(observableCollectionA[9].IntPrimitive, 100);

                Assert.AreEqual(10, observableCollectionB.Count);
                Assert.IsInstanceOf(typeof(SupportBean), observableCollectionB[0]);
                Assert.AreEqual(observableCollectionB[0].IntPrimitive, 91);
                Assert.AreEqual(observableCollectionB[1].IntPrimitive, 92);
                Assert.AreEqual(observableCollectionB[2].IntPrimitive, 93);
                Assert.AreEqual(observableCollectionB[3].IntPrimitive, 94);
                Assert.AreEqual(observableCollectionB[4].IntPrimitive, 95);
                Assert.AreEqual(observableCollectionB[5].IntPrimitive, 96);
                Assert.AreEqual(observableCollectionB[6].IntPrimitive, 97);
                Assert.AreEqual(observableCollectionB[7].IntPrimitive, 98);
                Assert.AreEqual(observableCollectionB[8].IntPrimitive, 99);
                Assert.AreEqual(observableCollectionB[9].IntPrimitive, 100);
            }
        }

        public MiniBean SendMiniBean(string str, int iPrimitive)
        {
            var evObject = new MiniBean(str, iPrimitive);
            _serviceProvider.EPRuntime.SendEvent(evObject);
            return evObject;
        }

        public IDictionary<string, object> SendMapBean(string str, int iPrimitive)
        {
            var evTable = new Dictionary<string, object>();
            evTable["TheString"] = str;
            evTable["IntPrimitive"] = iPrimitive;
            _serviceProvider.EPRuntime.SendEvent(evTable, "MapBean");
            return evTable;
        }

        public class MiniBean
        {
            public int IntPrimitive { get; set; }
            public string TheString { get; set; }

            public MiniBean()
            {
            }

            public MiniBean(string stringValue, int intPrimitive)
            {
                IntPrimitive = intPrimitive;
                TheString = stringValue;
            }

            public bool Equals(MiniBean other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return other.IntPrimitive == IntPrimitive && Equals(other.TheString, TheString);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(MiniBean)) return false;
                return Equals((MiniBean)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (IntPrimitive * 397) ^ (TheString != null ? TheString.GetHashCode() : 0);
                }
            }

            public override string ToString()
            {
                return string.Format("IntPrimitive: {0}, String: {1}", IntPrimitive, TheString);
            }
        }

    }
}
