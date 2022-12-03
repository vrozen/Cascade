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
 * Desugar replaces 'none' AST nodes by bodies with an empty list,
 * and adds class names where they are omitted in event calls.
 */

module lang::tel::Desugar

import lang::tel::AST;
import lang::tel::Printer;

public Package desugar(Package p)
{
  Package p2 = completeCalls(p);
  Package p3 = desugarEmpty(p2);
  Package p4 = desugarType(p3);
  Package p5 = desugarHelperMethod(p4);
  Package p6 = fixStrings(p5);
  return p6;
}

private Package fixStrings(Package p) = 
  visit(p)
  {
    case v_str(str sval) => v_str(sval[1..-1])
  };

private Package desugarHelperMethod(Package p) = 
  visit(p)
  {
    case Event e: h_method (Vis vis, Imp imp, Poly poly, Typ rtyp, ID name, list[Param] params, Body b: body(list[Statement] statements)) =>
      h_method(vis, imp, poly, rtyp, name, params, 
        visit(b)
        {
          case Statement s: s_call_effect(Name name, list[Exp] operands) =>
           s_call(name, operands)   
        }
      )
  };

private default Statement desugarHelperMethod(Statement s)
  = s;

private Package desugarType(Package p) = 
  visit(p)
  {
    case t_class(Name n) => t_class(id(toString(n)))
  };

private Package desugarEmpty(Package p) =
  visit(p)
  {
    case Ops ops: ops_none() => ops_body([])
    case TPost post: tpost_none() => tpost_body(body([]))
    case Pre pre: pre_none() => pre_body(body([]))    
    case Post post: post_none() => post_body(body([])) 
  };

private Package completeCalls(Package p) =
  visit(p)
  {
    case Unit u: evt_class(_, _, ID className, _, _, _) =>
      completeCalls(u, className)
  };

private Unit completeCalls(Unit u, ID className) =
  visit(u)
  {
    case Statement s: s_call_effect(Name name, list[Exp] operands) =>
      s_call_effect(completeCall(name,className), operands)
  };
  
  
private Name completeCall(Name n: name(id("Reconstruct"),[]), ID className) =
  n;
  
private Name completeCall(Name n: name(ID i), ID className) =
  name(className, [p_field(i)]);

private Name completeCall(Name n: name(ID i, []), ID className) =
  name(className, [p_field(i)]);
  
private default Name completeCall(Name n, ID className) = n;