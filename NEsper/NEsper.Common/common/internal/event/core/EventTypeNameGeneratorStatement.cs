///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventTypeNameGeneratorStatement
    {
        private readonly int statementNumber;
        private int seqNum;

        public EventTypeNameGeneratorStatement(int statementNumber)
        {
            this.statementNumber = statementNumber;
        }

        public string AnonymousTypeName => FormatSingleConst("out");

        public string GetAnonymousTypeNameWithInner(int expressionNum)
        {
            return FormatNV("eval", Convert.ToString(expressionNum));
        }

        public string GetAnonymousDBHistorical(int streamNum)
        {
            return FormatNV("dbpoll", Convert.ToString(streamNum));
        }

        public string GetAnonymousMethodHistorical(int streamNum)
        {
            return FormatNV("methodpoll", Convert.ToString(streamNum));
        }

        public string AnonymousRowrecogCompositeName => FormatSingleConst("mrcomp");

        public string AnonymousRowrecogRowName => FormatSingleConst("mrrow");

        public string GetAnonymousRowrecogMultimatchDefineName(int defineNum)
        {
            return FormatNV("mrmd", Convert.ToString(defineNum));
        }

        public string AnonymousRowrecogMultimatchAllName => FormatSingleConst("mrma");

        public string GetPatternTypeName(int stream)
        {
            return FormatNV("pat", Convert.ToString(stream));
        }

        public string GetViewDerived(string name, int streamNum)
        {
            return FormatNV("view", name + "(" + streamNum + ")");
        }

        public string GetViewExpr(int streamNumber)
        {
            return FormatNV("expr", Convert.ToString(streamNumber));
        }

        public string GetViewGroup(int streamNum)
        {
            return FormatNV("grp", Convert.ToString(streamNum));
        }

        public string GetAnonymousTypeNameEnumMethod(string enumMethod, string propertyName)
        {
            return FormatNV("enum", enumMethod + "(" + propertyName + ")");
        }

        public string GetAnonymousTypeSubselectMultirow(int subselectNumber)
        {
            return FormatNV("subq", Convert.ToString(subselectNumber));
        }

        public string GetAnonymousTypeNameUDFMethod(string methodName, string typeName)
        {
            return FormatNV("mth", methodName + "(" + typeName + ")");
        }

        public string GetAnonymousPatternName(int streamNum, short factoryNodeId)
        {
            return FormatNV("pan", streamNum + "(" + factoryNodeId + ")");
        }

        public string GetAnonymousPatternNameWTag(int streamNum, short factoryNodeId, string tag)
        {
            return FormatNV("pwt", streamNum + "(" + factoryNodeId + "_" + tag + ")");
        }

        public string GetContextPropertyTypeName(string contextName)
        {
            return FormatNV("ctx", contextName);
        }

        public string GetContextStatementTypeName(string contextName)
        {
            return FormatNV("ctxout", contextName);
        }

        public string GetDataflowOperatorTypeName(int operatorNumber)
        {
            return FormatNV("df", "op_" + operatorNumber);
        }

        private string FormatSingleConst(string postfixConst)
        {
            var builder = new StringBuilder();
            builder.Append("stmt");
            builder.Append(statementNumber);
            builder.Append("_");
            builder.Append(postfixConst);
            builder.Append(seqNum++);
            return builder.ToString();
        }

        private string FormatNV(string postfixNameOne, string postfixValueOne)
        {
            var builder = new StringBuilder();
            builder.Append("stmt");
            builder.Append(statementNumber);
            builder.Append("_");
            builder.Append(postfixNameOne);
            builder.Append("_");
            builder.Append(postfixValueOne);
            builder.Append("_");
            builder.Append(seqNum++);
            return builder.ToString();
        }
    }
} // end of namespace