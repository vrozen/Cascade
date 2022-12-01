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

namespace Delta.Model.DesignSpace
{
  public class History
  {
    private readonly Juncture start; //start / root (nothing)
    private Juncture now;   //cursor on the current state
    private Juncture head;  //current path head

    public History()
    {
      this.start = new Beginning();
      this.now = start;
      this.head = start;
    }

    public void addEvent(Event evt)
    {
      if (head != now)
      {
        throw new PatchException("inconsistent state, cannot apply event");
      }
      
      Juncture juncture = new Juncture(evt, head);
      head.addNext(juncture);
      head = juncture;
      now = juncture;
    }

    public Juncture getStart()
    {
      return start;
    }

    public Juncture getNow()
    {
      return now;
    }

    public Juncture getHead()
    {
      return head;
    }

    public void setNow(Juncture now)
    {
      this.now = now;
    }
    
    public void setHead(Juncture head)
    {
      this.head = head;
    }

    //rolls back the cursor to the cause
    //note: the cause is in the past by definition
    public Event setCursorToRootCauseOfHead()
    {
      Event e = head.getEvent();
      Event cause = e.getCause();
      
      while (cause.getCause() != null)
      {
        cause = cause.getCause();
      }

      while (head.getEvent() != cause)
      {
        head = head.getPrevious();
      }

      return cause;
    }
    
    
    public List<Event> getPastEvents()
    {
      List<Event> events = new List<Event>();
      Juncture cur = now;
      while (cur != start)
      {
        if (cur is Juncture juncture)
        {
          Event evt = juncture.getEvent();
          events.add(evt);
          cur = juncture.getPrevious();
        }
      }
      events.Reverse();
      return events;
    }

    public void accept(IVisitor visitor)
    {
      visitor.visit(this);
    }
  }
}