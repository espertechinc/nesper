///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.support
{
    public class SupportExprValidationContextFactory
    {
        public static ExprValidationContext MakeEmpty(IContainer container)
        {
            return Make(container, new StreamTypeServiceImpl(false));
        }

        public static ExprValidationContext MakeEmpty(
            IContainer container,
            ThreadingProfile threadingProfile)
        {
            return MakeEmpty(container);
        }

        public static ExprValidationContext Make(
            IContainer container,
            StreamTypeService streamTypeService)
        {
            var moduleServices = new ModuleCompileTimeServices(container);
            moduleServices.Configuration = new Configuration();
            moduleServices.ImportServiceCompileTime = SupportClasspathImport.GetInstance(container);
            var services = new StatementCompileTimeServices(1, moduleServices);
            var raw = new StatementRawInfo(1, "abc", null, StatementType.SELECT, null, null, null, null);
            return new ExprValidationContextBuilder(streamTypeService, raw, services).Build();
        }
    }
} // end of namespace