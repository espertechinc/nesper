///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.path;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.module
{
    public interface EPStatementInitServices
    {
        IContainer Container { get; }

        Attribute[] Annotations { get; }

        string DeploymentId { get; }

        string RuntimeURI { get; }

        IObjectCopier ObjectCopier { get; }

        AggregationServiceFactoryService AggregationServiceFactoryService { get; }

        ContextManagementService ContextManagementService { get; }

        ContextServiceFactory ContextServiceFactory { get; }

        DataInputOutputSerdeProvider DataInputOutputSerdeProvider { get; }

        ImportServiceRuntime ImportServiceRuntime { get; }

        RuntimeSettingsService RuntimeSettingsService { get; }

        RuntimeExtensionServices RuntimeExtensionServices { get; }

        EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        EventTableIndexService EventTableIndexService { get; }

        EventTypeAvroHandler EventTypeAvroHandler { get; }

        EventTypeResolver EventTypeResolver { get; }

        ExceptionHandlingService ExceptionHandlingService { get; }

        PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry { get; }

        FilterSharedBoolExprRegistery FilterSharedBoolExprRegistery { get; }

        FilterSharedLookupableRegistery FilterSharedLookupableRegistery { get; }

        FilterSpecActivatableRegistry FilterSpecActivatableRegistry { get; }

        FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; }

        InternalEventRouteDest InternalEventRouteDest { get; }

        NamedWindowFactoryService NamedWindowFactoryService { get; }

        NamedWindowManagementService NamedWindowManagementService { get; }

        NamedWindowDispatchService NamedWindowDispatchService { get; }

        PatternFactoryService PatternFactoryService { get; }

        PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry { get; }

        ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; }

        StatementResourceService StatementResourceService { get; }

        StatementResultService StatementResultService { get; }

        TableManagementService TableManagementService { get; }

        PathRegistry<string, TableMetaData> TablePathRegistry { get; }

        TimeAbacus TimeAbacus { get; }

        TimeProvider TimeProvider { get; }

        TimeSourceService TimeSourceService { get; }

        VariableManagementService VariableManagementService { get; }

        PathRegistry<string, VariableMetaData> VariablePathRegistry { get; }

        ViewableActivatorFactory ViewableActivatorFactory { get; }

        ViewFactoryService ViewFactoryService { get; }

        void ActivateNamedWindow(string name);

        void ActivateVariable(string name);

        void ActivateContext(
            string name,
            ContextDefinition definition);

        void ActivateExpression(string name);

        void ActivateTable(string name);

        void AddReadyCallback(StatementReadyCallback readyCallback);
    }

    public class EPStatementInitServicesConstants
    {
        public const string AGGREGATIONSERVICEFACTORYSERVICE = "AggregationServiceFactoryService";
        public const string CONTEXTSERVICEFACTORY = "ContextServiceFactory";
        public const string DATAINPUTOUTPUTSERDEPROVIDER = "DataInputOutputSerdeProvider";
        public const string IMPORTSERVICERUNTIME = "ImportServiceRuntime";
        public const string RUNTIMESETTINGSSERVICE = "RuntimeSettingsService";
        public const string EVENTBEANTYPEDEVENTFACTORY = "EventBeanTypedEventFactory";
        public const string EVENTTABLEINDEXSERVICE = "EventTableIndexService";
        public const string EVENTTYPERESOLVER = "EventTypeResolver";
        public const string FILTERSHAREDBOOLEXPRREGISTERY = "FilterSharedBoolExprRegistery";
        public const string FILTERSHAREDLOOKUPABLEREGISTERY = "FilterSharedLookupableRegistery";
        public const string FILTERSPECACTIVATABLEREGISTRY = "FilterSpecActivatableRegistry";
        public const string FILTERBOOLEANEXPRESSIONFACTORY = "FilterBooleanExpressionFactory";
        public const string INTERNALEVENTROUTEDEST = "InternalEventRouteDest";
        public const string PATTERNFACTORYSERVICE = "PatternFactoryService";
        public const string RESULTSETPROCESSORHELPERFACTORY = "ResultSetProcessorHelperFactory";
        public const string STATEMENTRESULTSERVICE = "StatementResultService";
        public const string TIMEPROVIDER = "TimeProvider";
        public const string VIEWFACTORYSERVICE = "ViewFactoryService";
        public const string VIEWABLEACTIVATORFACTORY = "ViewableActivatorFactory";
        public static readonly CodegenExpressionRef REF = Ref("stmtInitSvc");
    }
} // end of namespace