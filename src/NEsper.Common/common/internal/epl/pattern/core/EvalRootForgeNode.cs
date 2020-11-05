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

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    /// This class represents an observer expression in the evaluation tree representing an pattern expression.
    /// </summary>
    public class EvalRootForgeNode : EvalForgeNodeBase
    {
        public EvalRootForgeNode(
            bool attachPatternText,
            EvalForgeNode childNode,
            Attribute[] annotations)
            : base(attachPatternText)
        {
            AddChildNode(childNode);
            bool audit = AuditEnum.PATTERN.GetAudit(annotations) != null ||
                         AuditEnum.PATTERNINSTANCES.GetAudit(annotations) != null;
            AssignFactoryNodeIds(audit);
        }

        protected override Type TypeOfFactory()
        {
            return typeof(EvalRootFactoryNode);
        }

        protected override string NameOfFactory()
        {
            return "Root";
        }

        protected override void InlineCodegen(
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod childMake = ChildNodes[0].MakeCodegen(method, symbols, classScope);
            method.Block
                .SetProperty(Ref("node"), "ChildNode", LocalMethod(childMake));
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (!ChildNodes.IsEmpty()) {
                ChildNodes[0].ToEPL(writer, Precedence);
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence {
            get => PatternExpressionPrecedenceEnum.MINIMUM;
        }

        public override void CollectSelfFilterAndSchedule(
            IList<FilterSpecCompiled> filters,
            IList<ScheduleHandleCallbackProvider> schedules)
        {
            // none here
        }

        public IList<EvalForgeNode> CollectFactories()
        {
            IList<EvalForgeNode> factories = new List<EvalForgeNode>(8);
            foreach (EvalForgeNode factoryNode in ChildNodes) {
                CollectFactoriesRecursive(factoryNode, factories);
            }

            return factories;
        }

        private static void CollectFactoriesRecursive(
            EvalForgeNode factoryNode,
            IList<EvalForgeNode> factories)
        {
            factories.Add(factoryNode);
            foreach (EvalForgeNode childNode in factoryNode.ChildNodes) {
                CollectFactoriesRecursive(childNode, factories);
            }
        }

        // assign factory ids, a short-type number assigned once-per-statement to each pattern node
        // return the count of all ids
        private void AssignFactoryNodeIds(bool audit)
        {
            short count = 0;
            FactoryNodeId = count;
            IsAudit = audit;

            IList<EvalForgeNode> factories = CollectFactories();
            foreach (EvalForgeNode factoryNode in factories) {
                count++;
                factoryNode.FactoryNodeId = count;
                factoryNode.IsAudit = audit;
            }
        }
    }
} // end of namespace