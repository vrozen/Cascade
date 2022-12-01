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
using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using System.Reflection;
using Delta.Model.BaseOperation;
using Delta.Model.QualifiedName;

/*
 * The Patcher controls and adjusts objects under its control at run time.
 * Operation object id's (or keys) must be valid and issued by the Patcher,
 * this guarantees that lookups do not fail.
 * Objects under management must implement Patchable interface,
 * and must only be modified using operations via the Patcher.
 */
namespace Delta.Runtime
{
  public class Patcher : Model.BaseOperation.IVisitor
  {
    private readonly Map<ID, IMetaObject> objectSpace; //objects under management
    private readonly History history;                  //history of committed transactions
    private int keyCount;                              //used for issuing unique identifiers
    private readonly Resolver resolver;                //resolves qualified names
    private readonly PrettyPrinter printer;            //used for descriptive error messages

    public static readonly MetaObject<Patcher> SystemCause = new MetaObject<Patcher>(new UUID(0));
    
    public Patcher()
    {
      history = new History();
      printer = new PrettyPrinter();
      objectSpace = new Map<ID, IMetaObject>();
      keyCount = 1;
      resolver = new Resolver(this);
    }

    public PrettyPrinter getPrettyPrinter()
    {
      return printer;
    }

    public Resolver getResolver()
    {
      return resolver;
    }

    public void commit(Event evt)
    {
      history.addEvent(evt);
      //fixme: todo: use visitor
      if (evt is Effect effect)
      {
        foreach (Operation op in effect.getOperations())
        {
          op.accept(this);
        }
      }
    }

    public History getHistory()
    {
      return history;
    }

    /*
    public void visit(History h)
    {
      throw new PatchException("Patcher Error: Inlining an external history is not supported");
    }
    
    public void visit(Effect effect)
    {

    }

    public void visit(Cause cause)
    {
      //causes do not modify objects
    }

    public void visit(Signal signal)
    {
      //signals do not modify objects
    }*/

    public void visit(ObjectCreate op)
    {
      StringValue classNameValue = op.getClassName();
      ID id = op.getId();
      IMetaObject metaObject = lookupMetaObject(id);
      
      string className = classNameValue.getValue();
      Type type = Type.GetType(className);
      if (type == null)
      {
        throw new PatchException("Patcher Error: Cannot create instance of missing class " + printer.printAt(classNameValue));
      }

      Object obj = Activator.CreateInstance(type);
      metaObject.bind(obj);
    }

    public void visit(ObjectDelete op)
    {
      ID id = op.getId();
      IMetaObject metaObject = lookupMetaObject(id);
      metaObject.unbind();
      //metaObject.clear();
    }

    public void visit(ObjectKey op)
    {
      ID oldObjectId = op.getId();
      ID newObjectId = op.getNewId();
      IMetaObject metaObject = lookupMetaObject(oldObjectId);
      metaObject.setKey(newObjectId);
      objectSpace.Remove(oldObjectId);
      objectSpace.Add(newObjectId, metaObject);
    }
    
    public void visit(ObjectSet op)
    {
      Path path = op.getPath();
      Field field = op.getField();
      string fieldName = field.getFieldName();
      
      object parentObject = resolver.resolve(path);
      object oldValue = resolver.resolveChild(field);
      
      Value newValueOperand = op.getNewValue();
      object newValue = resolver.resolve(newValueOperand);

      Type type = parentObject.GetType();
      BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
      FieldInfo fieldInfo = type.GetField(fieldName, flags);
      
      if (fieldInfo == null)
      {
        throw new PatchException("Patcher Error: Cannot set value of missing class field " +
                                 printer.printAt(field));
      }

      if (newValue != null)
      {
        Type expectedType = fieldInfo.FieldType;
        Type foundType = newValue.GetType();
        bool instanceOf = expectedType.IsInstanceOfType(newValue);

        if (instanceOf == false)
        {
          throw new PatchException("Patcher Error: Expected value of type " + expectedType + 
                                   ", found " + foundType);
        }
      }

      fieldInfo.SetValue(parentObject, newValue);
      updateOldValue(op, oldValue);
    }

    public void visit(ListInsert op)
    {
      Path path = op.getPath();
      IList targetList = resolver.resolve<IList>(path);
      IntValue indexValue = op.getIndex();
      int index = indexValue.getValue();
      Value operand = op.getValue();
      object value = resolver.resolve(operand);
      
      if (index >= 0 && index <= targetList.getCount())
      {
        targetList.insert(index, value);
      }
      else
      {
        int count = targetList.getCount();
        throw new PatchException("Patcher Error: Cannot insert element in list, index "
                                 + printer.printAt(indexValue) + " out of bounds [0.." + count + "]");
      }
    }

    public void visit(ListRemove op)
    {
      Path path = op.getPath();
      IntValue indexValue = op.getIndex();
      int index = indexValue.getValue();
      Value operand = op.getValue();
      IList targetList = resolver.resolve<IList>(path);
      object removedValue = resolver.resolve(operand);

      if (index >= 0 || index < targetList.getCount())
      {
        targetList.removeAt(index);
        updateOldValue(op, removedValue);
      }
      else
      {
        int validRange = targetList.getCount() - 1;
        Location loc = indexValue.getLocation();
        throw new PatchException("Patcher Error: Cannot remove element from list, index "
                                 + index + " out of bounds [0.."+validRange+"] "+printer.printAt(loc));
      }
    }

    public void visit(ListSet op)
    {

      Path path = op.getPath();
      IList list = resolver.resolve<IList>(path);

      Value indexValue = op.getIndex(); 
      Lookup lookup = new Lookup(indexValue);
      object oldValue = resolver.resolveChild(lookup);
      
      int index = resolver.resolve<int>(indexValue);

      Value newValueOperand = op.getNewValue();
      object newValue = resolver.resolve(newValueOperand);
      
      list.storeAt(index, newValue);
      updateOldValue(op, oldValue);
    }

    public void visit(ListPush op)
    {
      Path path = op.getPath();
      IList targetList = resolver.resolve<IList>(path);
      Value operand = op.getValue();
      object value = resolver.resolve(operand);
      targetList.add(value);
    }

    public void visit(ListPop op)
    {
      Path path = op.getPath();
      IList targetList = resolver.resolve<IList>(path);

      if(targetList.getCount() > 0)
      {
        int index = targetList.getCount() - 1;
        object removedValue = targetList.elementAt(index); 
        updateOldValue(op, removedValue);
        targetList.removeAt(index);
      }
      else
      {
        throw new PatchException("Patcher Error: Cannot pop element from empty list "+
                                 printer.printAt(path));
      }
    }

    public void visit(SetAdd op)
    {
      Path path = op.getPath();
      ISet targetSet = resolver.resolve<ISet>(path);
      Value operand = op.getValue();
      object value = resolver.resolve(operand);
      targetSet.add(value);
    }

    public void visit(SetRemove op)
    {
      Path path = op.getPath();
      ISet targetSet = resolver.resolve<ISet>(path);
      Value operand = op.getValue();
      object value = resolver.resolve(operand);
      targetSet.add(value);
      
      if (targetSet.contains(value) == false)
      {
        throw new PatchException("Patcher Error: Cannot remove element from set, missing element " +
                                 printer.printAt(operand));  
      }
      
      targetSet.remove(value);
    }

    public void visit(MapAdd op)
    {
      Path path = op.getPath();
      IMap map = resolver.resolve<IMap>(path);
      Value keyValue = op.getKey();
      object key = resolver.resolve(keyValue);
      
      if (map.containsKey(key))
      {
        throw new PatchException("Patcher Error: Attempt to insert duplicate map record " + 
                                 printer.printAt(path));
      }

      map.put(key, null); //put the default object value in the map
    }

    public void visit(MapRemove op)
    {
      Path path = op.getPath();
      IMap map = resolver.resolve<IMap>(path);
      Value keyValue = op.getKey();
      object key = resolver.resolve(keyValue);

      if (map.containsKey(key) == false)
      {
        throw new PatchException("Patcher Error: Missing map record, cannot remove missing key " +
                                 printer.printAt(path));
      }

      if (map.get(key) != null)
      {
        throw new PatchException("Patcher Error: Map record not empty, cannot remove key " +
                                 printer.printAt(path));
      }

      map.remove(key);
    }

    public void visit(MapSet op)
    {
      Path path = op.getPath();
      IMap map = resolver.resolve<IMap>(path);
      
      Value keyValue = op.getKey(); 
      Lookup lookup = new Lookup(keyValue);
      object oldValue = resolver.resolveChild(lookup);
      
      object key = resolver.resolve(keyValue);

      Value newValueOperand = op.getNewValue();
      object newValue = resolver.resolve(newValueOperand);
      
      map.put(key, newValue);
      updateOldValue(op, oldValue);
    }

    /**
     * Stores the old value inside the operation if it has not been set.
     */
    private void updateOldValue(IUpdateable op, object removedValue)
    {
      if (op.hasOldValue() == false)
      {
        Value replacedValueOperand = encode(removedValue);
        op.setOldValue(replacedValueOperand);
      }
      else if(op is IReplaceable op2)
      {
        checkOperationSanity(op2, removedValue);
      }
    }

    /**
     * Checks if a value that has been removed is the same as the one inside the operation.
     */ 
    private void checkOperationSanity(IReplaceable op, object removedValue)
    {
      Value oldValueOperand = op.getOldValue();
      object oldValue = resolver.resolve(oldValueOperand);
      if (oldValue != null)
      {
        if (oldValue.Equals(removedValue) == false)
        {
          //todo: add source location to message
          //todo: add old and new value to message
          throw new PatchException("Patcher Error: Replaced value and operand differ.");
        }
      }
      else if (removedValue != null)
      {
        throw new PatchException("Patcher Error: Replaced value and operand differ.");
      }
    }
    
    public IMetaObject lookupMetaObject(ID key)
    {
      if(objectSpace.ContainsKey(key) == false)
      {
        string str = printer.print(key);
        throw new PatchException("Patcher Error: Lookup failed, found external (unmanaged) key " + str);     
      }
      
      return objectSpace[key];
    }

    public void deleteMetaObject(IMetaObject metaObject)
    {
      objectSpace.remove(metaObject.getKey());
    }
    
    public MetaObject<T> createMetaObject<T>()
    {
      keyCount++;
      UUID id = new UUID(keyCount);
      MetaObject<T> metaObject = new MetaObject<T>(id);
      objectSpace[id] = metaObject;
      return metaObject;
    }

    public IMetaObject createMetaObject(Type t)
    {
      keyCount++;
      UUID id = new UUID(keyCount);
      object[] args = {id};
      IMetaObject metaObject = (IMetaObject) Activator.CreateInstance(t, args);
      objectSpace[id] = metaObject;
      return metaObject;
    }

    public int getKeyCount()
    {
      return keyCount;
    }

    public void setKeyCount(int keyCount)
    {
      this.keyCount = keyCount;
    }

    public static Value encode(object obj)
    {
      Value val;

      if (obj == null)
      {
        //val = null;
        val = NullValue.Null;
      }
      else if (obj is int iVal)
      {
        val = new IntValue(iVal);
      }
      else if (obj is bool bVal)
      {
        val = new BoolValue(bVal);
      }
      else if(obj is string sVal)
      {
        val = new StringValue(sVal);
      }
      else if (obj.GetType().IsEnum)
      {
        val = new EnumValue(obj);
      }
      else if(obj is IPatchable patchable)
      {
        //Problem: meta objects are not values, but they are operands of effects
        //Question: how to encode them?
        IMetaObject metaObject = patchable.getMetaObject();
        if (metaObject == null)
        {
          throw new PatchException("Patcher Error: missing meta-object?");
        }
        val = new Path(new Lookup(metaObject.getKey()));
      }
      else
      {
        Type type = obj.GetType();
        throw new PatchException("Patcher Error: Cannot encode object of type " + type);
      }

      return val;
    }
    
    /*    
    // Retrieves an object of type T with identifier key from the object space.
    // @param key object identifier
    public T lookup<T>(ID key)
    {
      IMetaObject metaObject = lookupMetaObject(key);

      object obj = metaObject.getObject();

      if (obj is T == false)
      {
        throw new PatchException("Patcher Error: Failed lookup for object with key " + printer.print(key) +
                                 "expected type " + typeof(T) + " found type " + obj.GetType());
      }

      return (T) obj;
    }*/
    
    /*
   public IMetaObject createMetaObject(String className)
   {
     keyCount++;
     UUID id = new UUID(keyCount);

     Type type = Type.GetType(className);
     if (type == null)
     {
       throw new PatchException("Patcher Error: Cannot create instance of missing class " + className);
     }

     Type mType = typeof(MetaObject<>);
     Type[] typeArgs = {type};
     Type tType = mType.MakeGenericType(typeArgs);

     IMetaObject metaObject = (IMetaObject) Activator.CreateInstance(tType);
     metaObject.setKey(id);

     return metaObject;
   }*/   
    
    /*
    //Caution: please avoid reverse lookups.
    public IMetaObject reverseLookup(object obj)
    {
      IMetaObject rMetaObject = null;
      foreach(IMetaObject metaObject in objectSpace.Values)
      {
        object otherObject = metaObject.getObject();
        if (otherObject.Equals(obj))
        {
          rMetaObject = metaObject;
          break;
        }
      }
      return rMetaObject;
    }*/
  }
}