///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    ///     View for use with pre-processing statement such as "Update istream" for indicating previous and current event.
    /// </summary>
    public class InternalRoutePreprocessView : ViewSupport
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly EventType _eventType;

        /// <summary>Ctor. </summary>
        /// <param name="eventType">the type of event to indicator</param>
        /// <param name="statementResultService">determines whether listeners or subscribers are attached.</param>
        public InternalRoutePreprocessView(
            EventType eventType,
            StatementResultService statementResultService)
        {
            _eventType = eventType;
            StatementResultService = statementResultService;
        }

        public override EventType EventType => _eventType;

        /// <summary>Returns true if a subscriber or listener is attached. </summary>
        /// <value>indicator</value>
        public bool IsIndicate => StatementResultService.IsMakeNatural || StatementResultService.IsMakeSynthetic;

        public StatementResultService StatementResultService { get; }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".Update Received Update, " +
                    "  newData.Length==" + (newData == null ? 0 : newData.Length) +
                    "  oldData.Length==" + (oldData == null ? 0 : oldData.Length));
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return CollectionUtil.NULL_EVENT_ITERATOR;
        }

        /// <summary>Indicate an modified event and its previous version. </summary>
        /// <param name="newEvent">modified event</param>
        /// <param name="oldEvent">previous version event</param>
        public void Indicate(
            EventBean newEvent,
            EventBean oldEvent)
        {
            try {
                if (StatementResultService.IsMakeNatural) {
                    var natural = new NaturalEventBean(_eventType, new[] {newEvent.Underlying}, newEvent);
                    var naturalOld = new NaturalEventBean(_eventType, new[] {oldEvent.Underlying}, oldEvent);
                    child.Update(new[] {natural}, new[] {naturalOld});
                }
                else {
                    child.Update(new[] {newEvent}, new[] {oldEvent});
                }
            }
            catch (Exception ex) {
                Log.Error("Unexpected error updating child view: " + ex.Message);
            }
        }
    }
}