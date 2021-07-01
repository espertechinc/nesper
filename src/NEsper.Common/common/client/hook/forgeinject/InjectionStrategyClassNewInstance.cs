///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.client.hook.forgeinject
{
    /// <summary>
    ///     Provides the compiler with code that allocates and initializes an instance of some class
    ///     by using "new" and by using setters.
    /// </summary>
    public class InjectionStrategyClassNewInstance : InjectionStrategy
    {
        private readonly IDictionary<string, object> _constants = new Dictionary<string, object>();
        private readonly IDictionary<string, ExprNode> _expressions = new Dictionary<string, ExprNode>();

        /// <summary>
        ///     The class to be instantiated.
        /// </summary>
        /// <param name="clazz">class</param>
        public InjectionStrategyClassNewInstance(Type clazz)
        {
            Clazz = clazz ?? throw new ArgumentNullException(nameof(clazz), "Invalid null value for class");
            FullyQualifiedClassName = null;
        }

        /// <summary>
        ///     The class name of the class to be instantiated.
        /// </summary>
        /// <param name="fullyQualifiedClassName">class name</param>
        public InjectionStrategyClassNewInstance(string fullyQualifiedClassName)
        {
            FullyQualifiedClassName = fullyQualifiedClassName ??
                                      throw new ArgumentNullException(
                                          nameof(fullyQualifiedClassName),
                                          "Invalid null value for class name");
            Clazz = null;
        }

        /// <summary>
        ///     Returns the class, or null if providing a class name instead
        /// </summary>
        /// <returns>class</returns>
        public Type Clazz { get; }

        /// <summary>
        ///     Returns the class name, or null if providing a class instead
        /// </summary>
        /// <returns>class name</returns>
        public string FullyQualifiedClassName { get; }

        /// <summary>
        ///     Returns the builder consumer, a consumer that the strategy invokes when it is ready to build the code
        /// </summary>
        /// <value>builder consumer</value>
        public Consumer<SAIFFInitializeBuilder> BuilderConsumer { get; set; }

        public CodegenExpression GetInitializationExpression(CodegenClassScope classScope)
        {
            var symbols = new SAIFFInitializeSymbol();
            SAIFFInitializeBuilder builder;
            CodegenMethod init;
            if (Clazz != null) {
                init = classScope.NamespaceScope.InitMethod
                    .MakeChildWithScope(Clazz, GetType(), symbols, classScope)
                    .AddParam(
                        typeof(EPStatementInitServices),
                        EPStatementInitServicesConstants.REF.Ref);
                builder = new SAIFFInitializeBuilder(Clazz, GetType(), "instance", init, symbols, classScope);
            }
            else {
                init = classScope.NamespaceScope.InitMethod
                    .MakeChildWithScope(FullyQualifiedClassName, GetType(), symbols, classScope)
                    .AddParam(
                        typeof(EPStatementInitServices),
                        EPStatementInitServicesConstants.REF.Ref);
                builder = new SAIFFInitializeBuilder(
                    FullyQualifiedClassName,
                    GetType(),
                    "instance",
                    init,
                    symbols,
                    classScope);
            }

            BuilderConsumer?.Invoke(builder);

            foreach (var constantEntry in _constants) {
                builder.Constant(constantEntry.Key, constantEntry.Value);
            }

            foreach (var exprEntry in _expressions) {
                builder.Exprnode(exprEntry.Key, exprEntry.Value);
            }

            init.Block.MethodReturn(builder.Build());
            return LocalMethod(init, EPStatementInitServicesConstants.REF);
        }

        /// <summary>
        ///     Add a constant to be provided by invoking the setter method of thNamespaceScopedeployment time
        /// </summary>
        /// <param name="name">property name</param>
        /// <param name="value">constant value</param>
        /// <returns>itself</returns>
        public InjectionStrategyClassNewInstance AddConstant(
            string name,
            object value)
        {
            _constants.Put(name, value);
            return this;
        }

        /// <summary>
        ///     Add an expression to be provided by invoking the setter method of the class, at deployment time,
        ///     the setter should accept an
        ///     <seealso cref="com.espertech.esper.common.@internal.epl.expression.core.ExprEvaluator" /> instance.
        /// </summary>
        /// <param name="name">property name</param>
        /// <param name="value">expression</param>
        /// <returns>itself</returns>
        public InjectionStrategyClassNewInstance AddExpression(
            string name,
            ExprNode value)
        {
            _expressions.Put(name, value);
            return this;
        }

        /// <summary>
        ///     Sets the builder consumer, a consumer that the strategy invokes when it is ready to build the code
        /// </summary>
        /// <param name="builderConsumer">builder consumer</param>
        public InjectionStrategyClassNewInstance WithBuilderConsumer(Consumer<SAIFFInitializeBuilder> builderConsumer)
        {
            BuilderConsumer = builderConsumer;
            return this;
        }
    }
} // end of namespace