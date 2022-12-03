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
module lang::tel::gen::Marshaller

import String;
import IO;
import Node;
import lang::tel::AST;
import lang::tel::Printer;
import lang::tel::Checker;
import lang::tel::gen::Util;

private Extends ExtendsMarshaller =
  ext_class([name(id("Model"),[p_field(id("IVisitor"))])]); //name(id("IMarshaller"))

private Attribute PatcherAttribute = genPrivateAttribute("Patcher", "patcher"); 

private Method Constructor = 
  constructor(vis_public(), id("Marshaller"), [
    param(when_none(), t_class(id("Patcher")), id("patcher"))
  ], body([
    s_assign(name(id("this"),[p_field(id("patcher"))]), e_val(v_var(name(id("patcher")))))
  ]));

private Method MarshalMethod =
  method(vis_public(), poly_none(), t_bool(), id("marshal"), [param(when_none(),t_class(id("Model.INode")),id("node"))],
    body([
      s_call(name(id("node"), [p_field(id("accept"))]), [e_val(v_var(name(id("this"))))]),
      s_return(e_val(v_true())) //todo check success
    ])
  );

private Method genVisitMethod(Unit cls: class(Vis vis, Imp imp, ID n, Extends extends, list[Attribute] attributes, list[Method] methods))
{
  list[Statement] sts = [];
  
  sts += [s_declare_assign(t_meta(t_class(name(n))), id("metaObject"),
    e_call(name(id("patcher"),[p_field(id("createMetaObject\<<n.val>\>"))]),[]))
  ];
  
  sts += 
    [s_call(name(id("node"),[p_field(id("setMetaObject"))]),[e_val(v_var(name(id("metaObject"))))]),
     s_call(name(id("metaObject"), [p_field(id("bind"))]), [e_val(v_var(name(id("node"))))]) 
    ];
 
  list[Attribute] ownedAttributes = [a | Attribute a <- attributes, a.own == own_self()];

  sts += [*genVisitStatements(a) | Attribute a <- ownedAttributes];
     
  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(n),id("node"))],
    body(sts));
}

private list[Statement] genVisitStatements(Attribute a)
{
  println(toString(a));
  return genVisitStatements(a, a.typ);
}

private list[Statement] getVisitStatements(Attribute a, Typ ntyp: t_list(_)) =
  [
    s_declare_assign(ntyp, id("childList<capitalize(a.name.val)>"), e_call(name(id("node"),[p_field(id("get<capitalize(a.name.val)>"))]),[])),
    s_foreach(ntyp.vtyp, id("childNode<capitalize(a.name.val)>"), name(id("childList<capitalize(a.name.val)>")), 
    [
      s_call(name(id("childNode<capitalize(a.name.val)>"),[p_field(id("accept"))]),[e_val(v_var(name(id("this"))))])
    ])
  ];


private list[Statement] getVisitStatements(Attribute a, Typ ntyp: t_set(_)) =
  [
    s_declare_assign(ntyp, id("childSet<capitalize(a.name.val)>"), e_call(name(id("node"),[p_field(id("get<capitalize(a.name.val)>"))]),[])),
    s_foreach(ntyp.vtyp, id("childNode<capitalize(a.name.val)>"), name(id("childSet<capitalize(a.name.val)>")), 
    [
      s_call(name(id("childNode<capitalize(a.name.val)>"),[p_field(id("accept"))]),[e_val(v_var(name(id("this"))))])
    ])
  ];

private list[Statement] getVisitStatements(Attribute a, Typ ntyp: t_map(_, _)) =
  [
    s_declare_assign(ntyp, id("childMap<capitalize(a.name.val)>"), e_call(name(id("node"),[p_field(id("get<capitalize(a.name.val)>"))]),[])),
    s_foreach(ntyp.vtyp, id("childNode<capitalize(a.name.val)>"), name(id("childMap<capitalize(a.name.val)>",[p_field(id("Values"))])), 
    [
      s_call(name(id("childNode<capitalize(a.name.val)>"),[p_field(id("accept"))]),[e_val(v_var(name(id("this"))))])
    ])
  ];

private list[Statement] genVisitStatements(Attribute a, Typ ntyp: t_class(_)) =
  [
    s_declare_assign(t_class(id("Model.INode")), id("childNode<capitalize(a.name.val)>"), e_call(name(id("node"),[p_field(id("get<capitalize(a.name.val)>"))]),[])),
    s_call(name(id("childNode<capitalize(a.name.val)>"),[p_field(id("accept"))]),[e_val(v_var(name(id("this"))))])
  ];
  
private default list[Statement] genVisitStatements(Attribute a, Typ ntyp) =
  [
  ];
  
public Package genMarshaller(Package inputPackage, Package modelPackage)
{
  Name runtimePackageName = name(inputPackage.name.head, inputPackage.name.path + [p_field(id("Runtime"))]); 

  Using using = genRuntimeUsing(inputPackage);
   
  list[Attribute] attributes = [PatcherAttribute];
  
  list[Method] methods = [Constructor, MarshalMethod];

  for(Unit cls: class(_, imp_concrete(), _, _, _, _) <- modelPackage.units)
  {
    methods += [genVisitMethod(cls)];
  }
  
  return package(using, runtimePackageName,
    [
      class(vis_public(), imp_concrete(), id("Marshaller"), ExtendsMarshaller, attributes, methods)
    ]);
}
