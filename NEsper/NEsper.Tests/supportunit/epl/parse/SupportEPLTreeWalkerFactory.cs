///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.pattern;
using com.espertech.esper.supportunit.util;

namespace com.espertech.esper.supportunit.epl.parse
{
    public class SupportEPLTreeWalkerFactory
    {
        public static EPLTreeWalkerListener MakeWalker(CommonTokenStream tokenStream, EngineImportService engineImportService, VariableService variableService)
        {
            var container = SupportContainer.Instance;
            return new EPLTreeWalkerListener(
                container,
                tokenStream, engineImportService, variableService, 
                new SupportSchedulingServiceImpl(),
                SelectClauseStreamSelectorEnum.ISTREAM_ONLY, "uri", 
                new Configuration(container),
                new PatternNodeFactoryImpl(),
                null, null,
                new ExprDeclaredServiceImpl(container.LockManager()),
                new TableServiceImpl(container));
        }

        public static EPLTreeWalkerListener MakeWalker(CommonTokenStream tokenStream)
        {
            var container = SupportContainer.Instance;
            return MakeWalker(
                tokenStream,
                SupportEngineImportServiceFactory.Make(container),
                new VariableServiceImpl(
                    container, 0, null, container.Resolve<EventAdapterService>(), null));
        }
    }
}
