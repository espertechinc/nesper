///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.runtime.client.linq
{
    public static class StdViewExtensions
    {
        /// <summary>
        /// Generates a view that includes only the most recent among events having the same value for
        /// the result of the sepcified list of properties.
        /// <para />
        /// This view acts as a length window of size 1 for each distinct value returned by a property, or combination
        /// of values returned by multiple properties.  It thus posts as old events the prior event of the same value(s),
        /// if any.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The property or expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> Unique<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentException("at least one property must be provided");
            }

            return esperQuery
                .FilterView(() => View
                    .Create("unique", properties
                        .Select(p => new PropertyValueExpression(p))
                        .Cast<Expression>()
                        .ToArray()));
        }

        /// <summary>
        /// Generates a view that includes only the most recent among events having the same value for
        /// the result of the sepcified list of properties.
        /// <para />
        /// This view acts as a length window of size 1 for each distinct value returned by a property, or combination
        /// of values returned by multiple properties.  It thus posts as old events the prior event of the same value(s),
        /// if any.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="expressions">The expressions.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">at least one property must be provided</exception>
        public static EsperQuery<T> Unique<T>(this EsperQuery<T> esperQuery, params System.Linq.Expressions.Expression<Func<T, object>>[] expressions)
        {
            if (expressions == null)
            {
                throw new ArgumentException("at least one property must be provided");
            }

            return esperQuery.FilterView(() => View.Create("unique",
                expressions.Select(e => LinqToSoda.LinqToSodaExpression(e)).ToArray()));
        }

        /// <summary>
        /// Generates a view that groups events into sub-views by the value returned by the
        /// combination of values returned by a list of properties.
        /// <para/>
        /// The properties return one or more group keys, by which the view creates sub-views for each distinct group key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The property or expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> GroupData<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentException("at least one property must be provided");
            }

            return esperQuery.FilterView(() => View.Create("group",
                properties.Select(p => new PropertyValueExpression(p)).Cast<Expression>().ToArray()));
        }

        /// <summary>
        /// Generates a view that counts the number of events received from a stream or view plus
        /// any additional event properties or expression values listed as parameters.  The count is
        /// then output to the observer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <returns></returns>
        public static EsperQuery<T> Counting<T>(this EsperQuery<T> esperQuery)
        {
            return esperQuery.FilterView(() => View.Create("size"));
        }

        /// <summary>
        /// Generates a view that retains only the most recent arriving event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <returns></returns>
        public static EsperQuery<T> Last<T>(this EsperQuery<T> esperQuery)
        {
            return esperQuery.FilterView(() => View.Create("lastevent"));
        }

        /// <summary>
        /// Generates a view that retains only the first arriving event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <returns></returns>
        public static EsperQuery<T> First<T>(this EsperQuery<T> esperQuery)
        {
            return esperQuery.FilterView(() => View.Create("firstevent"));
        }

        /// <summary>
        /// Generates a view that retains only the first among events having the same value for the specified
        /// properties.  If used within a named window and an on-delete clause deletes the event, the view
        /// resets and will retain the next arriving event for the expression result values of the deleted events.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">at least one property must be provided</exception>
        public static EsperQuery<T> FirstUnique<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentException("at least one property must be provided");
            }

            return esperQuery.FilterView(() => View.Create("firstunique",
                properties.Select(p => new PropertyValueExpression(p)).Cast<Expression>().ToArray()));
        }
    }
}