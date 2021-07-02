///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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

        public string NamedWindowOrTableName {
            get => namedWindow.EventType.Name;
        }

        public EventType EventTypeRspInputEvents {
            get => namedWindow.EventType;
        }

        public EventType EventTypePublic {
            get => namedWindow.EventType;
        }

        public string ContextName {
            get => namedWindow.ContextName;
        }

        public string[][] UniqueIndexes {
            get => EventTableIndexMetadataUtil.GetUniqueness(namedWindow.IndexMetadata, namedWindow.Uniqueness);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(FireAndForgetProcessorNamedWindow),
                GetType(),
                classScope);
            CodegenExpressionRef nw = Ref("nw");
            method.Block
                .DeclareVar<FireAndForgetProcessorNamedWindow>(
                    nw.Ref,
                    NewInstance(typeof(FireAndForgetProcessorNamedWindow)))
                .SetProperty(
                    nw,
                    "NamedWindow",
                    NamedWindowDeployTimeResolver.MakeResolveNamedWindow(namedWindow, symbols.GetAddInitSvc(method)))
                .MethodReturn(nw);
            return LocalMethod(method);
        }
    }
} // end of namespace