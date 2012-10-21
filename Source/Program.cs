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
        static readonly ConcurrentDictionary<string, string> _states = new ConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {
            var info = GuessDirectory(args);
            Console.WriteLine("Watching *.ddd files in {0}.", info.FullName);
            
            var files = info.GetFiles("*.ddd", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                Console.WriteLine("  Found: {0}", fileInfo.Name);
                var text = File.ReadAllText(fileInfo.FullName);
                Changed(fileInfo.FullName, text);
                Rebuild(text, fileInfo.FullName);
            }

            Console.WriteLine("Files checked. We'll watch changes to these files till you press <Enter>");

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

        static string TrimEndFolder(string path, params string[] folders)
        {
            foreach (var folder in folders)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return "";
                var dir = new DirectoryInfo(path);
                if ((dir.Name).Equals(folder, StringComparison.InvariantCultureIgnoreCase) && dir.Parent != null)
                {
                    path = dir.Parent.FullName;
                }
            }
            return path;
        }

        static DirectoryInfo GuessDirectory(string[] args)
        {
            var provided = string.Join(" ", args);
            if (!string.IsNullOrWhiteSpace(provided))
                return new DirectoryInfo(provided);
            Console.WriteLine("No lookup path provided on start, guessing...");
            var path = Directory.GetCurrentDirectory();

            path = TrimEndFolder(path, "debug", "release", "bin");

            if (!string.IsNullOrWhiteSpace(path))
                return new DirectoryInfo(path);
            return new DirectoryInfo(Directory.GetCurrentDirectory());
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
            var generator = new TemplatedGenerator
                {
                    GenerateInterfaceForEntityWithModifiers = "?",
                    TemplateForInterfaceName = "public interface I{0}Aggregate",
                    TemplateForInterfaceMember = "void When({0} c);",
                    ClassNameTemplate = @"
    

[DataContract(Namespace = {1})]
public partial class {0}",
                    MemberTemplate = "[DataMember(Order = {0})] public {1} {2} {{ get; private set; }}",
                    
                };


            try
            {
                File.WriteAllText(Path.ChangeExtension(fullPath, "cs"), GeneratorUtil.Build(dsl, generator));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Parse error: {0}\r\nFile: {1}", ex.Message, fullPath);
            }

        }
    }
}