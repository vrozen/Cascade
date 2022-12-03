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
module lang::tel::gen::PreMigrator

import IO;
import Node;
import String;
import List;
import lang::tel::AST;
import lang::tel::Printer;
import lang::tel::Checker;
import lang::tel::gen::Util;
import lang::tel::gen::Generator;
import lang::tel::gen::PostMigrator;

private Extends ExtendsIPreMigrator =
  ext_class([name(id("IPreMigrator")),name(id("Operation"),[p_field(id("IVisitor"))])]);

private Attribute PatcherAttribute = genPrivateReadolyAttribute("Patcher", "patcher"); 

private Method PreMigrateMethod =
  method(vis_public(), poly_none(), t_void(), id("preMigrate"), [param(when_none(),t_class(id("Event")),id("evt"))],
    body([
       s_if(
         e_is(name(id("evt")),t_class(name(id("Operation"),[p_field(id("IOperation"))])),id("op")),
         body([
           s_call(name(id("op"), [p_field(id("accept"))]), [e_val(v_var(name(id("this"))))])
         ])
       )
    ])
  );

public Package genMigrator(Package inputPackage, Package modelPackage, Package operationPackage,
  set[Package] eachModel, set[Package] eachOperation)
{
  Name runtimePackageName = name(inputPackage.name.head, inputPackage.name.path + [p_field(id("Runtime"))]); 

  Using using = genRuntimeUsing(inputPackage);
  
  Unit preMigrator = genPreMigrator(inputPackage, modelPackage, operationPackage, eachModel, eachOperation);
  
  Unit postMigrator = genPostMigrator(inputPackage, modelPackage, operationPackage, eachModel, eachOperation);
  
  list[Unit] units = [preMigrator, postMigrator];
  
  return package(using, runtimePackageName, units);
}

private Unit genPreMigrator(Package inputPackage, Package modelPackage, Package operationPackage,
  set[Package] eachModel, set[Package] eachOperation)
{
  list[Attribute] attributes = [PatcherAttribute];

  ID className = id("PreMigrator");

  Method constructor = genConstructor(className, [PatcherAttribute]);

  list[Method] methods = [constructor, PreMigrateMethod];
  
  for(Unit cls: evt_class(_,_,_,_,_,_) <- inputPackage.units)
  {
    for(Event event <- cls.events)
    {    
      ID eventClassName = id(cls.name.val + capitalize(event.name.val));
   
      Unit operationClass = lookupClass(operationPackage, eventClassName);
      
      //println(eventClassName);
      
      methods += [genVisitMethod(event, operationClass, cls.name, modelPackage, operationPackage, eachModel, eachOperation)];
    };
  };
 
  return class(vis_public(), imp_concrete(), className, ExtendsIPreMigrator, attributes, methods);
}

private list[Statement] genVisitMethodLocals(Event event, Unit eventClass, ID className,
  set[Package] eachModel, set[Package] eachOperation)
{
  list[Statement] statements = [];

  int pos = 0;
  Param param;
  for(Attribute attr <- eventClass.attributes)
  {
    if(startsWith(attr.name.val, "old"))
    {
      continue;
    }
    
    param = event.params[pos]; 
    pos = pos + 1;
    
    if(t_class(id("IMetaObject")) := attr.typ)
    {
      //todo: check this only happens with first param
      statements += [genLocalVarIMeta(name(attr.name)),
                     genLocalVarCast(t_class(className), name(attr.name))];   
      
    }
    else if(t_meta(Typ ctyp) := delAnnotationsRec(attr.typ))
    {
      if(t_class(ID tid) := delAnnotationsRec(ctyp))
      {
        //println("BLA");
        if(endsWith(eventClass.name.val, "Create") == false)
        {
          statements += [genLocalVarMeta(ctyp, name(attr.name)),
                         genLocalVar(ctyp, name(attr.name))];   
        }
        /*
        Unit attrClass = lookupClass(eachModel, tid);
        if(enum(_,_,_) := attrClass) continue; //hack
        
        for(Attribute attr2 <- attrClass.attributes)
        {
          if(delAnnotationsRec(attr2.own) == own_self() && isBaseType(attr2.typ) == false)
          {
            statements += [genLocalVarFromModel(attr2.typ, name(attr.name, [p_field(attr2.name)])),
                           genLocalVarMetaFromModel(name(attr.name, [p_field(attr2.name)]))];
          }
        }*/
      }
      else
      {
        throw "error type <toString(attr.typ)>";
      }
    }
    else
    {
      statements += [genLocalVarValue(attr.typ, name(attr.name)),
                     genLocalVarBaseType(param.typ, name(attr.name))]; 
    }
  }
  
  return statements;
}

private Method genVisitMethod(Event event: trigger(Imp imp, ID name, list[Param] params, TPost tpost), 
    Unit eventClass, ID className, Package modelPackage, Package operationPackage,
    set[Package] eachModel, set[Package] eachOperation) =
  method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body([]));

private Method genVisitMethod(Event event: signal(ID name, list[Param] params),
    Unit eventClass, ID className, Package modelPackage, Package operationPackage,
    set[Package] eachModel, set[Package] eachOperation) =
  method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body([]));

private Method genVisitMethod(Event event: effect(Imp imp, Inverse inverse, bool sideEffect, ID name, list[Param] params, Ops ops, Pre pre, Post post),
    Unit eventClass, ID className, Package modelPackage, Package operationPackage,
    set[Package] eachModel, set[Package] eachOperation)
{
  Pre pre2 = 
    visit(pre)
    {
      case Body b => genBody(b, modelPackage, operationPackage, eachModel, eachOperation)
    };

  list[Statement] sts = [];
  
  if(pre2.body.statements != [])
  {    
    sts = genVisitMethodLocals(event, eventClass, className, eachModel, eachOperation);  
  }
  
  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body(sts + pre2.body.statements));
}

private Body genBody(Body b, Package modelPackage, Package operationPackage, set[Package] eachModel, set[Package] eachOperation) =
  body([*genStatements(s, modelPackage, operationPackage, eachModel, eachOperation) |  Statement s <- b.statements]);

private list[Statement] genStatements(Statement s: s_foreach(Typ typ, ID arg, Name n, body(list[Statement] statementsIn)),
  Package modelPackage, Package operationPackage, set[Package] eachModel, set[Package] eachOperation)
{
  list[Statement] statements =  [*genStatements(s2, modelPackage, operationPackage, eachModel, eachOperation) | s2 <- statementsIn];
  
  return [s_foreach(typ, arg, n, body(statements))];
}


private list[Statement] genStatements(Statement s: s_call_effect(Name n, list[Exp] operands),
  Package modelPackage, Package operationPackage, set[Package] eachModel, set[Package] eachOperation)
{
  Unit operationClass = lookupClass(eachOperation, id(capitalize(genName(n))));
  //println("pre migrator gen s_call_effect\n  <s>\n  <toString(operationClass.name)>");

  list[Statement] decls = [];
  list[Exp] generatedOperands = [];

  list[Param] params = getConstructorParams(operationClass);
  
  int argPos = 0;
  if(size(params) != size(operands))
  {
    throw "error <toString(n)>( ), found <size(operands)> arguments, expected <size(params)>";
  }
  
  
  for(Exp operand <- operands)
  {
    //println("operand <toString(operand)>");
    Attribute a = operationClass.attributes[argPos];   
     
    if(when_future() := a.when)
    {
      if(e_val(v_var(Name opName)) := operand)
      {
        generatedOperands += [e_cast(a.typ, e_val(v_var(name(id("<genName(opName)>Meta")))))];
      }
      else
      {
        throw "Expected name <operand>";
      }
    }
    else if(when_past() := a.when)
    {
      if(e_val(v_var(Name opName)) := operand)
      {
        generatedOperands += [e_cast(a.typ, e_val(v_var(name(id("<toString(opName)>.getMetaObject()")))))];
      }
      else
      {
        throw "Expected name <operand>";
      }
    }    
    else
    {
      //if(e_val(v_var(Name n)) := operand)
      //{
      generatedOperands += [encode(params[argPos].typ, operand)];
      //}
      //else
      //{
      //  throw "Expected name <operand>";
      //}
    }
    
    argPos = argPos + 1;
  }
  
  list[Statement] r = decls +
  [
    s_call(name(id("op"),[p_field(id("addPreEffect"))]),[
      e_new(t_class(id(capitalize(genName(n)))), generatedOperands
      )])
  ];
  return r;
}

private default list[Statement] genStatements(Statement s, Package modelPackage, Package operationPackage, set[Package] eachModel, set[Package] eachOperation) 
  = [s];


/*
private list[Statement] genStatements(Statement s: s_call_effect(Name n, list[Exp] operands),
  Package modelPackage, Package operationPackage, set[Package] eachModel, set[Package] eachOperation)
{
  Unit operationClass = lookupClass(eachOperation, id(capitalize(genName(n))));
  //println("generate operation <toString(operationClass.name)>");

  list[Statement] decls = [];
  int argPos = 0;
  for(Exp operand <- operands)
  { 
    //println("operand <toString(operand)>");
    Attribute a = operationClass.attributes[argPos];

    if(contains(capitalize(genName(n)), "Create"))
    {
      decls +=
      s_declare_assign(a.typ, id("<toString(operand)>Meta"),
        e_call(
          name(id("patcher"),[p_field(id("createMetaObject\<<toString(a.typ.typ)>\>"))]), 
          []));
    }
    argPos = argPos + 1;
  }
  
  list[Param] params = getConstructorParams(operationClass);
  
  list[Statement] r =
  decls +
  [
    s_call(name(id("op"),[p_field(id("addPreEffect"))]),[
      e_new(t_class(id(capitalize(genName(n)))),
        [encode(params[i].typ, operands[i]) | int i <- [0..size(operands)]]
      )])
  ];
  return r;
}

private default list[Statement] genStatements(Statement s, Package modelPackage, Package operationPackage, set[Package] eachModel, set[Package] eachOperation) = [s];
*/
