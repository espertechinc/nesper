///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;

namespace com.espertech.esper.common.@internal.view.unique
{
    /// <summary>
    ///     Factory for <seealso cref="UniqueByPropertyView" /> instances.
    /// </summary>
    public class UniqueByPropertyViewForge : ViewFactoryForgeBase,
        DataWindowViewForge,
        DataWindowViewForgeUniqueCandidate
    {
        public const string NAME = "Unique-By";

        private ExprNode[] _criteriaExpressions;
        private IList<ExprNode> _viewParameters;
        private MultiKeyClassRef _multiKeyClassNames;

        public ISet<string> UniquenessCandidatePropertyNames =>
            ExprNodeUtilityQuery.GetPropertyNamesIfAllProps(_criteriaExpressions);

        public override string ViewName => NAME;

        public ExprNode[] CriteriaExpressions => _criteriaExpressions;

        public override void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            _viewParameters = parameters;
        }

        public override void Attach(
            EventType parentEventType,
            int streamNumber,
            ViewForgeEnv viewForgeEnv)
        {
            _criteriaExpressions = ViewForgeSupport.Validate(
                ViewName,
                parentEventType,
                _viewParameters,
                false,
                viewForgeEnv,
                streamNumber);

            if (_criteriaExpressions.Length == 0) {
                var errorMessage =
                    ViewName + " view requires a one or more expressions providing unique values as parameters";
                throw new ViewParameterException(errorMessage);
            }

            this.eventType = parentEventType;
        }


        public override IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            var desc = MultiKeyPlanner.PlanMultiKey(
                _criteriaExpressions,
                false,
                viewForgeEnv.StatementRawInfo,
                viewForgeEnv.SerdeResolver);
            _multiKeyClassNames = desc.ClassRef;
            return desc.MultiKeyForgeables;
        }

        internal override Type TypeOfFactory()
        {
            return typeof(UniqueByPropertyViewFactory);
        }

        internal override string FactoryMethod()
        {
            return "Unique";
        }

        internal override void Assign(
            CodegenMethod method,
            CodegenExpressionRef factory,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            ViewMultiKeyHelper.Assign(_criteriaExpressions, _multiKeyClassNames, method, factory, symbols, classScope);
        }
    }
} // end of namespace