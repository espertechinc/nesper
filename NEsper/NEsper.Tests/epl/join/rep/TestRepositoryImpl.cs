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
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.epl.join;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.rep
{
    [TestFixture]
    public class TestRepositoryImpl
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _s0Event = SupportEventBeanFactory.CreateObject(new Object());
            _repository = new RepositoryImpl(0, _s0Event, 6);
        }

        #endregion

        private EventBean _s0Event;
        private RepositoryImpl _repository;

        private static void TryEnumeratorEmpty(IEnumerator<Cursor> enumerator)
        {
            Assert.IsFalse(enumerator.MoveNext());
        }

        private static Cursor[] Read(IEnumerator<Cursor> enumerator)
        {
            var cursors = new List<Cursor>();
            while (enumerator.MoveNext()) {
                Cursor cursor = enumerator.Current;
                cursors.Add(cursor);
            }
            return cursors.ToArray();
        }

        [Test]
        public void TestAddResult()
        {
            ICollection<EventBean> results = SupportJoinResultNodeFactory.MakeEventSet(2);

            _repository.AddResult(_repository.GetCursors(0).Advance(), results, 1);
            Assert.AreEqual(1, _repository.NodesPerStream[1].Count);

            try {
                _repository.AddResult(_repository.GetCursors(0).Advance(), new HashSet<EventBean>(), 1);
                Assert.Fail();
            }
            catch (ArgumentException) {
                // expected
            }
            try {
                _repository.AddResult(_repository.GetCursors(0).Advance(), null, 1);
                Assert.Fail();
            }
            catch (NullReferenceException) {
                // expected
            }
        }

        [Test]
        public void TestFlow()
        {
            // Lookup from s0
            Cursor[] cursors = Read(_repository.GetCursors(0));
            Assert.AreEqual(1, cursors.Length);

            ICollection<EventBean> resultsS1 = SupportJoinResultNodeFactory.MakeEventSet(2);
            _repository.AddResult(cursors[0], resultsS1, 1);

            // Lookup from s1
            cursors = Read(_repository.GetCursors(1));
            Assert.AreEqual(2, cursors.Length);

            ICollection<EventBean>[] resultsS2 = SupportJoinResultNodeFactory.MakeEventSets(new[] {2, 3});
            _repository.AddResult(cursors[0], resultsS2[0], 2);
            _repository.AddResult(cursors[1], resultsS2[1], 2);

            // Lookup from s2
            cursors = Read(_repository.GetCursors(2));
            Assert.AreEqual(5, cursors.Length); // 2 + 3 for s2

            ICollection<EventBean>[] resultsS3 = SupportJoinResultNodeFactory.MakeEventSets(new[] {2, 1, 3, 5, 1});
            _repository.AddResult(cursors[0], resultsS3[0], 3);
            _repository.AddResult(cursors[1], resultsS3[1], 3);
            _repository.AddResult(cursors[2], resultsS3[2], 3);
            _repository.AddResult(cursors[3], resultsS3[3], 3);
            _repository.AddResult(cursors[4], resultsS3[4], 3);

            // Lookup from s3
            cursors = Read(_repository.GetCursors(3));
            Assert.AreEqual(12, cursors.Length);
        }

        [Test]
        public void TestGetCursors()
        {
            // get cursor for root stream lookup
            IEnumerator<Cursor> enumerator = _repository.GetCursors(0);
            Assert.IsTrue(enumerator.MoveNext());
            Cursor cursor = enumerator.Current;
            Assert.AreSame(_s0Event, cursor.Event);
            Assert.AreEqual(0, cursor.Stream);

            Assert.IsFalse(enumerator.MoveNext());
            TryEnumeratorEmpty(enumerator);

            // try invalid get cursor for no results
            try {
                var enumeration = _repository.GetCursors(2);
                enumeration.MoveNext();
                Assert.Fail();
            }
            catch (NullReferenceException) {
                // expected
            }
        }
    }
}
