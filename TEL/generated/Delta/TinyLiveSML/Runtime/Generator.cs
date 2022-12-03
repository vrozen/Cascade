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
  public class Generator : IGenerator, Operation.IVisitor {
    private readonly Patcher patcher;
    private static readonly Type MachObjectType = typeof(Mach);
    private static readonly StringValue MachType = new StringValue(MachObjectType.ToString());
    private static readonly Type MachMetaObjectObjectType = typeof(IMetaObject);
    private static readonly StringValue MachMetaObjectType = new StringValue(MachMetaObjectObjectType.ToString());
    private static readonly Field MachMetaObjectField = new Field("metaObject");
    private static readonly Type MachNameObjectType = typeof(string);
    private static readonly StringValue MachNameType = new StringValue(MachNameObjectType.ToString());
    private static readonly Field MachNameField = new Field("name");
    private static readonly Type MachStatesObjectType = typeof(Set<State>);
    private static readonly StringValue MachStatesType = new StringValue(MachStatesObjectType.ToString());
    private static readonly Field MachStatesField = new Field("states");
    private static readonly Type MachInstancesObjectType = typeof(Set<MachInst>);
    private static readonly StringValue MachInstancesType = new StringValue(MachInstancesObjectType.ToString());
    private static readonly Field MachInstancesField = new Field("instances");
    private static readonly Type MachInstObjectType = typeof(MachInst);
    private static readonly StringValue MachInstType = new StringValue(MachInstObjectType.ToString());
    private static readonly Type MachInstMetaObjectObjectType = typeof(IMetaObject);
    private static readonly StringValue MachInstMetaObjectType = new StringValue(MachInstMetaObjectObjectType.ToString());
    private static readonly Field MachInstMetaObjectField = new Field("metaObject");
    private static readonly Type MachInstDefObjectType = typeof(Mach);
    private static readonly StringValue MachInstDefType = new StringValue(MachInstDefObjectType.ToString());
    private static readonly Field MachInstDefField = new Field("def");
    private static readonly Type MachInstCurObjectType = typeof(StateInst);
    private static readonly StringValue MachInstCurType = new StringValue(MachInstCurObjectType.ToString());
    private static readonly Field MachInstCurField = new Field("cur");
    private static readonly Type MachInstSisObjectType = typeof(Map<State,StateInst>);
    private static readonly StringValue MachInstSisType = new StringValue(MachInstSisObjectType.ToString());
    private static readonly Field MachInstSisField = new Field("sis");
    private static readonly Type MachInstInvalidObjectType = typeof(StateInst);
    private static readonly StringValue MachInstInvalidType = new StringValue(MachInstInvalidObjectType.ToString());
    private static readonly Field MachInstInvalidField = new Field("invalid");
    private static readonly Type StateObjectType = typeof(State);
    private static readonly StringValue StateType = new StringValue(StateObjectType.ToString());
    private static readonly Type StateMetaObjectObjectType = typeof(IMetaObject);
    private static readonly StringValue StateMetaObjectType = new StringValue(StateMetaObjectObjectType.ToString());
    private static readonly Field StateMetaObjectField = new Field("metaObject");
    private static readonly Type StateNameObjectType = typeof(string);
    private static readonly StringValue StateNameType = new StringValue(StateNameObjectType.ToString());
    private static readonly Field StateNameField = new Field("name");
    private static readonly Type StateIObjectType = typeof(Set<Trans>);
    private static readonly StringValue StateIType = new StringValue(StateIObjectType.ToString());
    private static readonly Field StateIField = new Field("i");
    private static readonly Type StateOObjectType = typeof(Set<Trans>);
    private static readonly StringValue StateOType = new StringValue(StateOObjectType.ToString());
    private static readonly Field StateOField = new Field("o");
    private static readonly Type StateInstObjectType = typeof(StateInst);
    private static readonly StringValue StateInstType = new StringValue(StateInstObjectType.ToString());
    private static readonly Type StateInstMetaObjectObjectType = typeof(IMetaObject);
    private static readonly StringValue StateInstMetaObjectType = new StringValue(StateInstMetaObjectObjectType.ToString());
    private static readonly Field StateInstMetaObjectField = new Field("metaObject");
    private static readonly Type StateInstDefObjectType = typeof(State);
    private static readonly StringValue StateInstDefType = new StringValue(StateInstDefObjectType.ToString());
    private static readonly Field StateInstDefField = new Field("def");
    private static readonly Type StateInstCountObjectType = typeof(int);
    private static readonly StringValue StateInstCountType = new StringValue(StateInstCountObjectType.ToString());
    private static readonly Field StateInstCountField = new Field("count");
    private static readonly Type TransObjectType = typeof(Trans);
    private static readonly StringValue TransType = new StringValue(TransObjectType.ToString());
    private static readonly Type TransMetaObjectObjectType = typeof(IMetaObject);
    private static readonly StringValue TransMetaObjectType = new StringValue(TransMetaObjectObjectType.ToString());
    private static readonly Field TransMetaObjectField = new Field("metaObject");
    private static readonly Type TransSrcObjectType = typeof(State);
    private static readonly StringValue TransSrcType = new StringValue(TransSrcObjectType.ToString());
    private static readonly Field TransSrcField = new Field("src");
    private static readonly Type TransEvtObjectType = typeof(string);
    private static readonly StringValue TransEvtType = new StringValue(TransEvtObjectType.ToString());
    private static readonly Field TransEvtField = new Field("evt");
    private static readonly Type TransTgtObjectType = typeof(State);
    private static readonly StringValue TransTgtType = new StringValue(TransTgtObjectType.ToString());
    private static readonly Field TransTgtField = new Field("tgt");
  
    public Generator(Patcher patcher){
      this.patcher = patcher;
    }
  
    public void generate(Event evt){
      if(evt is Operation.IOperation op){
        op.accept(this);
      };
    }
  
    public void visit(MachCreate op){
      //object Mach m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      //owned attribute Set<State> states;
      IMetaObject /*MetaObject<Set<State>>*/ mStatesMeta = patcher.createMetaObject<Set<State>>();
      ID mStatesId = mStatesMeta.getKey();
      Value mStatesValue = new Path(mStatesId);
      //owned attribute Set<MachInst> instances;
      IMetaObject /*MetaObject<Set<MachInst>>*/ mInstancesMeta = patcher.createMetaObject<Set<MachInst>>();
      ID mInstancesId = mInstancesMeta.getKey();
      Value mInstancesValue = new Path(mInstancesId);
      //value parameter StringValue name;
      StringValue nameValue = op.getName();
      //operations;
      op.addOperation(new ObjectCreate(mId, MachType));
      op.addOperation(new ObjectSet((Path) mValue, MachNameField, nameValue));
      op.addOperation(new ObjectCreate(mStatesId, MachStatesType));
      op.addOperation(new ObjectSet((Path) mValue, MachStatesField, mStatesValue));
      op.addOperation(new ObjectCreate(mInstancesId, MachInstancesType));
      op.addOperation(new ObjectSet((Path) mValue, MachInstancesField, mInstancesValue));
    }
  
    public void visit(MachDelete op){
      //object Mach m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      Mach m = (Mach) mMeta.getObject();
      //meta attribute Set<State> states;
      Set<State> mStates = m.getStates();
      IMetaObject mStatesMeta = mStates.getMetaObject();
      ID mStatesId = mStatesMeta.getKey();
      Value mStatesValue = new Path(mStatesId);
      //meta attribute Set<MachInst> instances;
      Set<MachInst> mInstances = m.getInstances();
      IMetaObject mInstancesMeta = mInstances.getMetaObject();
      ID mInstancesId = mInstancesMeta.getKey();
      Value mInstancesValue = new Path(mInstancesId);
      //store old value parameter type: StringValue, param: m.name, attr: m.name;
      op.setOldName((StringValue) Patcher.encode(m.name));
      //operations;
      op.addOperation(new ObjectSet((Path) mValue, MachNameField, NullValue.Null));
      op.addOperation(new ObjectDelete(mStatesId, MachStatesType));
      op.addOperation(new ObjectDelete(mInstancesId, MachInstancesType));
      op.addOperation(new ObjectDelete(mId, MachType));
    }
  
    public void visit(MachAddState op){
      //object Mach m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      Mach m = (Mach) mMeta.getObject();
      //meta attribute Set<State> m.states;
      Set<State> mStates = m.getStates();
      IMetaObject mStatesMeta = mStates.getMetaObject();
      ID mStatesId = mStatesMeta.getKey();
      Value mStatesValue = new Path(mStatesId);
      //meta parameter IMetaObject /*MetaObject<State>*/ s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //operations;
      op.addOperation(new SetAdd((Path) mStatesValue, sValue));
    }
  
    public void visit(MachRemoveState op){
      //object Mach m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      Mach m = (Mach) mMeta.getObject();
      //meta attribute Set<State> m.states;
      Set<State> mStates = m.getStates();
      IMetaObject mStatesMeta = mStates.getMetaObject();
      ID mStatesId = mStatesMeta.getKey();
      Value mStatesValue = new Path(mStatesId);
      //meta parameter IMetaObject /*MetaObject<State>*/ s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //operations;
      op.addOperation(new SetRemove((Path) mStatesValue, sValue));
    }
  
    public void visit(MachAddMachInst op){
      //object Mach m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      Mach m = (Mach) mMeta.getObject();
      //meta attribute Set<MachInst> m.instances;
      Set<MachInst> mInstances = m.getInstances();
      IMetaObject mInstancesMeta = mInstances.getMetaObject();
      ID mInstancesId = mInstancesMeta.getKey();
      Value mInstancesValue = new Path(mInstancesId);
      //meta parameter IMetaObject /*MetaObject<MachInst>*/ mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      MachInst mi = (MachInst) miMeta.getObject();
      //operations;
      op.addOperation(new SetAdd((Path) mInstancesValue, miValue));
    }
  
    public void visit(MachRemoveMachInst op){
      //object Mach m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      Mach m = (Mach) mMeta.getObject();
      //meta attribute Set<MachInst> m.instances;
      Set<MachInst> mInstances = m.getInstances();
      IMetaObject mInstancesMeta = mInstances.getMetaObject();
      ID mInstancesId = mInstancesMeta.getKey();
      Value mInstancesValue = new Path(mInstancesId);
      //meta parameter IMetaObject /*MetaObject<MachInst>*/ mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      MachInst mi = (MachInst) miMeta.getObject();
      //operations;
      op.addOperation(new SetRemove((Path) mInstancesValue, miValue));
    }
  
    public void visit(MachInstCreate op){
      //object MachInst mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      //owned attribute Map<State,StateInst> sis;
      IMetaObject /*MetaObject<Map<State,StateInst>>*/ miSisMeta = patcher.createMetaObject<Map<State,StateInst>>();
      ID miSisId = miSisMeta.getKey();
      Value miSisValue = new Path(miSisId);
      //meta parameter IMetaObject /*MetaObject<Mach>*/ def;
      IMetaObject /*MetaObject<Mach>*/ defMeta = op.getDef();
      ID defId = defMeta.getKey();
      Value defValue = new Path(defId);
      Mach def = (Mach) defMeta.getObject();
      //operations;
      op.addOperation(new ObjectCreate(miId, MachInstType));
      op.addOperation(new ObjectSet((Path) miValue, MachInstDefField, defValue));
      op.addOperation(new ObjectSet((Path) miValue, MachInstCurField, NullValue.Null));
      op.addOperation(new ObjectCreate(miSisId, MachInstSisType));
      op.addOperation(new ObjectSet((Path) miValue, MachInstSisField, miSisValue));
    }
  
    public void visit(MachInstDelete op){
      //object MachInst mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      MachInst mi = (MachInst) miMeta.getObject();
      //meta attribute Map<State,StateInst> sis;
      Map<State,StateInst> miSis = mi.getSis();
      IMetaObject miSisMeta = miSis.getMetaObject();
      ID miSisId = miSisMeta.getKey();
      Value miSisValue = new Path(miSisId);
      //store old meta parameter type: IMetaObject /*MetaObject<Mach>*/, param: def, attr: mi.def;
      if(mi.def != null){
        op.setOldDef((IMetaObject /*MetaObject<Mach>*/) mi.def.getMetaObject());
      } else {
        op.setOldDef((IMetaObject /*MetaObject<Mach>*/) null);
      };
      //operations;
      op.addOperation(new ObjectSet((Path) miValue, MachInstDefField, NullValue.Null));
      op.addOperation(new ObjectSet((Path) miValue, MachInstCurField, NullValue.Null));
      op.addOperation(new ObjectDelete(miSisId, MachInstSisType));
      op.addOperation(new ObjectDelete(miId, MachInstType));
    }
  
    public void visit(MachInstAddStateInst op){
      //object MachInst mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      MachInst mi = (MachInst) miMeta.getObject();
      //meta attribute Map<State,StateInst> mi.sis;
      Map<State,StateInst> miSis = mi.getSis();
      IMetaObject miSisMeta = miSis.getMetaObject();
      ID miSisId = miSisMeta.getKey();
      Value miSisValue = new Path(miSisId);
      //meta parameter IMetaObject /*MetaObject<StateInst>*/ si;
      IMetaObject /*MetaObject<StateInst>*/ siMeta = op.getSi();
      ID siId = siMeta.getKey();
      Value siValue = new Path(siId);
      StateInst si = (StateInst) siMeta.getObject();
      //meta parameter IMetaObject /*MetaObject<State>*/ s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //operations;
      op.addOperation(new MapAdd((Path) miSisValue, sValue));
      op.addOperation(new MapSet((Path) miSisValue, sValue, siValue));
    }
  
    public void visit(MachInstRemoveStateInst op){
      //object MachInst mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      MachInst mi = (MachInst) miMeta.getObject();
      //meta attribute Map<State,StateInst> mi.sis;
      Map<State,StateInst> miSis = mi.getSis();
      IMetaObject miSisMeta = miSis.getMetaObject();
      ID miSisId = miSisMeta.getKey();
      Value miSisValue = new Path(miSisId);
      //meta parameter IMetaObject /*MetaObject<State>*/ s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //operations;
      op.addOperation(new MapSet((Path) miSisValue, sValue, NullValue.Null));
      op.addOperation(new MapRemove((Path) miSisValue, sValue));
    }
  
    public void visit(MachInstInitialize op){
    }
  
    public void visit(MachInstSetCurState op){
      //object MachInst mi;
      IMetaObject /*MetaObject<MachInst>*/ miMeta = op.getMi();
      ID miId = miMeta.getKey();
      Value miValue = new Path(miId);
      MachInst mi = (MachInst) miMeta.getObject();
      //meta parameter IMetaObject /*MetaObject<StateInst>*/ cur;
      IMetaObject /*MetaObject<StateInst>*/ curMeta = op.getCur();
      ID curId = curMeta.getKey();
      Value curValue = new Path(curId);
      StateInst cur = (StateInst) curMeta.getObject();
      //store old meta parameter type: IMetaObject /*MetaObject<StateInst>*/, param: cur, attr: mi.cur;
      if(mi.cur != null){
        op.setOldCur((IMetaObject /*MetaObject<StateInst>*/) mi.cur.getMetaObject());
      } else {
        op.setOldCur((IMetaObject /*MetaObject<StateInst>*/) null);
      };
      //operations;
      op.addOperation(new ObjectSet((Path) miValue, MachInstCurField, curValue));
    }
  
    public void visit(MachInstTrigger op){
    }
  
    public void visit(MachInstMissingCurrentState op){
    }
  
    public void visit(MachInstQuiescence op){
    }
  
    public void visit(StateCreate op){
      //object State s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      //owned attribute Set<Trans> i;
      IMetaObject /*MetaObject<Set<Trans>>*/ sIMeta = patcher.createMetaObject<Set<Trans>>();
      ID sIId = sIMeta.getKey();
      Value sIValue = new Path(sIId);
      //owned attribute Set<Trans> o;
      IMetaObject /*MetaObject<Set<Trans>>*/ sOMeta = patcher.createMetaObject<Set<Trans>>();
      ID sOId = sOMeta.getKey();
      Value sOValue = new Path(sOId);
      //value parameter StringValue name;
      StringValue nameValue = op.getName();
      //meta parameter IMetaObject /*MetaObject<Mach>*/ m;
      IMetaObject /*MetaObject<Mach>*/ mMeta = op.getM();
      ID mId = mMeta.getKey();
      Value mValue = new Path(mId);
      Mach m = (Mach) mMeta.getObject();
      //operations;
      op.addOperation(new ObjectCreate(sId, StateType));
      op.addOperation(new ObjectCreate(sIId, StateIType));
      op.addOperation(new ObjectSet((Path) sValue, StateIField, sIValue));
      op.addOperation(new ObjectCreate(sOId, StateOType));
      op.addOperation(new ObjectSet((Path) sValue, StateOField, sOValue));
      op.addOperation(new ObjectSet((Path) sValue, StateNameField, nameValue));
    }
  
    public void visit(StateDelete op){
      //object State s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //meta attribute Set<Trans> i;
      Set<Trans> sI = s.getI();
      IMetaObject sIMeta = sI.getMetaObject();
      ID sIId = sIMeta.getKey();
      Value sIValue = new Path(sIId);
      //meta attribute Set<Trans> o;
      Set<Trans> sO = s.getO();
      IMetaObject sOMeta = sO.getMetaObject();
      ID sOId = sOMeta.getKey();
      Value sOValue = new Path(sOId);
      //store old value parameter type: StringValue, param: s.name, attr: s.name;
      op.setOldName((StringValue) Patcher.encode(s.name));
      //operations;
      op.addOperation(new ObjectSet((Path) sValue, StateNameField, NullValue.Null));
      op.addOperation(new ObjectDelete(sIId, StateIType));
      op.addOperation(new ObjectDelete(sOId, StateOType));
      op.addOperation(new ObjectDelete(sId, StateType));
    }
  
    public void visit(StateAddIn op){
      //object State s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //meta attribute Set<Trans> s.i;
      Set<Trans> sI = s.getI();
      IMetaObject sIMeta = sI.getMetaObject();
      ID sIId = sIMeta.getKey();
      Value sIValue = new Path(sIId);
      //meta parameter IMetaObject /*MetaObject<Trans>*/ t;
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      ID tId = tMeta.getKey();
      Value tValue = new Path(tId);
      Trans t = (Trans) tMeta.getObject();
      //operations;
      op.addOperation(new SetAdd((Path) sIValue, tValue));
    }
  
    public void visit(StateRemoveIn op){
      //object State s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //meta attribute Set<Trans> s.i;
      Set<Trans> sI = s.getI();
      IMetaObject sIMeta = sI.getMetaObject();
      ID sIId = sIMeta.getKey();
      Value sIValue = new Path(sIId);
      //meta parameter IMetaObject /*MetaObject<Trans>*/ t;
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      ID tId = tMeta.getKey();
      Value tValue = new Path(tId);
      Trans t = (Trans) tMeta.getObject();
      //operations;
      op.addOperation(new SetRemove((Path) sIValue, tValue));
    }
  
    public void visit(StateAddOut op){
      //object State s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //meta attribute Set<Trans> s.o;
      Set<Trans> sO = s.getO();
      IMetaObject sOMeta = sO.getMetaObject();
      ID sOId = sOMeta.getKey();
      Value sOValue = new Path(sOId);
      //meta parameter IMetaObject /*MetaObject<Trans>*/ t;
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      ID tId = tMeta.getKey();
      Value tValue = new Path(tId);
      Trans t = (Trans) tMeta.getObject();
      //operations;
      op.addOperation(new SetAdd((Path) sOValue, tValue));
    }
  
    public void visit(StateRemoveOut op){
      //object State s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //meta attribute Set<Trans> s.o;
      Set<Trans> sO = s.getO();
      IMetaObject sOMeta = sO.getMetaObject();
      ID sOId = sOMeta.getKey();
      Value sOValue = new Path(sOId);
      //meta parameter IMetaObject /*MetaObject<Trans>*/ t;
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      ID tId = tMeta.getKey();
      Value tValue = new Path(tId);
      Trans t = (Trans) tMeta.getObject();
      //operations;
      op.addOperation(new SetRemove((Path) sOValue, tValue));
    }
  
    public void visit(StateInstCreate op){
      //object StateInst si;
      IMetaObject /*MetaObject<StateInst>*/ siMeta = op.getSi();
      ID siId = siMeta.getKey();
      Value siValue = new Path(siId);
      //meta parameter IMetaObject /*MetaObject<State>*/ s;
      IMetaObject /*MetaObject<State>*/ sMeta = op.getS();
      ID sId = sMeta.getKey();
      Value sValue = new Path(sId);
      State s = (State) sMeta.getObject();
      //operations;
      op.addOperation(new ObjectCreate(siId, StateInstType));
      op.addOperation(new ObjectSet((Path) siValue, StateInstCountField, new IntValue(0)));
      op.addOperation(new ObjectSet((Path) siValue, StateInstDefField, sValue));
    }
  
    public void visit(StateInstDelete op){
      //object StateInst si;
      IMetaObject /*MetaObject<StateInst>*/ siMeta = op.getSi();
      ID siId = siMeta.getKey();
      Value siValue = new Path(siId);
      StateInst si = (StateInst) siMeta.getObject();
      //store old meta parameter type: IMetaObject /*MetaObject<State>*/, param: def, attr: si.def;
      if(si.def != null){
        op.setOldDef((IMetaObject /*MetaObject<State>*/) si.def.getMetaObject());
      } else {
        op.setOldDef((IMetaObject /*MetaObject<State>*/) null);
      };
      //operations;
      op.addOperation(new ObjectSet((Path) siValue, StateInstDefField, NullValue.Null));
      op.addOperation(new ObjectSet((Path) siValue, StateInstCountField, new IntValue(0)));
      op.addOperation(new ObjectDelete(siId, StateInstType));
    }
  
    public void visit(StateInstSetCount op){
      //object StateInst si;
      IMetaObject /*MetaObject<StateInst>*/ siMeta = op.getSi();
      ID siId = siMeta.getKey();
      Value siValue = new Path(siId);
      StateInst si = (StateInst) siMeta.getObject();
      //value parameter IntValue count;
      IntValue countValue = op.getCount();
      //store old value parameter type: IntValue, param: si.count, attr: si.count;
      op.setOldCount((IntValue) Patcher.encode(si.count));
      //operations;
      op.addOperation(new ObjectSet((Path) siValue, StateInstCountField, countValue));
    }
  
    public void visit(TransCreate op){
      //object Trans t;
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      ID tId = tMeta.getKey();
      Value tValue = new Path(tId);
      //meta parameter IMetaObject /*MetaObject<State>*/ src;
      IMetaObject /*MetaObject<State>*/ srcMeta = op.getSrc();
      ID srcId = srcMeta.getKey();
      Value srcValue = new Path(srcId);
      State src = (State) srcMeta.getObject();
      //value parameter StringValue evt;
      StringValue evtValue = op.getEvt();
      //meta parameter IMetaObject /*MetaObject<State>*/ tgt;
      IMetaObject /*MetaObject<State>*/ tgtMeta = op.getTgt();
      ID tgtId = tgtMeta.getKey();
      Value tgtValue = new Path(tgtId);
      State tgt = (State) tgtMeta.getObject();
      //operations;
      op.addOperation(new ObjectCreate(tId, TransType));
      op.addOperation(new ObjectSet((Path) tValue, TransSrcField, srcValue));
      op.addOperation(new ObjectSet((Path) tValue, TransEvtField, evtValue));
      op.addOperation(new ObjectSet((Path) tValue, TransTgtField, tgtValue));
    }
  
    public void visit(TransDelete op){
      //object Trans t;
      IMetaObject /*MetaObject<Trans>*/ tMeta = op.getT();
      ID tId = tMeta.getKey();
      Value tValue = new Path(tId);
      Trans t = (Trans) tMeta.getObject();
      //store old meta parameter type: IMetaObject /*MetaObject<State>*/, param: src, attr: t.src;
      if(t.src != null){
        op.setOldSrc((IMetaObject /*MetaObject<State>*/) t.src.getMetaObject());
      } else {
        op.setOldSrc((IMetaObject /*MetaObject<State>*/) null);
      };
      //store old value parameter type: StringValue, param: t.evt, attr: t.evt;
      op.setOldEvt((StringValue) Patcher.encode(t.evt));
      //store old meta parameter type: IMetaObject /*MetaObject<State>*/, param: tgt, attr: t.tgt;
      if(t.tgt != null){
        op.setOldTgt((IMetaObject /*MetaObject<State>*/) t.tgt.getMetaObject());
      } else {
        op.setOldTgt((IMetaObject /*MetaObject<State>*/) null);
      };
      //operations;
      op.addOperation(new ObjectSet((Path) tValue, TransSrcField, NullValue.Null));
      op.addOperation(new ObjectSet((Path) tValue, TransEvtField, NullValue.Null));
      op.addOperation(new ObjectSet((Path) tValue, TransTgtField, NullValue.Null));
      op.addOperation(new ObjectDelete(tId, TransType));
    }
  }
}