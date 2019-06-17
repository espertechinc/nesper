///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.client.linq
{
    public static class EsperQueryExtensions
    {
        #region Select

        /// <summary>
        /// Selects the specified "item" from the esper query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <returns></returns>
        public static DisposableObservableCollection<T> Select<T>(this EsperQuery<T> esperQuery)
        {
            esperQuery.Compile();
            return esperQuery.Statement.AsObservableCollection<T>(true);
        }

        /// <summary>
        /// Selects the specified "item" from the esper query.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static DisposableObservableCollection<T2> Select<T1, T2>(this EsperQuery<T1> esperQuery, Expression<Func<T1, T2>> expression)
        {
            var transform = expression.Compile();
            esperQuery.Compile();
            if (typeof(T1) == typeof(T2))
            {
                return esperQuery.Statement.AsObservableCollection<T2>(true);
            }

            return new CascadeObservableCollection<T1, T2>(
                esperQuery.Statement.AsObservableCollection<T1>(true),
                transform,
                true);
        }

        #endregion Select

        #region AddProperty

        /// <summary>
        /// Add a property / column to the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static EsperQuery<T> AddProperty<T>(this EsperQuery<T> esperQuery, String property)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.SelectClause.Add(property);
                return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        /// <summary>
        /// Add a property / column to the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="property">The property.</param>
        /// <param name="asPropertyName">Name of as property.</param>
        /// <returns></returns>
        public static EsperQuery<T> AddProperty<T>(this EsperQuery<T> esperQuery, String property, String asPropertyName)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.SelectClause.AddWithAsProvidedName(property, asPropertyName);
                return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> AddProperty<T>(this EsperQuery<T> esperQuery, String propertyName, System.Linq.Expressions.Expression<Func<object>> expression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.SelectClause.Add(
                    LinqToSoda.LinqToSodaExpression(expression),
                    propertyName);
                return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        #endregion AddProperty

        /// <summary>
        /// Joins the specified outer.
        /// </summary>
        /// <typeparam name="TOuter">The type of the outer.</typeparam>
        /// <typeparam name="TInner">The type of the inner.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="outer">The outer.</param>
        /// <param name="inner">The inner.</param>
        /// <param name="outerKeySelector">The outer key selector.</param>
        /// <param name="innerKeySelector">The inner key selector.</param>
        /// <param name="resultSelector">The result selector.</param>
        /// <returns></returns>
        public static EsperQuery<TResult> Join<TOuter, TInner, TKey, TResult>(
            this EsperQuery<TOuter> outer, EsperQuery<TInner> inner,
            System.Linq.Expressions.Expression<Func<TOuter, TKey>> outerKeySelector,
            System.Linq.Expressions.Expression<Func<TInner, TKey>> innerKeySelector,
            System.Linq.Expressions.Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            var parentObjectModel = outer.ObjectModel;
            var deriveObjectModel = new EPStatementObjectModel();

            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.Annotations = parentObjectModel.Annotations;

                var innerKey = LinqToSoda.LinqToSodaExpression(innerKeySelector);
                var outerKey = LinqToSoda.LinqToSodaExpression(outerKeySelector);

                deriveObjectModel.FromClause = new FromClause(
                    outer.ObjectModel.FromClause.Streams.Concat(
                        inner.ObjectModel.FromClause.Streams).ToArray());

                var parametersArray = resultSelector.Parameters.ToArray();
                for (int ii = 0; ii < parametersArray.Length; ii++)
                {
                    deriveObjectModel.FromClause.Streams[ii].StreamName =
                        parametersArray[ii].Name;
                }

                deriveObjectModel.FromClause.OuterJoinQualifiers = new List<OuterJoinQualifier>();
                deriveObjectModel.FromClause.OuterJoinQualifiers.Add(
                    new OuterJoinQualifier(
                        OuterJoinType.LEFT,
                        outerKey,
                        innerKey,
                        new PropertyValueExpressionPair[0]));

                deriveObjectModel.SelectClause = LinqToSoda.LinqToSelectClause(resultSelector);

                var toEPL = deriveObjectModel.ToEPL();

                return new EsperQuery<TResult>(outer.ServiceProvider, deriveObjectModel);
            }
        }

        #region Where

        /// <summary>
        /// Constrains the specified esper query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> Where<T>(this EsperQuery<T> esperQuery, System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();

            // Adapt or set the where clause according to the expression contents
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var sodaExpression = LinqToSoda.LinqToSodaExpression(expression);
                deriveObjectModel.WhereClause = deriveObjectModel.WhereClause == null
                    ? sodaExpression
                    : Expressions.And(deriveObjectModel.WhereClause, sodaExpression);
                deriveObjectModel.FromClause.Streams[0].StreamName =
                    expression.Parameters[0].Name;

                return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        #endregion Where

        #region Having

        /// <summary>
        /// Constrains the specified esper query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> Having<T>(this EsperQuery<T> esperQuery, System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();

            // Adapt or set the where clause according to the expression contents
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var sodaExpression = LinqToSoda.LinqToSodaExpression(expression);
                deriveObjectModel.HavingClause = deriveObjectModel.HavingClause == null
                    ? sodaExpression
                    : Expressions.And(deriveObjectModel.WhereClause, sodaExpression);
                deriveObjectModel.FromClause.Streams[0].StreamName =
                    expression.Parameters[0].Name;

                return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        #endregion Having

        #region OutputLimit

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="timePeriodExpression">The time period expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, TimePeriodExpression timePeriodExpression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(timePeriodExpression);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="timePeriodExpression">The time period expression.</param>
        /// <returns></returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, OutputLimitSelector selector, TimePeriodExpression timePeriodExpression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(selector, timePeriodExpression);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="frequency">The frequency.</param>
        /// <returns></returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, OutputLimitSelector selector, double frequency)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(selector, frequency);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="selector">is the events to select</param>
        /// <param name="frequencyVariable">is the variable providing the output limit frequency</param>
        /// <returns>clause</returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, OutputLimitSelector selector, String frequencyVariable)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(selector, frequencyVariable);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="frequency">a frequency to output at</param>
        /// <returns>clause</returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, double frequency)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(frequency);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="frequencyVariable">is the variable name providing output rate frequency values</param>
        /// <returns>clause</returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, String frequencyVariable)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(frequencyVariable);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="whenExpression">the expression that returns true to trigger output</param>
        /// <returns>clause</returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, esper.client.soda.Expression whenExpression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.Create(whenExpression);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limit the output of the query.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="scheduleParameters">the crontab schedule parameters</param>
        /// <returns>clause</returns>
        public static EsperQuery<T> OutputLimit<T>(this EsperQuery<T> esperQuery, esper.client.soda.Expression[] scheduleParameters)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.OutputLimitClause = OutputLimitClause.CreateSchedule(scheduleParameters);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        #endregion OutputLimit

        #region RowLimit

        /// <summary>
        /// Limits the number of rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="numRowsVariable">The num rows variable.</param>
        /// <returns></returns>
        public static EsperQuery<T> RowLimit<T>(this EsperQuery<T> esperQuery, String numRowsVariable)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.RowLimitClause = RowLimitClause.Create(numRowsVariable);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limits the number of rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="numRowsVariable">The num rows variable.</param>
        /// <param name="offsetVariable">The offset variable.</param>
        /// <returns></returns>
        public static EsperQuery<T> RowLimit<T>(this EsperQuery<T> esperQuery, String numRowsVariable, String offsetVariable)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.RowLimitClause = RowLimitClause.Create(numRowsVariable, offsetVariable);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limits the number of rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="numRows">The num rows.</param>
        /// <returns></returns>
        public static EsperQuery<T> RowLimit<T>(this EsperQuery<T> esperQuery, int numRows)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.RowLimitClause = RowLimitClause.Create(numRows);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        /// <summary>
        /// Limits the number of rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="numRows">The num rows.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static EsperQuery<T> RowLimit<T>(this EsperQuery<T> esperQuery, int numRows, int offset)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();
            deriveObjectModel.RowLimitClause = RowLimitClause.Create(numRows, offset);
            return new EsperQuery<T>(esperQuery.ServiceProvider, deriveObjectModel);
        }

        #endregion RowLimit

        #region OrderBy

        /// <summary>
        /// Orders the results of the expression.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="keySelectionExpression">The key selection expression.</param>
        /// <returns></returns>
        public static EsperQuery<TSource> OrderBy<TSource, TKey>(this EsperQuery<TSource> esperQuery,
                                                                 Expression<Func<TSource, TKey>> keySelectionExpression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();

            // Adapt or set the where clause according to the expression contents
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var sodaExpression = LinqToSoda.LinqToSodaExpression(keySelectionExpression);
                deriveObjectModel.OrderByClause = new OrderByClause();
                deriveObjectModel.OrderByClause.Add(sodaExpression, false);
                return new EsperQuery<TSource>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        /// <summary>
        /// Orders the results of the expression.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="keySelectionExpression">The key selection expression.</param>
        /// <returns></returns>
        public static EsperQuery<TSource> OrderByDescending<TSource, TKey>(this EsperQuery<TSource> esperQuery,
                                                                           Expression<Func<TSource, TKey>> keySelectionExpression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();

            // Adapt or set the where clause according to the expression contents
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var sodaExpression = LinqToSoda.LinqToSodaExpression(keySelectionExpression);
                deriveObjectModel.OrderByClause = new OrderByClause();
                deriveObjectModel.OrderByClause.Add(sodaExpression, true);
                return new EsperQuery<TSource>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        #endregion OrderBy

        #region GroupBy

        /// <summary>
        /// Groups the results of the expression.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="keySelectionExpression">The key selection expression.</param>
        /// <returns></returns>
        public static EsperQuery<TSource> GroupBy<TSource, TKey>(this EsperQuery<TSource> esperQuery,
                                                                 Expression<Func<TSource, TKey>> keySelectionExpression)
        {
            var parentObjectModel = esperQuery.ObjectModel;
            var deriveObjectModel = parentObjectModel.ShallowCopy();

            // Adapt or set the where clause according to the expression contents
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                var sodaExpression = LinqToSoda.LinqToSodaExpression(keySelectionExpression);
                deriveObjectModel.GroupByClause = GroupByClause.Create(sodaExpression);
                return new EsperQuery<TSource>(esperQuery.ServiceProvider, deriveObjectModel);
            }
        }

        #endregion GroupBy

        #region FromTypeAs

        /// <summary>
        /// Creates a query view from the service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="asName">As name.</param>
        /// <returns></returns>
        public static EsperQuery<T> FromTypeAs<T>(this EPServiceProvider serviceProvider, string asName)
        {
            var selectClause = SelectClause.Create();
            selectClause.AddWildcard();

            var objectModel = new EPStatementObjectModel();
            objectModel.SelectClause = selectClause;
            objectModel.FromClause = FromClause.Create();
            objectModel.FromClause.Add(FilterStream.Create(typeof(T).FullName, asName));
            objectModel.MakeIterableUnbound();

            return new EsperQuery<T>(serviceProvider, objectModel);
        }

        #endregion FromTypeAs

        #region FromStreamAs

        /// <summary>
        /// Creates a query view from the service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="asName">The name of the stream.</param>
        /// <returns></returns>
        public static EsperQuery<T> FromStreamAs<T>(this EPServiceProvider serviceProvider, string stream, string asName)
        {
            var selectClause = SelectClause.Create();
            selectClause.AddWildcard();

            var objectModel = new EPStatementObjectModel();
            objectModel.SelectClause = selectClause;
            objectModel.FromClause = FromClause.Create();
            objectModel.FromClause.Add(FilterStream.Create(stream, asName));
            objectModel.MakeIterableUnbound();

            return new EsperQuery<T>(serviceProvider, objectModel);
        }

        #endregion FromStreamAs

        #region From

        /// <summary>
        /// Creates a query view from the service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns></returns>
        public static EsperQuery<T> From<T>(this EPServiceProvider serviceProvider)
        {
            var selectClause = SelectClause.Create();
            selectClause.AddWildcard();

            var objectModel = new EPStatementObjectModel();
            objectModel.SelectClause = selectClause;
            objectModel.FromClause = FromClause.Create();
            objectModel.FromClause.Add(FilterStream.Create(typeof(T).FullName));
            objectModel.MakeIterableUnbound();

            return new EsperQuery<T>(serviceProvider, objectModel);
        }

        /// <summary>
        /// Creates a query view from the service provider.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="typeList">The type list.</param>
        /// <returns></returns>
        public static EsperQuery<T> From<T>(this EPServiceProvider serviceProvider, params Type[] typeList)
        {
            var selectClause = SelectClause.Create();
            selectClause.AddWildcard();

            var objectModel = new EPStatementObjectModel();
            objectModel.SelectClause = selectClause;
            objectModel.FromClause = FromClause.Create();
            objectModel.MakeIterableUnbound();

            for (int ii = 0; ii < typeList.Length; ii++)
            {
                var type = typeList[ii];
                var streamName = String.Format("s{0}", ii);
                objectModel.FromClause.Add(FilterStream.Create(type.FullName, streamName));
            }

            return new EsperQuery<T>(serviceProvider, objectModel);
        }

        /// <summary>
        /// Creates a query view from the service provider.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="streamNames">The stream names.</param>
        /// <returns></returns>
        public static EsperQuery<T> From<T>(this EPServiceProvider serviceProvider, params string[] streamNames)
        {
            var selectClause = SelectClause.Create();
            selectClause.AddWildcard();

            var objectModel = new EPStatementObjectModel();
            objectModel.SelectClause = selectClause;
            objectModel.FromClause = FromClause.Create();
            objectModel.MakeIterableUnbound();

            for (int ii = 0; ii < streamNames.Length; ii++)
            {
                objectModel.FromClause.Add(FilterStream.Create(streamNames[ii]));
            }

            return new EsperQuery<T>(serviceProvider, objectModel);
        }

        #endregion From
    }
}