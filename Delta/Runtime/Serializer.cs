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

//no longer used

/*
using System;
using System.Reflection;
using Delta.Model.DesignSpace;
using Delta.Model.BaseType;
using Delta.Model.BaseOperation;

namespace Delta.Runtime
{
  public class Serializer
  {
    private readonly Patcher patcher;
    private Effect effect;
    //private Map<object, ID> ids;
    //private List<Tuple<object, ID>> patches;

    public Serializer(Patcher patcher)
    {
      this.patcher = patcher;
    }
    
    // Serializes an unmanaged object tree into the managed history
    // @note: the tree must be self-contained (no external references)

    public History serialize(object obj)
    {
      //patches = new List<Tuple<object, ID>>();
      //ids = new Map<object, ID>();
      effect = new SnapShot();
      History history = new History();
      history.addEvent(effect);
      serializeObject(obj);
      //patch();
      return history;
    }

    public Patcher getPatcher()
    {
      return patcher;
    }

    private void patch()
    {
      foreach (Tuple<object, ID> tuple in patches)
      {
        object obj = tuple.Item1;
        UUID patchId = (UUID) tuple.Item2;
        if (ids.ContainsKey(obj) == false)
        {
          throw new PatchException("Cannot serialize object, containment tree broken.");
        }

        UUID trueId = (UUID) ids[obj];
        int trueKey = trueId.getKey();
        patchId.patchKey(trueKey);
      }
    }

    private void addPatch(object obj, UUID id)
    {
      patches.Add(new Tuple<object, ID>(obj, id));
    }

    private ID createId(object obj)
    {
      IMetaObject metaObject = patcher.createMetaObject<object>();
      ID id = metaObject.getKey();
      ids[obj] = id;
      return id;
    }

    //serialize a root (owned) object
    // -- create the object
    // -- set each attribute
    private Value serializeObject(object obj)
    {
      ID id = createId(obj);
      Path path = new Path(id);
      Type type = obj.GetType();
      String typeName = type.ToString();
      Operation createOp = new ObjectCreate(path, typeName);
      effect.prependOperation(createOp);

      foreach (FieldInfo field in type.GetRuntimeFields())
      {
        Type attrType = field.FieldType;
        String attrName = field.Name;
        Object attrValue = field.GetValue(obj);

        //skip static fields
        if (field.IsStatic)
        {
          continue;
        }

        //skip internal attributes
        if (attrName.StartsWith("_", StringComparison.CurrentCulture))
        {
          continue;
        }

        bool isReference = (field.GetCustomAttribute(typeof(Reference)) != null);

        Value val = null;
        if (isReference)
        {
          val = new UUID(-1);
          addPatch(attrValue, (UUID) val);
        }
        else
        {
          val = serializeValue(attrValue, false);
        }

        Path fieldPath = new Path(id, attrName);
        Operation setOp = new ObjectSet(fieldPath, val);
        effect.addOperation(setOp);
      }

      return id;
    }

    private bool isOwnedType(object obj)
    {
      return (obj is string || obj is int || obj is bool ||
              obj is Delta.Model.BaseType.List<object> ||
              obj is Delta.Model.BaseType.Map<object, object> ||
              obj is Delta.Model.BaseType.Set<object>);
    }

    private Value serializeValue(object oVal, bool isReference)
    {
      Value val = null;
      if (oVal is string sVal)
      {
        val = new StringValue(sVal);
      }
      else if (oVal is bool bVal)
      {
        val = new BoolValue(bVal);
      }
      else if (oVal is int iVal)
      {
        val = new IntValue(iVal);
      }
      else if (oVal is IList)
      {
        val = serializeList((IList) oVal, isReference);
      }
      else if (oVal is IMap)
      {
        val = serializeMap((IMap) oVal, isReference);
      }
      else if (oVal is ISet)
      {
        val = serializeSet((ISet) oVal, isReference);
      }
      else
      {
        val = serializeObject(oVal);
      }

      return val;
    }


    private Value serializeList(IList list, bool isReference)
    {
      ID id = createId(list);
      Path path = new Path(id);
      string typeName = list.GetType().ToString();
      Operation createOp = new ObjectCreate(path, typeName);
      effect.prependOperation(createOp);

      for (int pos = list.getCount() - 1; pos >= 0; pos--)
      {
        Object oVal = list.elementAt(pos);
        Value val = null;
        if (isReference)
        {
          val = new UUID(-1);
          addPatch(oVal, (UUID) id);
        }
        else
        {
          val = serializeValue(oVal, false);
        }

        Operation insertOp = new ListPush(path, val);
        effect.addOperation(insertOp);
      }

      return id;
    }

    private Value serializeMap(IMap map, bool isReference)
    {
      ID id = createId(map);
      //Path path = new Path(id);
      string typeName = map.GetType().ToString();
      StringValue typeNameVal = new StringValue(typeName);
      Operation createOp = new ObjectCreate(id, typeNameVal);
      effect.prependOperation(createOp);

      foreach (Object key in map.getKeys())
      {
        Value val = serializeValue(key, isReference);
        Path keyPath = new Path(id, val);
        Operation addOp = new MapAdd(keyPath);
        effect.addOperation(addOp);
        Operation setOp = new MapSet(keyPath, val);
        effect.addOperation(setOp);
      }

      return id;
    }

    private Value serializeSet(ISet set, bool isReference)
    {
      ID id = createId(set);
      Path path = new Path(id);
      string typeName = set.GetType().ToString();
      StringValue typeNameVal = new StringValue(typeName);
      Operation createOp = new ObjectCreate(id, typeNameVal);
      effect.prependOperation(createOp);

      foreach (Object oVal in set)
      {
        Value val = null;
        if (isReference)
        {
          val = new UUID(-1);
          addPatch(oVal, (UUID) val);
        }
        else
        {
          val = serializeValue(oVal, false);
        }

        Operation addOp = new SetAdd(path, val);
        effect.addOperation(addOp);
      }

      return id;
    }
  }
}*/