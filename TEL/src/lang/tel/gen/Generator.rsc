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
module lang::tel::gen::Generator

import String;
import IO;
import Node;
import lang::tel::AST;
import lang::tel::Printer;
import lang::tel::Checker;
import lang::tel::gen::Util;

private Extends ExtendsIGenerator =
  ext_class([name(id("IGenerator")),name(id("Operation"),[p_field(id("IVisitor"))])]);

public Attribute PatcherAttribute = genPrivateReadolyAttribute("Patcher", "patcher"); 

private list[Attribute] genStaticAttributes(Package modelPackage)
{
  list[Attribute] attrs = [];

  for(class(Vis vis, Imp imp, ID n, Extends extends,
           list[Attribute] attributes, list[Method] methods) <- modelPackage.units)
  {
    str classObjectTypeName = n.val + "ObjectType"; 
    Exp classObjectTypeValue = e_call(name(id("typeof")), [e_val(v_var(name(id("<toString(n)>"))))]);
    Attribute classObjectTypeAttr = genConstant("Type", classObjectTypeName, classObjectTypeValue);
  
    str classTypeName = n.val + "Type"; 
    Exp classTypeValue = e_new(t_class(id("StringValue")),
      [
        e_call(name(id(classObjectTypeName),[p_field(id("ToString"))]), [])
      ]);      
    Attribute classTypeAttr = genConstant("StringValue", classTypeName, classTypeValue);

    attrs += [classObjectTypeAttr, classTypeAttr];
 
    for(Attribute a <- attributes)
    {   
      str objectTypeName = n.val + capitalize(a.name.val) + "ObjectType";     
      Exp objectTypeValue = e_call(name(id("typeof")), [e_val(v_var(name(id("<toString(a.typ)>"))))]);
      Attribute objectTypeAttr = genConstant("Type", objectTypeName, objectTypeValue);
  
      str typeName = n.val + capitalize(a.name.val) + "Type";  
      Exp typeValue = e_new(t_class(id("StringValue")),
        [
          e_call(name(id(objectTypeName),[p_field(id("ToString"))]), [])
        ]);
      Attribute typeAttr = genConstant("StringValue", typeName, typeValue);

      str fieldName = n.val + capitalize(a.name.val) + "Field";      
      Exp fieldValue = e_new(t_class(id("Field")),[e_val(v_str(a.name.val))]);
      Attribute fieldAttr = genConstant("Field", fieldName, fieldValue);   
      
      attrs += [objectTypeAttr, typeAttr, fieldAttr];
    }
  }
 
  return attrs;
}
  
private Method GenerateMethod =
  method(vis_public(), poly_none(), t_void(), id("generate"), [param(when_none(),t_class(id("Event")),id("evt"))],
    body([
       s_if(
         e_is(name(id("evt")),t_class(name(id("Operation"),[p_field(id("IOperation"))])),id("op")),
         body([
           s_call(name(id("op"), [p_field(id("accept"))]), [e_val(v_var(name(id("this"))))])
         ])
       )
    ])
  );

/**
 * The genLocalVar functions generate different kinds of local variables.
 */ 
public Statement genLocalVarMeta(Typ t, Name n) =
  s_declare_assign(t_meta(t), id("<genName(n)>Meta"),
    e_call(
      name(id("op"),[p_field(id("get<capitalize(genNameTail(n))>"))]), 
      []));

public Statement genLocalVarIMeta(Name n) =
  s_declare_assign(t_class(id("IMetaObject")), id("<genName(n)>Meta"),
    e_call(
      name(id("op"),[p_field(id("get<capitalize(genNameTail(n))>"))]), 
      []));

//note: t must be a non-basetype
public Statement genLocalVarMetaNew(Typ t, Name n) =
  s_declare_assign(t_meta(t), id("<genName(n)>Meta"),
    e_call(
      name(id("patcher"),[p_field(id("createMetaObject\<<toString(t)>\>"))]), 
      []));

public Statement genLocalVar(Typ t, Name n) =
  s_declare_assign(t, id(genName(n)),
    e_cast(
      t,
      e_call(
        name(id("<genName(n)>Meta"),[p_field(id("getObject"))]),
        []))
    );

public Statement genLocalVarCast(Typ t, Name n) =
  s_declare_assign(t, id(genName(n)),
    e_cast(t,
      e_call(
        name(id("<genName(n)>Meta"),[p_field(id("getObject"))]),
        [])));

public Statement genLocalVarFromModel(Typ t, Name n) =
  s_declare_assign(t, id(genName(n)),
    e_call(
      name(id(genNameFront(n)),[p_field(id("get<capitalize(genNameTail(n))>"))]),  //fixme name
      []));
      
public Statement genLocalVarMetaFromModel(Name n) =
  s_declare_assign(t_class(id("IMetaObject")), id("<genName(n)>Meta"),
    e_call(
      name(id(genName(n)),[p_field(id("getMetaObject"))]),
      []));
    
public Statement genLocalVarId(Name n) =
  s_declare_assign(t_class(id("ID")), id("<genName(n)>Id"),
    e_call(
      name(id("<genName(n)>Meta"),[p_field(id("getKey"))]),
      []));
        
public Statement genLocalVarPath(Name n) =
  s_declare_assign(t_class(id("Value")), id("<genName(n)>Value"),
    e_new(
      t_class(id("Path")),
      [e_val(v_var(name(id("<genName(n)>Id"))))])); 
      
public Statement genLocalVarValue(Typ t, Name n) =
  s_declare_assign(t, id("<genName(n)>Value"),
    e_call(
      name(id("op"),[p_field(id("get<capitalize(n.head.val)>"))]), 
      []));
      
public Statement genLocalVarBaseType(Typ t, Name n) =
  s_declare_assign(t, id(genName(n)),
    e_cast(t,
      e_call(
        name(id("<genName(n)>Value"),[p_field(id("getValue"))]), 
        [])));
      
public Statement genLocalValWrapped(Name n) =
  s_declare_assign(t_class(id("Value")), id("<genName(n)>Value"),
    e_call(
      name(id("Patcher"),[p_field(id("encode"))]),
      [e_val(v_var(name(id(genName(n)))))]));
      
public Statement genStoreOldValue(Typ typ, ID param, Name val) =
  s_call
  (
    name(id("op"),[p_field(id("setOld<capitalize(param.val)>"))]), 
    [
      e_cast
      (
        typ,
        e_call(name(id("Patcher"),[p_field(id("encode"))]),[e_val(v_var(val))])
      )
    ]
  );

public Statement genStoreOldMetaValue(Typ typ, ID param, Name val) =
  s_if_else(
    e_neq(e_val(v_var(val)),e_val(v_null())),
    body(
    [
      s_call(
        name(id("op"),[p_field(id("setOld<capitalize(param.val)>"))]), 
        [e_cast(typ, 
          e_call(name(val.head, val.path+[p_field(id("getMetaObject"))]),[])
        )])
    ]),
    body(
    [
      s_call(
        name(id("op"),[p_field(id("setOld<capitalize(param.val)>"))]), 
        [e_cast(typ, 
          e_val(v_null())
        )])
    ]));
      
/*
public list[Statement] genRetrieveObjects(Typ t, Name qname)
{
  list[Statement] retrieval = [];
  
  bottom-up visit(qname)
  {
    case p_lookup(Value key: v_var(Name name)):
    {
      //retrieve the object value for the head
      retrieval += genLocalVar(t, name.head);
    }
  }

  //retrieve the object root.
  retrieval += genLocalVar(t, qname.head);
            
  return retrieval;
}
*/

//Generates a "Create" visitor method (a constructor)
//for creating new objects and setting attributes
private Method genVisitorMethodCreate(Event event: 
  effect(_, _, _, _, _, _, _, _),
  Unit eventClass, ID className, Package modelPackage)
{
  list[Statement] setup = [];
    
  //for each operation in the effect body
  //  generate the associated operation 
  //todo: check that ObjectCreate(oID, oT) creates the object of the correct type
  list[Statement] operations = 
    [s_comment("operations")] + [genStatement(op, className) | op <- event.ops.ops];
  
  set[str] uses = {};
  visit(operations)
  {
    case Value v: v_var(Name nm):
    {
      uses += toString(nm);
    }
  }
  //setup += s_comment("uses <uses>");

  //Assumed: the first parameter is of the type 'future'
  Param futureParam = event.params[0];
  Name objName = name(futureParam.name);
  Typ objType = futureParam.typ;
  ID objTypeName = id(toString(futureParam.typ));

  setup += s_comment("object <toString(objType)> <toString(objName)>");
  
  //1. Retrieve the meta parameter (oMeta) from the Effect.
  setup += genLocalVarMeta(objType, objName);
    
  //2. Retrieve the id (oID) from that meta parameter (oMeta).
  setup += genLocalVarId(objName);
 
  //3. Create the path/value (oV) to the new object,
  //   which is used in ObjectSet operations for attributes.
  setup += genLocalVarPath(objName);
  
  //Note: the metaObject must already exist!
  //      By design, the object life line must start before its birth.
   
  //for each of the 'owned' attributes that are not base-type attributes.
  //  Node: We therefore only process 'normal' parameters (without assigned expression).
  //  todo: check all String attributes are assigned because null is not a desired outcome.
  
  //Note: [1..] skips the bookkeeping
  Unit modelClass = lookupClass(modelPackage, objTypeName);
  for(Attribute attr: attr(own_self(), _, _, _, _, Typ t, ID n) <- modelClass.attributes) //[1..]
  {
    //only create metaobjects for contained lists, sets and maps
    //other owened objects need to be created upon post-migration
    if(t_list(_) := delAnnotationsRec(t) || t_set(Typ typ) := delAnnotationsRec(t) || t_map(_, _) := delAnnotationsRec(t))
    {
      //  1. create meta-object(aType)
      //     Define the beginning of the lifetime of the object that will exist.
      // MetaObject<Set<Trans>> sOMeta = patcher.createMetaObject<Set<Trans>>();
      setup += s_comment("owned attribute <toString(t)> <n.val>");
    
      Name attrName = name(futureParam.name,[p_field(n)]);
   
      setup += genLocalVarMetaNew(t, attrName);
    
      //  2. retrieve the id (aID) from the meta-object.
      setup += genLocalVarId(attrName);    
    
      //  3. create the path/vaue (aV) to the attribute using the id.
      setup += genLocalVarPath(attrName);

      //TODO: check an operation in the effect creates a new object of the specified attribute type.
      //TODO: check an operation in the effect sets the attribute to the newly created parent object.       
    }
  }
  
  //for each parameter of the remaining parameters
  //todo: check that parameters are not change parameters: not allowed in Create
  //todo: check that all parameters are actually used
  
  //for each attribute
  for(Attribute attr: attr(_ , _, _, _, _, Typ typ, ID n) <- eventClass.attributes[1..])
  {  
    //for a metaObject, also retrieve the inner object
    if(t_meta(Typ st) := delAnnotationsRec(typ))
    {
      setup += s_comment("meta parameter <toString(typ)> <n.val>");
 
      //1. retrieve the meta object
      setup += genLocalVarMeta(st, name(n));
      
      //2. retrieve its id      
      setup += genLocalVarId(name(n));
      
      //3. create a value for use in operations
      setup += genLocalVarPath(name(n)); 
      
      //4. retrieve value from meta object
      setup += genLocalVar(st, name(n)); 
    }
    else
    {
      setup += s_comment("value parameter <toString(typ)> <n.val>"); 
         
      //retrieve the constant basetype value for use in operations
      setup += genLocalVarValue(typ, name(n));
      //setup += genLocalVar
    }
  }

  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body(setup+operations));
}

//Generates a "Delete" visitor method (a destructor)
//for deleting existing objects and deleting owned attributes
private Method genVisitorMethodDelete(Event event: 
  effect(_, _, _, _, list[Param] params, _, _, _),
  Unit eventClass, ID className, Package modelPackage)
{ 
  list[Statement] setup = [];
  //setup += s_comment("Delete");
  
  //for each operation in the effect body
  //  generate the associated operation 
  //todo: check that ObjectDelete(oID, oT) deletes the object of the correct type 
  list[Statement] operations = 
    [s_comment("operations")] + [genStatement(op, className) | op <- event.ops.ops];
   
  set[str] uses = {};
  visit(operations)
  {
    case Value v: v_var(Name n):
    {
      uses += toString(n);
    }
  }
  //setup += s_comment("uses <uses>");

  //Assumed: the first parameter is of the type 'past'
  Param pastParam = event.params[0];
  Name objName = name(pastParam.name);
  Typ objType = pastParam.typ;
  ID objTypeName = id(toString(pastParam.typ));

  setup += s_comment("object <toString(objType)> <toString(objName)>");
  
  //1. Retrieve the meta parameter (oMeta) from the Effect.
  setup += genLocalVarMeta(objType, objName);
    
  //2. Retrieve the id (oID) from that meta parameter (oMeta).
  setup += genLocalVarId(objName);
 
  //3. Create the path/value (oV) to the new object,
  //   which is used in ObjectSet operations for attributes.
  setup += genLocalVarPath(objName);
  
  //4. obtain object
  setup += genLocalVar(objType, objName); 
   
  //for each of the 'owned' attributes that are not base-type attributes
  //TODO: check that every attribute is 'saved' in the effect
  //TODO: check the effect sets each attribute to null.  
  
  //Note: [1..] skips the bookkeeping
  Unit modelClass = lookupClass(modelPackage, objTypeName);
  for(Attribute attr: attr(_,_, _, _, _, _, _) <- modelClass.attributes) //[1..]
  {
    Name attrName = name(pastParam.name,[p_field(attr.name)]);
  
    if(isBaseType(attr.typ))
    {      
      if("<genName(attrName)>Value" in uses)
      {
        setup += s_comment("value attribute <toString(attr.typ)> <toString(attr.name)>");   
    
        //1. retrieve the object(aType) from the existing object
        setup += genLocalVarFromModel(attr.typ, attrName);
            
        //2. wrap the basetype in a value
        setup += genLocalValWrapped(attrName);
      }
    }
    else
    {
      if("<genName(attrName)>Value" in uses || "<genName(attrName)>Id" in uses)
      {
        setup += s_comment("meta attribute <toString(attr.typ)> <toString(attr.name)>");
      
        //1. retrieve the object(aType) from the existing object
        setup += genLocalVarFromModel(attr.typ, attrName);
    
        //2. retrieve its metaobject
        setup += genLocalVarMetaFromModel(attrName);
    
        //3. retrieve the id (aID) from the meta-object.
        setup += genLocalVarId(attrName);    
     
        //4. create the path/vaue (aV) to the attribute using the id.
        setup += genLocalVarPath(attrName);
        
        //TODO: check the effect deletes the attribute  
      }  
    }
  }
  
  //for each parameter of the remaining parameters
  //todo: check that only change parameters are allowed
  //todo: check that all parameters are actually used
  
  for(Attribute attr: attr(_ , _, _, _, _, Typ typ, ID n) <- eventClass.attributes[1..])
  {  
    //for a metaObject, also retrieve the inner object
    
    if(startsWith(n.val, "old")) //yes not pretty
    {    
      ID paramName = id(toLowerCase(n.val[3..]));
      Name attrName = name(pastParam.name, [p_field(id(toLowerCase(n.val[3..])))]);
    
      if(t_meta(Typ t) := delAnnotationsRec(typ))
      {
        //fixme: retrieve the assigned variable from the parameter instead of the event
        //now it only works if the names accidentally match (which they mostly do)
        //Idea: enforce this in the checker
        
        //store the old meta-object in the effect
        setup += s_comment("store old meta parameter type: <toString(typ)>, param: <toString(paramName)>, attr: <toString(attrName)>");
        setup += genStoreOldMetaValue(typ, paramName, attrName);

      }
      else
      {
        //store the old value in the effect
        setup += s_comment("store old value parameter type: <toString(typ)>, param: <toString(attrName)>, attr: <toString(attrName)>");        
        setup += genStoreOldValue(typ, paramName, attrName);
      }
    }
    else
    {
      if(t_meta(Typ t) := delAnnotationsRec(typ))
      {
        if("<n.val>Value" in uses || "<n.val>Id" in uses)
        {     
          setup += s_comment("meta parameter <toString(typ)> <n.val>");    
    
          //1. retrieve the meta object
          setup += genLocalVarMeta(t, name(n));
      
          //2. retrieve its id      
          setup += genLocalVarId(name(n));
      
          //3. create a value for use in operations
          setup += genLocalVarPath(name(n));
        
          //4. retrieve value from meta object
          setup += genLocalVar(t, name(n));
        }
      }
      else
      {
        if("<n.val>Value" in uses)
        {      
          setup += s_comment("value parameter <toString(typ)> <n.val>"); 
          
          //1. retrieve the constant basetype value for use in operations
          setup += genLocalVarValue(typ, name(n));
        }
      }
    }
  }

  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body(setup+operations));
}

private Method genVisitorMethodChange(Event event: 
  effect(_, _, _, _, list[Param] params, _, _, _),
  Unit eventClass, ID className, Package modelPackage)
{  
  list[Statement] setup = [];
  
  list[Statement] operations =
    [s_comment("operations")] + [genStatement(op, className) | op <- event.ops.ops];
  
  set[str] uses = {};
  visit(operations)
  {
    case Value v: v_var(Name n):
    {
      uses += toString(n);
    }
  }
  //setup += s_comment("uses <uses>");
  
  //Assumed: the first parameter is the object that will change
  Param objParam = event.params[0];
  Name objName = name(objParam.name);
  Typ objType = objParam.typ;
  ID objTypeName = id(toString(objParam.typ));

  setup += s_comment("object <toString(objType)> <toString(objName)>");
  
  //1. Retrieve the meta parameter (oMeta) from the Effect.
  setup += genLocalVarMeta(objType, objName);
    
  //2. Retrieve the id (oID) from that meta parameter (oMeta).
  setup += genLocalVarId(objName);
 
  //3. Create the path/value (oV) to the new object,
  //   which is used in ObjectSet operations for attributes.
  setup += genLocalVarPath(objName); 

  //4. obtain object
  setup += genLocalVar(objType, objName); 
    
  //for each of the 'owned' attributes that are not base-type attributes
  //TODO: check that every attribute is 'saved' in the effect
  //TODO: check the effect sets each attribute to null.  
  
  //Note: [1..] skips the bookkeeping
  Unit modelClass = lookupClass(modelPackage, objTypeName);
  for(Attribute attr: attr(_ , _, _, _, _, _, _) <- modelClass.attributes) //[1..]
  {
    Name attrName = name(objParam.name,[p_field(attr.name)]);
  
    if(isBaseType(attr.typ))
    {    
      if("<genName(attrName)>Value" in uses)
      {
        setup += s_comment("value attribute <toString(attr.typ)> <toString(attrName)>");   
     
        //1. retrieve the object(aType) from the existing object
        setup += genLocalVarFromModel(attr.typ, attrName);
            
        //2. wrap the basetype in a value
        setup += genLocalValWrapped(attrName);
      }
    }
    else
    {
      if("<genName(attrName)>Value" in uses || "<genName(attrName)>Id" in uses)
      {      
        setup += s_comment("meta attribute <toString(attr.typ)> <toString(attrName)>");
    
        //1. retrieve the object(aType) from the existing object
        setup += genLocalVarFromModel(attr.typ, attrName);
    
        //2. retrieve its metaobject
        setup += genLocalVarMetaFromModel(attrName);
    
        //3. retrieve the id (aID) from the meta-object.
        setup += genLocalVarId(attrName);    
    
        //4. create the path/value (aV) to the attribute using the id.
        setup += genLocalVarPath(attrName);       
      }
    }
  }
  
  //for each parameter of the remaining parameters
  //todo: check that only change parameters are allowed
  //todo: check that all parameters are actually used
  
  for(Attribute attr: attr(_ , _, _, _, _, Typ typ, ID n) <- eventClass.attributes[1..])
  {  
    //for a metaObject, also retrieve the inner object    
    if(startsWith(n.val, "old")) //yes not pretty
    {    
      ID paramName = id(toLowerCase(n.val[3..]));
      Name attrName = name(objParam.name, [p_field(id(toLowerCase(n.val[3..])))]);
    
      if(t_meta(Typ t) := delAnnotationsRec(typ))
      {
        //fixme: retrieve the assigned variable from the parameter instead of the event
        //now it only works if the names accidentally match (which they mostly do)
        //Idea: enforce this in the checker
        
        //store the old meta-object in the effect
        setup += s_comment("store old meta parameter type: <toString(typ)>, param: <toString(paramName)>, attr: <toString(attrName)>");
        setup += genStoreOldMetaValue(typ, paramName, attrName);
      }
      else
      {
        //store the old value in the effect
        setup += s_comment("store old value parameter type: <toString(typ)>, param: <toString(attrName)>, attr: <toString(attrName)>");        
        setup += genStoreOldValue(typ, paramName, attrName);
      }
    }
    else
    {
      if(t_meta(Typ t) := delAnnotationsRec(typ))
      {
        if("<n.val>Value" in uses || "<n.val>Id" in uses)
        {
          setup += s_comment("meta parameter <toString(typ)> <n.val>");    
            
          //1. retrieve the meta object
          setup += genLocalVarMeta(t, name(n));
      
          //2. retrieve its id      
          setup += genLocalVarId(name(n));
      
          //3. create a value for use in operations
          setup += genLocalVarPath(name(n));
        
          //4. store value
          setup += genLocalVar(t, name(n));
        }
      }
      else
      {
        if("<n.val>Value" in uses)
        {
          setup += s_comment("value parameter <toString(typ)> <n.val>");
          //1. retrieve the constant basetype value for use in operations
          setup += genLocalVarValue(typ, name(n));
        }
      }
    }
  }
  
  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body(setup+operations));
}

private Method genVisitorMethod(Event: evt: effect(_, _, _, ID name, _, _, _, _), Unit eventClass, ID className, Package modelPackage)
{
  Unit modelClass = lookupClass(modelPackage, className);
  if(imp_abstract() := delAnnotationsRec(modelClass.imp))
  {
    return method(vis_public(), poly_none(), t_void(), id("visit"),
      [param(when_none(),t_class(eventClass.name),id("op"))], body([]));
  }
  else if(startsWith(name.val, "Create"))
  {
    return genVisitorMethodCreate(evt, eventClass, className, modelPackage);
  }
  else if(startsWith(name.val, "Delete"))
  {
    return genVisitorMethodDelete(evt, eventClass, className, modelPackage);
  }
  else
  {
    return genVisitorMethodChange(evt, eventClass, className, modelPackage);
  }
}

private default Method genVisitorMethod(Event event, Unit eventClass, ID className, Package modelPackage)
{
  return method(vis_public(), poly_none(), t_void(), id("visit"),
    [param(when_none(),t_class(eventClass.name),id("op"))], body([]));
}

/**
 * The genStatement function generates a C# statement for a given Operation and class name.
 * Note: the class name is used to disambiguate variable names.
 * Note: relies on the genExp function to specialize the statement for different operations.
 */
private Statement genStatement(Operation op, ID className) =
  s_call(name(id("op"),[p_field(id("addOperation"))]), [genExp(op, className)]);

/**
 * The genExp function generates a C# expression for a given Operation and class name.
 * Note: the class name is used to disambiguate variable names.
 * Note: relies on the genValue function for generating constituent C# values.
 * Note: relies on the genType function for generating type names.
 */
private Exp genExp(Operation op: o_new(Name n, Typ typ), ID className) =
  e_new(t_class(id("ObjectCreate")),
    [e_val(v_var(name(id("<genName(n)>Id")))),
     e_val(v_var(name(id("<className.val><capitalize(genType(n))>Type"))))
    ]);
 
private Exp genExp(Operation op: o_del(Name n, Typ typ), ID className) =
  e_new(t_class(id("ObjectDelete")),
    [e_val(v_var(name(id("<genName(n)>Id")))),
     e_val(v_var(name(id("<className.val><capitalize(genType(n))>Type"))))
    ]);
  
private Exp genExp(Operation op: o_set(Name n, ID field, Value newVal), ID className) =
  e_new(t_class(id("ObjectSet")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     e_val(v_var(name(id("<className.val><capitalize(field.val)>Field")))),
     genValue(newVal)
    ]);
  
private Exp genExp(Operation op: m_set(Name n, Value key, Value val), ID className) =
  e_new(t_class(id("MapSet")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key),
     genValue(val)
    ]);
  
private Exp genExp(Operation op: m_add(Name n, Value key), ID className) =
  e_new(t_class(id("MapAdd")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);
  
private Exp genExp(Operation op: m_remove(Name n, Value key), ID className) =
  e_new(t_class(id("MapRemove")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);

private Exp genExp(Operation op: s_add(Name n, Value key), ID className) =
  e_new(t_class(id("SetAdd")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);

private Exp genExp(Operation op: s_remove(Name n, Value key), ID className) =
  e_new(t_class(id("SetRemove")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);

private Exp genExp(Operation op: l_insert(Name n, Value key, Value val), ID className) =
  e_new(t_class(id("ListInsert")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key),
     genValue(val)
    ]);
    
private Exp genExp(Operation op: l_remove(Name n, Value key), ID className) =
  e_new(t_class(id("ListRemove")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);    

private Exp genExp(Operation op: l_remove(Name r, Name n, Value key), ID className) =
  e_new(t_class(id("ListRemove")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);
    
private Exp genExp(Operation op: l_push(Name n, Value key), ID className) =
  e_new(t_class(id("ListPush")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value"))))),
     genValue(key)
    ]);  
    
private Exp genExp(Operation op: l_pop(Name n), ID className) =
  e_new(t_class(id("ListPop")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value")))))
    ]);

private Exp genExp(Operation op: l_pop(Name r, Name n), ID className) =
  e_new(t_class(id("ListPop")),
    [e_cast(t_class(id("Path")), e_val(v_var(name(id("<genName(n)>Value")))))
    ]);

private default Exp genExp(Operation op, ID className)
{
  throw op;
}

/**
 * The genValue function generates a C# expression for a given value.
 */
//note: this dynamic wrap could have been static
//private Exp genValue(Value v: v_var(Name n)) =
//  e_call(
//    name(id("patcher"),[p_field(id("encode"))]),
//    [e_val(v)]);

private Exp genValue(Value v: v_int(int val)) = 
  e_new(t_class(id("IntValue")),[e_val(v)]);

private Exp genValue(Value v: v_str(str val)) = 
  e_new(t_class(id("StringValue")),[e_val(v)]);

private Exp genValue(Value v: v_false()) = 
  e_new(t_class(id("BoolValue")),[e_val(v)]);

private Exp genValue(Value v: v_true()) = 
  e_new(t_class(id("BoolValue")),[e_val(v)]);

private Exp genValue(Value v: v_enum(ID enum, ID val)) = 
  e_new(t_class(id("EnumValue")), [e_val(v)]);

private Exp genValue(Value v: v_null()) = 
  e_val(v_var(name(id("NullValue"),[p_field(id("Null"))])));
  

private default Exp genValue(Value v: v_var(Name n)) = 
  e_val(v_var(name(id("<genName(n)>Value"))));

//todo: catch errors
private default Exp genValue(Value v)
{
  throw "cannot generate value <v>";
}

/**
 * The genType function generates type names.
 */
private str genType(name(ID n)) = "";
private str genType(name(ID n, [])) = "";
private str genType(name(ID n, list[Path] path)) = path[0].field.val;

/**
 * The genGenerator function generates a C# package containing the Generator class.
 */
public Package genGenerator(Package inputPackage, Package modelPackage, Package operationPackage)
{
  Name runtimePackageName = name(inputPackage.name.head, inputPackage.name.path + [p_field(id("Runtime"))]); 

  Using using = genRuntimeUsing(inputPackage);
   
  ID generatorName = id("Generator");

  list[Attribute] attributes = [PatcherAttribute] + genStaticAttributes(modelPackage); 

  Method generatorConstructor = genConstructor(generatorName, [PatcherAttribute]);

  list[Method] methods = [generatorConstructor, GenerateMethod];

  for(Unit cls: evt_class(_, _, _, _, _, _) <- inputPackage.units)
  {
    for(Event event <- cls.events)
    {      
      ID eventClassName = id(cls.name.val + capitalize(event.name.val));
   
      Unit operationClass = lookupClass(operationPackage, eventClassName);

      println("<toString(eventClassName)>");
      methods += [genVisitorMethod(event, operationClass, cls.name, modelPackage)];
    };
  };
 
  return package(using, runtimePackageName,
    [
      class(vis_public(), imp_concrete(), generatorName, ExtendsIGenerator, attributes, methods)
    ]);
}
