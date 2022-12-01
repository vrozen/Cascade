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
using Delta.Model.QualifiedName;
using Delta.Model.BaseType;

namespace Delta.Model.BaseOperation
{
  public class ObjectSet : RelativeOperation, IReplaceable
  {
    private readonly Field field;
    private readonly Value newVal; //new value
    private Value oldVal;          //old value
    private bool assignedOldValue;
    
    public ObjectSet(Path path, Field field, Value newVal) : base(path)
    {
      this.field = field;
      this.newVal = newVal;
      this.oldVal = null;
      assignedOldValue = false;
    }
    
    public ObjectSet(Path path, Field field, Value newVal, Value oldVal) : base(path)
    {
      this.field = field;
      this.newVal = newVal;
      this.oldVal = oldVal;
      assignedOldValue = true;
    }

    public Field getField()
    {
      return field;
    }
    
    public Value getNewValue()
    {
      return newVal;
    }

    public Value getOldValue()
    {
      return oldVal;
    }

    public bool hasOldValue()
    {
      return assignedOldValue;
    }

    public void setOldValue(Value oldValue)
    {
      if (assignedOldValue)
      {
        throw new PatchException("Cannot reassign old value");
      }
      
      this.assignedOldValue = true;
      this.oldVal = oldValue;
    }
    
    public override void accept(IVisitor visitor)
    {
      visitor.visit(this);
    }
  }
}