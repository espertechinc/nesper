using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.hash;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.module;
using com.espertech.esper.common.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace generated {
  public class ModuleProvider_e14685ee45728e45463445af0445caad22971c04 : ModuleProvider {

    public ModuleProvider_e14685ee45728e45463445af0445caad22971c04(){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public string ModuleName {
      get {
        return null;
      }
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public IDictionary<ModuleProperty,object> ModuleProperties {
      get {
        return Collections.GetEmptyMap<ModuleProperty,object>();
      }
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public ModuleDependenciesRuntime ModuleDependencies {
      get {
        ModuleDependenciesRuntime md=new ModuleDependenciesRuntime();
        md.PathEventTypes=NameAndModule.EMPTY_ARRAY;
        md.PathNamedWindows=NameAndModule.EMPTY_ARRAY;
        md.PathTables=NameAndModule.EMPTY_ARRAY;
        md.PathVariables=NameAndModule.EMPTY_ARRAY;
        md.PathContexts=NameAndModule.EMPTY_ARRAY;
        md.PathExpressions=NameAndModule.EMPTY_ARRAY;
        md.PathIndexes=ModuleIndexMeta.EMPTY_ARRAY;
        md.PathScripts=NameParamNumAndModule.EMPTY_ARRAY;
        md.PublicEventTypes=new string[] {"SupportBean_S0"};
        md.PublicVariables=new string[]{};
        return md;
      }
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():0
    public IList<StatementProvider> Statements {
      get {
        IList<StatementProvider> statements=new List<StatementProvider>(1);
        statements.Add(new generated.StatementProvider_e14685ee45728e45463445af0445caad22971c04_ctx());
        return statements;
      }
    }
    // EPCompilerImpl --- CompilerHelperModuleProvider.MakeInitEventTypes():488
    public void InitializeEventTypes(EPModuleEventTypeInitServices moduleETInitSvc){
      M0(moduleETInitSvc);
      M2(moduleETInitSvc);
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():203
    public void InitializeNamedWindows(EPModuleNamedWindowInitServices moduleNWInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():219
    public void InitializeTables(EPModuleTableInitServices moduleTableInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():229
    public void InitializeIndexes(EPModuleIndexInitServices moduleIdxInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():240
    public void InitializeContexts(EPModuleContextInitServices moduleCtxInitSvc){
      M4(moduleCtxInitSvc);
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():256
    public void InitializeVariables(EPModuleVariableInitServices moduleVarInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():272
    public void InitializeExprDeclareds(EPModuleExprDeclaredInitServices moduleDeclInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.CompileModule():288
    public void InitializeScripts(EPModuleScriptInitServices moduleScriptInitSvc){
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.RegisterEventTypeCodegen():598
    void M0(EPModuleEventTypeInitServices moduleETInitSvc){
      EventTypeMetadata metadata=new EventTypeMetadata("stmt0_ctx_HashByUserCtx_0",null,EventTypeTypeClass.CONTEXTPROPDERIVED,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L));
      LinkedHashMap<string,object> props=M1(moduleETInitSvc);
      moduleETInitSvc.EventTypeCollector.RegisterMap(metadata,props,null,null,null);
    }

    // CompilerHelperModuleProvider --- CompilerHelperModuleProvider.MakePropsCodegen():768
    LinkedHashMap<string,object> M1(EPModuleEventTypeInitServices moduleETInitSvc){
      LinkedHashMap<string,object> props=new LinkedHashMap<string,object>();
      props.Put("name",typeof(string));
      props.Put("id",typeof(int));
      return props;
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.RegisterEventTypeCodegen():598
    void M2(EPModuleEventTypeInitServices moduleETInitSvc){
      EventTypeMetadata metadata=new EventTypeMetadata("stmt0_ctxout_HashByUserCtx_1",null,EventTypeTypeClass.STATEMENTOUT,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L));
      LinkedHashMap<string,object> props=M3(moduleETInitSvc);
      moduleETInitSvc.EventTypeCollector.RegisterMap(metadata,props,null,null,null);
    }

    // CompilerHelperModuleProvider --- CompilerHelperModuleProvider.MakePropsCodegen():768
    LinkedHashMap<string,object> M3(EPModuleEventTypeInitServices moduleETInitSvc){
      LinkedHashMap<string,object> props=new LinkedHashMap<string,object>();
      return props;
    }

    // EPCompilerImpl --- CompilerHelperModuleProvider.RegisterContextCodegen():566
    void M4(EPModuleContextInitServices moduleCtxInitSvc){
      ContextMetaData detail=new ContextMetaData("HashByUserCtx",null,NameAccessModifier.PUBLIC,moduleCtxInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("stmt0_ctx_HashByUserCtx_0",null,EventTypeTypeClass.CONTEXTPROPDERIVED,EventTypeApplicationType.MAP,NameAccessModifier.TRANSIENT,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(-1L,-1L))),new ContextControllerPortableInfo[]{new ContextControllerHashValidation(new ContextControllerHashValidationItem[]{new ContextControllerHashValidationItem(moduleCtxInitSvc.EventTypeResolver.Resolve(new EventTypeMetadata("SupportBean_S0",null,EventTypeTypeClass.STREAM,EventTypeApplicationType.CLASS,NameAccessModifier.PRECONFIGURED,EventTypeBusModifier.NONBUS,false,new EventTypeIdPair(90755863L,-1L))))})});
      moduleCtxInitSvc.ContextCollector.RegisterContext("HashByUserCtx",detail);
    }
  }
}
