///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client.soda;

namespace com.espertech.esper.linq
{
    public static class ExtViewExtensions
    {
        /// <summary>
        /// Generates a view that sorts by values returned by the specified expression or list of expressions and keeps
        /// only the top (or bottom) events up to the given size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="size">The size.</param>
        /// <param name="sortCriteria">The sort criteria.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">at least one property must be provided</exception>
        public static EsperQuery<T> Sorted<T>(this EsperQuery<T> esperQuery, int size, params SortCriteria[] sortCriteria)
        {
            if (sortCriteria == null || sortCriteria.Length < 1)
                throw new ArgumentException("at least one sort criteria must be provided");

            var expressionList = new List<Expression>();
            expressionList.Add(
                new ConstantExpression(size));
            expressionList.AddRange(
                sortCriteria.Select(s => s.ToSodaExpression()));

            return esperQuery.FilterView(() => View.Create("ext", "sort", expressionList));
        }

        /// <summary>
        /// Generates a view that orders events that arrive out-of-order, using timestamp-values
        /// provided by an expression, and by comparing that timestamp value to engine system time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="property">The property to use for the timestamp.</param>
        /// <param name="timePeriod">the time period specifying the time interval that an arriving event should maximally be held, in order to consider older events arriving at a later time</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">at least one property must be provided</exception>
        public static EsperQuery<T> TimeOrdered<T>(this EsperQuery<T> esperQuery, string property, TimeSpan timePeriod)
        {
            return esperQuery.FilterView(() => View.Create("ext", "time_order", 
                new PropertyValueExpression(property),
                timePeriod.ToTimePeriodExpression()));
        }

        /// <summary>
        /// Generates a view retains only the most recent among events having the same value for the criteria
        /// expression(s), sorted by sort criteria expressions and keeps only the top events up to the given size.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="uniqueProperties">The unique properties.</param>
        /// <param name="size">The size.</param>
        /// <param name="sortCriteria">The sort criteria.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">at least one property must be provided</exception>
        public static EsperQuery<T> Ranked<T>(this EsperQuery<T> esperQuery, IEnumerable<string> uniqueProperties, int size, params SortCriteria[] sortCriteria)
        {
            if (uniqueProperties == null || uniqueProperties.Count() < 1)
                throw new ArgumentException("at least one unique property must be provided");
            if (sortCriteria == null || sortCriteria.Length < 1)
                throw new ArgumentException("at least one sort criteria must be provided");

            var expressionList = new List<Expression>();
            expressionList.AddRange(
                uniqueProperties.Select(p => new PropertyValueExpression(p)).Cast<Expression>());
            expressionList.Add(
                new ConstantExpression(size));
            expressionList.AddRange(
                sortCriteria.Select(s => s.ToSodaExpression()));

            return esperQuery.FilterView(() => View.Create("ext", "rank", expressionList));
        }
    }
}
