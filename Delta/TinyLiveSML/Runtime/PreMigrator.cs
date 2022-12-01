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
  public class PreMigrator : IPreMigrator, Operation.IVisitor {
    private readonly Patcher patcher;
  
    public PreMigrator(Patcher patcher){
      this.patcher = patcher;
    }
  
    public void preMigrate(Event evt){
      if(evt is Operation.IOperation op){
        op.accept(this);
      };
    }
  
    public void visit(MachCreate op){
    }
  
    public void visit(MachDelete op){
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      Mach m = (Mach) mMeta.getObject();
      StringValue nameValue = op.getName();
      string name = (string) nameValue.getValue();
      foreach(State s in m.states) {
        op.addPreEffect(new StateDelete((IMetaObject /*MetaObject<State>*/) s.getMetaObject(), (StringValue) Patcher.encode(s.name), (IMetaObject /*MetaObject<Mach>*/) m.getMetaObject()));
      };
      foreach(MachInst mi in m.instances) {
        op.addPreEffect(new MachInstDelete((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<Mach>*/) m.getMetaObject()));
      };
    }
  
    public void visit(MachAddState op){
    }
  
    public void visit(MachRemoveState op){
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      Mach m = (Mach) mMeta.getObject();
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      State s = (State) sMeta.getObject();
      foreach(MachInst mi in m.instances) {
        StateInst si = mi.sis[s];
        op.addPreEffect(new MachInstRemoveStateInst((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) si.getMetaObject(), (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
        op.addPreEffect(new StateInstDelete((IMetaObject /*MetaObject<StateInst>*/) si.getMetaObject(), (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
        op.addPreEffect(new MachInstInitialize((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
      };
    }
  
    public void visit(MachAddMachInst op){
    }
  
    public void visit(MachRemoveMachInst op){
    }
  
    public void visit(MachInstCreate op){
    }
  
    public void visit(MachInstDelete op){
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      MachInst mi = (MachInst) miMeta.getObject();
      IMetaObject /*MetaObject<Mach>*/ defMeta = op.getDef();
      Mach def = (Mach) defMeta.getObject();
      op.addPreEffect(new MachRemoveMachInst((IMetaObject /*MetaObject<Mach>*/) mi.def.getMetaObject(), (IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
      foreach(StateInst si in mi.sis.Values) {
        op.addPreEffect(new MachInstRemoveStateInst((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) si.getMetaObject(), (IMetaObject /*MetaObject<State>*/) si.def.getMetaObject()));
        op.addPreEffect(new StateInstDelete((IMetaObject /*MetaObject<StateInst>*/) si.getMetaObject(), (IMetaObject /*MetaObject<State>*/) si.def.getMetaObject()));
      };
    }
  
    public void visit(MachInstAddStateInst op){
    }
  
    public void visit(MachInstRemoveStateInst op){
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      MachInst mi = (MachInst) miMeta.getObject();
      IMetaObject /*MetaObject<StateInst>*/ siMeta = op.getSi();
      StateInst si = (StateInst) siMeta.getObject();
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      State s = (State) sMeta.getObject();
      if(si == mi.cur){
        op.addPreEffect(new MachInstSetCurState((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) null));
      };
    }
  
    public void visit(MachInstInitialize op){
    }
  
    public void visit(MachInstSetCurState op){
    }
  
    public void visit(MachInstTrigger op){
    }
  
    public void visit(MachInstMissingCurrentState op){
    }
  
    public void visit(MachInstQuiescence op){
    }
  
    public void visit(StateCreate op){
    }
  
    public void visit(StateDelete op){
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      State s = (State) sMeta.getObject();
      StringValue nameValue = op.getName();
      string name = (string) nameValue.getValue();
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      Mach m = (Mach) mMeta.getObject();
      foreach(Trans t in s.o) {
        op.addPreEffect(new TransDelete((IMetaObject /*MetaObject<Trans>*/) t.getMetaObject(), (IMetaObject /*MetaObject<State>*/) t.src.getMetaObject(), (StringValue) Patcher.encode(t.evt), (IMetaObject /*MetaObject<State>*/) t.tgt.getMetaObject()));
      };
      foreach(Trans t in s.i) {
        op.addPreEffect(new TransDelete((IMetaObject /*MetaObject<Trans>*/) t.getMetaObject(), (IMetaObject /*MetaObject<State>*/) t.src.getMetaObject(), (StringValue) Patcher.encode(t.evt), (IMetaObject /*MetaObject<State>*/) t.tgt.getMetaObject()));
      };
      op.addPreEffect(new MachRemoveState((IMetaObject /*MetaObject<Mach>*/) m.getMetaObject(), (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
    }
  
    public void visit(StateAddIn op){
    }
  
    public void visit(StateRemoveIn op){
    }
  
    public void visit(StateAddOut op){
    }
  
    public void visit(StateRemoveOut op){
    }
  
    public void visit(StateInstCreate op){
    }
  
    public void visit(StateInstDelete op){
    }
  
    public void visit(StateInstSetCount op){
    }
  
    public void visit(TransCreate op){
    }
  
    public void visit(TransDelete op){
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      Trans t = (Trans) tMeta.getObject();
      IMetaObject /*MetaObject<State>*/ srcMeta = op.getSrc();
      State src = (State) srcMeta.getObject();
      StringValue evtValue = op.getEvt();
      string evt = (string) evtValue.getValue();
      IMetaObject /*MetaObject<State>*/ tgtMeta = op.getTgt();
      State tgt = (State) tgtMeta.getObject();
      op.addPreEffect(new StateRemoveOut((IMetaObject /*MetaObject<State>*/) src.getMetaObject(), (IMetaObject /*MetaObject<Trans>*/) t.getMetaObject()));
      op.addPreEffect(new StateRemoveIn((IMetaObject /*MetaObject<State>*/) tgt.getMetaObject(), (IMetaObject /*MetaObject<Trans>*/) t.getMetaObject()));
    }
  }
}