///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.pattern;
using com.espertech.esper.support.events;
using com.espertech.esper.support.schedule;

namespace com.espertech.esper.support.epl.parse
{
    public class SupportEPLTreeWalkerFactory
    {
        public static EPLTreeWalkerListener MakeWalker(CommonTokenStream tokenStream, EngineImportService engineImportService, VariableService variableService)
        {
            return new EPLTreeWalkerListener(
                tokenStream, engineImportService, variableService, new SupportSchedulingServiceImpl(),
                SelectClauseStreamSelectorEnum.ISTREAM_ONLY, "uri", new Configuration(), new PatternNodeFactoryImpl(),
                null, null,
                new ExprDeclaredServiceImpl(),
                new TableServiceImpl());
        }

        public static EPLTreeWalkerListener MakeWalker(CommonTokenStream tokenStream)
        {
            return MakeWalker(
                tokenStream, 
                new EngineImportServiceImpl(true, true, true, false, null),
                new VariableServiceImpl(0, null, SupportEventAdapterService.Service, null));
        }
    }
}
