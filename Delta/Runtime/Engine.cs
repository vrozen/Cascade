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
using System;
using System.Collections.Generic;
using System.Threading;
using Delta.Model.DesignSpace;
using Delta.REPL.Runtime;

namespace Delta.Runtime
{
  public class Engine
  {
    private readonly SemaphoreSlim readLock; //used to await input
    private readonly Queue<Event> events;    //input queue of events to be processed
    private readonly Mutex eventsLock;       //protects events queue

    private readonly Patcher patcher;
    private readonly Scheduler scheduler;
    private readonly Dispatcher dispatcher;

    //private readonly Language repl;

    private static readonly string simpleScript =
      "var m;" +
      "var mi;" +
      "var mi2;"+
      "var s1;" +
      "var s2;" +
      "var s3;" +
      "var t1;" +
      "var t2;" +
      "var t3;" +
      "var t4;";

    private static readonly string script =
      "var p;" +
      "var n1;" +
      "var n2;" +
      "var n3;" +
      "var e1;" +
      "var e2;" +
      "var engine;" +
      "Delta.LiveMM.Diagram.Program.Create(p);" +
      "Delta.LiveMM.Diagram.Node.Create(n1, p, \"A\");" +
      "Delta.LiveMM.Diagram.Node.Create(n2, p, \"B\");" +
      "Delta.LiveMM.Diagram.Node.Create(n3, p, \"C\");" +
      "Delta.LiveMM.Diagram.FlowEdge.Create(e1, p, \"E1\", n1, 1, n2);"+
      "Delta.LiveMM.Diagram.FlowEdge.Create(e1, p, \"E2\", n2, 3, n3);"+
      "Delta.LiveMM.Diagram.Node.BecomePool(n1, 10, 10);"+
      "Delta.LiveMM.RTState.MMEngine.Create(engine, p);" +
      "engine;"+
      "Delta.LiveMM.Diagram.Node.BecomeConverter(n2);"+
      "Delta.LiveMM.Diagram.Node.SetWhen(n2, Delta.LiveMM.Diagram.Model.When.User);" +
      "Delta.LiveMM.RTState.MMEngine.Trigger(engine, \"B\");"+
      "engine;" +
      "p;";
    
    private static readonly string script6 =
      "var m;" +
      "var s1;" +
      "var s2;" +
      "var s3;" +
      "var t1;" +
      "var t2;" +
      "var t3;" +
      "var t4;" +
      "var mi;" +
      "Delta.TinyLiveSML.Mach.Create(m, \"door\");" +
      "Delta.TinyLiveSML.State.Create(s1, \"closed\", m);" +
      "m;"+
      "Delta.TinyLiveSML.MachInst.Create(mi, m);" +
      "mi;"+
      "Delta.TinyLiveSML.State.Create(s2, \"opened\", m);" +
      "Delta.TinyLiveSML.State.Create(s3, \"locked\", m);" +
      "Delta.TinyLiveSML.Trans.Create(t1, s1, \"open\", s2);" +
      "Delta.TinyLiveSML.Trans.Create(t2, s2, \"close\", s1);" +
      "Delta.TinyLiveSML.Trans.Create(t3, s1, \"lock\", s3);" +
      "Delta.TinyLiveSML.Trans.Create(t4, s3, \"unlock\", s1);" +
      "m;" +
      "mi;" +
      "Delta.TinyLiveSML.MachInst.Trigger(mi, \"lock\");"+
      "m;" +
      "mi;" +
      "Delta.TinyLiveSML.State.Delete(s3, \"locked\", m);" +
      "m;" +
      "mi;" +
      "Delta.TinyLiveSML.Mach.Delete(m, \"door\");";   
    
    
    private static readonly string script2 =
      "var f;" +
      "var q1; " +
      "var q2; " +
      "Delta.LiveQL.RTForm.Form.Create(f, \"foo\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q1, f, f.b, \"bar!\", Delta.LiveQL.RTForm.Model.QType.Int, \"bar\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.SetExpression(q1, f, \"baz*2-1\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q2, f, f.b, \"baz?\", Delta.LiveQL.RTForm.Model.QType.Int, \"baz\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.GiveAnswer(q2, f, \"2\");" +
      "f;";

    private static readonly string script3 =
      "var f;" +
      "var i; " +
      "var q1; " +
      "var q2; " +
      "var q3; " +
      "var q4; " +
      "var q5; " +
      "var q6; " +
      "Delta.LiveQL.RTForm.Form.Create(f, \"taxOfficeExample\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q1, f, f.b, \"did you sell a house in 2010?\", Delta.LiveQL.RTForm.Model.QType.Bool, \"hasSoldHouse\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q2, f, f.b, \"did you buy a house in 2010?\", Delta.LiveQL.RTForm.Model.QType.Bool, \"hasBoughtHouse\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q3, f, f.b,  \"did you enter a loan?\", Delta.LiveQL.RTForm.Model.QType.Bool, \"hasMaintLoan\");" +
      "f;" +
      "Delta.LiveQL.RTForm.IfStat.Create(i, f, f.b);" +
      "f;" +
      "Delta.LiveQL.RTForm.IfStat.SetCondition(i, f, \"hasSoldHouse\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q4, f, i.ib, \"what was the selling price?\", Delta.LiveQL.RTForm.Model.QType.Int, \"sellingPrice\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q5, f, i.ib, \"private debts for the sold house\", Delta.LiveQL.RTForm.Model.QType.Int, \"privateDebt\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.Create(q6, f, i.ib, \"residue\", Delta.LiveQL.RTForm.Model.QType.Int, \"residue\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.SetExpression(q6, f, \"sellingPrice - privateDebt\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.GiveAnswer(q1, f, \"true\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.GiveAnswer(q4, f, \"200000\");" +
      "f;" +
      "Delta.LiveQL.RTForm.Question.GiveAnswer(q5, f, \"50000\");" +
      "f;" +      
      "Delta.LiveQL.RTForm.Question.GiveAnswer(q1, f, \"false\");" +      
      "f;";


    
/*
private static readonly string script = 
  "var m;" +
  "var mi;"+
  "var s1;" +
  "var s2;" +
  "var t1;" +
  "var t2;" +
  "SML.Mach.Create(m, \"door\");" +
  "SML.State.Create(s1, \"closed\", m);" +
  "SML.MachInst.Create(mi, m);"+
  "m;"+
  "mi;"+
  "SML.State.Create(s2, \"opened\", m);" +
  "SML.Trans.Create(t1, s1, \"open\", s2);" +
  "SML.Trans.Create(t2, s2, \"close\", s1);" +
  "m";
*/
      //SML.MachInst.Create(mi, m);
      //SML.MachInst.Trigger(mi, "open");
      //SML.State.Delete(s3, "locked", m);
      
    
    public Engine()
    {
      patcher = new Patcher();
      dispatcher = new Dispatcher();
      scheduler = new Scheduler(patcher, dispatcher);

      readLock = new SemaphoreSlim(0);
      events = new Queue<Event>();
      eventsLock = new Mutex();
    }

    public void start()
    {
      ThreadStart start = new ThreadStart(this.run);
      Thread thread = new Thread(start);
      thread.Start();
    }
    
    private void run()
    {
      while(true)
      {
        readLock.Wait();
        Event evt = getNextEvent();
        
        try
        {
          scheduler.schedule(evt);
        }
        catch(ParserException e)
        {
          Console.Error.WriteLine(e.ToString());

          ILanguage language = evt.getLanguage();
          if (language != null)
          {
            language.writeError(e.Message);
          }
        }
      }
    }
    
    public static void Main(string[] args)
    {
      Engine engine = new Engine();
      engine.start();

      Language repl = new Language(engine);
      engine.register(repl);
      //engine.setRepl(repl); //for writing commands to the repl
      
      Delta.TinyLiveSML.Runtime.Language tinySml = new Delta.TinyLiveSML.Runtime.Language(engine.getPatcher());
      engine.register(tinySml);
      
      repl.runScript(simpleScript);

      /*
      Delta.LiveSML.Program.Runtime.Language programSml = new Delta.LiveSML.Program.Runtime.Language(engine.getPatcher());
      engine.register(programSml);
      Delta.LiveSML.RTState.Runtime.Language stateSml = new Delta.LiveSML.RTState.Runtime.Language(engine.getPatcher());
      engine.register(stateSml);
      
      Delta.LiveMM.Diagram.Runtime.Language mmDiagram = new Delta.LiveMM.Diagram.Runtime.Language(engine.getPatcher());
      engine.register(mmDiagram);
      
      Delta.LiveMM.RTState.Runtime.Language mmRuntime = new Delta.LiveMM.RTState.Runtime.Language(engine.getPatcher());
      engine.register(mmRuntime);
      
      Delta.LiveQL.Expression.Runtime.Language expressionLanguage =
         new Delta.LiveQL.Expression.Runtime.Language(engine.getPatcher());
      engine.register(expressionLanguage);

      Delta.LiveQL.Eval.Runtime.Language evalLanguage =
        new Delta.LiveQL.Eval.Runtime.Language(engine.getPatcher());
      engine.register(evalLanguage);
      
      Delta.LiveQL.RTForm.Runtime.Language formLanguage =
        new Delta.LiveQL.RTForm.Runtime.Language(engine.getPatcher());
      engine.register(formLanguage);
      
      Delta.LiveQL.RTState.Runtime.Language stateLanguage =
        new Delta.LiveQL.RTState.Runtime.Language(engine.getPatcher());
      engine.register(stateLanguage);
      */
      

    }

    public void schedule(Event evt)
    {
      eventsLock.WaitOne();
      events.Enqueue(evt);
      eventsLock.ReleaseMutex();
      readLock.Release();
    }
    
    public void register(ILanguage language)
    {
      dispatcher.addLanguage(language);
      language.init();
    }

    public void unregister(ILanguage language)
    {
      language.deinit();
      dispatcher.removeLanguage(language);
    }
    
    private Event getNextEvent()
    {
      Event evt = null;
      eventsLock.WaitOne();
      if (events.Count > 0)
      {
        evt = events.Dequeue();
      }
      eventsLock.ReleaseMutex();
      return evt;
    }

    public Patcher getPatcher()
    {
      return patcher;
    }

    public Dispatcher getDispatcher()
    {
      return dispatcher;
    }

    public Scheduler getScheduler()
    {
      return scheduler;
    }
  }
}