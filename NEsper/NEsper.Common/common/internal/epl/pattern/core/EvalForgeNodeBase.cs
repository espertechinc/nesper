///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public abstract class EvalForgeNodeBase : EvalForgeNode
    {
        protected bool audit;
        protected short factoryNodeId;

        /// <summary>
        ///     Constructor creates a list of child nodes.
        /// </summary>
        public EvalForgeNodeBase()
        {
            ChildNodes = new List<EvalForgeNode>();
        }

        public abstract PatternExpressionPrecedenceEnum Precedence { get; }

        public abstract void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules);

        /// <summary>
        ///     Adds a child node.
        /// </summary>
        /// <param name="childNode">is the child evaluation tree node to add</param>
        public void AddChildNode(EvalForgeNode childNode)
        {
            ChildNodes.Add(childNode);
        }

        public void AddChildNodes(ICollection<EvalForgeNode> childNodesToAdd)
        {
            ChildNodes.AddAll(childNodesToAdd);
        }

        /// <summary>
        ///     Returns list of child nodes.
        /// </summary>
        /// <value>list of child nodes</value>
        public IList<EvalForgeNode> ChildNodes { get; }

        public short FactoryNodeId {
            get => factoryNodeId;
            set => factoryNodeId = value;
        }

        public bool IsAudit {
            get => audit;
            set => audit = value;
        }

        public void ToEPL(
            TextWriter writer,
            PatternExpressionPrecedenceEnum parentPrecedence)
        {
            if (this.Precedence.GetLevel() < parentPrecedence.GetLevel()) {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer);
                writer.Write(")");
            }
            else {
                ToPrecedenceFreeEPL(writer);
            }
        }

        public CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(TypeOfFactory(), GetType(), classScope);
            method.Block
                .DeclareVar(
                    TypeOfFactory(),
                    "node",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add(NameOfFactory()))
                .SetProperty(Ref("node"), "FactoryNodeId", Constant(factoryNodeId));
            if (audit || classScope.IsInstrumented) {
                var writer = new StringWriter();
                ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
                var expressionText = writer.ToString();
                method.Block.SetProperty(Ref("node"), "TextForAudit", Constant(expressionText));
            }

            InlineCodegen(method, symbols, classScope);
            method.Block.MethodReturn(Ref("node"));
            return method;
        }

        protected abstract Type TypeOfFactory();

        protected abstract string NameOfFactory();

        protected abstract void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract void ToPrecedenceFreeEPL(TextWriter writer);
    }
} // end of namespace