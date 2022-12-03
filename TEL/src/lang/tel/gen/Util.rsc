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
module lang::tel::gen::Util

import String;
import IO;
import Set;
import Node;
import lang::tel::AST;
import lang::tel::Printer;

public Using UsingBaseType = using([name(id("Delta"),[p_field(id("Model")),p_field(id("BaseType"))])]);

public Using UsingBaseOperation = using([name(id("Delta"),[p_field(id("Model")),p_field(id("BaseOperation"))])]);

public Using UsingDesignSpace = using([name(id("Delta"),[p_field(id("Model")),p_field(id("DesignSpace"))])]);

public Using UsingQualifiedName = using([name(id("Delta"),[p_field(id("Model")),p_field(id("QualifiedName"))])]);

public Using UsingRuntime = using([name(id("Delta"),[p_field(id("Runtime"))])]);

public Using UsingSystem = using([name(id("System"))]);

public Using UsingSystemLinq = using([name(id("System"),[p_field(id("Linq"))])]);

public Using combine(list[Using] us){

 set[Name] names = {};

 for(Using u <- us)
 {
   names = names + {n | Name n <- delAnnotationsRec(u.units)}; 
 }
 
 return using(toList(names));
}


public Attribute genReferencePrivateAttribute(str className, str attrName) = 
  attr(own_ref(), when_none(), vis_private(), store_dynamic(), a_readwrite(), t_class(id(className)),id(attrName));

public Attribute genReferencePrivateReadolyAttribute(str className, str attrName) = 
  attr(own_ref(), when_none(), vis_private(), store_dynamic(), a_readonly(), t_class(id(className)),id(attrName));

public Attribute genPrivateReadolyAttribute(str className, str attrName) = 
  attr(own_self(), when_none(), vis_private(), store_dynamic(), a_readonly(), t_class(id(className)),id(attrName));

public Attribute genPrivateAttribute(str className, str attrName) = 
  attr(own_self(), when_none(), vis_private(), store_dynamic(), a_readwrite(), t_class(id(className)),id(attrName));
  
public Attribute genConstant(str className, str attrName, Exp val) =
  attr(own_self(), when_none(), vis_private(), store_static(), a_readonly(), t_class(id(className)), id(attrName), val);

public Attribute MetaObjectAttribute = genReferencePrivateAttribute("IMetaObject", "metaObject");

public Method genAcceptMethod(Poly poly) = 
  method(
    vis_public(), poly, t_void(), id("accept"),
    [param(when_none(),t_class(id("IVisitor")),id("visitor"))],   
    body([
      s_call(name(id("visitor"),[p_field(id("visit"))]),[e_val(v_var(name(id("this"))))])
    ])
  );
  
public Method AbstractAcceptMethod = 
  abs_method(
    vis_public(), poly_none(), t_void(), id("accept"),
    [param(when_none(),t_class(id("IVisitor")),id("visitor"))]
  );
  
public Method genGetter(Attribute attr) =
  method(
    vis_public(), poly_none(), attr.typ, id("get"+capitalize(attr.name.val)),
    [],
    body([s_return(e_val(v_var(name(attr.name))))])
  );
  
public Method genSetter(Attribute attr) =
  method(
    vis_public(), poly_none(), t_void(), id("set"+capitalize(attr.name.val)),
    [param(when_none(), attr.typ, attr.name)],
    body([
      s_assign(name(id("this"), [p_field(attr.name)]), e_val(v_var(name(attr.name))))
    ])
  );
  
public Method GetMetaObjectMethod = genGetter(MetaObjectAttribute);

public Method SetMetaObjectMethod = genSetter(MetaObjectAttribute);
  
public Method genConstructor(ID n, list[Attribute] attrs) =
  constructor(vis_public(), n, [param(a.when, a.typ, a.name ) | a <- attrs],
    body([s_assign(name(id("this"),[p_field(a.name)]), e_val(v_var(name(a.name)))) | a <- attrs])
  );

public Unit genVisitorInterface(list[Unit] units) =
  interface(vis_public(), id("IVisitor"), [genVisitorSignature(unit) | Unit unit: class(_,imp_concrete(),_,_,_,_) <- units]);

public Signature genVisitorSignature(class(Vis vis, Imp imp, ID n, Extends extends,
           list[Attribute] attributes, list[Method] methods)) =
  signature(vis_public(), t_void(), id("visit"), [param(when_none(), t_class(n), id("op"))]);

public default Signature genVisitorSignature(Unit unit){ throw "Generator Error: Invalid unit <unit>."; }

public Using genOperationUsing(Package inputPackage)
{
 Using usingRuntimeUsing = genRuntimeUsing(inputPackage.name);
 list[Using] usingIncludedPackages = [genRuntimeUsing(name) | Name name <- inputPackage.using.units];
 return combine([UsingBaseType, UsingDesignSpace, usingRuntimeUsing] + usingIncludedPackages);
}


public Using genModelUsing(Package inputPackage)
{
 Using usingModelPackage = genModelUsing(inputPackage.name);
 list[Using] usingIncludedPackages = [genModelUsing(name) | Name name <- inputPackage.using.units];
 return combine([UsingSystem, UsingBaseType, usingModelPackage] + usingIncludedPackages);
}

private Using genModelUsing(Name n)
{
  Name modelPackageName = name(n.head, n.path + [p_field(id("Model"))]);
  return using([modelPackageName]);
}

public Using genRuntimeUsing(Package inputPackage)
{ 
  Using usingRuntimePackage = genRuntimeUsing(inputPackage.name);
  
  list[Using] usingIncludedPackages = [genRuntimeUsing(name) | Name name <- inputPackage.using.units];
  
  return combine([UsingSystem, UsingSystemLinq, UsingBaseType, UsingBaseOperation,
                  UsingDesignSpace, UsingQualifiedName, UsingRuntime,
                  usingRuntimePackage] + usingIncludedPackages);
}

private Using genRuntimeUsing(Name n)
{
  Name operationPackageName = name(n.head, n.path + [p_field(id("Operation"))]);
  Name modelPackageName = name(n.head, n.path + [p_field(id("Model"))]);
  return using([operationPackageName, modelPackageName]);
}

public Unit lookupClass(Package p, ID name)
{
  for(Unit u <- p.units)
  {
    if(u.name == name)
    {
      return u;
    }
  }
  throw "Error: missing unit <toString(name)> in package <toString(p.name)>";
}

public Unit lookupClass(set[Package] ps, ID name)
{
  for(Package p <- ps)
  {
    for(Unit u <- p.units)
    {
      if(u.name == name)
      {
        return u;
      }
    }
  }
  //writeFile(|project://TEL/models/LiveQL/test.txt|, ps);
  
  //hack
  if(endsWith(name.val,"Reconstruct"))
  {
    return class(vis_public(), imp_concrete(), id("Reconstruct"), ext_none(), [], 
                [                  
                  constructor(vis_public(), id("Reconstruct"), [], body([]))
                ]);
  }
  
  throw "Error: missing unit <toString(name)>"; // <name.location>
}


public Exp encode(Typ expectedType: t_meta(Typ t), Exp e: e_val(v_null())) =
  e_cast(expectedType, e);  

public Exp encode(Typ expectedType: t_meta(Typ t), Exp e)
{
//  println();
//  println(e);
  return e_cast(expectedType, e_call(name(id(toString(e.val.name)),[p_field(id("getMetaObject"))]),[]));
}

public Exp encode(Typ expectedType: t_class(id("IMetaObject")), Exp e)
{
//  println();
//  println(e);
  return e_cast(expectedType, e_call(name(id(toString(e.val.name)),[p_field(id("getMetaObject"))]),[]));
}

public default Exp encode(Typ expectedType, Exp e) =
  e_cast(expectedType, e_call(name(id("Patcher"),[p_field(id("encode"))]),[e]));

public list[Param] getConstructorParams(Unit unit: class(_, _, _, _, _, list[Method] methods))
{
  for(Method m <- methods)
  {
    if(constructor(_, _, list[Param] params, _) := m)
    {
      return params;
    }
  }
  println(toString(unit));
  throw "error";
}