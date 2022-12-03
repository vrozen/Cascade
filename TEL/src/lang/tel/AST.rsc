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
/**
 * ASTs express the structure of TEL programs and C# programs.
 * By using the same set of datatypes for both formalisms,
 * TEL ASTs can be step-wise rewritten into valid C# ASTs.
 */
 
module lang::tel::AST

import IO;
import ParseTree;
import String;

import lang::tel::Syntax;
import lang::tel::Printer;

anno loc Package@location;
anno loc Unit@location;
anno loc Vis@location;
anno loc Imp@location;
anno loc Own@location;
anno loc Attribute@location;
anno loc Signature@location;
anno loc Method@location;
anno loc Event@location;
anno loc Operation@location;
anno loc Param@location;
anno loc When@location;
anno loc Body@location;
anno loc Pre@location;
anno loc Post@location;
anno loc Inverse@location;
anno loc Statement@location;
anno loc Value@location;
anno loc Name@location;
anno loc Path@location;
anno loc Typ@location;
anno loc ID@location;

anno Typ Exp@typ;
anno Typ Name@typ;
anno Typ Path@typ;
anno Typ Path@typ;
anno Typ Value@typ;
anno Typ Operation@typ;

data Package
  = package(Using using, Name name, list[Unit] units);

data Using
  = using(list[Name] units);

data Unit
  = evt_class(Vis vis, Imp imp, ID name, Extends extends, list[Attribute] attributes, list[Event] events)
  | class(Vis vis, Imp imp, ID name, Extends extends, list[Attribute] attributes, list[Method] methods)         
  | interface(Vis vis, ID name, list[Signature] signatures)
  | enum(Vis vis, ID name, list[ID] values);
  
data Vis
  = vis_default()
  | vis_private()
  | vis_protected()
  | vis_public();

data Imp
  = imp_concrete()
  | imp_abstract();
  
data Extends
  = ext_none()
  | ext_class(list[Name] names);
  
data Store
  = store_dynamic()
  | store_static();
  
data Access
  = a_readwrite()
  | a_readonly();

data Own
  = own_self()
  | own_ref();
  
data Attribute
  = attr(Own own, When when, Vis vis, Store store, Access access, Typ typ, ID name)
  | attr(Own own, When when, Vis vis, Store store, Access access, Typ typ, ID name, Exp val);
  
data Signature
  = signature(Vis vis, Typ rtyp, ID name, list[Param] params);

data Poly
  = poly_none()
  | poly_override();
  
data Method
  = method(Vis vis, Poly poly, Typ rtyp, ID name, list[Param] params, Body body)
  | abs_method(Vis vis, Poly poly, Typ rtyp, ID name, list[Param] params)
  | constructor(Vis vis, ID name, list[Param] params, Body body);

data Event
  = effect  (Imp imp, Inverse inverse, bool sideEffect, ID name, list[Param] params, Ops ops, Pre pre, Post post) 
  | trigger (Imp imp, ID name, list[Param] params, TPost tpost)
  | signal  (ID name, list[Param] params)
  | h_method (Vis vis, Imp imp, Poly poly, Typ rtyp, ID name, list[Param] params, Body body);

data Ops
  = ops_none()
  | ops_body(list[Operation] ops);

data Inverse
  = inv_none()
  | inv_self()
  | inv_prev();  

//notes:
//1) old values are not included in set operations,
//   these are instead resolved at run time
//2) x_ shorthand operations are rewritten by the specializer
data Operation
  = o_new    (Name name, Typ typ)   //expands into o_new + o_set
  | o_del    (Name name)            //specializes into o_del(name, typ)
  | o_del    (Name name, Typ typ)
  | o_set    (Name name, ID field, Value val)
  | x_assign (Name name, Value val) //specializes into o_set, l_set or m_add + m_set
  | x_remove (Name name, Value val) //specializes into l_remove, s_remove or m_set + m_remove
  | l_insert (Name name, Value index, Value val)
  | l_remove (Name name, Value index)
  | l_remove (Name rval, Name name, Value index) //with return
  | l_set    (Name name, Value index, Value val)
  | l_push   (Name name, Value val)
  | l_pop    (Name name)
  | l_pop    (Name rval, Name name) //with return
  | m_add    (Name name, Value key)
  | m_remove (Name name, Value key)
  | m_set    (Name name, Value key, Value val)
  | s_add    (Name name, Value val)
  | s_remove (Name name, Value val);

//data MethodParam
//  = m_param(Typ typ, ID name); 

data Param
  = param(When when, Typ typ, ID name)
  | param_change(When when, Typ typ, ID name, Exp val);

data When
  = when_none()
  | when_past()
  | when_future();
  
data Body
  = body_none()
  | body(list[Statement] statements);

data Pre
  = pre_none()
  | pre_body(Body body);
 
data Post
  = post_none()
  | post_body(Body body);
 
data TPost
  = tpost_none()
  | tpost_body(Body body); 
 
data Statement
  = s_call(Name name, list[Exp] operands)
  | s_call_effect(Name name, list[Exp] operands)
  //| s_call_method(Name name, list[Exp] operands)
  | s_declare_assign(Typ typ, ID var, Exp val)
  | s_declare(Typ typ, ID var)
  | s_assign(Name name, Exp val)
  | s_foreach(Typ typ, ID arg, Name name, Body body)
  | s_while(Exp exp, Body body)  
  | s_if(Exp exp, Body tbody)
  | s_if_else(Exp exp, Body tbody, Body fbody)  
  | s_break()
  | s_return()
  | s_return(Exp exp)
  | s_begin(Typ typ, ID var)
  | s_end(Name name)
  | s_comment(str message); //single line comment
    
data Exp
  = e_call(Name name, list[Exp] operands)
  | e_new (Typ typ, list[Exp] operands)
  | e_new (Typ typ)
  | e_cast (Typ typ, Exp exp)
  | e_is(Name name, Typ typ, ID vName)
  | e_val (Value val)
  | e_ovr (Exp exp)
  | e_unm (Exp exp)
  | e_not (Exp exp)
  | e_mul (Exp lhs, Exp rhs)
  | e_div (Exp lhs, Exp rhs)
  | e_add (Exp lhs, Exp rhs)
  | e_sub (Exp lhs, Exp rhs)
  | e_lt  (Exp lhs, Exp rhs)
  | e_gt  (Exp lhs, Exp rhs)
  | e_le  (Exp lhs, Exp rhs)
  | e_ge  (Exp lhs, Exp rhs)
  | e_neq (Exp lhs, Exp rhs)
  | e_eq  (Exp lhs, Exp rhs)
  | e_and (Exp lhs, Exp rhs)
  | e_or  (Exp lhs, Exp rhs);

data Value
  = v_unknown()
  | v_true()
  | v_false()
  | v_null()
  | v_int(int ival)
  | v_str(str sval)
  | v_var(Name name)
  | v_enum(ID enum, ID val) //rewrite v_var into v_enum based on type checking
  | v_array(list[Exp] values);

data Name
  = name(ID head, list[Path] path)
  | name(ID head);

data Path
  = p_field(ID field)
  | p_lookup(Value key);

data Typ
  = t_unknown()
  | t_error()
  | t_int()
  | t_str()
  | t_bool()
  | t_void()
  | t_meta(Typ typ)
  | t_class(ID name)
  | t_class(Name n) //for disambiguation of generated code
  | t_enum(ID name) //rewrite t_class into t_enum based on type checking
  | t_array(ID name)
  | t_list(Typ typ)
  | t_set(Typ typ)
  | t_map(Typ ktyp, Typ vtyp);

data ID
  = id(str val);
  
public Package tel_build(loc file)
  = implode(#Package, tel_parse(file));