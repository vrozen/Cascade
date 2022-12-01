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
using Delta.Model.BaseType;
using Delta.Model.BaseOperation;
using Delta.Model.DesignSpace;
using Delta.Model.QualifiedName;

//inverts base operations
namespace Delta.Runtime
{
  public class Inverter : Delta.Model.BaseOperation.IVisitor
  {
    private Operation iOp;
    
    /*
     * The reverse method creates am Event
     * with reversed effects for a given Event evt.
     */
    public static Event reverse(Event evt)
    {
      ILanguage language = evt.getLanguage();
      Event rEvent = language.invert(evt);
      rEvent.setLanguage(language);
      rEvent.markGenerated();
      rEvent.markPreMigrated();
      rEvent.markPostMigrated();
      
      foreach (Event e in evt.getPreEffects())
      {
        Event iE = reverse(e);
        iE.setCause(rEvent);
        rEvent.addPreEffect(iE);
      }

      foreach (Event e in evt.getPostEffects())
      {
        Event iE = reverse(e);
        iE.setCause(rEvent);
        rEvent.addPostEffect(iE);
      }

      rEvent.getPreEffects().Reverse();
      rEvent.getPostEffects().Reverse();

      return rEvent;
    }
    
    /**
     * Inverts the operations from Effect effect
     * and stores them in the inverted Effect iEffect.
     */
    public void invertOperations(Effect effect, Effect iEffect)
    {
      List<Operation> operations = effect.getOperations();
      foreach (Operation op in operations)
      {
        Operation iOperation = invert(op);
        Location opLoc = op.getLocation();
        iOperation.setLocation(opLoc);
        iEffect.addOperation(iOperation);
      }
      List<Operation> iOperations = iEffect.getOperations();
      iOperations.Reverse();
    }
    
    private Operation invert(Operation op)
    {
      op.accept(this);
      return iOp;
    }

    public void visit(ObjectCreate op)
    {
      ID id = op.getId();
      StringValue type = op.getClassName();
      iOp = new ObjectDelete(id, type);
    }

    public void visit(ObjectDelete op)
    {
      ID id = op.getId();
      StringValue type = op.getClassName();
      iOp = new ObjectCreate(id, type);
    }

    public void visit(ObjectKey op)
    {
      ID oldId = op.getId();
      ID newId = op.getNewId();
      iOp = new ObjectKey(newId, oldId);
    }

    public void visit(ObjectSet op)
    {
      Path path = op.getPath();
      Field field = op.getField();
      Value newValue = op.getNewValue();
      Value oldValue = op.getOldValue();
      iOp = new ObjectSet(path, field, oldValue, newValue);
    }

    public void visit(ListInsert op)
    {
      Path path = op.getPath();
      IntValue index = op.getIndex();
      Value val = op.getValue();
      iOp = new ListRemove(path, index, val);
    }

    public void visit(ListRemove op)
    {
      Path path = op.getPath();
      IntValue index = op.getIndex();
      Value val = op.getValue();
      iOp = new ListInsert(path, index, val);
    }

    public void visit(ListPush op)
    {
      Path path = op.getPath();
      Value val = op.getValue();
      iOp = new ListPop(path, val);
    }

    public void visit(ListPop op)
    {
      Path path = op.getPath();
      Value val = op.getOldValue();
      iOp = new ListPush(path, val);
    }

    public void visit(ListSet op)
    {
      Path path = op.getPath();
      Value index = op.getIndex();
      Value newValue = op.getNewValue();
      Value oldValue = op.getOldValue();
      iOp = new ListSet(path, index, oldValue, newValue);      
    }

    public void visit(SetAdd op)
    {
      Path path = op.getPath();
      Value val = op.getValue();
      iOp = new SetRemove(path, val);
    }

    public void visit(SetRemove op)
    {
      Path path = op.getPath();
      Value val = op.getValue();
      iOp = new SetAdd(path, val);
    }

    public void visit(MapAdd op)
    {
      Path path = op.getPath();
      Value key = op.getKey();
      iOp = new MapRemove(path, key);
    }

    public void visit(MapRemove op)
    {
      Path path = op.getPath();
      Value key = op.getKey();
      iOp = new MapAdd(path, key);
    }

    public void visit(MapSet op)
    {
      Path path = op.getPath();
      Value key = op.getKey();
      Value newValue = op.getNewValue();
      Value oldValue = op.getOldValue();
      
      iOp = new MapSet(path, key, oldValue, newValue);
    }
  }
}