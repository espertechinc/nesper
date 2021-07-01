///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1;
using com.espertech.esper.common.@internal.context.compile;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class StatementRawInfo
    {
        public StatementRawInfo(
            int statementNumber,
            string statementName,
            Attribute[] annotations,
            StatementType statementType,
            ContextCompileTimeDescriptor optionalContextDescriptor,
            string intoTableName,
            Compilable compilable,
            string moduleName)
        {
            StatementNumber = statementNumber;
            StatementName = statementName;
            Annotations = annotations;
            StatementType = statementType;
            OptionalContextDescriptor = optionalContextDescriptor;
            IntoTableName = intoTableName;
            Compilable = compilable;
            ModuleName = moduleName;
        }

        public int StatementNumber { get; }

        public string StatementName { get; }

        public StatementType StatementType { get; }

        public ContextCompileTimeDescriptor OptionalContextDescriptor { get; }

        public string ContextName => OptionalContextDescriptor == null ? null : OptionalContextDescriptor.ContextName;

        public string IntoTableName { get; }

        public Compilable Compilable { get; }

        public string ModuleName { get; }

        public Attribute[] Annotations { get; }

        public void AppendCodeDebugInfo(TextWriter writer)
        {
            writer.Write("statement ");
            writer.Write(Convert.ToString(StatementNumber));
            writer.Write(" name ");
            writer.Write(StatementName.Replace("\\", ""));
        }
    }
} // end of namespace