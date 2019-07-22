///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectProcessorArgs
    {
        public SelectProcessorArgs(
            SelectClauseElementCompiled[] selectionList,
            GroupByRollupInfo groupByRollupInfo,
            bool isUsingWildcard,
            EventType optionalInsertIntoEventType,
            ForClauseSpec forClauseSpec,
            StreamTypeService typeService,
            ContextCompileTimeDescriptor contextDescriptor,
            bool isFireAndForget,
            Attribute[] annotations,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            SelectionList = selectionList;
            GroupByRollupInfo = groupByRollupInfo;
            IsUsingWildcard = isUsingWildcard;
            OptionalInsertIntoEventType = optionalInsertIntoEventType;
            ForClauseSpec = forClauseSpec;
            TypeService = typeService;
            ContextDescriptor = contextDescriptor;
            IsFireAndForget = isFireAndForget;
            Annotations = annotations;
            StatementRawInfo = statementRawInfo;
            CompileTimeServices = compileTimeServices;
        }

        public IContainer Container => CompileTimeServices.Container;

        public SelectClauseElementCompiled[] SelectionList { get; }

        public bool IsUsingWildcard { get; }

        public EventType OptionalInsertIntoEventType { get; set; }

        public ForClauseSpec ForClauseSpec { get; }

        public StreamTypeService TypeService { get; }

        public ImportServiceCompileTime ImportService =>
            CompileTimeServices.ImportServiceCompileTime;

        public VariableCompileTimeResolver VariableCompileTimeResolver =>
            CompileTimeServices.VariableCompileTimeResolver;

        public string StatementName => StatementRawInfo.StatementName;

        public Attribute[] Annotations { get; }

        public ContextCompileTimeDescriptor ContextDescriptor { get; }

        public Configuration Configuration => CompileTimeServices.Configuration;

        public EventTypeCompileTimeRegistry EventTypeCompileTimeRegistry =>
            CompileTimeServices.EventTypeCompileTimeRegistry;

        public int StatementNumber => StatementRawInfo.StatementNumber;

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate =>
            CompileTimeServices.BeanEventTypeFactoryPrivate;

        public StatementCompileTimeServices CompileTimeServices { get; }

        public StatementRawInfo StatementRawInfo { get; }

        public NamedWindowCompileTimeResolver NamedWindowCompileTimeResolver =>
            CompileTimeServices.NamedWindowCompileTimeResolver;

        public TableCompileTimeResolver TableCompileTimeResolver => CompileTimeServices.TableCompileTimeResolver;

        public bool IsFireAndForget { get; }

        public EventTypeAvroHandler EventTypeAvroHandler => CompileTimeServices.EventTypeAvroHandler;

        public EventTypeCompileTimeResolver EventTypeCompileTimeResolver =>
            CompileTimeServices.EventTypeCompileTimeResolver;

        public GroupByRollupInfo GroupByRollupInfo { get; }

        public string ModuleName => StatementRawInfo.ModuleName;
    }
} // end of namespace