/******************************************************************************
 * Copyright (c) 2022, Centrum Wiskunde & Informatica (CWI)
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Contributors:
 *   * Riemer van Rozen - rozen@cwi.nl - CWI
 ******************************************************************************/
module lang::tel::gen::Language
import lang::tel::AST;
import lang::tel::gen::Util;
import lang::tel::Printer;

private Extends ExtendsILanguage =
  ext_class([name(id("ILanguage")),name(id("IPatchable"))]);

private Attribute AttributePreMigrator = genPrivateReadolyAttribute("PreMigrator", "preMigrator");

private Attribute AttributePostMigrator = genPrivateReadolyAttribute("PostMigrator", "postMigrator");

private Attribute AttributeGenerator = genPrivateReadolyAttribute("Generator", "generator");

private Attribute AttributeInverter = genPrivateReadolyAttribute("Inverter", "inverter");

private Attribute genAttributeLanguageName(Name langName) =
  genConstant("String", "LanguageName", e_val(v_str(toString(langName))));

private Method LanguageConstructor = 
  constructor(vis_public(), id("Language"),
    [param(when_none(), t_class(id("Patcher")), id("patcher"))],
    body([
      s_declare_assign(t_class(id("UUID")), id("id"),
        e_new(t_class(id("UUID")), [e_val(v_int(0))])),
      s_assign(name(id("metaObject")),
        e_new(t_class(id("MetaObject\<Language\>")), [e_val(v_var(name(id("id"))))])),
      s_call(name(id("metaObject"), [p_field(id("bind"))]), [e_val(v_var(name(id("this"))))]),
      s_assign(name(id("preMigrator")),
        e_new(t_class(id("PreMigrator")), [e_val(v_var(name(id("patcher"))))])),
      s_assign(name(id("postMigrator")),
        e_new(t_class(id("PostMigrator")), [e_val(v_var(name(id("patcher"))))])),
      s_assign(name(id("generator")),
        e_new(t_class(id("Generator")), [e_val(v_var(name(id("patcher"))))])),
      s_assign(name(id("inverter")),
        e_new(t_class(id("Inverter")), []))
  ]));

private Method InitMethod =
  method(vis_public(), poly_none(), t_void(), id("init"), [], body([]));

private Method DeinitMethod =
  method(vis_public(), poly_none(), t_void(), id("deinit"), [], body([]));
  
private Method GetNameMethod =
  method(vis_public(), poly_none(), t_str(), id("getName"), [],
    body([
      s_return(e_val(v_var(name(id("LanguageName"))))) 
  ]));

private Method GenerateMethod = 
  method(vis_public(), poly_none(), t_void(), id("generate"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
      s_call(name(id("generator"),[p_field(id("generate"))]),
        [e_val(v_var(name(id("evt"))))])
    ]));

private Method PreMigrateMethod = 
  method(vis_public(), poly_none(), t_void(), id("preMigrate"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
      s_call(name(id("preMigrator"),[p_field(id("preMigrate"))]),
        [e_val(v_var(name(id("evt"))))])
    ]));

private Method PostMigrateMethod = 
  method(vis_public(), poly_none(), t_void(), id("postMigrate"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
      s_call(name(id("postMigrator"),[p_field(id("postMigrate"))]),
        [e_val(v_var(name(id("evt"))))])
    ]));

private Method InvertMethod = 
  method(vis_public(), poly_none(), t_class(id("Event")), id("invert"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
      s_return(e_call(name(id("inverter"),[p_field(id("invert"))]),
        [e_val(v_var(name(id("evt"))))]))
    ]));

private Method NotifyScheduledMethod = 
  method(vis_public(), poly_none(), t_void(), id("notifyScheduled"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
    ]));

private Method NotifyGeneratedMethod = 
  method(vis_public(), poly_none(), t_void(), id("notifyGenerated"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
    ]));

private Method NotifyCommittedMethod = 
  method(vis_public(), poly_none(), t_void(), id("notifyCommitted"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
    ]));

private Method NotifyCompletedMethod = 
  method(vis_public(), poly_none(), t_void(), id("notifyCompleted"),
    [param(when_none(), t_class(id("Event")), id("evt"))],
    body([
    ]));
    
private Method WriteMethod = 
  method(vis_public(), poly_none(), t_void(), id("write"),
    [param(when_none(), t_class(id("string")), id("text"))],
    body([
    ]));
    
private Method WriteErrorMethod = 
  method(vis_public(), poly_none(), t_void(), id("writeError"),
    [param(when_none(), t_class(id("string")), id("text"))],
    body([
    ]));

private Method genGetEventTypes(list[ID] eventClassNames) =
  method(vis_public(), poly_none(), t_array(id("Type")), id("getEventTypes"),
    [],
    body([
      s_declare_assign(t_array(id("Type")), id("types"),
        e_val(v_array(
        [e_call(name(id("typeof")),[e_val(v_var(name(eventClassName)))]) | ID eventClassName <- eventClassNames]
        ))
      ),
      s_return(e_val(v_var(name(id("types")))))
    ]));

public Package genLanguage(Package inputPackage, Package modelPackage, Package operationPackage)
{
  Name runtimePackageName = name(inputPackage.name.head, inputPackage.name.path + [p_field(id("Runtime"))]); 

  Using using = genRuntimeUsing(inputPackage);
  
  ID languageName = id("Language");

  list[Attribute] attributes = [genAttributeLanguageName(inputPackage.name), AttributePreMigrator, AttributePostMigrator, AttributeGenerator, AttributeInverter, MetaObjectAttribute];

  list[ID] eventClassNames = [];
  visit(operationPackage)
  {
    case class(Vis vis, Imp imp, ID eventClassName, Extends extends,
           list[Attribute] attributes, list[Method] methods):   
    {
      eventClassNames += eventClassName;
    }
  }
  
  list[Method] methods = [LanguageConstructor, InitMethod, DeinitMethod, GetNameMethod,
    GenerateMethod, PreMigrateMethod, PostMigrateMethod, InvertMethod,
    NotifyScheduledMethod, NotifyGeneratedMethod, NotifyCommittedMethod, NotifyCompletedMethod,
    WriteMethod, WriteErrorMethod, genGetEventTypes(eventClassNames),
    GetMetaObjectMethod, SetMetaObjectMethod];  

  return package(using, runtimePackageName,
    [
      class(vis_public(), imp_concrete(), languageName, ExtendsILanguage, attributes, methods)
    ]);
}