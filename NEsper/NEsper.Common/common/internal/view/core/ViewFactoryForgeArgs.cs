///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewFactoryForgeArgs
    {
        public ViewFactoryForgeArgs(
            int streamNum,
            bool isSubquery,
            int subqueryNumber,
            StreamSpecOptions options,
            string optionalCreateNamedWindowName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices compileTimeServices)
        {
            StatementRawInfo = statementRawInfo;
            StreamNum = streamNum;
            Options = options;
            IsSubquery = isSubquery;
            SubqueryNumber = subqueryNumber;
            OptionalCreateNamedWindowName = optionalCreateNamedWindowName;
            CompileTimeServices = compileTimeServices;
        }

        public int StreamNum { get; }

        public StreamSpecOptions Options { get; }

        public bool IsSubquery { get; }

        public int SubqueryNumber { get; }

        public ImportServiceCompileTime ImportService => CompileTimeServices.ImportServiceCompileTime;

        public Configuration Configuration => CompileTimeServices.Configuration;

        public ViewResolutionService ViewResolutionService => CompileTimeServices.ViewResolutionService;

        public BeanEventTypeFactory BeanEventTypeFactoryPrivate => CompileTimeServices.BeanEventTypeFactoryPrivate;

        public EventTypeCompileTimeRegistry EventTypeModuleCompileTimeRegistry => CompileTimeServices.EventTypeCompileTimeRegistry;

        public Attribute[] Annotations => StatementRawInfo.Annotations;

        public string StatementName => StatementRawInfo.StatementName;

        public int StatementNumber => StatementRawInfo.StatementNumber;

        public StatementCompileTimeServices CompileTimeServices { get; }

        public StatementRawInfo StatementRawInfo { get; }

        public string OptionalCreateNamedWindowName { get; }
    }
} // end of namespace