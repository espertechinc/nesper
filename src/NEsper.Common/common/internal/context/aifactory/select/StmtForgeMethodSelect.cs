///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StmtForgeMethodSelect : StmtForgeMethod
    {
        private readonly StatementBaseInfo _base;

        public StmtForgeMethodSelect(StatementBaseInfo @base)
        {
            _base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var forgeablesResult = StmtForgeMethodSelectUtil.Make(
                services.Container,
                false,
                @namespace,
                classPostfix,
                _base,
                services);
            return forgeablesResult.ForgeResult;
        }
    }
} // end of namespace