using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace NEsper.Examples.OHLC
{
    public class OHLCBarPlugInViewForge : ViewFactoryForge
    {
        private IContainer _container;
        private IList<ExprNode> _viewParameters;
        private ExprNode _timestampExpression;
        private ExprNode _valueExpression;
        private EventType _eventType;

        public EventType EventType => _eventType;

        public string ViewName => nameof(OHLCBarPlugInView);

        public void SetViewParameters(
            IList<ExprNode> parameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            this._viewParameters = parameters;
        }

        public void Accept(ViewForgeVisitor visitor)
        {
        }

        public IList<StmtClassForgeableFactory> InitAdditionalForgeables(ViewForgeEnv viewForgeEnv)
        {
            return EmptyList<StmtClassForgeableFactory>.Instance;
        }

        public IList<ViewFactoryForge> InnerForges => EmptyList<ViewFactoryForge>.Instance;

        public void Attach(EventType parentEventType, ViewForgeEnv env)
        {
            if (_viewParameters.Count != 2) {
                throw new ViewParameterException(
                    "View requires a two parameters: the expression returning timestamps and the expression supplying OHLC data points");
            }

            var validatedNodes = ViewForgeSupport.Validate("OHLC view", parentEventType, _viewParameters, false, env);

            _timestampExpression = validatedNodes[0];
            _valueExpression = validatedNodes[1];

            if (!_timestampExpression.Forge.EvaluationType.IsInt64()) {
                throw new ViewParameterException("View requires long-typed timestamp values in parameter 1");
            }
            if (!_valueExpression.Forge.EvaluationType.IsTypeDouble()) {
                throw new ViewParameterException("View requires double-typed values for in parameter 2");
            }

            /*
             * Allocate a custom event type for this example. This event type will be a Bean event type.
             */
            // make event type name
            var outputEventTypeName = env.StatementCompileTimeServices.EventTypeNameGeneratorStatement.GetViewDerived(ViewName, env.StreamNumber);

            // make event type metadata
            var metadata = new EventTypeMetadata(
                outputEventTypeName,
                env.ModuleName,
                EventTypeTypeClass.VIEWDERIVED,
                EventTypeApplicationType.CLASS,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());

            // for Bean event types, make a stem
            var stem = env.StatementCompileTimeServices.BeanEventTypeStemService.GetCreateStem(typeof(OHLCBarValue), null);

            // make bean event type
            _eventType = new BeanEventType(
                _container,
                stem,
                metadata,
                env.BeanEventTypeFactoryProtected,
                null,
                null,
                null,
                null);

            // register bean type
            env.EventTypeModuleCompileTimeRegistry.NewType(_eventType);
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            return new SAIFFInitializeBuilder(typeof(OHLCBarPlugInViewFactory), GetType(), "factory", parent, (SAIFFInitializeSymbol) symbols, classScope)
                .Exprnode("timestampExpression", _timestampExpression)
                .Exprnode("valueExpression", _valueExpression)
                .Build();
        }

        public T Accept<T>(ViewFactoryForgeVisitor<T> visitor)
        {
            return visitor.VisitExtension(this);
        }
    }
}
