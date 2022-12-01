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
using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.REPL.Model;
using Delta.REPL.Operation;
using Delta.Runtime;

namespace Delta.REPL.Runtime
{
  public class Language : ILanguage //it isn't
  {
    private readonly Engine engine;
    private readonly Patcher patcher;
    
    private readonly PrettyPrinter printer;
    
    private readonly PreMigrator preMigrator;
    private readonly PostMigrator postMigrator;
    private readonly Generator generator;
    private readonly Inverter inverter;
    
    private readonly MetaObject<SymbolTable> symbolTableMeta;

    private readonly Server server;
    private readonly Aligner aligner;
    private readonly Parser parser;

    private readonly SemaphoreSlim readLock;
    private readonly Queue<string> commands;
    private readonly Mutex commandsLock;
    
    //private MetaObject<Language> metaObject;
    
    public Language(Engine engine)
    {
      this.engine = engine;
      this.patcher = engine.getPatcher();
      Dispatcher dispatcher = engine.getDispatcher();
      this.printer = patcher.getPrettyPrinter();

      //UUID id = new UUID(0);
      //metaObject = new MetaObject<Language>(id);
      //metaObject.bind(this);
      
      server = new Server(this);
      parser = new Parser();
      symbolTableMeta = patcher.createMetaObject<SymbolTable>();
      aligner = new Aligner(symbolTableMeta);
      preMigrator = new PreMigrator(patcher);
      postMigrator = new PostMigrator(engine, this, aligner, symbolTableMeta);
      generator = new Generator(patcher);
      inverter = new Inverter();

      readLock = new SemaphoreSlim(0);
      commands = new Queue<string>();
      commandsLock = new Mutex();
    }

    private void start()
    {
      ThreadStart start = new ThreadStart(this.run);
      Thread thread = new Thread(start);
      thread.Start(); //start this thread
      
      server.start(); //start the server
    }
    
    public void init()
    {
      Initialization initialization = new Initialization();
      engine.schedule(initialization);
      start();
    }

    public void deinit()
    {
      Effect delete = new SymbolTableDelete(symbolTableMeta);
      engine.schedule(delete);
    }

    private void run()
    {
      while(true)
      {
        readLock.Wait();
        string command = getNextCommand();
        try
        {
          write(command);
          Event evt = (Event) parser.parseCommand(command, "REPL"); 
          engine.schedule(evt);
        }
        catch (ParserException e)
        {
          writeError(e.Message);
        }
      }
    }

    public void runScript(string script)
    {
      foreach (string line in script.Split(';'))
      {
        if (line != "")
        {
          addCommand(line + ";");
        }
      }
    }

    /*
    public void resolve(Event evt)
    {
      if (evt is Assignment assignment)
      {
        //align variable names in lookups with object space
        aligner.align(assignment.getValue());
      }
    }*/

    public void addCommand(string command)
    {
      commandsLock.WaitOne();
      commands.Enqueue(command);
      commandsLock.ReleaseMutex();
      readLock.Release();
    }
    
    private string getNextCommand()
    {
      commandsLock.WaitOne();
      string command = commands.Dequeue();
      commandsLock.ReleaseMutex();
      return command;
    }

    public string getName()
    {
      return "REPL";
    }

    public void generate(Event evt)
    {
      generator.generate(evt);
    }

    public void preMigrate(Event evt)
    {
      preMigrator.preMigrate(evt);
    }

    public void postMigrate(Event evt)
    {
      postMigrator.postMigrate(evt);
    }
    
    public Event invert(Event evt)
    {
     return inverter.invert(evt);
    }
    
    public void notifyScheduled(Event evt)
    {
    }

    public void notifyGenerated(Event evt)
    {
    }

    public void notifyCommitted(Event evt)
    {
      //string output = printer.print(evt);
      //server.writeText(output);
    }
    
    public void notifyCompleted(Event evt)
    {
      string output = printer.print(evt);
      server.writeText(output);
      
      //History history = patcher.getHistory();
      //plantUmlGenerator.printUML(history);
    }

    public void write(string text)
    {
      server.writeText(text);
    }

    public void writeError(string text)
    {
      server.writeError(text);
    }

    public Type[] getEventTypes()
    {
      Type[] eventTypes =
      {
        typeof(Initialization),
        typeof(Assignment),
        typeof(Declaration),
        typeof(Deletion),
        typeof(Help),
        typeof(Print),
        typeof(Import),
        typeof(SymbolTableCreate),
        typeof(SymbolTableDelete),
        typeof(SymbolTableStoreVariable),
        typeof(SymbolTableRemoveVariable),
        typeof(Trigger),
        typeof(Undo),
        typeof(VarCreate),
        typeof(VarDelete),
        typeof(VarSetValue)
      };
      return eventTypes;
    }
  }
}