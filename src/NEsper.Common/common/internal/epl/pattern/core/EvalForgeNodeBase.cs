///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.util;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    public abstract class EvalForgeNodeBase : EvalForgeNode
    {
        private readonly bool _attachPatternText;

        /// <summary>
        ///     Constructor creates a list of child nodes.
        /// </summary>
        /// <param name="attachPatternText">whether to attach EPL subexpression text</param>
        protected EvalForgeNodeBase(bool attachPatternText)
        {
            ChildNodes = new List<EvalForgeNode>();
            _attachPatternText = attachPatternText;
        }

        protected abstract Type TypeOfFactory { get; }

        protected abstract string NameOfFactory { get; }

        public StateMgmtSetting StateMgmtSettings { get; set; }

        public abstract PatternExpressionPrecedenceEnum Precedence { get; }

        public abstract void CollectSelfFilterAndSchedule(
            Func<short, CallbackAttribution> callbackAttribution,
            IList<FilterSpecTracked> filters,
            IList<ScheduleHandleTracked> schedules);

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

        public short FactoryNodeId { get; set; }

        public bool IsAudit { get; set; }

        public virtual void ToEPL(
            TextWriter writer,
            PatternExpressionPrecedenceEnum parentPrecedence)
        {
            if (Precedence.GetLevel() < parentPrecedence.GetLevel()) {
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
            var method = parent.MakeChild(TypeOfFactory, GetType(), classScope);
            method.Block
                .DeclareVar(
                    TypeOfFactory,
                    "node",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.PATTERNFACTORYSERVICE)
                        .Add(
                            NameOfFactory,
                            StateMgmtSettings == null ? ConstantNull() : StateMgmtSettings.ToExpression()))
                .SetProperty(Ref("node"), "FactoryNodeId", Constant(FactoryNodeId));
            if (IsAudit || classScope.IsInstrumented || _attachPatternText) {
                var writer = new StringWriter();
                ToEPL(writer, PatternExpressionPrecedenceEnum.MINIMUM);
                var expressionText = writer.ToString();
                method.Block.SetProperty(Ref("node"), "TextForAudit", Constant(expressionText));
            }

            InlineCodegen(method, symbols, classScope);
            method.Block.MethodReturn(Ref("node"));
            return method;
        }

        public abstract AppliesTo AppliesTo();

        protected abstract void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope);

        public abstract void ToPrecedenceFreeEPL(TextWriter writer);
    }
} // end of namespace