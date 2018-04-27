using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Reflection;

namespace Radar_Starter
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        List<string> dllFilesPath = new List<string>();
        List<byte[]> dllFilesBytes = new List<byte[]>();

        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
            dllFilesPath.Add(Environment.CurrentDirectory + "/arpspoof.exe");
            dllFilesPath.Add(Environment.CurrentDirectory + "/update.xml");
            dllFilesPath.Add(Environment.CurrentDirectory + "/MahApps.Metro.dll");
            dllFilesBytes.Add(Launcher_Namespace.Properties.Resources.arpspoof);
            dllFilesBytes.Add(Launcher_Namespace.Properties.Resources.update);
            dllFilesBytes.Add(Launcher_Namespace.Properties.Resources.MahApps_Metro);
            int n = 0;
            while (n <= 2)
            {
                using (FileStream fs = File.Create(dllFilesPath[n]))
                {
                    Byte[] info = dllFilesBytes[n];
                    fs.Write(info, 0, info.Length);
                    n++;
                }
            }
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                //gets the main Assembly
                var parentAssembly = Assembly.GetExecutingAssembly();
                //args.Name will be something like this
                //[ MahApps.Metro, Version=1.1.3.81, Culture=en-US, PublicKeyToken=null ]
                //so we take the name of the Assembly (MahApps.Metro) then add (.dll) to it
                var finalname = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
                //here we search the resources for our dll and get the first match
                var ResourcesList = parentAssembly.GetManifestResourceNames();
                string OurResourceName = null;
                //(you can replace this with a LINQ extension like [Find] or [First])
                for (int i = 0; i <= ResourcesList.Length - 1; i++)
                {
                    var name = ResourcesList[i];
                    if (name.EndsWith(finalname))
                    {
                        //Get the name then close the loop to get the first occuring value
                        OurResourceName = name;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(OurResourceName))
                {
                    //get a stream representing our resource then load it as bytes
                    using (Stream stream = parentAssembly.GetManifestResourceStream(OurResourceName))
                    {
                        //in vb.net use [ New Byte(stream.Length - 1) ]
                        //in c#.net use [ new byte[stream.Length]; ]
                        byte[] block = new byte[stream.Length];
                        stream.Read(block, 0, block.Length);
                        return Assembly.Load(block);
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
