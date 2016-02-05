using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Globalization;

namespace Vasm {
  public class DataMember : BaseAssemblerElement, IComparable<DataMember> {
    public string Name { get; private set; }
    public bool IsComment { get; set; }
    public byte[] RawDefaultValue { get; set; }
    public float? FloatValue { get; set; }
    public uint Alignment { get; set; }
    protected object[] UntypedDefaultValue;
    


    public string RawAsm = null;

    // Hack for not to emit raw data. See RawAsm
    public DataMember() {
      Name = "Dummy";
    }

    protected DataMember(string aName) {
      Name = aName;
    }
    string[] Labels = null;
    bool verbatim = false;
    public DataMember(string aName, string aValue, bool cnst,bool verb) {
      Name = aName;
      StrConst = cnst;
      verbatim = verb;
      //var xBytes = ASCIIEncoding.ASCII.GetBytes(aValue);
      //var xBytes2 = new byte[xBytes.Length + 1];
      //xBytes.CopyTo(xBytes2, 0);
      //xBytes2[xBytes2.Length - 1] = 0;
      //RawDefaultValue = xBytes2;
      StrVal = aValue;
    }
    public DataMember(string aName, float aValue)
    {
        Name = aName;
        FloatValue = aValue;
    }
    public DataMember(string aName,  object[] aDefaultValue)
    {
        Name = aName;
        UntypedDefaultValue = aDefaultValue;
    }
    public DataMember(string aName, string[] aDefaultValue)
    {
        Name = aName;
        Labels = aDefaultValue;
    }
    public DataMember(string aName, sbyte[] aDefaultValue)
    {
        Name = aName;
        List<byte> b = new List<byte>();
        foreach (sbyte s in aDefaultValue)
            b.Add((byte)s);
        RawDefaultValue = b.ToArray();
        //UntypedDefaultValue = aDefaultValue;
    }
    public DataMember(string aName, byte[] aDefaultValue) {
      Name = aName;
      RawDefaultValue = aDefaultValue;
      //UntypedDefaultValue = aDefaultValue;
    }

    public DataMember(string aName, short[] aDefaultValue) {
      Name = aName;
      RawDefaultValue = new byte[aDefaultValue.Length * 2];
      for (int i = 0; i < aDefaultValue.Length; i++) {
        Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                    RawDefaultValue, i * 2, 2);
      }
      //UntypedDefaultValue = aDefaultValue;
    }

    public DataMember(string aName, params ushort[] aDefaultValue) {
      Name = aName;
      RawDefaultValue = new byte[aDefaultValue.Length * 2];
      for (int i = 0; i < aDefaultValue.Length; i++) {
        Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
                    RawDefaultValue, i * 2, 2);
      }
      //UntypedDefaultValue = aDefaultValue;
    }

    public DataMember(string aName, params uint[] aDefaultValue) {
      Name = aName;
      //RawDefaultValue = new byte[aDefaultValue.Length * 4];
      //for (int i = 0; i < aDefaultValue.Length; i++) {
      //    Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
      //                RawDefaultValue, i * 4, 4);
      //}
      UntypedDefaultValue = aDefaultValue.Cast<object>().ToArray();
    }

    public DataMember(string aName, params int[] aDefaultValue) {
      Name = aName;
      //RawDefaultValue = new byte[aDefaultValue.Length * 4];
      //for (int i = 0; i < aDefaultValue.Length; i++) {
      //    Array.Copy(BitConverter.GetBytes(aDefaultValue[i]), 0,
      //                RawDefaultValue, i * 4, 4);
      //}
      UntypedDefaultValue = aDefaultValue.Cast<object>().ToArray();
    }
    string StrVal;
    public DataMember(string aName, Stream aData) {
      Name = aName;
      RawDefaultValue = new byte[aData.Length];
      aData.Read(RawDefaultValue, 0, RawDefaultValue.Length);
    }

    string ReferenceName = null;
    public DataMember(string aName, string ValueName)
    {
        Name = aName;
        ReferenceName = ValueName;
    }

    public static string GetStaticFieldName(FieldInfo aField) {
      return FilterStringForIncorrectChars("static_field__" + LabelName.GetFullName(aField.DeclaringType) + "." + aField.Name);
    }

    public const string IllegalIdentifierChars = "&.,+$<>{}-`\'/\\ ()[]*!=";
    public static string FilterStringForIncorrectChars(string aName) {
      string xTempResult = aName;
      foreach (char c in IllegalIdentifierChars) {
        xTempResult = xTempResult.Replace(c, '_');
      }
      return String.Intern(xTempResult);
    }
    bool StrConst = false;

    public override void WriteText(AsmContext ec, AssemblyWriter aOutput) {
      if (RawAsm != null) {
        aOutput.WriteLine(RawAsm);
        return;
      }
      if (Labels != null)
      {

          aOutput.Write(Name);
           foreach (string lb in Labels)
               aOutput.WriteLine ( " dw " + lb);
         
          aOutput.WriteLine();
          return;
      }
      if (ReferenceName != null)
      {
         
          aOutput.Write(Name);
          aOutput.Write(" dw " + ReferenceName);
          aOutput.WriteLine();
          return;
      }
      if (FloatValue.HasValue)
      {

          aOutput.Write(Name);
          aOutput.Write(" dd " + FloatValue.ToString().Replace(",","."));
          aOutput.WriteLine();
          return;
      }
      if (StrVal != null)
      {
          if (IsGlobal)
          {
              aOutput.Write("global ");
              aOutput.WriteLine(Name);
          }
          if (string.IsNullOrEmpty(StrVal))
          {
              aOutput.Write(Name);
              aOutput.Write(" times 256 db 0");
              return;
          }
          aOutput.Write(Name);
          aOutput.Write(" db ");
       
     
          //foreach (char c in StrVal)
          //{
          //    if (c == '\r' || c == '\n')
          //    {
          //        if (!string.IsNullOrEmpty(last))
          //              strdecl+= "\"" + last + "\", ";
          //        strdecl += b[i].ToString();
          //        strdecl += ",";
          //        last = "";
          //    }
          //    else last += c + "";
          //    i++;
          //}
          //if (!string.IsNullOrEmpty(last))
          //    strdecl += "\"" + last + "\"";
          //else strdecl = strdecl.Remove(strdecl.Length - 1, 1);
            if (!verbatim)
              StringHelper.WriteNormalizedString(StrVal, aOutput);
          else aOutput.Write("\"" + StringHelper.StringFromVerbatimLiteral(StrVal) +"\"");
         
          if (!StrConst)
          {
              aOutput.WriteLine();
              aOutput.Write("\t\t  times " + (255 - StrVal.Length).ToString() + " db 0");
          }
          else aOutput.Write(",0");
          return;
      }

      if (RawDefaultValue != null) {
        if (RawDefaultValue.Length == 0) {
          aOutput.Write(Name);
          aOutput.Write(":");
          return;
        }
        if ((from item in RawDefaultValue
             group item by item
               into i
               select i).Count() > 1 || RawDefaultValue.Length < 10) {
          if (IsGlobal) {
            aOutput.Write("global ");
            aOutput.WriteLine(Name);
          }
          aOutput.Write(Name);
          aOutput.Write(" db ");
      
         
          for (int i = 0; i < (RawDefaultValue.Length - 1); i++) {
            aOutput.Write(RawDefaultValue[i]);
            aOutput.Write(", ");
          }
          aOutput.Write(RawDefaultValue.Last());
        } else {
          //aOutputWriter.WriteLine("TIMES 0x50000 db 0");
            if (IsGlobal)
            {
                aOutput.Write("global ");
                aOutput.WriteLine(Name);
            }
          aOutput.Write(Name);
          aOutput.Write(": TIMES ");
          aOutput.Write(RawDefaultValue.Length);
          aOutput.Write(" db ");
          aOutput.Write(RawDefaultValue[0]);
        }
        return;
      }
      if (UntypedDefaultValue != null) {
        StringBuilder xSB = new StringBuilder();
        if (IsGlobal) {
          aOutput.Write("global ");
          aOutput.WriteLine(Name);
        }
        aOutput.Write(Name);
        aOutput.Write(" dw ");
        Func<object, string> xGetTextForItem = delegate(object aItem) {
                                                     var xElementRef = aItem as ElementReference;
                                                     if (xElementRef == null) {
                                                       return (aItem ?? 0).ToString();
                                                     } else {
                                                       if (xElementRef.Offset == 0) {
                                                         return xElementRef.Name;
                                                       }
                                                       return xElementRef.Name + " + " + xElementRef.Offset;
                                                     }
                                                   };
        for (int i = 0; i < (UntypedDefaultValue.Length - 1); i++) {
          aOutput.Write(xGetTextForItem(UntypedDefaultValue[i]));
          aOutput.Write(", ");
        }
        aOutput.Write(xGetTextForItem(UntypedDefaultValue.Last()));
        return;
      }
      
      throw new Exception("Situation unsupported!");
    }

    public int CompareTo(DataMember other) {
      return String.Compare(Name, other.Name);
    }

    public bool IsGlobal {
      get;
      set;
    }

    public override ulong? ActualAddress {
      get {
        // TODO: for now, we dont have any data alignment
        return StartAddress;
      }
    }

    public override void UpdateAddress(AsmContext ec, ref ulong xAddress) {
      if (Alignment > 0) {
        if (xAddress % Alignment != 0) {
          xAddress += Alignment - (xAddress % Alignment);
        }
      }
      base.UpdateAddress(ec, ref xAddress);
      if (RawDefaultValue != null) {
        xAddress += (ulong)RawDefaultValue.LongLength;
      }
      if (UntypedDefaultValue != null) {
        // TODO: what to do with 64bit target platforms? right now we only support 32bit
        xAddress += (ulong)(UntypedDefaultValue.LongLength * 4);
      }
    }

    public override bool IsComplete(AsmContext ec) {
      if (RawAsm != null) {
        return true;
      }
      if (UntypedDefaultValue != null &&
          UntypedDefaultValue.LongLength > 0) {
        foreach (var xReference in (from item in UntypedDefaultValue
                                    let xRef = item as Vasm.ElementReference
                                    where xRef != null
                                    select xRef)) {
          var xRef = ec.TryResolveReference(xReference);
          if (xRef == null) {
            return false;
          } else if (!xRef.IsComplete(ec)) {
            return false;
          }
        }
      }
      return true;
    }

    public override void WriteData(AsmContext ec, Stream aOutput) {
      if (UntypedDefaultValue != null &&
          UntypedDefaultValue.LongLength > 0) {
        //var xBuff = (byte[])Array.CreateInstance(typeof(byte), UntypedDefaultValue.LongLength * 4);
        for (int i = 0; i < UntypedDefaultValue.Length; i++) {
          var xRef = UntypedDefaultValue[i] as ElementReference;
          //byte[] xTemp;
          if (xRef != null) {
            var xTheRef = ec.TryResolveReference(xRef);
            if (xTheRef == null) {
              throw new Exception("Reference not found!");
            }
            if (!xTheRef.ActualAddress.HasValue) {
              Console.Write("");
            }
            aOutput.Write(BitConverter.GetBytes(xTheRef.ActualAddress.Value), 0, 4);
            //xTemp = BitConverter.GetBytes();
          } else {
            if (UntypedDefaultValue[i] is int) {
              aOutput.Write(BitConverter.GetBytes((int)UntypedDefaultValue[i]), 0, 4);
              //xTemp = BitConverter.GetBytes((int)UntypedDefaultValue[i]);
            } else {
              if (UntypedDefaultValue[i] is uint) {
                aOutput.Write(BitConverter.GetBytes((uint)UntypedDefaultValue[i]), 0, 4);

                //xTemp = BitConverter.GetBytes((uint)UntypedDefaultValue[i]);
              } else {
                throw new Exception("Invalid value inside UntypedDefaultValue");
              }
            }
          }
          //Array.Copy(xTemp, 0, xBuff, i * 4, 4);
        }
      } else {
        aOutput.Write(RawDefaultValue, 0, RawDefaultValue.Length);
      }
    }
  }
  public static class StringHelper
  {
      public static void WriteNormalizedString(string source,AssemblyWriter writer)
      {
          writer.Write("\"");
          int pos = 0;
          while (pos < source.Length)
          {
              bool def = false;
              char c = source[pos];
              if (c == '\\')
              {
                  // --- Handle escape sequences
                  pos++;
                  if (pos >= source.Length) throw new ArgumentException("Missing escape sequence");
                  switch (source[pos])
                  {
                      // --- Simple character escapes
                      case '\'': c = '\'';       break;
                      case '\"': c = '\"'; break;
                      case '\\': c = '\\'; break;
                      case '0': c = '\0'; break;
                      case 'a': c = '\a'; break;
                      case 'b': c = '\b'; break;
                      case 'f': c = '\f'; break;
                      case 'n': c = '\n'; break;
                      case 'r': c = '\r'; break;
                      case 't': c = '\t'; break;
                      case 'v': c = '\v'; break;
                      case 'x':
                          // --- Hexa escape (1-4 digits)
                          StringBuilder hexa = new StringBuilder(10);
                          pos++;
                          if (pos >= source.Length)
                              throw new ArgumentException("Missing escape sequence");
                          c = source[pos];
                          if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                          {
                              hexa.Append(c);
                              pos++;
                              if (pos < source.Length)
                              {
                                  c = source[pos];
                                  if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                                  {
                                      hexa.Append(c);
                                      pos++;
                                      if (pos < source.Length)
                                      {
                                          c = source[pos];
                                          if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') ||
                                            (c >= 'A' && c <= 'F'))
                                          {
                                              hexa.Append(c);
                                              pos++;
                                              if (pos < source.Length)
                                              {
                                                  c = source[pos];
                                                  if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') ||
                                                    (c >= 'A' && c <= 'F'))
                                                  {
                                                      hexa.Append(c);
                                                      pos++;
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                          }
                          c = (char)Int32.Parse(hexa.ToString(), NumberStyles.HexNumber);
                          pos--;
                          break;
                      case 'u':
                          // Unicode hexa escape (exactly 4 digits)
                          pos++;
                          if (pos + 3 >= source.Length)
                              throw new ArgumentException("Unrecognized escape sequence");
                          try
                          {
                              uint charValue = UInt32.Parse(source.Substring(pos, 4),
                                NumberStyles.HexNumber);
                              c = (char)charValue;
                              pos += 3;
                          }
                          catch (SystemException)
                          {
                              throw new ArgumentException("Unrecognized escape sequence");
                          }
                          break;
                      case 'U':
                          // Unicode hexa escape (exactly 8 digits, first four must be 0000)
                          pos++;
                          if (pos + 7 >= source.Length)
                              throw new ArgumentException("Unrecognized escape sequence");
                          try
                          {
                              uint charValue = UInt32.Parse(source.Substring(pos, 8),
                                NumberStyles.HexNumber);
                              if (charValue > 0xffff)
                                  throw new ArgumentException("Unrecognized escape sequence");
                              c = (char)charValue;
                              pos += 7;
                          }
                          catch (SystemException)
                          {
                              throw new ArgumentException("Unrecognized escape sequence");
                          }
                          break;
                  }
            
                      writer.Write("\", ");
                      writer.Write((byte)c);
                      writer.Write(",\"");
                  
              }
              else writer.Write(c);
              pos++;
       
          }
          writer.Write("\"");
      }
      // --------------------------------------------------------------------------------
      /// <summary>
      /// Converts a C# literal string into a normal string.
      /// </summary>
      /// <param name="source">Source C# literal string.</param>
      /// <returns>
      /// Normal string representation.
      /// </returns>
      // --------------------------------------------------------------------------------
      public static string StringFromCSharpLiteral(string source)
      {
          StringBuilder sb = new StringBuilder(source.Length);
          int pos = 0;
          while (pos < source.Length)
          {
              char c = source[pos];
              if (c == '\\')
              {
                  // --- Handle escape sequences
                  pos++;
                  if (pos >= source.Length) throw new ArgumentException("Missing escape sequence");
                  switch (source[pos])
                  {
                      // --- Simple character escapes
                      case '\'': c = '\''; break;
                      case '\"': c = '\"'; break;
                      case '\\': c = '\\'; break;
                      case '0': c = '\0'; break;
                      case 'a': c = '\a'; break;
                      case 'b': c = '\b'; break;
                      case 'f': c = '\f'; break;
                      case 'n': c = '\n'; break;
                      case 'r': c = '\r'; break;
                      case 't': c = '\t'; break;
                      case 'v': c = '\v'; break;
                      case 'x':
                          // --- Hexa escape (1-4 digits)
                          StringBuilder hexa = new StringBuilder(10);
                          pos++;
                          if (pos >= source.Length)
                              throw new ArgumentException("Missing escape sequence");
                          c = source[pos];
                          if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                          {
                              hexa.Append(c);
                              pos++;
                              if (pos < source.Length)
                              {
                                  c = source[pos];
                                  if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                                  {
                                      hexa.Append(c);
                                      pos++;
                                      if (pos < source.Length)
                                      {
                                          c = source[pos];
                                          if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') ||
                                            (c >= 'A' && c <= 'F'))
                                          {
                                              hexa.Append(c);
                                              pos++;
                                              if (pos < source.Length)
                                              {
                                                  c = source[pos];
                                                  if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') ||
                                                    (c >= 'A' && c <= 'F'))
                                                  {
                                                      hexa.Append(c);
                                                      pos++;
                                                  }
                                              }
                                          }
                                      }
                                  }
                              }
                          }
                          c = (char)Int32.Parse(hexa.ToString(), NumberStyles.HexNumber);
                          pos--;
                          break;
                      case 'u':
                          // Unicode hexa escape (exactly 4 digits)
                          pos++;
                          if (pos + 3 >= source.Length)
                              throw new ArgumentException("Unrecognized escape sequence");
                          try
                          {
                              uint charValue = UInt32.Parse(source.Substring(pos, 4),
                                NumberStyles.HexNumber);
                              c = (char)charValue;
                              pos += 3;
                          }
                          catch (SystemException)
                          {
                              throw new ArgumentException("Unrecognized escape sequence");
                          }
                          break;
                      case 'U':
                          // Unicode hexa escape (exactly 8 digits, first four must be 0000)
                          pos++;
                          if (pos + 7 >= source.Length)
                              throw new ArgumentException("Unrecognized escape sequence");
                          try
                          {
                              uint charValue = UInt32.Parse(source.Substring(pos, 8),
                                NumberStyles.HexNumber);
                              if (charValue > 0xffff)
                                  throw new ArgumentException("Unrecognized escape sequence");
                              c = (char)charValue;
                              pos += 7;
                          }
                          catch (SystemException)
                          {
                              throw new ArgumentException("Unrecognized escape sequence");
                          }
                          break;

                  }
              }
              pos++;
              sb.Append(c);
          }
          return sb.ToString();
      }

      // --------------------------------------------------------------------------------
      /// <summary>
      /// Converts a C# verbatim literal string into a normal string.
      /// </summary>
      /// <param name="source">Source C# literal string.</param>
      /// <returns>
      /// Normal string representation.
      /// </returns>
      // --------------------------------------------------------------------------------
      public static string StringFromVerbatimLiteral(string source)
      {
          StringBuilder sb = new StringBuilder(source.Length);
          int pos = 0;
          while (pos < source.Length)
          {
              char c = source[pos];
              if (c == '\"')
              {
                  // --- Handle escape sequences
                  pos++;
                  if (pos >= source.Length) throw new ArgumentException("Missing escape sequence");
                  if (source[pos] == '\"') c = '\"';
                  else throw new ArgumentException("Unrecognized escape sequence");
              }
              pos++;
              sb.Append(c);
          }
          return sb.ToString();
      }


      public static char CharFromCSharpLiteral(string source)
      {
          string result = StringFromCSharpLiteral(source);
          if (result.Length != 1)
              throw new ArgumentException("Invalid char literal");
          return result[0];
      }
  }
}
