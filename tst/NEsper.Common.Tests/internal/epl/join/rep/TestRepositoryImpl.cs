///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.epl.join.rep
{
    [TestFixture]
    public class TestRepositoryImpl : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            s0Event = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean());
            repository = new RepositoryImpl(0, s0Event, 6);
        }

        private EventBean s0Event;
        private RepositoryImpl repository;

        private Cursor[] Read(IEnumerator<Cursor> iterator)
        {
            IList<Cursor> cursors = new List<Cursor>();
            while (iterator.MoveNext()) {
                var cursor = iterator.Current;
                cursors.Add(cursor);
            }

            return cursors.ToArray();
        }

        [Test]
        public void TestAddResult()
        {
            var results = supportJoinResultNodeFactory.MakeEventSet(2);
            var cursors = repository.GetCursors(0);
            Assert.That(cursors.MoveNext(), Is.True);
            repository.AddResult(cursors.Current, results, 1);
            ClassicAssert.AreEqual(1, repository.NodesPerStream[1].Count);

            cursors = repository.GetCursors(0);
            Assert.That(
                () => {
                    cursors.MoveNext();
                    repository.AddResult(cursors.Current, new HashSet<EventBean>(), 1);
                },
                Throws.InstanceOf<ArgumentException>());

            cursors = repository.GetCursors(0);
            Assert.That(
                () => {
                    cursors.MoveNext();
                    repository.AddResult(cursors.Current, null, 1);
                },
                Throws.InstanceOf<NullReferenceException>());
        }

        [Test]
        public void TestFlow()
        {
            // Lookup from s0
            var cursors = Read(repository.GetCursors(0));
            ClassicAssert.AreEqual(1, cursors.Length);

            var resultsS1 = supportJoinResultNodeFactory.MakeEventSet(2);
            repository.AddResult(cursors[0], resultsS1, 1);

            // Lookup from s1
            cursors = Read(repository.GetCursors(1));
            ClassicAssert.AreEqual(2, cursors.Length);

            var resultsS2 = supportJoinResultNodeFactory.MakeEventSets(new[] {2, 3});
            repository.AddResult(cursors[0], resultsS2[0], 2);
            repository.AddResult(cursors[1], resultsS2[1], 2);

            // Lookup from s2
            cursors = Read(repository.GetCursors(2));
            ClassicAssert.AreEqual(5, cursors.Length); // 2 + 3 for s2

            var resultsS3 = supportJoinResultNodeFactory.MakeEventSets(new[] {2, 1, 3, 5, 1});
            repository.AddResult(cursors[0], resultsS3[0], 3);
            repository.AddResult(cursors[1], resultsS3[1], 3);
            repository.AddResult(cursors[2], resultsS3[2], 3);
            repository.AddResult(cursors[3], resultsS3[3], 3);
            repository.AddResult(cursors[4], resultsS3[4], 3);

            // Lookup from s3
            cursors = Read(repository.GetCursors(3));
            ClassicAssert.AreEqual(12, cursors.Length);
        }

        [Test]
        public void TestGetCursors()
        {
            // get cursor for root stream lookup
            var cursors = repository.GetCursors(0);
            ClassicAssert.IsTrue(cursors.MoveNext());
            var cursor = cursors.Current;
            Assert.That(s0Event, Is.SameAs(cursor.TheEvent));
            Assert.That(cursor.Stream, Is.Zero);

            ClassicAssert.IsFalse(cursors.MoveNext());

            // try invalid get cursor for no results
            Assert.That(() => repository.GetCursors(2), Throws.InstanceOf<NullReferenceException>());
        }
    }
} // end of namespace
