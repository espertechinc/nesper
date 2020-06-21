///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.json.parser.forge
{
    public class JsonDelegateRefs
    {
        private static readonly CodegenExpression BASEHANDLER = Ref("baseHandler");

        public static readonly JsonDelegateRefs INSTANCE = new JsonDelegateRefs(BASEHANDLER);

        private JsonDelegateRefs(CodegenExpression baseHandler)
        {
            BaseHandler = baseHandler;
        }

        public CodegenExpression BaseHandler { get; }

        public CodegenExpression This => Ref("this");
    }
} // end of namespace