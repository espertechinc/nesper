///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorNamedWindowForge : FireAndForgetProcessorForge
    {
        private readonly NamedWindowMetaData namedWindow;

        public FireAndForgetProcessorNamedWindowForge(NamedWindowMetaData namedWindow)
        {
            this.namedWindow = namedWindow;
        }

        public string ProcessorName => namedWindow.EventType.Name;

        public EventType EventTypeRSPInputEvents => namedWindow.EventType;

        public EventType EventTypePublic => namedWindow.EventType;

        public string ContextName => namedWindow.ContextName;

        public string[][] UniqueIndexes => EventTableIndexMetadataUtil.GetUniqueness(
            namedWindow.IndexMetadata,
            namedWindow.Uniqueness);

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(FireAndForgetProcessorNamedWindow),
                GetType(),
                classScope);
            var nw = Ref("nw");
            method.Block
                .DeclareVarNewInstance<FireAndForgetProcessorNamedWindow>(nw.Ref)
                .SetProperty(
                    nw,
                    "NamedWindow",
                    NamedWindowDeployTimeResolver.MakeResolveNamedWindow(namedWindow, symbols.GetAddInitSvc(method)))
                .MethodReturn(nw);
            return LocalMethod(method);
        }
    }
} // end of namespace