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
using com.espertech.esper.compat;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.vaevent
{
    public class VAERevisionEventPropertyGetterDeclaredGetVersioned : EventPropertyGetterSPI
    {
        private readonly RevisionGetterParameters _parameters;

        public VAERevisionEventPropertyGetterDeclaredGetVersioned(RevisionGetterParameters parameters)
        {
            _parameters = parameters;
        }

        public object Get(EventBean eventBean)
        {
            var riv = (RevisionEventBeanDeclared) eventBean;
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
                Cast(typeof(RevisionEventBeanDeclared), beanExpression), "GetVersionedValue",
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
            throw RevisionImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingExists(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw RevisionImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw RevisionImplementationNotProvided();
        }

        internal static UnsupportedOperationException RevisionImplementationNotProvided()
        {
            return new UnsupportedOperationException(
                "Revision event type does not provide an implementation for underlying get");
        }
    }
} // end of namespace