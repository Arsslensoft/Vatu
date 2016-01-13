using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace cpy
{
    class Program
    {
        static void Main(string[] args)
        {
            int i,size,k=2,ftab,sect=3;
            byte[] buf = new byte[512];
            byte vbuf;
	        int off=0;
            if (File.Exists("filetable"))
                File.Delete("filetable");
            if (File.Exists(args[0]))
                File.Delete(args[0]);
            BinaryReader br = new BinaryReader(File.OpenRead(args[1]));
             br.Read(buf,0, 512);
             br.Close();
             Console.WriteLine("Bootsector file: "+ args[1]);

             BinaryWriter devw = new BinaryWriter(File.Open(args[0], FileMode.CreateNew));
              Console.WriteLine("Dest file: " + args[0]);
              devw.Write(buf, 0, 512);

              BinaryWriter ftw = new BinaryWriter(File.Open("filetable", FileMode.CreateNew));
              ftw.Write("{");

              for (i = 2; i < args.Length; i++)
              {


                  off = off + (k * 512);
                  devw.Seek(off, SeekOrigin.Begin);
                  BinaryReader fil_descr = new BinaryReader(File.OpenRead(args[i]));
    
               
                  size = 0;
                  byte[] b = fil_descr.ReadBytes((int)fil_descr.BaseStream.Length);
                  devw.Write(b, 0, b.Length);
                  size = (int)fil_descr.BaseStream.Length;
                  k = (size / 512);
                  if (size % 512 != 0)
                      k++;

                  ftw.Write(string.Format("{0}-{1}", args[i], sect));
                 
                  Console.WriteLine("Input file \'{0}\' written at offset %{1} at {3} end at {2} size {4}\n", args[i], off,sect+ k,sect,size);
                     fil_descr.Close();

                  sect = sect + k;
              }
              ftw.Write("}");
              ftw.Close();

              off = off + (k * 512);
           
              devw.Seek(off, SeekOrigin.Begin);
            
	   
           BinaryReader ftr = new BinaryReader(File.OpenRead("filetable"));  
            ftr.Read(buf,0,512);
	        devw.Write(buf, 0,512);
	        ftr.Close();


            // Floppy
            for (i = (int)devw.BaseStream.Position; i < (2880) * 512; i++)
            {
                devw.Write((byte)0);
            }
            devw.Close();

          //  Process.Start(@"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe", "internalcommands createrawvmdk -filename vm10.vmdk -rawdisk a.bin");
            Console.Read();
        }
    }
}
