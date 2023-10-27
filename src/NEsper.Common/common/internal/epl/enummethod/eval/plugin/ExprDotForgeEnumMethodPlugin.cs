///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.enummethod;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.enummethod.dot;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plugin
{
    public partial class ExprDotForgeEnumMethodPlugin : ExprDotForgeEnumMethodBase
    {
        private readonly EnumMethodForgeFactory forgeFactory;
        private EnumMethodModeStaticMethod mode;

        public ExprDotForgeEnumMethodPlugin(EnumMethodForgeFactory forgeFactory)
        {
            this.forgeFactory = forgeFactory;
        }

        public override void Initialize(
            DotMethodFP footprint,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            IList<ExprNode> parameters,
            StreamTypeService streamTypeService,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            // validate
            var ctx = new EnumMethodValidateContext(
                footprint,
                inputEventType,
                collectionComponentType,
                streamTypeService,
                enumMethod,
                parameters,
                statementRawInfo);
            var enumMethodMode = forgeFactory.Validate(ctx);
            if (!(enumMethodMode is EnumMethodModeStaticMethod method)) {
                throw new ExprValidationException(
                    "Unexpected EnumMethodMode implementation, expected a provided implementation");
            }

            mode = method;
        }

        public override EnumForgeDescFactory GetForgeFactory(
            DotMethodFP footprint,
            IList<ExprNode> parameters,
            EnumMethodEnum enumMethod,
            string enumMethodUsedName,
            EventType inputEventType,
            Type collectionComponentType,
            ExprValidationContext validationContext)
        {
            if (mode == null) {
                throw new IllegalStateException("Initialize did not take place");
            }

            return new EnumForgeDescFactoryPlugin(
                mode,
                enumMethodUsedName,
                footprint,
                parameters,
                inputEventType,
                collectionComponentType,
                validationContext.StatementRawInfo,
                validationContext.StatementCompileTimeService);
        }

        public EnumMethodModeStaticMethod Mode => mode;
    }
} // end of namespace