///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.compat;
using View = com.espertech.esper.common.@internal.view.core.View;

namespace com.espertech.esper.runtime.client.linq
{
    /// <summary>
    /// Set of extensions for use with NEsper EPServiceProviders.
    /// </summary>

    public static class ServiceProviderExtensions
    {
        #region CreateVariable

        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableType">Type of the variable.</param>
        public static void CreateVariable(this EPServiceProvider serviceProvider,
                                          string variableName,
                                          Type variableType)
        {
            CreateVariable(serviceProvider, variableName, variableType.FullName);
        }

        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableType">Type of the variable.</param>
        public static void CreateVariable(this EPServiceProvider serviceProvider,
                                          string variableName,
                                          string variableType)
        {
            CreateVariable(serviceProvider, variableName, variableType, null);
        }

        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="assignment">The assignment.</param>
        public static void CreateVariable(this EPServiceProvider serviceProvider,
                                          string variableName,
                                          Type variableType,
                                          System.Linq.Expressions.Expression<Func<object>> assignment)
        {
            CreateVariable(serviceProvider, variableName, variableType.FullName, assignment);
        }

        /// <summary>
        /// Creates the variable.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableType">Type of the variable.</param>
        /// <param name="assignment">The assignment.</param>
        public static void CreateVariable(this EPServiceProvider serviceProvider,
                                          string variableName,
                                          string variableType,
                                          System.Linq.Expressions.Expression<Func<object>> assignment)
        {
            var objectModel = new EPStatementObjectModel();
            objectModel.CreateVariable = CreateVariableClause.Create(variableType, variableName);
            objectModel.CreateVariable.OptionalAssignment = LinqToSoda.LinqToSodaExpression(assignment);
            serviceProvider.EPAdministrator.Create(objectModel);
        }

        #endregion CreateVariable

        #region CreateSelectTrigger

        /// <summary>
        /// Creates the select trigger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <returns></returns>
        public static EPStatement CreateSelectTrigger<T>(this EPServiceProvider serviceProvider,
                                                         string windowName,
                                                         string asName,
                                                         EsperQuery<T> fromClause)
        {
            return CreateSelectTrigger(
                serviceProvider,
                windowName,
                asName,
                fromClause,
                (Func<Expression>) null);
        }

        /// <summary>
        /// Creates the select trigger with one stream expression capability.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        public static EPStatement CreateSelectTrigger<T>(this EPServiceProvider serviceProvider,
                                                         string windowName,
                                                         string asName,
                                                         EsperQuery<T> fromClause,
                                                         System.Linq.Expressions.Expression<Func<T, bool>> whereClause)
        {
            return CreateSelectTrigger(
                serviceProvider,
                windowName,
                asName,
                fromClause,
                () => LinqToSoda.LinqToSodaExpression(whereClause));
        }

        /// <summary>
        /// Creates a select trigger with two stream expression capability.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        public static EPStatement CreateSelectTrigger<T1, T2>(this EPServiceProvider serviceProvider,
                                                              string windowName,
                                                              string asName,
                                                              EsperQuery<T1> fromClause,
                                                              System.Linq.Expressions.Expression<Func<T1, T2, bool>> whereClause)
        {
            return CreateSelectTrigger(
                serviceProvider,
                windowName,
                asName,
                fromClause,
                () => LinqToSoda.LinqToSodaExpression(whereClause));
        }

        /// <summary>
        /// Creates the select trigger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <param name="deferredWhereClause">The deferred where clause.</param>
        /// <returns></returns>
        public static EPStatement CreateSelectTrigger<T>(EPServiceProvider serviceProvider,
                                                         string windowName,
                                                         string asName,
                                                         EsperQuery<T> fromClause,
                                                         Func<Expression> deferredWhereClause)
        {
            var deriveObjectModel = DeriveObjectModel(fromClause);
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.OnExpr = OnClause.CreateOnSelect(windowName, asName);

                if (deferredWhereClause != null)
                {
                    var whereClause = deferredWhereClause.Invoke();
                    if (whereClause != null)
                    {
                        deriveObjectModel.WhereClause = whereClause;
                    }
                }

                return serviceProvider.EPAdministrator.Create(deriveObjectModel);
            }
        }

        #endregion CreateSelectTrigger

        #region CreateDeleteTrigger

        /// <summary>
        /// Creates the delete trigger with no where clause.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <returns></returns>
        public static EPStatement CreateDeleteTrigger<T>(this EPServiceProvider serviceProvider,
                                                         string windowName,
                                                         string asName,
                                                         EsperQuery<T> fromClause)
        {
            return CreateDeleteTrigger(
                serviceProvider,
                windowName,
                asName,
                fromClause,
                (Func<Expression>) null);
        }

        /// <summary>
        /// Creates the delete trigger with one stream expression capability.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        public static EPStatement CreateDeleteTrigger<T>(this EPServiceProvider serviceProvider,
                                                         string windowName,
                                                         string asName,
                                                         EsperQuery<T> fromClause,
                                                         System.Linq.Expressions.Expression<Func<T, bool>> whereClause)
        {
            return CreateDeleteTrigger(
                serviceProvider,
                windowName,
                asName,
                fromClause,
                () => LinqToSoda.LinqToSodaExpression(whereClause));
        }

        /// <summary>
        /// Creates a delete trigger with two stream expression capability.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <returns></returns>
        public static EPStatement CreateDeleteTrigger<T1, T2>(this EPServiceProvider serviceProvider,
                                                              string windowName,
                                                              string asName,
                                                              EsperQuery<T1> fromClause,
                                                              System.Linq.Expressions.Expression<Func<T1, T2, bool>> whereClause)
        {
            return CreateDeleteTrigger(
                serviceProvider,
                windowName,
                asName,
                fromClause,
                () => LinqToSoda.LinqToSodaExpression(whereClause));
        }

        /// <summary>
        /// Creates the delete trigger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="asName">As name.</param>
        /// <param name="fromClause">From clause.</param>
        /// <param name="deferredWhereClause">The deferred where clause.</param>
        /// <returns></returns>
        public static EPStatement CreateDeleteTrigger<T>(EPServiceProvider serviceProvider,
                                                         string windowName,
                                                         string asName,
                                                         EsperQuery<T> fromClause,
                                                         Func<Expression> deferredWhereClause)
        {
            var deriveObjectModel = DeriveObjectModel(fromClause);
            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.OnExpr = OnClause.CreateOnDelete(windowName, asName);

                if (deferredWhereClause != null)
                {
                    var whereClause = deferredWhereClause.Invoke();
                    if (whereClause != null)
                    {
                        deriveObjectModel.WhereClause = whereClause;
                    }
                }

                return serviceProvider.EPAdministrator.Create(deriveObjectModel);
            }
        }

        #endregion CreateDeleteTrigger

        #region CreateWindow

        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="view">The view.</param>
        /// <param name="esperQuery">The esper query.</param>
        /// <returns></returns>
        public static EPStatement CreateWindow<T>(this EPServiceProvider serviceProvider, string windowName, View view, EsperQuery<T> esperQuery)
        {
            return CreateWindow(serviceProvider, windowName, view, esperQuery, null);
        }

        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="view">The view.</param>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="insertWhereExpression">The insert where expression.</param>
        /// <returns></returns>
        public static EPStatement CreateWindow<T>(
            this EPServiceProvider serviceProvider,
            string windowName,
            View view,
            EsperQuery<T> esperQuery,
            System.Linq.Expressions.Expression<Func<T, bool>> insertWhereExpression)
        {
            var statementObjectModel = CreateWindowAsObjectModel(
                serviceProvider,
                windowName,
                view,
                esperQuery,
                insertWhereExpression);
            return serviceProvider.EPAdministrator.Create(statementObjectModel);
        }

        /// <summary>
        /// Creates a window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="view">The view.</param>
        /// <param name="esperQuery">The esper query.</param>
        /// <param name="insertWhereExpression">The insert where expression.</param>
        /// <returns></returns>
        public static EPStatementObjectModel CreateWindowAsObjectModel<T>(
            this EPServiceProvider serviceProvider,
            string windowName,
            View view,
            EsperQuery<T> esperQuery,
            System.Linq.Expressions.Expression<Func<T, bool>> insertWhereExpression)
        {
            var deriveObjectModel = DeriveObjectModel(esperQuery);

            using (ScopedInstance<EPStatementObjectModel>.Set(deriveObjectModel))
            {
                deriveObjectModel.CreateWindow = CreateWindowClause.Create(windowName, view);
                deriveObjectModel.CreateWindow.IsInsert = false;
                if (insertWhereExpression != null)
                {
                    deriveObjectModel.CreateWindow.InsertWhereClause =
                        LinqToSoda.LinqToSodaExpression(insertWhereExpression);
                    deriveObjectModel.CreateWindow.IsInsert = true;
                }

                return deriveObjectModel;
            }
        }

        private static EPStatementObjectModel DeriveObjectModel<T>(EsperQuery<T> esperQuery)
        {
            EPStatementObjectModel deriveObjectModel;

            if (esperQuery == null)
            {
                deriveObjectModel = new EPStatementObjectModel();
            }
            else
            {
                var parentObjectModel = esperQuery.ObjectModel;
                deriveObjectModel = parentObjectModel.ShallowCopy();
            }
            return deriveObjectModel;
        }

        #endregion CreateWindow
    }
}