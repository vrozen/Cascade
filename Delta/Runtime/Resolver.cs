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
using System;
using System.Collections.Generic;
using System.Reflection;
using Delta.Model.BaseType;
using Delta.Model.QualifiedName;

namespace Delta.Runtime
{
  public class Resolver : Delta.Model.QualifiedName.IVisitor, Delta.Model.BaseType.IVisitor
  {
    private class Context
    {
      private object curObject;
      private object curKey;
      private Name curName;

      public void setCurObject(object obj)
      {
        curObject = obj;
      }
      
      public void setCurName(Name name)
      {
        curName = name;
      }

      public void setCurKey(object curKey)
      {
        this.curKey = curKey;
      }

      public Name getCurName()
      {
        return curName;
      }

      public object getCurObject()
      {
        return curObject;
      }

      public object getCurKey()
      {
        return curKey;
      }
    }

    private readonly Patcher patcher;
    private readonly Stack<Context> ctx;
    private Context curContext;
    private readonly PrettyPrinter printer;

    public Resolver(Patcher patcher)
    {
      this.patcher = patcher;
      this.printer = patcher.getPrettyPrinter();
      this.ctx = new Stack<Context>();
    }

    /*
     * Resolves the path in the object up to (but excluding) its suffix.
     * After resolving, retrieve the suffix using getResolvedName.
     * After resolving, retrieve the resolved parent using getResolvedParentObject
     * After resolving, retrieve the resolved object using gerResolvedObject.
     */
    public object resolve(Value value)
    {
      curContext = new Context();
      if (value != null)
      {
        value.accept(this);
      }
      return curContext.getCurObject();
    }
    
    //curContext remains the same. context has to be a list
    public object resolveChild(Name name)
    {
      name.accept(this);
      return curContext.getCurObject();
    }
    
    public T resolve<T>(Value value)
    {
      object obj = resolve(value);
      
      T val = default(T);
      if (obj == null)
      {
        val = default(T);
      }
      else if (obj is T)
      {
        val = (T) obj;
      }
      else
      {
        throw new PatchException("Resolver Error: Expected object of type " + typeof(T) +
                                 ", found type " + obj.GetType() + printer.printAt(curContext.getCurName()));
      }
      return val;
    }

    public void visit(IntValue value)
    {
      int iVal = value.getValue(); 
      curContext.setCurObject(iVal);
    }

    public void visit(BoolValue value)
    {
      bool bVal = value.getValue();
      curContext.setCurObject(bVal);
    }

    public void visit(StringValue value)
    {
      string sVal = value.getValue();
      curContext.setCurObject(sVal);
    }

    public void visit(EnumValue value)
    {
      object eVal = value.getValue();
      curContext.setCurObject(eVal);      
    }

    public void visit(NullValue value)
    {
      curContext.setCurObject(null);
    }

    public void visit(Location value)
    {
      //IMetaObject metaObject = patcher.lookupMetaObject(value);
      //object obj = metaObject.getObject();
      curContext.setCurObject(value);
    }

    public void visit(UUID value)
    {
      //IMetaObject metaObject = patcher.lookupMetaObject(value);
      //object obj = metaObject.getObject();
      curContext.setCurObject(value);
    }
    
    public void visit(Path path)
    {
      Name name = path.getName();
      Name childName = null;
      bool success = false;

      if (name is Lookup lookup)
      {
        Value key = lookup.getKey();
        if (key is ID id)
        {
          IMetaObject metaObject = patcher.lookupMetaObject(id);
          object obj = metaObject.getObject();
          curContext.setCurObject(obj);
          childName = name.getName();
          curContext.setCurName(childName);
          success = true;
        }
      }

      if (success == false)
      {
        throw new PatchException("Resolver Error: Expected concrete path, found " + printer.printAt(path));
      }

      if (childName != null)
      {
        curContext.setCurName(childName);
        childName.accept(this);
      }
    }

    public void visit(Field name)
    {
      object curObject = curContext.getCurObject();

      if (curObject == null)
      {
        throw new PatchException("Resolver Error: Missing object in lookup of " + printer.printAt(name));
      }

      string fieldName = name.getFieldName(); 
      
      Type type = curObject.GetType();

      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      
      FieldInfo fieldInfo = type.GetField(fieldName, flags);

      if (fieldInfo == null)
      {
        throw new PatchException("Resolver Error: Missing class field in lookup of " + printer.printAt(name));
      }
      
      object childObject = fieldInfo.GetValue(curObject);
      curContext.setCurObject(childObject);

      Name childName = name.getName();
      
      if (childName != null)
      {
        curContext.setCurName(childName);
        childName.accept(this);
      }
    }
    
    public void visit(Lookup name)
    {
      object curObject = curContext.getCurObject();
      
      if (curObject == null)
      {
        throw new PatchException("Resolver Error: Missing object in lookup of " + printer.printAt(name));
      }
      
      if (curObject is IList list)
      {
        visitList(list, name); 
      }
      else if (curObject is IMap map)
      {
        visitMap(map, name);
      }
      else
      {
        Type type = curObject.GetType();
        throw new PatchException("Resolver Error: Expected List or Map, found " + type +
                                 " in lookup of " + printer.printAt(name));
      }
      
      Name childName = name.getName();
      if (childName != null)
      {
        curContext.setCurName(childName);
        childName.accept(this);
      }
    }

    private void visitList(IList curList, Lookup name)
    {
      Value operand = name.getKey();
      ctx.Push(curContext);
      int index = resolve<int>(operand);
      curContext = ctx.Pop();
      curContext.setCurKey(index);
      
      if (index >=0 && index < curList.getCount())
      {
        object obj = curList.elementAt(index);
        curContext.setCurObject(obj);
      }
      else
      {
        throw new PatchException("Resolver Error: Cannot resolve list element, index " + index +
                                 " out of bounds [0.." + curList.getCount()  + "] in lookup of "+ printer.printAt(name));
      }
    }

    private void visitMap(IMap curMap, Lookup name)
    {
      Value operand = name.getKey();
      ctx.Push(curContext);
      object key = resolve(operand);
      curContext = ctx.Pop();
      curContext.setCurKey(key);

      if(curMap.containsKey(key))
      {
        object obj = curMap.get(key);
        curContext.setCurObject(obj);
      }
      else
      {
        throw new PatchException("Resolver Error: Cannot resolve map key, index " + key +
                                 " in lookup of "+ printer.printAt(name));
      }
    }
  }
}