#region Copyright (c) 2006-2011 LOKAD SAS. All rights reserved

// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Media;

namespace Lokad.CodeDsl
{
    class Program
    {
        static ConcurrentDictionary<string, string> _states = new ConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {
            var info = new DirectoryInfo("..\\..\\..\\..");

            var files = info.GetFiles("*.ddd", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                var text = File.ReadAllText(fileInfo.FullName);
                Changed(fileInfo.FullName, text);
                Rebuild(text, fileInfo.FullName);
            }

            var notifiers = files
                .Select(f => f.DirectoryName)
                .Distinct()
                .Select(d => new FileSystemWatcher(d, "*.ddd"))
                .ToArray();

            foreach (var notifier in notifiers)
            {
                notifier.Changed += NotifierOnChanged;
                notifier.EnableRaisingEvents = true;
            }


            Console.ReadLine();
        }

        static void NotifierOnChanged(object sender, FileSystemEventArgs args)
        {
            if (!File.Exists(args.FullPath)) return;

            try
            {
                var text = File.ReadAllText(args.FullPath);

                if (!Changed(args.FullPath, text))
                    return;


                Console.WriteLine("{1}-{0}", args.Name, args.ChangeType);
                Rebuild(text, args.FullPath);
                SystemSounds.Beep.Play();
            }
            catch (IOException) {}
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                SystemSounds.Exclamation.Play();
            }
        }

        static bool Changed(string path, string value)
        {
            var changed = false;
            _states.AddOrUpdate(path, key =>
                {
                    changed = true;
                    return value;
                }, (s, s1) =>
                    {
                        changed = s1 != value;
                        return value;
                    });
            return changed;
        }

        static void Rebuild(string text, string fullPath)
        {
            var dsl = text;
            var generator = new TemplatedGenerator()
                {
                    GenerateInterfaceForEntityWithModifiers = "?",
                    TemplateForInterfaceName = "public interface I{0}Aggregate",
                    TemplateForInterfaceMember = "void When({0} c);",
                    ClassNameTemplate = @"
    

[DataContract(Namespace = {1})]
public partial class {0}",
                    MemberTemplate = "[DataMember(Order = {0})] public {1} {2} {{ get; private set; }}",
                    
                };

  
            File.WriteAllText(Path.ChangeExtension(fullPath, "cs"), GeneratorUtil.Build(dsl, generator));
        }
    }
}