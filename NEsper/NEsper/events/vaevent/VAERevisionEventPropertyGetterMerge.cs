///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.codegen.core;
using com.espertech.esper.codegen.model.expression;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.vaevent
{
    public class VAERevisionEventPropertyGetterMerge : EventPropertyGetterSPI
    {
        private readonly RevisionGetterParameters _parameters;

        public VAERevisionEventPropertyGetterMerge(RevisionGetterParameters parameters)
        {
            _parameters = parameters;
        }

        public object Get(EventBean eventBean)
        {
            var riv = (RevisionEventBeanMerge) eventBean;
            return riv.GetVersionedValue(_parameters);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public object GetFragment(EventBean eventBean)
        {
            return null; // fragments no provided by revision events
        }

        public ICodegenExpression CodegenEventBeanGet(ICodegenExpression beanExpression, ICodegenContext context)
        {
            var member = context.MakeAddMember(typeof(RevisionGetterParameters), _parameters);
            return ExprDotMethod(
                Cast(typeof(RevisionEventBeanMerge), beanExpression), "GetVersionedValue",
                Ref(member.MemberName));
        }

        public ICodegenExpression CodegenEventBeanExists(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenEventBeanFragment(ICodegenExpression beanExpression, ICodegenContext context)
        {
            return ConstantNull();
        }

        public ICodegenExpression CodegenUnderlyingGet(ICodegenExpression underlyingExpression, ICodegenContext context)
        {
            throw VAERevisionEventPropertyGetterDeclaredGetVersioned.RevisionImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantTrue();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            return ConstantNull();
        }
    }
} // end of namespace