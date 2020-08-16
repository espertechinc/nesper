using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.handthru;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select : ResultSetProcessorFactoryProvider {
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;
    internal EventType resultEventType;
    internal ResultSetProcessorFactory rspFactory;
    internal OrderByProcessorFactory orderByFactory;
    internal AggregationServiceFactory aggFactory;
    internal SelectExprProcessor selectExprProcessor;

    public ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select(EPStatementInitServices stmtInitSvc,StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields){
      this.statementFields=statementFields;
      resultEventType=stmtInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("stmt0_out0",null,EventTypeTypeClass.STATEMENTOUT,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L)));
      rspFactory=new RSPFactory(this);
      orderByFactory=null;
      aggFactory=M0(stmtInitSvc);
      selectExprProcessor=new SelectExprProcessorImpl(this,stmtInitSvc);
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.Forge():0
    public ResultSetProcessorFactory ResultSetProcessorFactory {
      get {
        return rspFactory;
      }
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.Forge():0
    public AggregationServiceFactory AggregationServiceFactory {
      get {
        return aggFactory;
      }
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.Forge():0
    public OrderByProcessorFactory OrderByProcessorFactory {
      get {
        return orderByFactory;
      }
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.Forge():0
    public ResultSetProcessorType ResultSetProcessorType {
      get {
        return ResultSetProcessorType.HANDTHROUGH;
      }
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.Forge():0
    public EventType ResultEventType {
      get {
        return resultEventType;
      }
    }
    // AggregationServiceFactoryCompiler --- AggregationServiceFactoryCompiler.MakeInnerClassesAndInit():76
    AggregationServiceFactory M0(EPStatementInitServices stmtInitSvc){
      return new AggFactory(this);
    }

      class RSPFactory : ResultSetProcessorFactory {
    internal ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;

    public RSPFactory(ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o){
      this.o=o;
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessorFactory():268
    public ResultSetProcessor Instantiate(OrderByProcessor orderByProcessor,AggregationService aggregationService,AgentInstanceContext agentInstanceContext){
      return new RSP(o,orderByProcessor,aggregationService,agentInstanceContext);
    }
  }

      class RSP : ResultSetProcessor {
    internal ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;
    internal OrderByProcessor orderByProcessor;
    internal AggregationService aggregationService;
    internal AgentInstanceContext agentInstanceContext;
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;

    public RSP(ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o,OrderByProcessor orderByProcessor,AggregationService aggregationService,AgentInstanceContext agentInstanceContext){
      this.o=o;
      this.orderByProcessor=orderByProcessor;
      this.aggregationService=aggregationService;
      this.agentInstanceContext=agentInstanceContext;
      this.statementFields=o.statementFields;
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():0
    public EventType ResultEventType {
      get {
        return o.resultEventType;
      }
    }
    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():362
    public UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData,EventBean[] oldData,bool isSynthesize){
      EventBean[] selectOldEvents=null;
      EventBean[] selectNewEvents=ResultSetProcessorHandThroughUtil.GetSelectEventsNoHavingHandThruView(o.selectExprProcessor,newData,true,isSynthesize,agentInstanceContext);
      return new UniformPair<EventBean[]>(selectNewEvents,selectOldEvents);
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():378
    public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newData,ISet<MultiKey<EventBean>> oldData,bool isSynthesize){
      // #1: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Common\common\internal\compile\stage3\StmtClassForgableRSPFactoryProvider.cs, Line: 388, Method: MakeResultSetProcessor;
      // #2: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Common\common\internal\compile\stage3\StmtClassForgableRSPFactoryProvider.cs, Line: 118, Method: Forge;
      // #3: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Compiler\internal\util\CompilerHelperStatementProvider.cs, Line: 301, Method: CompileItem;
      // #4: File: C:\Src\Espertech\NEsper-baseline\NEsper\NEsper.Compiler\internal\util\CompilerHelperModuleProvider.cs, Line: 108, Method: CompileToBytes;
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():403
    public IEnumerator<EventBean> GetEnumerator(Viewable viewable){
      return new TransformEventEnumerator(viewable.GetEnumerator(),new ResultSetProcessorHandtruTransform(this));
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():418
    public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinset){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():398
    public void Clear(){
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():569
    public void Stop(){
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():449
    public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet,bool isSynthesize){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():433
    public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList,bool isSynthesize){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():465
    public void SetAgentInstanceContext(AgentInstanceContext value){
      agentInstanceContext=value;
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():471
    public void ApplyViewResult(EventBean[] newData,EventBean[] oldData){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():483
    public void ApplyJoinResult(ISet<MultiKey<EventBean>> newData,ISet<MultiKey<EventBean>> oldData){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():495
    public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData,EventBean[] oldData,bool isSynthesize){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():511
    public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newData,ISet<MultiKey<EventBean>> oldData,bool isSynthesize){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():527
    public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():545
    public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize){
      throw new UnsupportedOperationException();
    }

    // ResultSetProcessorHandThroughFactoryForge --- StmtClassForgableRSPFactoryProvider.MakeResultSetProcessor():563
    public void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor){
    }
  }

      class AggSvc : AggregationService {
    internal ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;

    public AggSvc(ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o){
      this.o=o;
      this.statementFields=o.statementFields;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():0
    public bool IsGrouped {
      get {
        return false;
      }
    }
    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1074
    public void ApplyEnter(EventBean[] eventsPerStream,object groupKey,ExprEvaluatorContext exprEvalCtx){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1083
    public void ApplyLeave(EventBean[] eventsPerStream,object groupKey,ExprEvaluatorContext exprEvalCtx){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1092
    public void SetCurrentAccess(object groupKey,int agentInstanceId,AggregationGroupByRollupLevel rollupLevel){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1101
    public void ClearResults(ExprEvaluatorContext exprEvalCtx){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1106
    public void SetRemovedCallback(AggregationRowRemovedCallback callback){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1111
    public void Accept(AggregationServiceVisitor visitor){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1116
    public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor){
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1130
    public AggregationService GetContextPartitionAggregationService(int agentInstanceId){
      return this;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1139
    public object GetValue(int column,int agentInstanceId,EventBean[] eventsPerStream,bool isNewData,ExprEvaluatorContext exprEvalCtx){
      return null;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1150
    public ICollection<EventBean> GetCollectionOfEvents(int column,EventBean[] eventsPerStream,bool isNewData,ExprEvaluatorContext exprEvalCtx){
      return null;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1164
    public EventBean GetEventBean(int column,EventBean[] eventsPerStream,bool isNewData,ExprEvaluatorContext exprEvalCtx){
      return null;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1174
    public object GetGroupKey(int agentInstanceId){
      return null;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1179
    public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvalCtx){
      return null;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1188
    public ICollection<object> GetCollectionScalar(int column,EventBean[] eventsPerStream,bool isNewData,ExprEvaluatorContext exprEvalCtx){
      return null;
    }

    // AggregationServiceNullFactory --- AggregationServiceFactoryCompiler.MakeService():1202
    public void Stop(){
    }
  }

      class AggFactory : AggregationServiceFactory {
    internal ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;

    public AggFactory(ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o){
      this.o=o;
      this.statementFields=o.statementFields;
    }

    // AggregationServiceFactoryCompiler --- AggregationServiceFactoryCompiler.MakeFactory():1346
    public AggregationService MakeService(AgentInstanceContext agentInstanceContext,ImportServiceRuntime classpathImportService,bool isSubquery,int? subqueryNumber,int[] groupId){
      return AggregationServiceNull.INSTANCE;
    }
  }

      class SelectExprProcessorImpl : SelectExprProcessor {
    internal ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;
    internal EventBeanTypedEventFactory factory;

    public SelectExprProcessorImpl(ResultSetProcessorFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o,EPStatementInitServices stmtInitSvc){
      this.o=o;
      statementFields=o.statementFields;
      factory=stmtInitSvc.EventBeanTypedEventFactory;
    }

    // StmtClassForgableRSPFactoryProvider --- StmtClassForgableRSPFactoryProvider.MakeSelectExprProcessor():820
    public EventBean Process(EventBean[] eventsPerStream,bool isNewData,bool isSynthesize,ExprEvaluatorContext exprEvalCtx){
      SupportBean_S0 u0=((SupportBean_S0)(eventsPerStream[0]).Underlying);
      EventBean @out=M0(u0);
      return @out;
    }

    // SelectEvalNoWildcardMap --- SelectEvalNoWildcardMap.ProcessCodegen():47
    EventBean M0(SupportBean_S0 u0){
      IDictionary<string,object> props=new HashMap<string,object>();
      props.Put("P01",u0.P01);
      return factory.AdapterForTypedMap(props,o.resultEventType);
    }
  }
  }
}
