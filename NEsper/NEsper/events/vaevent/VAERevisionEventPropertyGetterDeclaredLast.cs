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
    public class VAERevisionEventPropertyGetterDeclaredLast : EventPropertyGetterSPI
    {
        private readonly EventPropertyGetterSPI _fullGetter;

        public VAERevisionEventPropertyGetterDeclaredLast(EventPropertyGetterSPI fullGetter)
        {
            _fullGetter = fullGetter;
        }

        public object Get(EventBean eventBean)
        {
            var riv = (RevisionEventBeanDeclared) eventBean;
            var bean = riv.LastBaseEvent;
            if (bean == null) return null;
            return _fullGetter.Get(bean);
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
            return LocalMethod(GetCodegen(context), beanExpression);
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
            throw VAERevisionEventPropertyGetterDeclaredGetVersioned.RevisionImplementationNotProvided();
        }

        public ICodegenExpression CodegenUnderlyingFragment(ICodegenExpression underlyingExpression,
            ICodegenContext context)
        {
            throw VAERevisionEventPropertyGetterDeclaredGetVersioned.RevisionImplementationNotProvided();
        }

        private string GetCodegen(ICodegenContext context)
        {
            return context.AddMethod(typeof(object), typeof(EventBean), "eventBean", GetType())
                .DeclareVar(typeof(RevisionEventBeanDeclared), "riv",
                    Cast(typeof(RevisionEventBeanDeclared), Ref("eventBean")))
                .DeclareVar(typeof(EventBean), "bean",
                    ExprDotMethod(Ref("riv"), "getLastBaseEvent"))
                .IfRefNullReturnNull("bean")
                .MethodReturn(_fullGetter.CodegenEventBeanGet(Ref("bean"), context));
        }
    }
} // end of namespace