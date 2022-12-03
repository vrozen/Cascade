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
 * The Specializer uses the type map produced by the Checker
 * for specializing and expanding operations whose semantics
 * cannot be determined from the syntax alone.
 *
 * Generic type-dependent operations become typed:
 * 1) x_assign becomes o_set, l_set or m_add + m_set. 
 *    by resolving the path head to the variable type,
 *    and also storing the tail key/index separately.
 * 2) x_remove becomes l_remove, s_remove, m_set + m_remove 
 *    by resolving the path to the variable type.
 * 
 * Implicit types are added:
 * 3) o_delete gains a type parameter.
 *
 * Created objects are also assigned:
 * 4) o_new expands into o_new + o_set.
 */ 
module lang::tel::Specializer

import IO;
import lang::tel::AST;
import lang::tel::Printer;
import lang::tel::Checker;

public Package specialize(Package p, map[loc,Typ] tmap) =
 visit(p)
 {
   case list[Operation] ops => specialize(tmap, ops)
 };

private Name cutFront(Name n) = name(n.head, n.path[0..-1]);

private Path cutTail(Name n)
{
  Path p = n.path[-1];
  //println("cutTail:\n  <n>\n  <p>");
  return p;
}

private list[Operation] specialize(Typ t: t_list(_), Operation op: x_assign(Name name, Value val)) = 
  [l_set(cutFront(name), cutTail(name).key, val)];

private list[Operation] specialize(Typ t: t_map(_,_), Operation op: x_assign(Name name, Value val)) =
  [ m_add(cutFront(name), cutTail(name).key),
    m_set(cutFront(name), cutTail(name).key, val)];

private default list[Operation] specialize(Typ t, Operation op: x_assign(Name name, Value val)) = 
  [o_set(cutFront(name), cutTail(name).field, val)];

private list[Operation] specialize(map[loc,Typ] tmap, Operation op: x_assign(Name name, Value val))
{
  loc l;
  if([Path p] := name.path)
  {
    l = name.head@location;
  }
  else
  {
    l = name.path[-2]@location;
  }
  return specialize(tmap[l], op);
}

private list[Operation] specialize(Typ t: t_set(_), Operation op: x_remove (Name n, Value val)) = 
  [s_remove(n, val)];

private list[Operation] specialize(Typ t: t_list(_), Operation op: x_remove (Name n, Value val)) = 
  //[l_remove(cutFront(n), cutTail(n).key, val)];
  [l_remove(n, val)];

private list[Operation] specialize(Typ t: t_map(_,Typ vTyp), Operation op: x_remove(Name n, Value val)) = 
  [m_set(n, val, nullValue(vTyp)), m_remove(n, val)];
  
private list[Operation] specialize(map[loc,Typ] tmap, Operation op: x_remove(Name n, Value val)) = 
  specialize(tmap[n@location], op);
  
private list[Operation] specialize(map[loc,Typ] tmap, Operation op: o_del(Name n)) =
 [o_del(n,tmap[n@location])];

private list[Operation] specialize(map[loc,Typ] tmap, Operation op: o_new(Name n: name(ID head, []), Typ typ)) =
 [op];

private list[Operation] specialize(map[loc,Typ] tmap, Operation op: o_new(Name n: name(ID head, list[Path] p), Typ typ)) =
 [op, o_set(name(head), p[-1].field, v_var(name(id(genName(n)))))];

private default list[Operation] specialize(map[loc,Typ] tmap, Operation op) = [op];

private list[Operation] specialize(map[loc,Typ] tmap, list[Operation] ops)
{
  list[Operation] newOps = [];
  
  for(Operation op <- ops)
  {
    list[Operation] newOpsPart = specialize(tmap, op);
    newOps += newOpsPart;
  }

  return newOps;
}