///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    /// <summary>
    ///     An event table for holding multiple tables for use when multiple indexes of the same dataset must be entered into a
    ///     cache
    ///     for use in historical data lookup.
    ///     <para />
    ///     Does not allow iteration, adding and removing events. Does allow clearing all tables and asking for
    ///     filled or empty tables. All tables are expected to be filled and empty at the same time,
    ///     reflecting multiple indexes on a single set of data.
    /// </summary>
    public class MultiIndexEventTable : EventTable
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="tables">tables to hold</param>
        /// <param name="organization">organization</param>
        public MultiIndexEventTable(
            EventTable[] tables,
            EventTableOrganization organization)
        {
            Tables = tables;
            Organization = organization;
        }

        /// <summary>
        ///     Returns all tables.
        /// </summary>
        /// <returns>tables</returns>
        public EventTable[] Tables { get; }

        public void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException();
        }

        public bool IsEmpty => Tables[0].IsEmpty;

        public void Clear()
        {
            for (var i = 0; i < Tables.Length; i++) {
                Tables[i].Clear();
            }
        }

        public void Destroy()
        {
            Clear();
        }

        public string ToQueryPlan()
        {
            var buf = new StringWriter();
            var delimiter = "";
            foreach (var table in Tables) {
                buf.Write(delimiter);
                buf.Write(table.ToQueryPlan());
                delimiter = ", ";
            }

            return GetType().GetSimpleName() + " " + buf;
        }

        public int? NumberOfEvents {
            get {
                foreach (var table in Tables) {
                    var num = table.NumberOfEvents;
                    if (num != null) {
                        return num;
                    }
                }

                return null;
            }
        }

        public int NumKeys => Tables[0].NumKeys;

        public object Index {
            get {
                var indexes = new object[Tables.Length];
                for (var i = 0; i < indexes.Length; i++) {
                    indexes[i] = Tables[i].Index;
                }

                return indexes;
            }
        }

        public EventTableOrganization Organization { get; }

        public Type ProviderClass => typeof(MultiIndexEventTable);
    }
} // end of namespace