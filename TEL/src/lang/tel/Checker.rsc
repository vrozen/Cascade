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
 * The Checker analyzes variable types, and produces a type map
 * that relates the souce locations of event parameters
 * and operation variables (referencing those parameters) to types.
 * This type map is used for specializing event operations.
 */

module lang::tel::Checker

import IO;
import lang::tel::AST;
import lang::tel::Printer;
import String;

public map[loc,Typ] check(Package p)
{
  map[loc,loc] usedef = ();
  
  map[loc,Typ] tmap = ();

  for(Unit cls: evt_class(_, _, _, _, _, _) <- p.units)
  {
    for(Event evt: effect(_,_,_,_,_,_,_,_) <- cls.events)
    {
      usedef += getUseDef(p, cls, evt);
      
      tmap += getTypeMap(p, cls, evt); 
    }
  }
  
  return tmap;
}

private Param lookupParam(list[Param] params, ID name)
{
  for(Param p <- params)
  {
    if(p.name.val == name.val)
    {
      return p;
    }
  }

  println("Type error, missing parameter <toString(name)> at <name@location>");
  throw "au";
}

private map[loc,loc] getUseDef(Package p,
  Unit cls: evt_class(Vis vis, Imp imp, ID className, Extends extends, list[Attribute] attributes, list[Event] events),
  Event evt: effect(Imp imp, Inverse inverse, bool sideEffect, ID effectName, list[Param] params, Ops ops, Pre pre, Post post))
{
  map[loc,loc] usedef = (); //maps name head to param
  
  if(ops_none() := ops)
  {
    return usedef;
  }
  
  visit(ops.ops)
  {
    case Name varName:
    { 
      ID head = varName.head;
      if(capitalize(head.val) != head.val)
      {
        loc use = head@location;
      
        Param p = lookupParam(params, head);
        loc def = p@location;
        usedef += (use:def);      
      }
    }
  }

  return usedef;
}

private map[loc, Typ] getTypeMap(Package p,
  Unit cls: evt_class(Vis vis, Imp imp, ID className, Extends extends, list[Attribute] attributes, list[Event] events),
  Event evt: effect(Imp imp, Inverse inverse, bool sideEffect, ID effectName, list[Param] params, Ops ops, Pre pre, Post post))
{
  map[loc, Typ] tmap = ();

  if(ops_none() := ops)
  {
    return tmap;
  }

  visit(ops.ops)
  {
    case Name varName:
    {
      ID head = varName.head;
      if(capitalize(head.val) != head.val)
      {
        Param defParam = lookupParam(params, head);
      
        Typ rootType = defParam.typ;
        Typ parentType = rootType;
    
        tmap += (head@location: rootType);
    
        Typ childType = parentType;
        for(Path path <- varName.path)
        {
          //println("Check type\n  <p>  \n  <parentType>  \n  <path>");
          childType = checkType(p, parentType, path);
      
          tmap += (path@location: childType);
      
          parentType = childType;
        }
    
        tmap += (varName@location: childType);    
      }
    }
  }

  return tmap;
}

private Unit lookupUnit(Package p, Typ parentType)
{
  for(Unit u <- p.units)
  {
    if(u.name.val == parentType.name.val)
    {
      return u;
    }
  }
  throw "Type error, unable to locate type <toString(parentType)>";
}

private Typ lookupType(Package p, Typ parentType, ID field)
{
  Unit u = lookupUnit(p, parentType);
  println(field);
  for(Attribute a <- u.attributes)
  {
    if(a.name.val == field.val)
    {
      return a.typ;
    }
  }
  println("Type error, missing field <field.val> at <field@location>");
}

private Typ checkType(Package p, Typ parentType, Path path: p_field(ID field))
 = lookupType(p, parentType, field);

private Typ checkType(Package p, Typ parentType: t_list(Typ typ), Path path: p_lookup(Value key))
{
  Typ keyType = typeof(key);
  if(keyType != t_int() && keyType != t_unknown())
  {
    throw "Type error: expected int found <toString(keyType)>";
  }
  return typ;
}

private Typ checkType(Package p, Typ parentType: t_map(Typ ktyp, Typ vtyp), Path path: p_lookup(Value key))
{
  Typ keyType = typeof(key);
  if(keyType != ktyp && keyType != t_unknown())
  {
    throw "Type error: expected <toString(ktyp)> found <toString(keyType)>";
  }  
  return vtyp;
}

private default Typ checkType(Package p, Typ parentType, Path path)
{
  throw "Type error";
}

public Typ typeof(Value v: v_unknown()) = t_unknown();
public Typ typeof(Value v: v_true()) = t_bool();  
public Typ tyepof(Value v: v_false()) = t_bool();
public Typ tyepof(Value v: v_null()) = t_unknown();
public Typ tyepof(Value v: v_int(int ival)) = t_int();
public Typ tyepof(Value v: v_str(str sval)) = t_str();
public Typ tyepof(Value v: v_var(Name n)) = t_unknown();
public default Typ typeof(Value v) = t_unknown();

public bool isBaseType(Typ t: t_meta(Typ typ)) = false;
public bool isBaseType(Typ t: t_class(ID name)) = false;
public bool isBaseType(Typ t: t_list(Typ typ)) = false;
public bool isBaseType(Typ t: t_set(Typ typ)) = false;
public bool isBaseType(Typ t: t_set(Typ typ)) = false;
public bool isBaseType(Typ t: t_map(Typ ktyp, Typ vtyp)) = false;
public default bool isBaseType(Typ t) = true;

public Value nullValue(Typ t: t_int()) = v_int(0);
public Value nullValue(Typ t: t_str()) = v_null();
public Value nullValue(Typ t: t_bool()) = v_false();
public default Value nullValue(Typ t) = v_null();


//resolve enum types and enum values
public Package resolveEnums(Package p, map[ID,list[ID]] enums)
{
  return visit(p)
  {
    case t_class(ID n):
    {
      if(n in enums)
      {
        //println("ENUM TYPE! <n.val>");
        insert t_enum(n);
      }
    }
    case v_var(Name name):
    {
      if(name.head in enums)
      {
        //println("ENUM VALUE! <toString(name)>");
        insert v_enum(name.head, name.path[0].field);
      }
    }
  }
}

public map[ID,list[ID]] getEnums(set[Package] eachModelPackage)
{
  map[ID,list[ID]] enums = ();
  for(Package p <- eachModelPackage)
  {
    enums += getEnums(p);  
  }
  return enums;
}
 
private map[ID,list[ID]] getEnums(Package modelPackage)
  = (name: values | Unit u: enum(Vis vis, ID name, list[ID] values) <- modelPackage.units);
  

//needed for generating hte inverter
public map[ID,ID] checkInverseRelation(Package inputPackage)
{
  map[ID,ID] i = ();
  ID prev;
  bool validPrev = false;

  visit(inputPackage)
  {
    case Event e: effect(Imp imp, Inverse inverse: inv_self(), bool sideEffect, ID n, list[Param] params, Ops ops, Pre pre, Post post):
    {
      i += (n : n);
      validPrev = false;
    }
    case Event e: effect(Imp imp, Inverse inverse: inv_prev(), bool sideEffect, ID n, list[Param] params, Ops ops, Pre pre, Post post):
    {
      if(validPrev == true)
      {
        i += (n:prev, prev:n);
        validPrev = false;
      }
      else
      {
        throw "no valid previous effect <e>";
      }
    }
    case Event e: effect(Imp imp, Inverse inverse: inv_none(), bool sideEffect, ID n, list[Param] params, Ops ops, Pre pre, Post post):
    {
      prev = n;
      validPrev = true;
    }
  }
  
  return i;
}

private Event inverse(Event e: trigger(Imp imp, ID n, list[Param] params, TPost tpost), rel[ID,ID] inverse)
 = inverse[n];

private Event inverse(Event e: signal(ID n, list[Param] params), rel[ID,ID] inverse)
 = inverse[n];
 
private Event inverse(Event e: effect(Imp imp, Inverse inverse, bool sideEffect, ID n, list[Param] params, Ops ops, Pre pre, Post post), rel[ID,ID] inverse)
 = inverse[n];

private default Event inverse(Event e, rel[ID,ID] inverse)
{
  throw "malformed effect, no inverse for <e>";
}