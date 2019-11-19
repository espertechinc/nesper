///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.util
{
    public class SimpleTypeParserCodegenFieldSharable : CodegenFieldSharable
    {
        private readonly SimpleTypeParserSPI parser;
        private readonly CodegenClassScope classScope;

        public SimpleTypeParserCodegenFieldSharable(
            SimpleTypeParserSPI parser,
            CodegenClassScope classScope)
        {
            this.parser = parser;
            this.classScope = classScope;
        }

        public Type Type()
        {
            return typeof(SimpleTypeParser);
        }

        public CodegenExpression InitCtorScoped()
        {
            var parse = new CodegenExpressionLambda(classScope.NamespaceScope.InitMethod.Block)
                .WithParam<string>("text");
            var anonymousClass = NewInstance<ProxySimpleTypeParser>(parse);

            //CodegenExpressionNewAnonymousClass) anonymousClass = NewAnonymousClass(
            //    classScope.NamespaceScope.InitMethod.Block,
            //    typeof(SimpleTypeParser));
            //CodegenMethod parse = CodegenMethod.MakeParentNode(typeof(object), this.GetType(), classScope)
            //    .AddParam(typeof(string), "text");
            //anonymousClass.AddMethod("Parse", parse);

            parse.Block.BlockReturn(parser.Codegen(@Ref("text")));
            return anonymousClass;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            SimpleTypeParserCodegenFieldSharable that = (SimpleTypeParserCodegenFieldSharable) o;

            return parser.Equals(that.parser);
        }

        public override int GetHashCode()
        {
            return parser.GetHashCode();
        }
    }
} // end of namespace