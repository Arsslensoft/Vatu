using System;
using System.IO;
using System.Text.RegularExpressions;

class PreproTest {
    static void Test(string s) {
        Console.WriteLine(ms.Substitute(s));
    }    
  
    static string Input() {
        Console.Write(">> ");
        return Console.In.ReadLine();
    }
    
    class MyMacroSubstitutor : MacroSubstitutor {
        public override string CustomReplacement(string s) {
            switch(s) {
            case "$DATE": return DateTime.Now.ToString();  
            case "$USER": return Environment.GetEnvironmentVariable("USERNAME"); 
            }
            return "";
        }
    }    
    
    static MacroSubstitutor ms = new MyMacroSubstitutor();    

    static void Main(string[] args) {                
       string line;
        ms.AddMacro("$DATE",MacroEntry.Custom,null);
        ms.AddMacro("$USER",MacroEntry.Custom,null);
        while((line = Input()) != null) {
            line = ms.ProcessLine(line);
            if (line != "")
                Console.WriteLine(line);
        }
     
    }
    
}
