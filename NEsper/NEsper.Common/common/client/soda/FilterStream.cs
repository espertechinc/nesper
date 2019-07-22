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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// A stream upon which projections (views) can be added that selects events by name and filter expression.
    /// </summary>
    [Serializable]
    public class FilterStream : ProjectedStream
    {
        private Filter filter;

        /// <summary>
        /// Ctor.
        /// </summary>
        public FilterStream()
        {
        }

        /// <summary>
        /// Creates a stream using a filter that provides the event type name and filter expression to filter for.
        /// </summary>
        /// <param name="filter">defines what to look for</param>
        /// <returns>stream</returns>
        public static FilterStream Create(Filter filter)
        {
            return new FilterStream(filter);
        }

        /// <summary>
        /// Creates a stream of events of the given name.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <returns>stream</returns>
        public static FilterStream Create(string eventTypeName)
        {
            return new FilterStream(Filter.Create(eventTypeName));
        }

        /// <summary>
        /// Creates a stream of events of the given event type name and names that stream. Example: "select * from MyeventTypeName as StreamName".
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <param name="streamName">is an optional stream name</param>
        /// <returns>stream</returns>
        public static FilterStream Create(
            string eventTypeName,
            string streamName)
        {
            return new FilterStream(Filter.Create(eventTypeName), streamName);
        }

        /// <summary>
        /// Creates a stream using a filter that provides the event type name and filter expression to filter for.
        /// </summary>
        /// <param name="filter">defines what to look for</param>
        /// <param name="streamName">is an optional stream name</param>
        /// <returns>stream</returns>
        public static FilterStream Create(
            Filter filter,
            string streamName)
        {
            return new FilterStream(filter, streamName);
        }

        /// <summary>
        /// Creates a stream of events of the given event type name and names that stream. Example: "select * from MyeventTypeName as StreamName".
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <param name="filter">is the filter expression removing events from the stream</param>
        /// <returns>stream</returns>
        public static FilterStream Create(
            string eventTypeName,
            Expression filter)
        {
            return new FilterStream(Filter.Create(eventTypeName, filter));
        }

        /// <summary>
        /// Creates a stream of events of the given event type name and names that stream. Example: "select * from MyeventTypeName as StreamName".
        /// </summary>
        /// <param name="eventTypeName">is the event type name to filter for</param>
        /// <param name="filter">is the filter expression removing events from the stream</param>
        /// <param name="streamName">is an optional stream name</param>
        /// <returns>stream</returns>
        public static FilterStream Create(
            string eventTypeName,
            string streamName,
            Expression filter)
        {
            return new FilterStream(Filter.Create(eventTypeName, filter), streamName);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filter">specifies what events to look for</param>
        public FilterStream(Filter filter)
            : base(new List<View>(), null)
        {
            this.filter = filter;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filter">specifies what events to look for</param>
        /// <param name="name">is the as-name for the stream</param>
        public FilterStream(
            Filter filter,
            string name)
            : base(new List<View>(), name)
        {
            this.filter = filter;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filter">specifies what events to look for</param>
        /// <param name="name">is the as-name for the stream</param>
        /// <param name="views">is a list of projections onto the stream</param>
        public FilterStream(
            Filter filter,
            string name,
            IList<View> views)
            : base(views, name)
        {
            this.filter = filter;
        }

        /// <summary>
        /// Returns the filter.
        /// </summary>
        /// <returns>filter</returns>
        public Filter Filter {
            get => filter;
            set => filter = value;
        }

        public override void ToEPLProjectedStream(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            filter.ToEPL(writer, formatter);
        }

        public override void ToEPLProjectedStreamType(TextWriter writer)
        {
            writer.Write(filter.EventTypeName);
        }
    }
} // end of namespace