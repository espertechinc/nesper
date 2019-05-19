///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StatementBaseInfo
    {
        public StatementBaseInfo(
            Compilable compilable,
            StatementSpecCompiled statementSpec,
            object userObjectCompileTime,
            StatementRawInfo statementRawInfo,
            string optionalModuleName)
        {
            Compilable = compilable;
            StatementSpec = statementSpec;
            UserObjectCompileTime = userObjectCompileTime;
            StatementRawInfo = statementRawInfo;
            ModuleName = optionalModuleName;
        }

        public Compilable Compilable { get; }

        public StatementSpecCompiled StatementSpec { get; set; }

        public string StatementName => StatementRawInfo.StatementName;

        public object UserObjectCompileTime { get; }

        public int StatementNumber => StatementRawInfo.StatementNumber;

        public StatementRawInfo StatementRawInfo { get; }

        public string ModuleName { get; }

        public ContextPropertyRegistry ContextPropertyRegistry {
            get {
                if (StatementRawInfo.OptionalContextDescriptor == null) {
                    return null;
                }

                return StatementRawInfo.OptionalContextDescriptor.ContextPropertyRegistry;
            }
        }

        public string ContextName {
            get {
                if (StatementRawInfo.OptionalContextDescriptor == null) {
                    return null;
                }

                return StatementRawInfo.OptionalContextDescriptor.ContextName;
            }
        }
    }
} // end of namespace