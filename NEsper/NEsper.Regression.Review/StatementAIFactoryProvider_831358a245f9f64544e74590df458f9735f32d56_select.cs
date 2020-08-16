using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.aifactory.select;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.view.access;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class StatementAIFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select : StatementAIFactoryProvider {
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;
    internal Guid uid;
    internal StatementAgentInstanceFactorySelect statementAIFactory;

    public StatementAIFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select(EPStatementInitServices stmtInitSvc,StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields){
      this.statementFields=statementFields;
      uid=Guid.NewGuid();
      statementFields.Init(stmtInitSvc);
      statementAIFactory=M0(stmtInitSvc);
    }

    // StmtClassForgableAIFactoryProviderSelect --- StmtClassForgableAIFactoryProviderBase.Forge():0
    public StatementAgentInstanceFactory Factory {
      get {
        return statementAIFactory;
      }
    }
    // StmtClassForgableAIFactoryProviderSelect --- StmtClassForgableAIFactoryProviderBase.Forge():101
    public void Assign(StatementAIFactoryAssignments assignments){
      statementFields.Assign(assignments);
    }

    // StmtClassForgableAIFactoryProviderSelect --- StmtClassForgableAIFactoryProviderBase.Forge():109
    public void Unassign(){
      statementFields.Unassign();
    }

    // StmtClassForgableStmtFields --- StmtClassForgableAIFactoryProviderBase.Forge():116
    public void SetValue(int index,object value){
    }

    // StmtClassForgableAIFactoryProviderSelect --- StmtClassForgableAIFactoryProviderSelect.CodegenConstructorInit():44
    StatementAgentInstanceFactorySelect M0(EPStatementInitServices stmtInitSvc){
      return M1(stmtInitSvc);
    }

    // StatementAgentInstanceFactorySelectForge --- StatementAgentInstanceFactorySelectForge.InitializeCodegen():82
    StatementAgentInstanceFactorySelect M1(EPStatementInitServices stmtInitSvc){
      StatementAgentInstanceFactorySelect saiff=new StatementAgentInstanceFactorySelect();
      saiff.StreamNames=new string[] {"stream_0"};
      ViewableActivator[] activators=new ViewableActivator[1];
      activators[0]=M2(stmtInitSvc);
      saiff.ViewableActivators=activators;
      ViewFactory[][] viewFactories=new ViewFactory[1][];
      viewFactories[0]=M5();
      saiff.ViewFactories=viewFactories;
      ViewResourceDelegateDesc[] viewResourceDelegates=new ViewResourceDelegateDesc[1];
      viewResourceDelegates[0]=new ViewResourceDelegateDesc(false,new SortedSet<int>(CompatExtensions.AsList(new int[]{})));
      saiff.ViewResourceDelegates=viewResourceDelegates;
      ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select rspFactoryProvider=new ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select(stmtInitSvc,statementFields);
      saiff.ResultSetProcessorFactoryProvider=rspFactoryProvider;
      OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select opvFactoryProvider=new OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select(stmtInitSvc,statementFields);
      saiff.OutputProcessViewFactoryProvider=opvFactoryProvider;
      saiff.OrderByWithoutOutputRateLimit=false;
      saiff.IsUnidirectionalJoin=false;
      return saiff;
    }

    // ViewableActivatorFilterForge --- ViewableActivatorFilterForge.MakeCodegen():48
    ViewableActivatorFilter M2(EPStatementInitServices stmtInitSvc){
      FilterSpecActivatable filterSpecCompiled=M3(stmtInitSvc);
      ViewableActivatorFilter activator=stmtInitSvc.ViewableActivatorFactory.CreateFilter();
      activator.Container=stmtInitSvc.Container;
      activator.FilterSpec=filterSpecCompiled;
      activator.CanIterate=false;
      activator.StreamNumFromClause=0;
      activator.IsSubSelect=false;
      activator.SubselectNumber=-1;
      return activator;
    }

    // FilterSpecCompiled --- FilterSpecCompiled.MakeCodegen():241
    FilterSpecActivatable M3(EPStatementInitServices stmtInitSvc){
      EventType eventType=stmtInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("SupportBean_S0",null,EventTypeTypeClass.STREAM,EventTypeApplicationType.CLASS,NameAccessModifier.PRECONFIGURED,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(90755863L,-1L)));
      FilterSpecParam[][] parameters=M4(eventType,stmtInitSvc);
      FilterSpecActivatable activatable=new FilterSpecActivatable(eventType,"SupportBean_S0",parameters,null,0);
      stmtInitSvc.FilterSpecActivatableRegistry.Register(activatable);
      return activatable;
    }

    // FilterSpecParamForge --- FilterSpecParamForge.MakeParamArrayArrayCodegen():66
    FilterSpecParam[][] M4(EventType eventType,EPStatementInitServices stmtInitSvc){
      FilterSpecParam[][] parameters=new FilterSpecParam[0][];
      return parameters;
    }

    // ViewFactoryForgeUtil --- ViewFactoryForgeUtil.CodegenForgesWInit():394
    ViewFactory[] M5(){
      ViewFactory[] factories=new ViewFactory[0];
      ViewFactoryContext ctx=new ViewFactoryContext();
      ctx.StreamNum=0;
      ctx.SubqueryNumber=null;
      ctx.IsGrouped=false;
      return factories;
    }
  }
}
