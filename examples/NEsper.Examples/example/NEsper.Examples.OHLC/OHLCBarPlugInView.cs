using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.OHLC
{
    public class OHLCBarPlugInView : ViewSupport
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OHLCBarPlugInView));

        private const int LateEventSlackSeconds = 5;

        private readonly OHLCBarPlugInViewFactory _factory;
        private readonly AgentInstanceViewFactoryChainContext _agentInstanceViewFactoryContext;
        private readonly long _scheduleSlot;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];

        private EPStatementHandleCallbackSchedule _handle;
        private long? _cutoffTimestampMinute;
        private long? _currentTimestampMinute;
        private double? _first;
        private double? _last;
        private double? _max;
        private double? _min;
        private EventBean _lastEvent;

        public OHLCBarPlugInView(
            OHLCBarPlugInViewFactory factory,
            AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            this._factory = factory;
            this._agentInstanceViewFactoryContext = agentInstanceViewFactoryContext;
            this._scheduleSlot = agentInstanceViewFactoryContext.StatementContext.ScheduleBucket.AllocateSlot();
        }

        public override EventType EventType => _factory.EventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData == null) {
                return;
            }

            foreach (var theEvent in newData) {
                _eventsPerStream[0] = theEvent;
                var timestamp = _factory.TimestampExpression.Evaluate(_eventsPerStream, true, _agentInstanceViewFactoryContext).AsInt64();
                var timestampMinute = RemoveSeconds(timestamp);
                var value = _factory.ValueExpression.Evaluate(_eventsPerStream, true, _agentInstanceViewFactoryContext).AsDouble();

                // test if this minute has already been published, the event is too late
                if ((_cutoffTimestampMinute != null) && (timestampMinute <= _cutoffTimestampMinute)) {
                    continue;
                }

                // if the same minute, aggregate
                if (timestampMinute.Equals(_currentTimestampMinute)) {
                    ApplyValue(value);
                } else {
                    // first time we see an event for this minute
                    // there is data to post
                    if (_currentTimestampMinute != null) {
                        PostData();
                    }

                    _currentTimestampMinute = timestampMinute;
                    ApplyValue(value);

                    // schedule a callback to fire in case no more events arrive
                    ScheduleCallback();
                }
            }
        } 

        private void ApplyValue(double value)
        {
            if (_first == null) {
                _first = value;
            }

            _last = value;
            if (_min == null) {
                _min = value;
            }
            else if (_min.CompareTo(value) > 0) {
                _min = value;
            }

            if (_max == null) {
                _max = value;
            }
            else if (_max.CompareTo(value) < 0) {
                _max = value;
            }
        }

        private static long RemoveSeconds(long timestamp)
        {
            var dtx = DateTimeEx.UtcInstance(timestamp);
            dtx.SetSecond(0);
            dtx.SetMillis(0);
            return dtx.UtcMillis;
        }

        private void ScheduleCallback()
        {
            if (_handle != null) {
                // remove old schedule
                _agentInstanceViewFactoryContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                _handle = null;
            }

            var currentTime = _agentInstanceViewFactoryContext.StatementContext.SchedulingService.Time;
            var currentRemoveSeconds = RemoveSeconds(currentTime);
            var targetTime = currentRemoveSeconds + (60 + LateEventSlackSeconds) * 1000; // leave some seconds for late comers
            var scheduleAfterMSec = targetTime - currentTime;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback(
                () => {
                    _handle = null; // clear out schedule handle
                    PostData();
                });

            _handle = new EPStatementHandleCallbackSchedule(_agentInstanceViewFactoryContext.EpStatementAgentInstanceHandle, callback);
            _agentInstanceViewFactoryContext.StatementContext.SchedulingService.Add(scheduleAfterMSec, _handle, _scheduleSlot);
        }

        private void PostData()
        {
            var barValue = new OHLCBarValue(_currentTimestampMinute.Value, _first, _last, _max, _min);
            var outgoing = _agentInstanceViewFactoryContext.StatementContext.EventBeanTypedEventFactory.AdapterForTypedObject(barValue, _factory.EventType);
            if (_lastEvent == null) {
                child.Update(new EventBean[] {outgoing}, null);
            }
            else {
                child.Update(new EventBean[] {outgoing}, new EventBean[] {_lastEvent});
            }

            _lastEvent = outgoing;

            _cutoffTimestampMinute = _currentTimestampMinute;
            _first = null;
            _last = null;
            _max = null;
            _min = null;
            _currentTimestampMinute = null;
        }
    }
}
