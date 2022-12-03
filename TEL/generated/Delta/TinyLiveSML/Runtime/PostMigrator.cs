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
  public class PostMigrator : IPostMigrator, Operation.IVisitor {
    private readonly Patcher patcher;
  
    public PostMigrator(Patcher patcher){
      this.patcher = patcher;
    }
  
    public void postMigrate(Event evt){
      if(evt is Operation.IOperation op){
        op.accept(this);
      };
    }
  
    public void visit(MachCreate op){
    }
  
    public void visit(MachDelete op){
    }
  
    public void visit(MachAddState op){
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      Mach m = (Mach) mMeta.getObject();
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      State s = (State) sMeta.getObject();
      foreach(MachInst mi in m.instances) {
        MetaObject<StateInst> siMeta = patcher.createMetaObject<StateInst>();
        op.addPostEffect(new StateInstCreate((IMetaObject /*MetaObject<StateInst>*/) siMeta, (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
        op.addPostEffect(new MachInstAddStateInst((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) siMeta, (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
        op.addPostEffect(new MachInstInitialize((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
      };
    }
  
    public void visit(MachRemoveState op){
    }
  
    public void visit(MachAddMachInst op){
    }
  
    public void visit(MachRemoveMachInst op){
    }
  
    public void visit(MachInstCreate op){
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      MachInst mi = (MachInst) miMeta.getObject();
      IMetaObject /*MetaObject<Mach>*/ defMeta = op.getDef();
      Mach def = (Mach) defMeta.getObject();
      op.addPostEffect(new MachAddMachInst((IMetaObject /*MetaObject<Mach>*/) mi.def.getMetaObject(), (IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
      foreach(State s in mi.def.states) {
        MetaObject<StateInst> siMeta = patcher.createMetaObject<StateInst>();
        op.addPostEffect(new StateInstCreate((IMetaObject /*MetaObject<StateInst>*/) siMeta, (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
        op.addPostEffect(new MachInstAddStateInst((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) siMeta, (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
      };
      op.addPostEffect(new MachInstInitialize((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
    }
  
    public void visit(MachInstDelete op){
    }
  
    public void visit(MachInstAddStateInst op){
    }
  
    public void visit(MachInstRemoveStateInst op){
    }
  
    public void visit(MachInstInitialize op){
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      MachInst mi = (MachInst) miMeta.getObject();
      if(mi.sis.Count > 0 && mi.cur == null){
        StateInst nextState = mi.sis.Values.First();
        op.addPostEffect(new MachInstSetCurState((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) nextState.getMetaObject()));
      };
    }
  
    public void visit(MachInstSetCurState op){
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      MachInst mi = (MachInst) miMeta.getObject();
      IMetaObject /*MetaObject<StateInst>*/ curMeta = op.getCur();
      StateInst cur = (StateInst) curMeta.getObject();
      if(mi.cur != null){
        op.addPostEffect(new StateInstSetCount((IMetaObject /*MetaObject<StateInst>*/) mi.cur.getMetaObject(), (IntValue) Patcher.encode(mi.cur.count + 1)));
      };
    }
  
    public void visit(MachInstTrigger op){
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      MachInst mi = (MachInst) miMeta.getObject();
      StringValue evtValue = op.getEvt();
      string evt = (string) evtValue.getValue();
      if(mi.cur == null){
        op.addPostEffect(new MachInstMissingCurrentState((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
        return;
      };
      foreach(Trans t in mi.cur.def.o) {
        if(t.evt == evt){
          StateInst nextState = mi.sis[t.tgt];
          op.addPostEffect(new MachInstSetCurState((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject(), (IMetaObject /*MetaObject<StateInst>*/) nextState.getMetaObject()));
          return;
        };
      };
      op.addPostEffect(new MachInstQuiescence((IMetaObject /*MetaObject<MachInst>*/) mi.getMetaObject()));
    }
  
    public void visit(MachInstMissingCurrentState op){
    }
  
    public void visit(MachInstQuiescence op){
    }
  
    public void visit(StateCreate op){
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      State s = (State) sMeta.getObject();
      StringValue nameValue = op.getName();
      string name = (string) nameValue.getValue();
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      Mach m = (Mach) mMeta.getObject();
      op.addPostEffect(new MachAddState((IMetaObject /*MetaObject<Mach>*/) m.getMetaObject(), (IMetaObject /*MetaObject<State>*/) s.getMetaObject()));
    }
  
    public void visit(StateDelete op){
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
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      Trans t = (Trans) tMeta.getObject();
      IMetaObject /*MetaObject<State>*/ srcMeta = op.getSrc();
      State src = (State) srcMeta.getObject();
      StringValue evtValue = op.getEvt();
      string evt = (string) evtValue.getValue();
      IMetaObject /*MetaObject<State>*/ tgtMeta = op.getTgt();
      State tgt = (State) tgtMeta.getObject();
      op.addPostEffect(new StateAddOut((IMetaObject /*MetaObject<State>*/) src.getMetaObject(), (IMetaObject /*MetaObject<Trans>*/) t.getMetaObject()));
      op.addPostEffect(new StateAddIn((IMetaObject /*MetaObject<State>*/) tgt.getMetaObject(), (IMetaObject /*MetaObject<Trans>*/) t.getMetaObject()));
    }
  
    public void visit(TransDelete op){
    }
  }
}