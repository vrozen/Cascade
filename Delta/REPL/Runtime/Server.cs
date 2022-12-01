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
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
// This code was helpful in the construction of this server:
// URL:       https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
// Filename:  HttpServer.cs        
// Author:    Benjamin N. Summerton <define-private-public>        
// License:   Unlicense (http://unlicense.org/)

namespace Delta.REPL.Runtime
{ 
  public class Server
  {
    private readonly HttpListener listener;
    private const string URL = "http://127.0.0.1:8000/";
    private const string PATH = "../../web";
    private readonly Language language;

    private readonly SemaphoreSlim readLock;
    private readonly Mutex bufferLock;
    private readonly Queue<Tuple<string,string>> buffer; //response buffer

    public Server(Language language)
    {
      this.language = language;

      bufferLock = new Mutex();
      buffer = new Queue<Tuple<string,string>>();
      readLock = new SemaphoreSlim(0);
      
      listener = new HttpListener();
      listener.Prefixes.Add(URL);
      listener.Start();
    }

    public void start()
    {
      ThreadStart start = new ThreadStart(this.run);
      Thread thread = new Thread(start);
      thread.Start();
    }

    private async Task<Tuple<string, string>> getNextItem()
    {
      Tuple<string, string> item = null;
      await readLock.WaitAsync();

      bufferLock.WaitOne();
      if (buffer.Count > 0)
      {
        item = buffer.Dequeue();
      }
      bufferLock.ReleaseMutex();
    
      return item;
    }
    
    private void run()
    {
      Console.WriteLine("Listening for connections on {0}", URL);
      bool runServer = true;
      // While a user hasn't visited the `shutdown` url, keep on handling requests
      while (runServer)
      {
        // Will wait here until we hear from a connection
        HttpListenerContext context = listener.GetContext();
        ServerRequest request = new ServerRequest(context,this);
        request.processAsync();
      }
      listener.Close();
    }

    public void processCommand(string command)
    {
      language.addCommand(command);
    }
    
    public void writeText(string text)
    {
      String channel = "text";
      
      //hack for demo
      if (text.Contains("machine instance "))
      {
        writeInstance(text);
      } else if (text.Contains("machine "))
      {
        writeProgram(text);
      }
      
      string[] lines = text.Split('\n');
      foreach(string line in lines)
      {
        bufferLock.WaitOne();
        Tuple<string, string> item = new Tuple<string, string>("text", line);
        Console.Out.WriteLine(line);
        buffer.Enqueue(item);
        bufferLock.ReleaseMutex();
        readLock.Release();
      }
    }

    private void writeProgram(string program)
    {
      bufferLock.WaitOne();
      Tuple<string, string> item = new Tuple<string, string>("program", program);
      Console.Out.WriteLine(program);
      buffer.Enqueue(item);
      bufferLock.ReleaseMutex();
      readLock.Release();      
    }

    private void writeInstance(string instance)
    {
      bufferLock.WaitOne();
      Tuple<string, string> item = new Tuple<string, string>("instance", instance);
      Console.Out.WriteLine(instance);
      buffer.Enqueue(item);
      bufferLock.ReleaseMutex();
      readLock.Release();
    }
    
    public void writeError(string error)
    {
      bufferLock.WaitOne();
      Tuple<string, string> item = new Tuple<string, string>("error", error); 
      //Console.Error.WriteLine(error);
      buffer.Enqueue(item);
      bufferLock.ReleaseMutex();
      readLock.Release();
    }

    /*
    public void writeDiagram(string fileName)
    {
      bufferLock.WaitOne();
      Tuple<string, string> item = new Tuple<string, string>("image", fileName); 
      buffer.Enqueue(item);
      bufferLock.ReleaseMutex();
      readLock.Release();
    }*/

    private class ServerRequest
    {
      private readonly Server server;
      private readonly HttpListenerContext context;
      private readonly HttpListenerRequest request;
      private readonly HttpListenerResponse response;

      public ServerRequest(HttpListenerContext context, Server server)
      {
        this.server = server;
        this.context = context;
        request = context.Request;
        response = context.Response;
      }
      
      public async Task<bool> processAsync()
      {
        bool runServer = true;
        
        string now = DateTime.Now.ToString();
        Console.WriteLine("["+now+"] requested url: "+request.Url);
        Console.WriteLine("["+now+"] http method: "+request.HttpMethod);

        string path = request.Url.AbsolutePath;
        switch (request.HttpMethod)
        {
          case "GET":
            await processGet();
            break;
          case "POST":
          {
            switch (path)
            {
              case "/data": 
                await update();
                break;
              case "/command": 
                await processCommand();
                break;
              case "/shutdown":
                runServer = false;
                break;
              //todo default error
            }
            break;
            //todo default error
          }
        }

        return runServer;
      }

      private async Task update()
      {
        Tuple<string,string> item = await server.getNextItem();

        string fieldName = item.Item1;
        string fieldValue = item.Item2;
        
        StringBuilder buf = new StringBuilder();
        
        fieldValue = fieldValue.Replace("\\", "\\\\");
        fieldValue = fieldValue.Replace("\"", "\\\"");
        fieldValue = fieldValue.Replace("\n", "\\r\\n");
        
        buf.Append("{");
        buf.Append("\""+fieldName+"\"");
        buf.Append(':');
        buf.Append("\""+fieldValue+"\"");
        buf.Append("}");
        
        string output = buf.ToString();
        await sendResponse(output);
      }
      
      private async Task processCommand()
      {
        System.IO.Stream body = request.InputStream;
        System.Text.Encoding encoding = request.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
        string input = await reader.ReadToEndAsync();
        string now = DateTime.Now.ToString();
        Console.WriteLine("["+now+ "] Schedule command: "+input);

        server.processCommand(input);

        await sendResponse("");
      }

      private async Task processGet()
      {
        string fileName = request.Url.AbsolutePath;
        string now = DateTime.Now.ToString();
        Console.WriteLine("["+now+ "] file requested: "+fileName);
        
        if (fileName == "/")
        {
          fileName = "/REPL.html";
        }
        
        fileName = PATH + fileName;
        
        string fileContents = await readAllLinesAsync(fileName);
        await sendResponse(fileContents);
      }
      
      private async Task sendResponse(string message)
      {
        string now = DateTime.Now.ToString();
        Console.WriteLine("["+now+ "] sending response");
        string fileName = request.Url.AbsolutePath;
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        response.ContentType = getMimeType(fileName);
        response.ContentEncoding = Encoding.UTF8;
        response.ContentLength64 = bytes.LongLength;

        // Write out to the response stream (asynchronously), then close it
        await response.OutputStream.WriteAsync(bytes, 0, bytes.Length);
        response.Close();
      }

      private const FileOptions DefaultOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
      private const int DefaultBufferSize = 4096*8*8*8;
      
      private static Task<string> readAllLinesAsync(string path)
      {
        return readAllLinesAsync(path, Encoding.UTF8);
      }

      private static async Task<string> readAllLinesAsync(string path, Encoding encoding)
      {
        StringBuilder buf = new StringBuilder();

        // Open the FileStream with the same FileMode, FileAccess
        // and FileShare as a call to File.OpenText would've done.
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultBufferSize, DefaultOptions))
        using (var reader = new StreamReader(stream, encoding))
        {
          string line;
          while ((line = await reader.ReadLineAsync()) != null)
          {
            buf.Append(line);
            buf.Append('\n');
          }
        }

        return buf.ToString();
      }
      
      private static string getMimeType(string fileName)
      {
        string contentType = "text/html";
        if (fileName.EndsWith("html"))
        {
          contentType = "text/html";
        }
        else if (fileName.EndsWith("css"))
        {
          contentType = "css";
        }
        else if (fileName.EndsWith("svg"))
        {
          contentType = "image/svg+xml";
        }

        return contentType;
      }
    }

  }
}