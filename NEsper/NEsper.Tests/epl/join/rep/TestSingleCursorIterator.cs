///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.epl.join.rep
{
    [TestFixture]
	public class TestSingleCursorIterator
    {
	    private IEnumerator<Cursor> _filledIterator;
        private IEnumerator<Cursor> _emptyIterator;
	    private Cursor _cursor;

        [SetUp]
	    public void SetUp()
        {
	        _cursor = new MyCursor();
	        _filledIterator = CreateSingleCursor(_cursor);
	        _emptyIterator = CreateSingleCursor(null);
	    }

        [Test]
	    public void TestCurrent()
        {
            Assert.IsTrue(_filledIterator.MoveNext());
            Assert.AreSame(_cursor, _filledIterator.Current);
            Assert.IsFalse(_filledIterator.MoveNext());
            Assert.IsFalse(_emptyIterator.MoveNext());
	    }

        [Test]
	    public void TestMoveNext() {
	        Assert.IsTrue(_filledIterator.MoveNext());
            Assert.NotNull(_filledIterator.Current);
            Assert.IsFalse(_filledIterator.MoveNext());

            Assert.IsFalse(_emptyIterator.MoveNext());
	    }

#if false
        [Test]
	    public void TestRemove() {
	        try {
	            _filledIterator.Remove();
	            Assert.IsTrue(false);
	        } catch (UnsupportedOperationException ex) {
	            // Expected exception
	        }
	    }

	    private Cursor MakeAnonymousCursor() {
	        return new ProxyCursor(null, 0, null) {

	            ProcGetLookupEvent = () =>  {
	                return null;
	            },

	            ProcGetLookupStream = () =>  {
	                return 0;
	            },

	            ProcGetIndexedStream = () =>  {
	                return 0;
	            },
	        };
	    }
#endif

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
