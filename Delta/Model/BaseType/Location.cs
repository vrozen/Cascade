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
namespace Delta.Model.BaseType
{
  public class Location : ID
  {
    private readonly string path;

    private readonly int offset;
    private readonly int length;

    private readonly int beginLine;
    private readonly int endLine;

    private readonly int beginColumn;
    private readonly int endColumn;

    public Location(string path, int offset, int length, int beginLine, int beginColumn, int endLine, int endColumn)
    {
      this.path = path;
      this.offset = offset;
      this.length = length;
      this.beginLine = beginLine;
      this.beginColumn = beginColumn;
      this.endLine = endLine;
      this.endColumn = endColumn;
    }

    public string getPath()
    {
      return path;
    }

    public int getOffset()
    {
      return offset;
    }

    public int getLength()
    {
      return length;
    }

    public int getBeginLine()
    {
      return beginLine;
    }

    public int getEndLine()
    {
      return endLine;
    }

    public int getBeginColumn()
    {
      return beginColumn;
    }

    public int getEndColumn()
    {
      return endColumn;
    }

    public bool equals(Location other)
    {
      return this.offset == other.offset &&
             this.length == other.length &&
             this.beginLine == other.beginLine &&
             this.beginColumn == other.beginColumn &&
             this.endLine == other.endLine &&
             this.endColumn == other.endColumn &&
             this.path.Equals(other.path);
    }

    public override bool Equals(object obj)
    {
      return obj is Location loc && Equals(loc);
    }

    public override int GetHashCode()
    {
      return offset.GetHashCode() +
             length.GetHashCode() +
             beginLine.GetHashCode() +
             beginLine.GetHashCode() +
             endLine.GetHashCode() +
             endColumn.GetHashCode() +
             path.GetHashCode();
    }

    public override void accept(IVisitor visitor)
    {
      visitor.visit(this);
    }
  }
}