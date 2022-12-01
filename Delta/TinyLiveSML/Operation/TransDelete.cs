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
using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.TinyLiveSML.Model;
using Delta.TinyLiveSML.Operation;

namespace Delta.TinyLiveSML.Operation {
  public class TransDelete : Effect, IOperation {
    [Past] private IMetaObject /*MetaObject<Trans>*/ t;
    private IMetaObject /*MetaObject<State>*/ src;
    private IMetaObject /*MetaObject<State>*/ oldSrc;
    private StringValue evt;
    private StringValue oldEvt;
    private IMetaObject /*MetaObject<State>*/ tgt;
    private IMetaObject /*MetaObject<State>*/ oldTgt;
  
    public TransDelete([Past] IMetaObject /*MetaObject<Trans>*/ t, IMetaObject /*MetaObject<State>*/ src, StringValue evt, IMetaObject /*MetaObject<State>*/ tgt){
      languageType = typeof(Delta.TinyLiveSML.Runtime.Language);
      this.t = t;
      this.src = src;
      this.evt = evt;
      this.tgt = tgt;
    }
  
    public override IMetaObject getSubject(){
      return t;
    }
  
    public IMetaObject /*MetaObject<Trans>*/ getT(){
      return t;
    }
  
    public IMetaObject /*MetaObject<State>*/ getSrc(){
      return src;
    }
  
    public IMetaObject /*MetaObject<State>*/ getOldSrc(){
      return oldSrc;
    }
  
    public StringValue getEvt(){
      return evt;
    }
  
    public StringValue getOldEvt(){
      return oldEvt;
    }
  
    public IMetaObject /*MetaObject<State>*/ getTgt(){
      return tgt;
    }
  
    public IMetaObject /*MetaObject<State>*/ getOldTgt(){
      return oldTgt;
    }
  
    public void setT(IMetaObject /*MetaObject<Trans>*/ t){
      this.t = t;
    }
  
    public void setSrc(IMetaObject /*MetaObject<State>*/ src){
      this.src = src;
    }
  
    public void setOldSrc(IMetaObject /*MetaObject<State>*/ oldSrc){
      this.oldSrc = oldSrc;
    }
  
    public void setEvt(StringValue evt){
      this.evt = evt;
    }
  
    public void setOldEvt(StringValue oldEvt){
      this.oldEvt = oldEvt;
    }
  
    public void setTgt(IMetaObject /*MetaObject<State>*/ tgt){
      this.tgt = tgt;
    }
  
    public void setOldTgt(IMetaObject /*MetaObject<State>*/ oldTgt){
      this.oldTgt = oldTgt;
    }
  
    public void accept(IVisitor visitor){
      visitor.visit(this);
    }
  }
}