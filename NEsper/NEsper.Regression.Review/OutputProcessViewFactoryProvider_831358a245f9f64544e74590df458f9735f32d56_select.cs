using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select : OutputProcessViewFactoryProvider {
    internal StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields;
    internal StatementResultService statementResultService;
    internal OutputProcessViewFactory opvFactory;

    public OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select(EPStatementInitServices stmtInitSvc,StatementFields_831358a245f9f64544e74590df458f9735f32d56_select statementFields){
      this.statementFields=statementFields;
      opvFactory=new OPVFactory(this);
      statementResultService=stmtInitSvc.StatementResultService;
    }

    // StmtClassForgableOPVFactoryProvider --- StmtClassForgableOPVFactoryProvider.Forge():0
    public OutputProcessViewFactory OutputProcessViewFactory {
      get {
        return opvFactory;
      }
    }

      class OPVFactory : OutputProcessViewFactory {
    internal OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;

    public OPVFactory(OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o){
      this.o=o;
    }

    // StmtClassForgableOPVFactoryProvider --- StmtClassForgableOPVFactoryProvider.MakeOPVFactory():183
    public OutputProcessView MakeView(ResultSetProcessor resultSetProcessor,AgentInstanceContext agentInstanceContext){
      return new OPV(o,resultSetProcessor,agentInstanceContext);
    }
  }

      class OPV : OutputProcessView {
    internal OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o;
    internal ResultSetProcessor resultSetProcessor;
    internal AgentInstanceContext agentInstanceContext;

    public OPV(OutputProcessViewFactoryProvider_831358a245f9f64544e74590df458f9735f32d56_select o,ResultSetProcessor resultSetProcessor,AgentInstanceContext agentInstanceContext){
      this.o=o;
      this.resultSetProcessor=resultSetProcessor;
      this.agentInstanceContext=agentInstanceContext;
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():0
    public override EventType EventType {
      get {
        return resultSetProcessor.ResultEventType;
      }
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():0
    public override int NumChangesetRows {
      get {
        return 0;
      }
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():0
    public override OutputCondition OptionalOutputCondition {
      get {
        return null;
      }
    }
    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():243
    public override void Update(EventBean[] newData,EventBean[] oldData){
      bool isGenerateSynthetic=o.statementResultService.IsMakeSynthetic;
      bool isGenerateNatural=o.statementResultService.IsMakeNatural;
      UniformPair<EventBean[]> newOldEvents=resultSetProcessor.ProcessViewResult(newData,oldData,isGenerateSynthetic);
      if ((!(isGenerateSynthetic) && !(isGenerateNatural))) {
        return;
      }
      if (child != null) {
        if (newOldEvents != null) {
          if ((newOldEvents.First != null || newOldEvents.Second != null)) {
            child.NewResult(newOldEvents);
          } else if ((newData == null && oldData == null)) {
            child.NewResult(newOldEvents);
          }
        } else {
          if ((newData == null && oldData == null)) {
            child.NewResult(newOldEvents);
          }
        }
      }
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():256
    public override void Process(ISet<MultiKey<EventBean>> newData,ISet<MultiKey<EventBean>> oldData,ExprEvaluatorContext notApplicable){
      throw new UnsupportedOperationException();
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():272
    public override IEnumerator<EventBean> GetEnumerator(){
      return OutputStrategyUtil.GetEnumerator(joinExecutionStrategy,resultSetProcessor,parentView,false);
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():294
    public override void Stop(AgentInstanceStopServices svc){
    }

    // OutputProcessViewDirectSimpleForge --- StmtClassForgableOPVFactoryProvider.MakeOPV():300
    public override void Terminated(){
    }
  }
  }
}
