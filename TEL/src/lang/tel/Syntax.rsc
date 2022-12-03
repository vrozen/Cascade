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
module lang::tel::Syntax

import ParseTree;

start syntax Package
  = package: Using using "package" Name name "{" Unit* units "}";
  
syntax Using
  = using: ("using" Name ";")* units;
  
syntax Unit
  = evt_class: Vis vis Imp imp "class" ID name Extends extends "{" (Attribute ";")* attributes Event* events "}"
  | enum: Vis vis "enum" ID name "{" {ID ","}* values "}";

syntax Vis
  = vis_default:   //empty
  | vis_private: "private"
  | vis_protected: "protected"
  | vis_public: "public";

syntax Imp
  = imp_concrete: //empty default
  | imp_abstract: "abstract";

syntax Own
  = own_self: //empty default
  | own_self: "own"
  | own_ref: "ref";

syntax Store
  = store_dynamic:
  | store_static: "static";

syntax Access
  = a_readwrite:
  | a_readonly: "readonly";
  
syntax Extends
  = ext_none: //empty default
  | ext_class: "extends" {Name ","}* names;

syntax Attribute
  = attr: Own own When when Vis vis Store store Access access Typ typ ID name;

syntax Event
  //note: abstract effects and triggers are no longer in the language
  = effect: Imp Inverse "side-"? "effect" ID name "(" {Param ","}* params ")" Ops ops Pre pre Post post
  | trigger: Imp "trigger" ID name "(" {Param ","}* params ")" TPost tpost
  | signal: "signal" ID name "(" {Param ","}* params ")" ";"
  | h_method: Vis vis Imp imp Poly poly Typ typ ID name "(" {Param ","}* ")" Body body;

syntax Inverse
  = inv_none:
  | inv_self: "invertible"
  | inv_prev: "inverse";
  
syntax Poly
  = poly_none:
  | poly_override: "override";
  
//syntax MethodParam
//  = m_param: Typ typ ID name;

syntax Param
  = param:        When when Typ typ ID name
  | param_change: When when Typ typ ID name "=" Exp val;

syntax When
  = when_none:
  | when_past: "past"
  | when_future: "future";
    
syntax Ops
  = ops_none: ";" 
  | ops_body: "{" (Operation ";")* ops "}";
  
syntax Body
  = body_none: ";"
  | body: "{" Statement* statements "}";
    

syntax TPost //trigger post (lacks post keyword)
  = tpost_none: ";"
  | tpost_body: Body body;

syntax Pre
  = pre_none: //nothing
  | pre_body: "pre" Body body;
  
syntax Post
  = post_none: //nothing
  | post_body: "post" Body body;

syntax Operation
  = o_new: Name name "=" "new" Typ typ "(" ")"
  | o_del: "delete" Name name
  | x_assign: Name name "=" Value val                  //becomes o_set, m_add + m_put, l_set
  | x_remove: Name name "." "remove" "(" Value val ")" //becomes m_remove, s_remove, l_remove without return
  | s_add: Name name "." "add" "(" Value val ")"
  | l_insert: Name name "." "insert" "(" Value index "," Value val ")"
  | l_remove: Name rval "=" Name name "." "remove" "(" Value index ")" //l_remove with return
  | l_push: Name name "." "push" "(" Value val ")"
  | l_pop: Name name "." "pop" "("")"                 //l_pop without return
  | l_pop: Name rval "=" Name name "." "pop" "("")"; //l_pop with return

syntax Statement
  = s_call_effect: Name name "(" {Exp ","}* operands ")" ";"
  | s_declare: Typ typ ID var ";"
  | s_declare_assign: Typ typ ID var "=" Exp val ";"
  | s_assign:  Name name "=" Exp val ";"
  | s_foreach: "foreach" "(" Typ typ ID arg "in" Name name ")" Body body
  | s_for: "for" "(" Typ typ ID var "=" Exp val ";" Exp conditionExp ";" ID var "=" Exp exp ")" Body body
  | s_while: "while" "(" Exp exp ")" Body body
  | s_if: "if" "(" Exp exp ")" Body tbody
  | s_if_else: "if" "(" Exp exp ")" Body tbody "else" Body fbody
  | s_break: "break" ";"
  | s_return: "return" ";"
  | s_return: "return" Exp exp ";"
  | s_begin: "begin" Typ typ ID var ";"
  | e_end: "end" Name ";"
  ;
  
syntax Exp
  = e_call: Name name "(" {Exp ","}* operands ")" //always a method call
  | e_new: "new" Typ typ "(" {Exp ","}* operands ")"
  | e_val: Value val
  | e_is: Name name "is" Typ typ ID vName
  | e_ovr:  "(" Exp exp ")"
  > e_unm: "-" Exp exp
  | e_not: "!" Exp exp
  > left
    ( left e_mul: Exp lhs "*" Exp rhs
    | left e_div: Exp lhs "/" Exp rhs
    )
  > left
    ( left e_add: Exp lhs "+" Exp rhs
    | left e_sub: Exp lhs "-" Exp rhs
    )
  > left
    ( left e_lt:  Exp lhs "\<" Exp rhs
    | left e_gt:  Exp lhs "\>" Exp rhs
    | left e_le:  Exp lhs "\<=" Exp rhs
    | left e_ge:  Exp lhs "\>=" Exp rhs
    | left e_neq: Exp lhs "!=" Exp rhs
    | left e_eq:  Exp lhs "==" Exp rhs 
    )
  > left e_and: Exp lhs "&&" Exp rhs
  > left e_or: Exp lhs "||" Exp rhs;

syntax Value
  = v_true:  "true"
  | v_false: "false"
  | v_null: "null"
  | v_int: INT ival
  | v_str: STRING sval
  | v_var: Name name
  | v_enum: "(" ID enum "." ID val ")"
  ;

syntax Name
  = name: ID head Path* path;

syntax Path
  = p_field:  "." ID field 
  | p_lookup: "[" Value key "]";
     
syntax Typ
  = t_int: "int"
  | t_str: "String"
  | t_str: "string"
  | t_bool: "bool"
  | t_void: "void"
  | t_class: Name name
  | t_list: "List" "\<" Typ typ "\>"
  | t_set: "Set" "\<" Typ typ "\>"
  | t_map: "Map" "\<" Typ ktyp "," Typ vtyp "\>";
  
lexical INT
  = [0-9]+;

lexical ID
  = id: ([a-zA-Z_$] [a-zA-Z0-9_$]* !>> [a-zA-Z0-9_$]) val \ Keyword;
  
lexical STRING
  = [\"] ![\"]* [\"];

layout LAYOUTLIST
  = LAYOUT* !>> [\t-\n \r \ ] !>> "//" !>> "/*";

lexical LAYOUT
  = Comment
  | [\t-\n \r \ ];
  
lexical Comment
  = "/*" (![*] | [*] !>> [/])* "*/" 
  | "//" ![\n]* [\n];

keyword Keyword
  = //"new" | "delete" | "push" | "pop" | "add" | "remove"  //edit operations
  | "ref" | "own"                                         //ownership
  | "private" | "protected" | "public"                    //visibility
  | "static" | "readonly"                                 //constants
  | "abstract"                                            //overriding and polymorphic calls
  | "trigger" | "signal" | "side-" | "effect"             //events
  | "past" | "future" |                                   //timelyness of event parameters
  | "package" | "class" | "enum" | "interface"            //compilation units
  | "pre" | "post" | "inverse" | "invertible"             //effect sections
  | "String" | "string" | "int" | "bool" | "void"         //base types
  | "List" | "Map" | "Set" |                              //composite types
  | "if" | "else" | "foreach" | "in" | "break" | "return" | "begin" | "end" //statements
  | "true" | "false" | "null"                             //values
  //avoid collisions
  | "continue" | "op" | "Op" | "Type" | "type" | "Value" | "value" | "ID" | "Name" | "is" | "operator" | "id" | "Id" | "as" | "As"
  ;          
  
public start[Package] tel_parse(str input, loc file) = 
  parse(#start[Package], input, file);
  
public start[Package] tel_parse(loc file) = 
  parse(#start[Package], file);