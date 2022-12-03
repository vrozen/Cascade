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
module lang::tel::gen::Inverter

import String;
import IO;
import Node;
import lang::tel::AST;
import lang::tel::Printer;
import lang::tel::Checker;
import lang::tel::gen::Util;

private Extends ExtendsInverter =
  ext_class([name(id("IInverter")),name(id("Operation"),[p_field(id("IVisitor"))])]); 

private Attribute OprationAttribute = genPrivateAttribute("Operation.IOperation", "iOp"); 

private Attribute InverterAttribute = genPrivateAttribute("Delta.Runtime.Inverter", "inverter");

private Method Constructor = 
  constructor(vis_public(), id("Inverter"), [], body([
    s_assign(name(id("this"),[p_field(id("inverter"))]), e_new(t_class(id("Delta.Runtime.Inverter")),[]))
  ]));

private Method InvertMethod =
  method(vis_public(), poly_none(), t_class(id("Event")), id("invert"), [param(when_none(),t_class(id("Event")),id("evt"))],
    body([
       s_if(
         e_is(name(id("evt")),t_class(name(id("Operation"),[p_field(id("IOperation"))])),id("op")),
         body([
           s_call(name(id("op"), [p_field(id("accept"))]), [e_val(v_var(name(id("this"))))])
         ])
       ),
       s_return(e_cast(t_class(id("Event")), e_val(v_var(name(id("iOp"))))))       
    ])
  );

private Method genVisitMethod(ID className, Event evt, Package operationPackage, map[ID,ID] inverseRelation)
{
  list[Statement] sts = [];

  ID operationName = id("<className.val><capitalize(evt.name.val)>");
  Unit operation = lookupClass(operationPackage, operationName);
  
  list[Statement] statements = [];
  
  if(effect(Imp imp, Inverse inverse, bool sideEffect, ID n, list[Param] params, Ops ops, Pre pre, Post post) := evt)
  {
    if(evt.name in inverseRelation)
    {
      if(inverseRelation[evt.name] == evt.name)
      {
        //case 1: visit method that throws an error because there is no inverse available
        //TODO: throw the UninvertibleException
        statements += [s_assign(name(id("iOp")), e_val(v_null()))];
      }
      else
      {
        ID inverseEventName = inverseRelation[evt.name];
        ID inverseOperationName = id("<className.val><capitalize(inverseEventName.val)>");
        
        Unit inverseOperation = lookupClass(operationPackage, inverseOperationName);
        
        list[Exp] arguments = [
          e_call(name(id("op"),[p_field(id("get<capitalize(a.name.val)>"))]),[]) |
          Attribute a <- operation.attributes, !startsWith(a.name.val, "old")
        ];
              
        //case 2: visit method that returns an inverse event type with the same parameters
        statements += [
          s_assign(name(id("iOp")), e_new(t_class(inverseOperationName), arguments)),
          s_call(name(id("inverter"),[p_field(id("invertOperations"))]), [e_val(v_var(name(id("op")))), e_cast(t_class(id("Effect")), e_val(v_var(name(id("iOp")))))])    
        ]; // += genVisitMethodInvertibleEvent(evt, inverseEventName);
      }
    }
    else
    {
      //case 3: visit method that return an event of the same type with old and new parameters reversed
      statements += []; //genVisitMethodUninvertibleException(evt);
    }
  }
  else
  {  
    //case 4: visit method that returns the event because it is its own inverse (the case for signals and triggers)
    list[Exp] arguments = [
       e_call(name(id("op"),[p_field(id("get<capitalize(a.name.val)>"))]),[]) |
       Attribute a <- operation.attributes, !startsWith(a.name.val, "old")
    ];
    
    statements += [
      s_assign(name(id("iOp")), e_new(t_class(operationName), arguments))
    ];
  }
  
  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(operationName),id("op"))],
    body(statements));
}



public Package genInverter(Package inputPackage, Package modelPackage, Package operationPackage, map[ID, ID] inverseRelation)
{
  Name runtimePackageName = name(inputPackage.name.head, inputPackage.name.path + [p_field(id("Runtime"))]); 

  Using using = genRuntimeUsing(inputPackage);
   
  list[Attribute] attributes = [InverterAttribute, OprationAttribute];
  
  list[Method] methods = [Constructor, InvertMethod];

  for(Unit cls: evt_class(_, _, _, _, _, _) <- inputPackage.units)
  {
    for(Event evt <- cls.events)
    {
      methods += [genVisitMethod(cls.name, evt, operationPackage, inverseRelation)];
    }
  }
  
  return package(using, runtimePackageName,
    [
      class(vis_public(), imp_concrete(), id("Inverter"), ExtendsInverter, attributes, methods)
    ]);
}

