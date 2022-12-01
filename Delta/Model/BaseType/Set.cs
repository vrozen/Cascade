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
using System.Collections.Generic;

namespace Delta.Model.BaseType
{
  /**
   * The ISet interface is used to modify sets in an untyped manner.
   */
  public interface ISet : IEnumerable, IPatchable
  { 
    void remove(object obj); 
    void add(object obj);
    bool contains(object obj);
  }
  
  /**
   * The Set class is a managed Set supported by the Patcher.
   */
  public class Set<T> : HashSet<T>, ISet
  {
    private IMetaObject metaObject;
    
    public void remove(object obj)
    {
      Remove((T) obj);
    }

    public void add(object obj)
    {
      Add((T) obj);
    }

    public bool contains(object obj)
    {
      return Contains((T)obj);
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