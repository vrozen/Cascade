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
// Generated code
using Delta.TinyLiveSML.Model;
using Delta.Model.BaseType;

namespace Delta.TinyLiveSML.Model {
  public class Trans : IPatchable, INode {
    [Reference] private IMetaObject metaObject;
    [Reference] public State src;
    public string evt;
    [Reference] public State tgt;
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public State getSrc(){
      return src;
    }
  
    public string getEvt(){
      return evt;
    }
  
    public State getTgt(){
      return tgt;
    }
  
    public override string ToString(){
      return "\"" + evt + "\" -> " + tgt;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}