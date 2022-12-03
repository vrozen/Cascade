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
module lang::tel::gen::Operation

import String;
import IO;
import lang::tel::Printer;
import lang::tel::AST;
import lang::tel::gen::Util;

private Extends genExtends(effect(_,_,_,_,_,_,_,_)) = ext_class([name(id("Effect")), name(id("IOperation"))]);

private Extends genExtends(trigger(_,_,_,_)) = ext_class([name(id("Trigger")), name(id("IOperation"))]);

private Extends genExtends(signal(_,_)) = ext_class([name(id("Signal")), name(id("IOperation"))]);

//future, past or constant parameter
private list[Attribute] genEventAttribute(param(When when, Typ typ, ID name)) =
  [attr(own_self(), when, vis_private(), store_dynamic(), a_readwrite(), genEventAttributeType(typ), name)];

//change parameter
private list[Attribute] genEventAttribute(param_change(When when, Typ typ, ID name, Exp val)) = 
  [
    attr(own_self(), when_none(), vis_private(), store_dynamic(), a_readwrite(), genEventAttributeType(typ), name),
    attr(own_self(), when_none(), vis_private(), store_dynamic(), a_readwrite(), genEventAttributeType(typ), id("old<capitalize(name.val)>"))
  ];
  
private default list[Attribute] genEventAttribute(Param p){ throw "Generator Error: Error on parameter <p>."; }

private Typ genEventAttributeType(t_str()) = t_class(id("StringValue"));

private Typ genEventAttributeType(t_int()) = t_class(id("IntValue"));

private Typ genEventAttributeType(t_bool()) = t_class(id("BoolValue"));

private Typ genEventAttributeType(t_enum(ID name)) = t_class(id("EnumValue"));

private Typ genEventAttributeType(t_class(ID name)) = t_meta(t_class(name));

//private Typ genEventAttributeType(t_list(t_class(ID name))) = t_meta(t_list(t_class(name)));

private default Typ genEventAttributeType(Typ typ){ throw "Generator Error: Error on type <typ>."; }

private Unit IOperationInterface = 
  interface(vis_public(), id("IOperation"),
    [signature(vis_default(), t_void(), id("accept"),
      [param(when_none(), t_class(id("IVisitor")), id("visitor"))]
    )]
  );

public Package genOperationPackage(Package p)
{
  Name modelPackageName = name(p.name.head, p.name.path + [p_field(id("Model"))]);
  Name operationPackageName = name(p.name.head, p.name.path + [p_field(id("Operation"))]);
    
  //Using usingModel = using([modelPackageName]);
  //Using using = combine([p.using, UsingBaseType, UsingDesignSpace, usingModel]);

  Using using = genOperationUsing(p);

  list[Unit] units = [IOperationInterface];

  for(Unit cls: evt_class(_, Imp imp, ID n, _, _, list[Event] events) <- p.units){
    for(Event evt <- events){
      //FIXME: filter
      if(h_method (Vis vis, Imp imp, Poly poly, Typ rtyp, ID name, list[Param] params, Body body) := evt)
      {
        continue;
      }
      
      //Todo: do not generate a class for abstract operations.
      //Assumption: these are only needed for type checking.
      ID eventClassName = id(n.val + capitalize(evt.name.val));
      
      list[Attribute] attrs = [];
      if(imp_concrete() := imp)
      {
        attrs = [*genEventAttribute(param)| param <- evt.params];
      }
      else
      {
        //first parameter must be an IMetaObject
        //todo type check
        attrs = [attr(own_self(), evt.params[0].when, vis_private(), store_dynamic(), a_readwrite(), t_class(id("IMetaObject")), evt.params[0].name)];
        
        attrs += [*genEventAttribute(param)| param <- evt.params[1..]];        
      }
      
      list[Attribute] attrsNotOld = [a | a <- attrs, !startsWith(a.name.val,"old")];
      
      //println("<toString(n)> <toString(evt.name)>");
      Attribute subject = attrs[0];
      str subjectName = subject.name.val;
      
      Method getSubjectMethod =
        method(
         vis_public(), poly_override(), t_class(id("IMetaObject")), id("getSubject"),
         [],
         body([s_return(e_val(v_var(name(id(subjectName)))))]));
      
      list[Method] methods = [genOperationConstructor(p.name, eventClassName, attrsNotOld), getSubjectMethod] +
         [genGetter(a) | a <- attrs] + [genSetter(a) | a <- attrs] + [genAcceptMethod(poly_none())];

      units += [class(vis_public(), imp_concrete(), eventClassName, genExtends(evt), attrs, methods)];
    }
  }
  
  units += genVisitorInterface(units);

  return package(using, operationPackageName, units);
}


private Method genOperationConstructor(Name packageName, ID eventClassName, list[Attribute] attrsNotOld)
{
  Method m = genConstructor(eventClassName, attrsNotOld);
  m.body.statements = [s_assign(name(id("languageType")),e_call(name(id("typeof")),[
    e_val(v_var(name(id("<toString(packageName)>.Runtime.Language"))))
  ]))] + m.body.statements;
  
  return m;
}
