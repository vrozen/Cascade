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
module lang::tel::gen::Model

import lang::tel::AST;
import lang::tel::gen::Util;

//private Extends ExtendsIPatchable = ext_class([name(id("IPatchable")),name(id("INode"))]);

private Extends genExtends(Extends: ext_none())
  = ext_class([name(id("IPatchable")), name(id("INode"))]);

private Extends genExtends(Extends ext: ext_class(list[Name] names))
  = ext;
    
private Unit INodeInterface = 
  interface(vis_public(), id("INode"),
    [signature(vis_default(), t_void(), id("accept"),
      [param(when_none(), t_class(id("IVisitor")), id("visitor"))]
    )]
  );

public Attribute genPublicAttribute(Attribute a)
{
  a.vis = vis_public();
  return a;
}

public Package genModelPackage(Package p)
{
  Name modelPackageName = name(p.name.head, p.name.path + [p_field(id("Model"))]);

  list[Unit] units = [];

  for(evt_class(Vis vis, Imp imp, ID name, Extends extends, list[Attribute] attributes, list[Event] events) <- p.units){

    Extends ext = genExtends(extends);
    
    list[Attribute] attrs = [];
    
    if(ext_none() := extends)
    {
      attrs += [MetaObjectAttribute];
    }
    
    attrs += [genPublicAttribute(a) | a <- attributes];
    
    list[Method] helperMethods =
      [method(vis2, poly, rtyp, n, params, body) | h_method(Vis vis2, Imp imp, Poly poly, Typ rtyp, ID n, list[Param] params, Body body: body(list[Statement] x)) <- events] +
      [abs_method(vis2, poly, rtyp, n, params) | h_method(Vis vis2, Imp imp: imp_abstract(), Poly poly, Typ rtyp, ID n, list[Param] params, Body body) <- events];

    list[Method] methods = [genGetter(a)| a <- attrs] + helperMethods;  

    if(ext_none() := extends)
    {
      methods += [SetMetaObjectMethod];
    }
    
    if(imp_concrete() := imp)
    {
      if(ext_none() := extends)
      {      
        methods += [genAcceptMethod(poly_none())];
      }
      else
      {
        methods += [genAcceptMethod(poly_override())];
      }
    }
    else if(imp_abstract() := imp && ext_none() := extends)
    {
      methods += [AbstractAcceptMethod];
    }

    units += [class(vis_public(), imp, name, ext, attrs, methods)];
  };
  
  units += [u | Unit u: enum(_, _, _) <- p.units];
  
  units += [INodeInterface] + genVisitorInterface(units);

  Using using = genModelUsing(p);

  return package(using, modelPackageName, units);
}