using System;
using System.Linq;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.createcontext;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class StatementAIFactoryProvider_e14685ee45728e45463445af0445caad22971c04_ctx : StatementAIFactoryProvider {
    internal StatementFields_e14685ee45728e45463445af0445caad22971c04_ctx statementFields;
    internal Guid uid;
    internal StatementAgentInstanceFactoryCreateContext statementAIFactory;

    public StatementAIFactoryProvider_e14685ee45728e45463445af0445caad22971c04_ctx(EPStatementInitServices stmtInitSvc,StatementFields_e14685ee45728e45463445af0445caad22971c04_ctx statementFields){
      this.statementFields=statementFields;
      uid=Guid.NewGuid();
      statementFields.Init(stmtInitSvc);
      statementAIFactory=M0(stmtInitSvc);
    }

    // StmtClassForgableAIFactoryProviderCreateContext --- StmtClassForgableAIFactoryProviderBase.Forge():0
    public StatementAgentInstanceFactory Factory {
      get {
        return statementAIFactory;
      }
    }
    // StmtClassForgableAIFactoryProviderCreateContext --- StmtClassForgableAIFactoryProviderBase.Forge():101
    public void Assign(StatementAIFactoryAssignments assignments){
      statementFields.Assign(assignments);
    }

    // StmtClassForgableAIFactoryProviderCreateContext --- StmtClassForgableAIFactoryProviderBase.Forge():109
    public void Unassign(){
      statementFields.Unassign();
    }

    // StmtClassForgableStmtFields --- StmtClassForgableAIFactoryProviderBase.Forge():116
    public void SetValue(int index,object value){
    }

    // StmtClassForgableAIFactoryProviderCreateContext --- StmtClassForgableAIFactoryProviderCreateContext.CodegenConstructorInit():57
    StatementAgentInstanceFactoryCreateContext M0(EPStatementInitServices stmtInitSvc){
      stmtInitSvc.ActivateContext("HashByUserCtx",M1(stmtInitSvc));
      return M11(stmtInitSvc);
    }

    // StmtClassForgableAIFactoryProviderCreateContext --- StmtClassForgableAIFactoryProviderCreateContext.GetDefinition():77
    ContextDefinition M1(EPStatementInitServices stmtInitSvc){
      ContextControllerFactory[] controllers=new ContextControllerFactory[1];
      controllers[0]=M2(stmtInitSvc);
      (controllers[0]).WithFactoryEnv(new ContextControllerFactoryEnv("HashByUserCtx","HashByUserCtx",1,1));
      ContextDefinition def=new ContextDefinition();
      def.ContextName="HashByUserCtx";
      def.ControllerFactories=controllers;
      def.EventTypeContextProperties=stmtInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("stmt0_ctx_HashByUserCtx_0",null,EventTypeTypeClass.CONTEXTPROPDERIVED,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L)));
      return def;
    }

    // ContextControllerHashFactoryForge --- ContextControllerHashFactoryForge.MakeCodegen():48
    ContextControllerHashFactory M2(EPStatementInitServices stmtInitSvc){
      ContextControllerHashFactory factory=stmtInitSvc.ContextServiceFactory.HashFactory();
      factory.HashSpec=M3(stmtInitSvc);
      return factory;
    }

    // ContextSpecHash --- ContextSpecHash.MakeCodegen():43
    ContextControllerDetailHash M3(EPStatementInitServices stmtInitSvc){
      ContextControllerDetailHashItem[] items=new ContextControllerDetailHashItem[1];
      items[0]=M4(stmtInitSvc);
      ContextControllerDetailHash detail=new ContextControllerDetailHash();
      detail.Items=items;
      detail.Granularity=10000000;
      detail.IsPreallocate=false;
      return detail;
    }

    // ContextSpecHashItem --- ContextSpecHashItem.MakeCodegen():46
    ContextControllerDetailHashItem M4(EPStatementInitServices stmtInitSvc){
      EventType eventType=stmtInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("SupportBean_S0",null,EventTypeTypeClass.STREAM,EventTypeApplicationType.CLASS,NameAccessModifier.PRECONFIGURED,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(90755863L,-1L)));
      ContextControllerDetailHashItem item=new ContextControllerDetailHashItem();
      ExprFilterSpecLookupable lookupable=M7(eventType,stmtInitSvc);
      item.FilterSpecActivatable=M5(stmtInitSvc);
      item.Lookupable=lookupable;
      stmtInitSvc.FilterSharedLookupableRegistery.RegisterLookupable(eventType,lookupable);
      return item;
    }

    // FilterSpecCompiled --- FilterSpecCompiled.MakeCodegen():241
    FilterSpecActivatable M5(EPStatementInitServices stmtInitSvc){
      EventType eventType=stmtInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("SupportBean_S0",null,EventTypeTypeClass.STREAM,EventTypeApplicationType.CLASS,NameAccessModifier.PRECONFIGURED,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(90755863L,-1L)));
      FilterSpecParam[][] parameters=M6(eventType,stmtInitSvc);
      FilterSpecActivatable activatable=new FilterSpecActivatable(eventType,"SupportBean_S0",parameters,null,0);
      stmtInitSvc.FilterSpecActivatableRegistry.Register(activatable);
      return activatable;
    }

    // FilterSpecParamForge --- FilterSpecParamForge.MakeParamArrayArrayCodegen():66
    FilterSpecParam[][] M6(EventType eventType,EPStatementInitServices stmtInitSvc){
      FilterSpecParam[][] parameters=new FilterSpecParam[0][];
      return parameters;
    }

    // ContextSpecHashItem --- ContextSpecHashItem.MakeCodegen():54
    ExprFilterSpecLookupable M7(EventType eventType,EPStatementInitServices stmtInitSvc){
      return M8(eventType,stmtInitSvc);
    }

    // ExprFilterSpecLookupableForge --- ExprFilterSpecLookupableForge.MakeCodegen():53
    ExprFilterSpecLookupable M8(EventType eventType,EPStatementInitServices stmtInitSvc){
      EventPropertyValueGetter getter=new ProxyEventPropertyValueGetter((EventBean bean) => {    return M9(bean);
});
      ExprFilterSpecLookupable lookupable=new ExprFilterSpecLookupable("consistent_hash_crc32(P00)",getter,typeof(int?),true);
      stmtInitSvc.FilterSharedLookupableRegistery.RegisterLookupable(eventType,lookupable);
      return lookupable;
    }

    // ContextControllerHashedGetterCRC32SingleForge --- ContextControllerHashedGetterCRC32SingleForge.EventBeanGetCodegen():68
    object M9(EventBean eventBean){
      EventBean[] events=new EventBean[]{eventBean};
      string code=((string)M10(events,true,null));
      return ContextControllerHashedGetterCRC32SingleForge.StringToCRC32Hash(code,10000000);
    }

    // CodegenLegoMethodExpression --- CodegenLegoMethodExpression.CodegenExpression():78
    string M10(EventBean[] eventsPerStream,bool isNewData,ExprEvaluatorContext exprEvalCtx){
      // #1: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Common\common\internal\epl\expression\codegen\CodegenLegoMethodExpression.cs, Line: 82, Method: CodegenExpression;
      // #2: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Common\common\internal\context\controller\hash\ContextControllerHashedGetterCRC32SingleForge.cs, Line: 70, Method: EventBeanGetCodegen;
      // #3: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Common\common\internal\epl\expression\core\ExprFilterSpecLookupableForge.cs, Line: 68, Method: MakeCodegen;
      // #4: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Common\common\internal\compile\stage1\spec\ContextSpecHashItem.cs, Line: 61, Method: MakeCodegen;
      SupportBean_S0 u0=((SupportBean_S0)(eventsPerStream[0]).Underlying);
      return ((string)((string)u0.P00));
    }

    // StatementAgentInstanceFactoryCreateContextForge --- StatementAgentInstanceFactoryCreateContextForge.InitializeCodegen():36
    StatementAgentInstanceFactoryCreateContext M11(EPStatementInitServices stmtInitSvc){
      StatementAgentInstanceFactoryCreateContext saiff=new StatementAgentInstanceFactoryCreateContext();
      saiff.ContextName="HashByUserCtx";
      saiff.StatementEventType=stmtInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("stmt0_ctxout_HashByUserCtx_1",null,EventTypeTypeClass.STATEMENTOUT,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L)));
      stmtInitSvc.AddReadyCallback(saiff);
      return saiff;
    }
  }
}
