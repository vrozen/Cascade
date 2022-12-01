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
using Delta.Runtime;
using Delta.TinyLiveSML.Model;
using System;
using Delta.TinyLiveSML.Operation;
using System.Linq;
using Delta.Model.QualifiedName;
using Delta.Model.BaseOperation;

namespace Delta.TinyLiveSML.Runtime {
  public class Marshaller : Model.IVisitor {
    private Patcher patcher;
  
    public Marshaller(Patcher patcher){
      this.patcher = patcher;
    }
  
    public bool marshal(Model.INode node){
      node.accept(this);
      return true;
    }
  
    public void visit(Mach node){
      IMetaObject /*MetaObject<Mach>*/ metaObject = patcher.createMetaObject<Mach>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  
    public void visit(MachInst node){
      IMetaObject /*MetaObject<MachInst>*/ metaObject = patcher.createMetaObject<MachInst>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
      Model.INode childNodeInvalid = node.getInvalid();
      childNodeInvalid.accept(this);
    }
  
    public void visit(State node){
      IMetaObject /*MetaObject<State>*/ metaObject = patcher.createMetaObject<State>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  
    public void visit(StateInst node){
      IMetaObject /*MetaObject<StateInst>*/ metaObject = patcher.createMetaObject<StateInst>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  
    public void visit(Trans node){
      IMetaObject /*MetaObject<Trans>*/ metaObject = patcher.createMetaObject<Trans>();
      node.setMetaObject(metaObject);
      metaObject.bind(node);
    }
  }
}