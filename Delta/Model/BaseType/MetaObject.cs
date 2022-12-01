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

namespace Delta.Model.BaseType
{
 public enum Binding
  {
    ERROR,
    ISSUED,
    RETIRED,
    BOUND
  }
 
 /*
 * Untyped meta-object interface.
 */
  public interface IMetaObject
  {
    void bind(object obj);
    void unbind();
    //void clear();
    ID getKey();
    void setKey(ID key);
    object getObject();
    T getObject<T>();
    Type getObjectType();
    bool isBound();
    Binding getStatus();
  }
 
 /*
  * Typed meta-object implementation.
  */
  public class MetaObject<T> : IMetaObject
  {
    private ID key;         //the key is not stable, may be updated
    private T obj;          //may be bound to only one object (but it may die and revive)
    private Binding status; //binding status

    public MetaObject(ID key)
    {
      this.key = key;
      this.status = Binding.ISSUED;
    }

    public void bind(object obj)
    {
      if (obj is T tObj)
      {
        bind(tObj);
      }
      else
      {
        throw new PatchException("Attempt to bind object of type "+obj.GetType()+", expected "+typeof(T));
      }
    }

    public void bind(T obj)
    {
      if (isBound())
      {
        throw new PatchException("Attempt to bind object twice");
      }

      if (obj is IPatchable patchable)
      {
        this.obj = obj;
        this.status = Binding.BOUND;
        patchable.setMetaObject(this);
      }
      else
      {
        throw new PatchException("Cannot bind object that is not patchable");
      }
    }

    public void unbind()
    {
      if (!isBound())
      {
        throw new PatchException("Attempt to unbind unbound object");
      }
      
      if (obj is IPatchable patchable)
      {
        //patchable.setMetaObject(null); //confusing, but you cannot do this if you refer back
        status = Binding.RETIRED;
        this.obj = default(T);
      }
      else
      {
        throw new PatchException("Cannot unbind object that is not patchable");
      }
    }

    /*public void clear()
    {
      if (status == Binding.RETIRED)
      {
        this.obj = default(T);
      }
    }*/

    public ID getKey()
    {
      return key;
    }

    public void setKey(ID key)
    {
      this.key = key;
    }
    
    public TR getObject<TR>()
    {
      if (!(obj is TR))
      {
        throw new PatchException("Attempt to cast object of type " + typeof(T) +
                                 " to incompatible type " + typeof(TR));
      }
      
      return (TR) (object) getObject();
    }

    public Type getObjectType()
    {
      return typeof(T);
    }

    public T getObject()
    {
      if (isBound() == false)
      {
        throw new PatchException("Attempt to retrieve unbound object, found status: "+getStatus());
      }
      
      return obj;
    }
    
    object IMetaObject.getObject()
    {
      return getObject();
    }

    public bool isBound()
    {
      return (status == Binding.BOUND);
    }

    public Binding getStatus()
    {
      return status;
    }
  }
}