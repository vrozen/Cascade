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
using System.Threading;
using Delta.Model.BaseType;
using Delta.Model.DesignSpace;
using Delta.Model.QualifiedName;

namespace Delta.Runtime
{
  public class Dispatcher
  {
    private readonly Mutex languagesLock;
    private readonly List<ILanguage> languages;
    
    public Dispatcher()
    {
      languagesLock = new Mutex();
      languages = new List<ILanguage>();
    }
    
    public void generate(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.generate(evt);
    }
    
    public void preMigrate(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.preMigrate(evt);
    }
    
    public void postMigrate(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.postMigrate(evt);
    }

    public void notifyScheduled(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.notifyScheduled(evt);
    }

    public void notifyCommitted(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.notifyCommitted(evt);
    }

    public void notifyGenerated(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.notifyGenerated(evt);
    }

    public void notifyCompleted(Event evt)
    {
      ILanguage language = evt.getLanguage();
      language.notifyCompleted(evt);
    }
    
    public void addLanguage(ILanguage language)
    {
      languagesLock.WaitOne();
      languages.add(language);
      languagesLock.ReleaseMutex();
    }

    public void removeLanguage(ILanguage language)
    {
      languagesLock.WaitOne();
      languages.remove(language);
      languagesLock.ReleaseMutex();
    }

    private static List<string> getFieldNames(Name qualifiedName)
    {
      List<string> fieldNames = new List<string>();
      Name name = qualifiedName;
      while(name != null)
      {
        if (name is Field field)
        {
          fieldNames.add(field.getFieldName());
        }
        else
        {
          throw new PatchException("Expected format: lang.class.event");
        }
        name = name.getName();
      }
      return fieldNames;
    }

    public Type resolveEventName(Name qualifiedName)
    {
      List<string> names = getFieldNames(qualifiedName);
      if (names.Count < 3)
      {
        throw new PatchException("Expected format: lang.package.class.event");
      }
      string lang = (string) names.elementAt(0);
      for (int pos = 1; pos < names.Count - 2; pos++)
      {
        lang += "." + (string) names.elementAt(pos);
      }
      string cls = (string) names.elementAt(names.Count-2);
      string evt = (string) names.elementAt(names.Count-1);
      string clsEvt = cls + evt;
      
      foreach (ILanguage language in languages)
      {
        string langName = language.getName();
        if (lang == langName)
        {
          foreach (Type eventType in language.getEventTypes())
          {
            string eventName = eventType.ToString();
            int dot = eventName.LastIndexOf(".");
            eventName = eventName.Substring(dot + 1);
            
            if (clsEvt == eventName)
            {
              return eventType;
            }
          }
        }
      }
      throw new PatchException("Unknown event type " + lang + "." + cls + "." + evt);
    }
    
    /*
    public Type resolveEventName(String eventTypeName)
    {
      foreach (ILanguage language in languages)
      {
        foreach (Type eventType in language.getEventTypes())
        {
          if (eventType.ToString() == eventTypeName)
          {
            return eventType;
          }
        }
      }
      throw new PatchException("Unknown event type " + eventTypeName);
    }*/
    
    public void resolveLanguage(Event evt)
    {
      languagesLock.WaitOne();
      Type languageType = evt.getLanguageType();
      foreach (ILanguage language in languages)
      {
        if (language.GetType() == languageType)
        {
          evt.setLanguage(language);
        }
      }
      languagesLock.ReleaseMutex();

      if (evt.getLanguage() == null)
      {
        throw new PatchException("Cannot find language for event " + evt.GetType());
      }
    }
  }
}