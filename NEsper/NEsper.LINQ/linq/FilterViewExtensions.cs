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
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.client.linq
{
    public static class FilterViewExtensions
    {
        /// <summary>
        /// Boilerplate for creating views on filter streams.  Make your own if you'd like.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="viewFactory">The viewFactory.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// no stream available to use for window
        /// or
        /// no stream available to use for window
        /// </exception>
        public static EsperQuery<T> FilterView<T>(this EsperQuery<T> esperQuery, Func<View> viewFactory)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            var deriveFromClause = deriveObjectModel.FromClause;

            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var streams = deriveFromClause.Streams;
                if (streams == null)
                {
                    throw new ArgumentException("no stream available to use for window");
                }

                var filter = streams.OfType<FilterStream>().Last();
                if (filter == null)
                {
                    throw new ArgumentException("no stream available to use for window");
                }

                filter.AddView(viewFactory.Invoke());
            }

            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Boilerplate for creating views on filter streams.  Make your own if you'd like.
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="viewFactory">The viewFactory.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">no stream available to use for window
        /// or
        /// no stream available to use for window</exception>
        public static EsperQuery<TOut> FilterView<TIn, TOut>(this EsperQuery<TIn> esperQuery, Func<View> viewFactory)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            var deriveFromClause = deriveObjectModel.FromClause;

            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var streams = deriveFromClause.Streams;
                if (streams == null)
                {
                    throw new ArgumentException("no stream available to use for window");
                }

                var filter = streams.OfType<FilterStream>().Last();
                if (filter == null)
                {
                    throw new ArgumentException("no stream available to use for window");
                }

                filter.AddView(viewFactory.Invoke());
            }

            return new EsperQuery<TOut>(esperQuery.ServiceProvider, deriveObjectModel);
        }
    }
}