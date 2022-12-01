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
using System.Collections;

namespace Delta.Model.BaseType
{
  /**
   * The IList interface is used to modify list in an untyped manner.
   */
  public interface IList: IEnumerable, IPatchable
  {
    int getCount();
    void insert(int pos, object obj);

    void add(object obj);

    void remove(object obj);

    object elementAt(int pos);

    void storeAt(int pos, object obj);
    
    void removeAt(int pos);
  }
  
  /**
   * The List class is a managed List supported by the Patcher.
   */
  public class List<T> : ArrayList, IList
  {
    private IMetaObject metaObject;
    
    public int getCount()
    {
      return Count;
    }

    public void insert(int pos, object obj)
    {
      Insert(pos, obj);
    }

    public void add(object obj)
    {
      Add(obj);
    }

    public void remove(object obj)
    {
      Remove(obj);
    }

    public void storeAt(int pos, object obj)
    {
      this[pos] = obj;
    }

    public object elementAt(int pos)
    {
      return this[pos];
    }

    public T ElementAt(int pos){
      return (T)this[pos];
    }

    public void removeAt(int pos)
    {
      RemoveAt(pos);
    }
    
    public IMetaObject getMetaObject()
    {
      return metaObject;
    }

    public void setMetaObject(IMetaObject metaObject)
    {
      this.metaObject = metaObject;
    }
  }
}