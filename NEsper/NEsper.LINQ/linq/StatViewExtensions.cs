///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.runtime.client.linq.statistics;

namespace com.espertech.esper.runtime.client.linq
{
    public static class StatViewExtensions
    {
        /// <summary>
        /// Generates a view that calculates univariate statistics on a numeric expression.  The view takes a
        /// single value property as a parameter plus any number of optional additional properties to return properties
        /// of the last event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The property or expression.</param>
        /// <returns></returns>
        public static EsperQuery<UnivariateStatistics> Univariate<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null || properties.Length < 1)
            {
                throw new ArgumentException("at least one property must be provided");
            }

            return esperQuery.FilterView<T, UnivariateStatistics>(() => View.Create("stat", "uni",
                properties.Select(p => new PropertyValueExpression(p)).Cast<Expression>().ToArray()));
        }

        /// <summary>
        /// Generates a view that calculates regression and related intermediate results on the values returned by two expressions.
        /// The view takes two value properties as parameters plus any number of optional additional properties to return properties
        /// of the last event. The value expressions must return a numeric value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The property or expression.</param>
        /// <returns></returns>
        public static EsperQuery<RegressionStatistics> Regression<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null || properties.Length < 2)
            {
                throw new ArgumentException("at least two property must be provided");
            }

            return esperQuery.FilterView<T, RegressionStatistics>(() => View.Create("stat", "linest",
                properties.Select(p => new PropertyValueExpression(p)).Cast<Expression>().ToArray()));
        }

        /// <summary>
        /// Generates a view that calculates the correlation value on the value returned by two expressions.
        /// The view takes two value properties as parameters plus any number of optional additional properties to return properties
        /// of the last event. The value expressions must be return a numeric value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The property or expression.</param>
        /// <returns></returns>
        public static EsperQuery<CorrelationStatistics> Correlation<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null || properties.Length < 2)
            {
                throw new ArgumentException("at least two property must be provided");
            }

            return esperQuery.FilterView<T, CorrelationStatistics>(() => View.Create("stat", "correl",
                properties.Select(p => new PropertyValueExpression(p)).Cast<Expression>().ToArray()));
        }

        /// <summary>
        /// Generates a view that calculates the weighted average given a property returning values to compute the average
        /// for and a property returning weight. The view takes two value properties as parameters plus any number of optional
        /// additional properties to return properties of the last event. The value expressions must return numeric values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="properties">The property or expression.</param>
        /// <returns></returns>
        public static EsperQuery<CorrelationStatistics> WeightedAverage<T>(this EsperQuery<T> esperQuery, params string[] properties)
        {
            if (properties == null || properties.Length < 2)
            {
                throw new ArgumentException("at least two property must be provided");
            }

            return esperQuery.FilterView<T, CorrelationStatistics>(() => View.Create("stat", "weighted_average",
                properties.Select(p => new PropertyValueExpression(p)).Cast<Expression>().ToArray()));
        }
    }
}