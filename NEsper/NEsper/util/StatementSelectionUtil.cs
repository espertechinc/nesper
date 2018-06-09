///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;

namespace com.espertech.esper.util
{
    public static class StatementSelectionUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string StatementMetaEventTypeName = "STATEMENT_META_EVENT_TYPE";

        public static BeanEventType GetStatementMetaEventType(IContainer container)
        {
            return container.Resolve<BeanEventType>(StatementMetaEventTypeName);
        }

        public static IContainer RegisterExpressionType(IContainer container)
        {
            container.Register<BeanEventType>(
                CreateExpressionTypeInstance,
                Lifespan.Singleton,
                StatementMetaEventTypeName);
            return container;
        }

        private static BeanEventType CreateExpressionTypeInstance(IContainer container)
        {
            if (!container.Has(StatementMetaEventTypeName))
            {
                var statementMetaEventType = (BeanEventType)container.Resolve<EventAdapterService>()
                    .AddBeanType("StatementRow", typeof(StatementRow), true, true, true);
                container.Register<BeanEventType>(
                    statementMetaEventType, Lifespan.Singleton, StatementMetaEventTypeName);
                return statementMetaEventType;
            }

            return container.Resolve<BeanEventType>(StatementMetaEventTypeName);
        }

        public static void ApplyExpressionToStatements(
            EPServiceProviderSPI engine,
            string filter,
            Action<EPServiceProvider, EPStatement> consumer)
        {
            // compile filter
            ExprNode filterExpr = null;
            var isUseFilter = false;

            if (!String.IsNullOrWhiteSpace(filter))
            {
                isUseFilter = true;
                var statementExprNode = CompileValidateStatementFilterExpr(engine, filter);
                if (statementExprNode.Second == null)
                {
                    filterExpr = statementExprNode.First;
                }
            }

            IList<string> statementNames = engine.EPAdministrator.StatementNames;
            foreach (var statementName in statementNames)
            {
                var epStmt = engine.EPAdministrator.GetStatement(statementName);
                if (epStmt == null)
                {
                    continue;
                }

                if (isUseFilter)
                {
                    if (filterExpr != null)
                    {
                        if (!EvaluateStatement(engine.Container, filterExpr, epStmt))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        var match = false;
                        string searchString = filter.ToLowerInvariant();
                        if ((epStmt.Name != null) && (epStmt.Name.ToLowerInvariant().Contains(searchString)))
                        {
                            match = true;
                        }
                        if (!match)
                        {
                            if ((epStmt.Text != null) && (epStmt.Text.ToLowerInvariant().Contains(searchString)))
                            {
                                match = true;
                            }
                            if (epStmt.State.ToString().ToLowerInvariant().Contains(searchString))
                            {
                                match = true;
                            }
                        }
                        if (!match)
                        {
                            continue;
                        }
                    }
                }

                consumer.Invoke(engine, epStmt);
            }
        }

        public static bool EvaluateStatement(IContainer container, ExprNode expression, EPStatement stmt)
        {
            if (expression == null)
            {
                return true;
            }

            var evaluator = expression.ExprEvaluator;
            if (evaluator.ReturnType.GetBoxedType() != typeof (bool?))
            {
                throw new EPException(
                    "Invalid expression, expected a bool return type for expression and received '" +
                    evaluator.ReturnType.GetCleanName() +
                    "' for expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expression) + "'");
            }

            try {
                var eventAdapterService = container.Resolve<EventAdapterService>();
                var statementMetaEventType = GetStatementMetaEventType(container);
                var row = GetRow(stmt);
                var rowBean = eventAdapterService.AdapterForTypedObject(row, statementMetaEventType);
                var evaluateParams = new EvaluateParams(new EventBean[] { rowBean }, true, null);
                var pass = (bool?) evaluator.Evaluate(evaluateParams);
                return !pass.GetValueOrDefault(false);
            }
            catch (Exception ex)
            {
                Log.Error(
                    "Unexpected exception filtering statements by expression, skipping statement: " + ex.Message, ex);
            }
            return false;
        }
        
        public static Pair<ExprNode, string> CompileValidateStatementFilterExpr(
            EPServiceProviderSPI engine,
            string filterExpression)
        {
            ExprNode exprNode;
            try
            {
                var spi = (EPAdministratorSPI) engine.EPAdministrator;
                exprNode = spi.CompileExpression(filterExpression);
            }
            catch (Exception ex)
            {
                return new Pair<ExprNode, string>(null, "Error compiling expression: " + ex.Message);
            }

            try {
                var statementMetaEventType = GetStatementMetaEventType(engine.Container);
                var streamTypeService = new StreamTypeServiceImpl(statementMetaEventType, null, true, engine.URI);
                exprNode = ExprNodeUtility.GetValidatedSubtree(
                    ExprNodeOrigin.SCRIPTPARAMS, exprNode,
                    new ExprValidationContext(
                        engine.Container,
                        streamTypeService,
                        engine.EngineImportService,
                        null, null,
                        engine.TimeProvider, null, null, null,
                        engine.EventAdapterService, "no-name-assigned", -1,
                        null, null, null, true, false, false, false, null, true));
            }
            catch (Exception ex)
            {
                return new Pair<ExprNode, string>(null, "Error validating expression: " + ex.Message);
            }
            return new Pair<ExprNode, string>(exprNode, null);
        }

        // Predefined properties available:
        // - name (string)
        // - description (string)
        // - epl (string)
        // - each tag individually (string)
        // - priority
        // - drop (bool)
        // - hint (string)
        private static StatementRow GetRow(EPStatement statement)
        {
            string description = null;
            string hint = null;
            var hintDelimiter = "";
            var priority = 0;
            IDictionary<string, string> tags = null;
            var drop = false;
    
            var annotations = statement.Annotations;
            foreach (var anno in annotations)
            {
                if (anno is HintAttribute) {
                    if (hint == null) {
                        hint = "";
                    }
                    hint += hintDelimiter + ((HintAttribute) anno).Value;
                    hintDelimiter = ",";
                }
                else if (anno is TagAttribute)
                {
                    var tag = (TagAttribute)anno;
                    if (tags == null) {
                        tags = new Dictionary<string, string>();
                    }
                    tags.Put(tag.Name, tag.Value);
                }
                else if (anno is PriorityAttribute)
                {
                    priority = ((PriorityAttribute) anno).Value;
                }
                else if (anno is DropAttribute)
                {
                    drop = true;
                }
                else if (anno is DescriptionAttribute)
                {
                    description = ((DescriptionAttribute)anno).Value;
                }
            }
    
            var name = statement.Name;
            var text = statement.Text;
            var state = statement.State.ToString();
            var userObject = statement.UserObject;
    
            return new StatementRow(
                    name,
                    text,
                    state,
                    userObject,
                    description,
                    hint,
                    priority,
                    drop,
                    tags
            );
        }
    
        public class StatementRow
        {
            public StatementRow(
                string name,
                string epl,
                string state,
                Object userObject,
                string description,
                string hint,
                int priority,
                bool? drop,
                IDictionary<string, string> tag)
            {
                Name = name;
                Epl = epl;
                State = state;
                UserObject = userObject;
                Description = description;
                Hint = hint;
                Priority = priority;
                IsDrop = drop;
                Tag = tag;
            }

            public string Name { get; set; }

            public string Epl { get; set; }

            public string State { get; set; }

            public object UserObject { get; set; }

            public string Description { get; set; }

            public string Hint { get; set; }

            public int Priority { get; set; }

            public bool? IsDrop { get; set; }

            public IDictionary<string, string> Tag { get; set; }
        }
    }
} // end of namespace
