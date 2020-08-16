using System;
using System.Linq;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class StatementProvider_e14685ee45728e45463445af0445caad22971c04_ctx : StatementProvider {
    internal StatementFields_e14685ee45728e45463445af0445caad22971c04_ctx statementFields;
    internal StatementInformationalsRuntime statementInformationals;
    internal StatementAIFactoryProvider factoryProvider;

    public StatementProvider_e14685ee45728e45463445af0445caad22971c04_ctx(){
      statementInformationals=M0();
    }

    // StmtClassForgableStmtProvider --- StmtClassForgableStmtProvider.Forge():0
    public StatementInformationalsRuntime Informationals {
      get {
        return statementInformationals;
      }
    }

    // StmtClassForgableStmtProvider --- StmtClassForgableStmtProvider.MakeGetStatementAIFactoryProvider():0
    public StatementAIFactoryProvider StatementAIFactoryProvider {
      get {
        return factoryProvider;
      }
    }
    // StmtClassForgableStmtProvider --- StmtClassForgableStmtProvider.MakeInitialize():116
    public void Initialize(EPStatementInitServices stmtInitSvc){
      statementFields=new StatementFields_e14685ee45728e45463445af0445caad22971c04_ctx();
      factoryProvider=new StatementAIFactoryProvider_e14685ee45728e45463445af0445caad22971c04_ctx(stmtInitSvc,statementFields);
    }

    // StatementInformationalsCompileTime --- StatementInformationalsCompileTime.Make():145
    StatementInformationalsRuntime M0(){
      StatementInformationalsRuntime info=new StatementInformationalsRuntime();
      info.StatementNameCompileTime="ctx";
      info.IsAlwaysSynthesizeOutputEvents=false;
      info.OptionalContextName=null;
      info.OptionalContextModuleName=null;
      info.OptionalContextVisibility=null;
      info.IsCanSelfJoin=false;
      info.HasSubquery=false;
      info.IsNeedDedup=false;
      info.IsStateless=false;
      info.Annotations=M1();
      info.UserObjectCompileTime=null;
      info.NumFilterCallbacks=1;
      info.NumScheduleCallbacks=0;
      info.NumNamedWindowCallbacks=0;
      info.StatementType=StatementType.CREATE_CONTEXT;
      info.Priority=0;
      info.IsPreemptive=false;
      info.HasVariables=true;
      info.IsWritesToTables=false;
      info.HasTableAccess=false;
      info.SelectClauseTypes=null;
      info.SelectClauseColumnNames=null;
      info.IsForClauseDelivery=false;
      info.GroupDeliveryEval=null;
      info.Properties=Collections.SingletonMap(StatementProperty.EPL,((object)"@Name('ctx') create context HashByUserCtx as coalesce by consistent_hash_crc32(P00) from SupportBean_S0 granularity 10000000"));
      info.HasMatchRecognize=false;
      info.AuditProvider=AuditProviderDefault.INSTANCE;
      info.IsInstrumented=false;
      info.InstrumentationProvider=null;
      info.SubstitutionParamTypes=null;
      info.SubstitutionParamNames=null;
      info.InsertIntoLatchName=null;
      info.IsAllowSubscriber=false;
      return info;
    }

    // AnnotationUtil --- AnnotationUtil.MakeAnnotations():121
    Attribute[] M1(){
      Attribute[] annotations=new Attribute[1];
      annotations[0]=new AnnotationName("ctx");
      return annotations;
    }
  }
}
