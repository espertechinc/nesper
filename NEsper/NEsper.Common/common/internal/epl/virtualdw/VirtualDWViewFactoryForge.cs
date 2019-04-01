///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDWViewFactoryForge : ViewFactoryForge,
        DataWindowViewForge
    {
        private readonly object _customConfigs;
        private readonly VirtualDataWindowForge _forge;
        private readonly string _namedWindowName;

        private IList<ExprNode> _parameters;
        private object[] _parameterValues;
        private int _streamNumber;
        private ExprNode[] _validatedParameterExpressions;
        private ViewForgeEnv _viewForgeEnv;

        public VirtualDWViewFactoryForge(Type clazz, string namedWindowName, object customConfigs)
        {
            if (!clazz.IsImplementsInterface(typeof(VirtualDataWindowForge))) {
                throw new ViewProcessingException(
                    "Virtual data window forge class " + clazz.Name + " does not implement the interface " +
                    typeof(VirtualDataWindowForge).Name);
            }

            _forge = TypeHelper.Instantiate<VirtualDataWindowForge>(clazz);
            _namedWindowName = namedWindowName;
            _customConfigs = customConfigs;
        }

        public ISet<string> UniqueKeys => _forge.UniqueKeyPropertyNames;

        public void Attach(EventType parentEventType, int streamNumber, ViewForgeEnv viewForgeEnv)
        {
            EventType = parentEventType;

            _validatedParameterExpressions = ViewForgeSupport.Validate(
                ViewName, parentEventType, _parameters, true, viewForgeEnv, streamNumber);
            _parameterValues = new object[_validatedParameterExpressions.Length];
            for (var i = 0; i < _validatedParameterExpressions.Length; i++) {
                try {
                    _parameterValues[i] = ViewForgeSupport.EvaluateAssertNoProperties(
                        ViewName, _validatedParameterExpressions[i], i);
                }
                catch (Exception ex) {
                    // expected
                }
            }

            // initialize
            try {
                _forge.Initialize(
                    new VirtualDataWindowForgeContext(
                        parentEventType, _parameterValues, _validatedParameterExpressions, _namedWindowName, viewForgeEnv,
                        _customConfigs));
            }
            catch (EPException ex) {
                throw;
            }
            catch (Exception ex) {
                throw new ViewParameterException(
                    "Validation exception initializing virtual data window '" + _namedWindowName + "': " + ex.Message,
                    ex);
            }
        }

        public EventType EventType { get; private set; }

        public string ViewName => "virtual-data-window";

        public CodegenExpression Make(
            CodegenMethodScope parent, SAIFFInitializeSymbol symbols, CodegenClassScope classScope)
        {
            var mode = _forge.FactoryMode;
            if (!(mode is VirtualDataWindowFactoryModeManaged)) {
                throw new ArgumentException("Unexpected factory mode " + mode);
            }

            var managed = (VirtualDataWindowFactoryModeManaged) mode;
            var injectionStrategy = (InjectionStrategyClassNewInstance) managed.InjectionStrategyFactoryFactory;
            var factoryField = classScope.AddFieldUnshared(
                true, typeof(VirtualDataWindowFactoryFactory),
                injectionStrategy.GetInitializationExpression(classScope));

            var builder = new SAIFFInitializeBuilder(
                typeof(VirtualDWViewFactory), GetType(), "factory", parent, symbols, classScope);
            builder
                .Eventtype("eventType", EventType)
                .Expression(
                    "factory",
                    ExprDotMethod(
                        factoryField, "createFactory",
                        NewInstance(typeof(VirtualDataWindowFactoryFactoryContext))))
                .Constant("parameters", _parameterValues)
                .Expression(
                    "parameterExpressions",
                    ExprNodeUtilityCodegen.CodegenEvaluators(
                        _validatedParameterExpressions, builder.Method, GetType(), classScope))
                .Constant("namedWindowName", _namedWindowName)
                .Expression(
                    "compileTimeConfiguration",
                    SerializerUtil.ExpressionForUserObject(_customConfigs));
            return builder.Build();
        }

        public void SetViewParameters(IList<ExprNode> parameters, ViewForgeEnv viewForgeEnv, int streamNumber)
        {
            _parameters = parameters;
            _viewForgeEnv = viewForgeEnv;
            _streamNumber = streamNumber;
        }

        public void Accept(ViewForgeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace