///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.linq
{
    public static class StatementExtensions
    {
        /// <summary>
        /// Creates a typed observable collection from the statement.  All events are forwarded to
        /// the observerable collection and are transformed into the typed class using the default
        /// event transformer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement">The statement.</param>
        /// <returns></returns>
        public static StatementObservableCollection<T> AsObservableCollection<T>(this EPStatement statement)
        {
            return AsObservableCollection<T>(statement, false);
        }

        /// <summary>
        /// Creates a typed observable collection from the statement.  All events are forwarded to
        /// the observerable collection and are transformed into the typed class using the default
        /// event transformer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement">The statement.</param>
        /// <param name="disposeStatement">if set to <c>true</c> [dispose statement].</param>
        /// <returns></returns>
        public static StatementObservableCollection<T> AsObservableCollection<T>(this EPStatement statement, bool disposeStatement)
        {
            return AsObservableCollection(statement, EventTransformationFactory.DefaultTransformation<T>(), disposeStatement);
        }

        /// <summary>
        /// Creates a typed observable collection from the statement.  All events are forwarded to
        /// the observerable collection and are transformed into the typed class using the provided
        /// event transformer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="statement">The statement.</param>
        /// <param name="eventTransformer">The event transformer.</param>
        /// <param name="disposeStatement">if set to <c>true</c> [dispose statement].</param>
        /// <returns></returns>
        public static StatementObservableCollection<T> AsObservableCollection<T>(this EPStatement statement,
                                                                                 Func<EventBean, T> eventTransformer,
                                                                                 bool disposeStatement)
        {
            var observerableCollection = new StatementObservableCollection<T>(statement, eventTransformer, disposeStatement);
            return observerableCollection;
        }
    }
}
