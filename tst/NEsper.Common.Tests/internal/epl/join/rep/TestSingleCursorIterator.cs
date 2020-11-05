///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.epl.join.rep
{
    [TestFixture]
    public class TestSingleCursorIterator : AbstractCommonTest
    {
        private IEnumerator<Cursor> filledIterator;
        private IEnumerator<Cursor> emptyIterator;
        private Cursor cursor;

        [SetUp]
        public void SetUp()
        {
            cursor = new MyCursor();
            filledIterator = CreateSingleCursor(cursor);
            emptyIterator = CreateSingleCursor(null);
        }

        [Test]
        public void TestNext()
        {
            Assert.That(filledIterator.MoveNext(), Is.True);
            Assert.AreSame(cursor, filledIterator.Current);

            Assert.That(filledIterator.MoveNext(), Is.False);
            //Assert.That(() => filledIterator.Current, Throws.InstanceOf<NoSuchElementException>());

            Assert.That(emptyIterator.MoveNext(), Is.False);
            //Assert.That(() => emptyIterator.Current, Throws.InstanceOf<NoSuchElementException>());
        }

        [Test]
        public void TestHasNext()
        {
            Assert.That(filledIterator.MoveNext(), Is.True);
            Assert.That(() => filledIterator.Current, Throws.Nothing);
            Assert.That(filledIterator.MoveNext(), Is.False);
        }

        private IEnumerator<Cursor> CreateSingleCursor(Cursor cursor)
        {
            if (cursor != null)
            {
                yield return cursor;
            }
        }

        internal class MyCursor : Cursor
        {
            public MyCursor() : base(null, 0, null)
            {
            }
        }
    }
} // end of namespace
