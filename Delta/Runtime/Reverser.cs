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
using System.Runtime.CompilerServices;
using Delta.Model.BaseType;
using Delta.Model.BaseOperation;
using Delta.Model.DesignSpace;

//reverses effects
namespace Delta.Runtime
{
  public class Reverser
  {
    private readonly Dispatcher dispatcher;
    private readonly Patcher patcher;

    public Reverser(Engine engine)
    {
      this.dispatcher = engine.getDispatcher();
      this.patcher = engine.getPatcher();
    }
    
    // Inverts history: what has been done can be undone.
    /*
    public void undoLast(Undo undo)
    {
      History history = patcher.getHistory();

      List<Event> future = history.getFutureEvents();
      if (future.Count > 0)
      {
        Event firstEvent = (Event) future[0];
        firstEvent.setCause(undo);
        undo.addPostEffect(firstEvent);
        future.insert(0, undo);
      }

      foreach (Event evt in future)
      {
        patcher.commit(evt);
        dispatcher.notifyCommitted(evt);
      }
    }*/

    public void reverse(int numberOfEvents)
    {
      History history = patcher.getHistory();
      
      
      
      
    }

    public Event reverse(Event evt)
    {

      ILanguage language = evt.getLanguage();
      Event rEvent = language.invert(evt);
      
      foreach (Event e in evt.getPreEffects())
      {
        Event iE = reverse(e);
        iE.setCause(rEvent);
        rEvent.addPostEffect(iE);
      }

      foreach (Event e in evt.getPostEffects())
      {
        Event iE = reverse(e);
        iE.setCause(rEvent);
        rEvent.addPreEffect(iE);
      }

      rEvent.getPreEffects().Reverse();
      rEvent.getPostEffects().Reverse();

      return rEvent;
    }
  }
}