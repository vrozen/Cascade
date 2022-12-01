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
  public class Language : ILanguage, IPatchable {
    private static readonly String LanguageName = "Delta.TinyLiveSML";
    private readonly PreMigrator preMigrator;
    private readonly PostMigrator postMigrator;
    private readonly Generator generator;
    private readonly Inverter inverter;
    [Reference] private IMetaObject metaObject;
  
    public Language(Patcher patcher){
      UUID id = new UUID(0);
      metaObject = new MetaObject<Language>(id);
      metaObject.bind(this);
      preMigrator = new PreMigrator(patcher);
      postMigrator = new PostMigrator(patcher);
      generator = new Generator(patcher);
      inverter = new Inverter();
    }
  
    public void init(){
    }
  
    public void deinit(){
    }
  
    public string getName(){
      return LanguageName;
    }
  
    public void generate(Event evt){
      generator.generate(evt);
    }
  
    public void preMigrate(Event evt){
      preMigrator.preMigrate(evt);
    }
  
    public void postMigrate(Event evt){
      postMigrator.postMigrate(evt);
    }
  
    public Event invert(Event evt){
      return inverter.invert(evt);
    }
  
    public void notifyScheduled(Event evt){
    }
  
    public void notifyGenerated(Event evt){
    }
  
    public void notifyCommitted(Event evt){
    }
  
    public void notifyCompleted(Event evt){
    }
  
    public void write(string text){
    }
  
    public void writeError(string text){
    }
  
    public Type[] getEventTypes(){
      Type[] types = {
        typeof(MachCreate),
        typeof(MachDelete),
        typeof(MachAddState),
        typeof(MachRemoveState),
        typeof(MachAddMachInst),
        typeof(MachRemoveMachInst),
        typeof(MachInstCreate),
        typeof(MachInstDelete),
        typeof(MachInstAddStateInst),
        typeof(MachInstRemoveStateInst),
        typeof(MachInstInitialize),
        typeof(MachInstSetCurState),
        typeof(MachInstTrigger),
        typeof(MachInstMissingCurrentState),
        typeof(MachInstQuiescence),
        typeof(StateCreate),
        typeof(StateDelete),
        typeof(StateAddIn),
        typeof(StateRemoveIn),
        typeof(StateAddOut),
        typeof(StateRemoveOut),
        typeof(StateInstCreate),
        typeof(StateInstDelete),
        typeof(StateInstSetCount),
        typeof(TransCreate),
        typeof(TransDelete)
      };
      return types;
    }
  
    public IMetaObject getMetaObject(){
      return metaObject;
    }
  
    public void setMetaObject(IMetaObject metaObject){
      this.metaObject = metaObject;
    }
  }
}