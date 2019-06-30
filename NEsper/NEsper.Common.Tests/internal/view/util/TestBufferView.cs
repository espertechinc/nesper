///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.view.util
{
    [TestFixture]
    public class TestBufferView : CommonTest
    {
        [SetUp]
        public void SetUp()
        {
            observer = new SupportBufferObserver();
            bufferView = new BufferView(1);
            bufferView.Observer = observer;
        }

        private BufferView bufferView;
        private SupportBufferObserver observer;

        private EventBean[] MakeBeans(
            string id,
            int numTrades)
        {
            var trades = new EventBean[numTrades];
            for (var i = 0; i < numTrades; i++)
            {
                var bean = new SupportBean_A(id + i);
                trades[i] = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
            }

            return trades;
        }

        public class SupportBufferObserver : BufferObserver
        {
            private bool hasNewData;
            private FlushedEventBuffer newEventBuffer;
            private FlushedEventBuffer oldEventBuffer;
            private int streamId;

            public void NewData(
                int streamId,
                FlushedEventBuffer newEventBuffer,
                FlushedEventBuffer oldEventBuffer)
            {
                if (hasNewData)
                {
                    throw new IllegalStateException("Observer already has new data");
                }

                hasNewData = true;
                this.streamId = streamId;
                this.newEventBuffer = newEventBuffer;
                this.oldEventBuffer = oldEventBuffer;
            }

            public bool GetAndResetHasNewData()
            {
                var result = hasNewData;
                hasNewData = false;
                return result;
            }

            public int GetAndResetStreamId()
            {
                var id = streamId;
                streamId = 0;
                return id;
            }

            public FlushedEventBuffer GetAndResetNewEventBuffer()
            {
                var buf = newEventBuffer;
                newEventBuffer = null;
                return buf;
            }

            public FlushedEventBuffer GetAndResetOldEventBuffer()
            {
                var buf = oldEventBuffer;
                oldEventBuffer = null;
                return buf;
            }
        }

        [Test]
        public void TestUpdate()
        {
            // Observer starts with no data
            Assert.IsFalse(observer.GetAndResetHasNewData());

            // Send some data
            var newEvents = MakeBeans("n", 1);
            var oldEvents = MakeBeans("o", 1);
            bufferView.Update(newEvents, oldEvents);

            // make sure received
            Assert.IsTrue(observer.GetAndResetHasNewData());
            Assert.AreEqual(1, observer.GetAndResetStreamId());
            Assert.IsNotNull(observer.GetAndResetNewEventBuffer());
            Assert.IsNotNull(observer.GetAndResetOldEventBuffer());

            // Reset and send null data
            Assert.IsFalse(observer.GetAndResetHasNewData());
            bufferView.Update(null, null);
            Assert.IsTrue(observer.GetAndResetHasNewData());
        }
    }
} // end of namespace