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
namespace Delta.Model.BaseType
{
  /**
   * The IMap interface is used to modify maps in an untyped manner.
   */
  public interface IMap: IPatchable
  {
    void put(object key, object value);
    object get(object key);

    void remove(object key);
      
    bool containsKey(object key);
    
    Delta.Model.BaseType.List<object> getKeys();
  }
  
  /**
   * The Map class is a managed Map supported by the Patcher.
   */
  public class Map<K,V> : System.Collections.Generic.Dictionary<K,V>, IMap
  {
    private IMetaObject metaObject;
    
    public void put(object key, object value)
    {
      K k = (K) key;
      V v;
      if (value == null)
      {
        v = default(V);
      }
      else
      {
        v = (V) value;
      }
      
      this[k] = v;
    }

    public object get(object key)
    {
      return this[(K)key];
    }

    public void remove(object key)
    {
      Remove((K)key);
    }

    public bool containsKey(object key)
    {
      K k = (K) key;
      if (k == null)
      {
        throw new PatchException("");
      }
      return ContainsKey(k);
    }

    public Delta.Model.BaseType.List<object> getKeys()
    {
      Delta.Model.BaseType.List<object> l = new List<object>();
      foreach (K key in Keys)
      {
        l.Add(key);
      }
      return l;
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