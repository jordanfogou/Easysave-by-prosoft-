using EasySoft.controller;
using EasySoft.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace EasySoft
{
    class Program
    {
        static void Main(string[] args)
        {
            // Mode CLI : EasySave.exe 1-3 ou 1;3
            if (args.Length > 0)
            {
                RunFromArgs(args[0]);
                return;
            }

            // Mode interactif
            Controller controller = new Controller();
        }

        /// <summary>
        /// Parse et exécute les jobs sélectionnés en CLI (1-3 ou 1;3)
        /// </summary>
        private static void RunFromArgs(string raw)
        {
            var model = new Model();
            var allJobs = LoadAllBackups(model);
            // Charge la liste de tous les jobs

            foreach (var idx in ParseSelection(raw))
            {
                if (idx >= 1 && idx <= allJobs.Count)
                {
                    var job = allJobs[idx - 1];
                    Console.WriteLine($"--- Exécution de la sauvegarde {idx}: {job.SaveName} ---");
                    model.LoadSave(job.SaveName);
                    Console.WriteLine($"[{job.SaveName}] terminé.");
                }
                else
                {
                    Console.WriteLine($"Indice {idx} hors plage (1–{allJobs.Count}).");
                }
            }
        }

        /// <summary>
        /// Transforme "1-3" en 1,2,3 ou "1;3;5" en 1,3,5
        /// </summary>
        private static IEnumerable<int> ParseSelection(string s)
        {
            s = s.Trim();
            if (s.Contains('-'))
            {
                var parts = s.Split('-', 2);
                if (int.TryParse(parts[0], out int start) &&
                    int.TryParse(parts[1], out int end) &&
                    end >= start)
                {
                    for (int i = start; i <= end; i++)
                        yield return i;
                }
            }
            else
            {
                foreach (var piece in s.Split(';'))
                {
                    if (int.TryParse(piece, out int num))
                        yield return num;
                }
            }
        }

        /// <summary>
        /// Charge tous les backups depuis un fichier JSON ou XML
        /// </summary>
        private static List<Backup> LoadAllBackups(Model model)
        {
            var jsonFile = model.BackupListFile;
            var xmlFile = jsonFile.Replace(".json", ".xml");

            if (File.Exists(jsonFile) && new FileInfo(jsonFile).Length > 0)
            {
                var json = File.ReadAllText(jsonFile);
                return JsonConvert.DeserializeObject<List<Backup>>(json) ?? new List<Backup>();
            }
            else if (File.Exists(xmlFile) && new FileInfo(xmlFile).Length > 0)
            {
                var serializer = new XmlSerializer(typeof(List<Backup>));
                using (var reader = new StreamReader(xmlFile))
                {
                    return (List<Backup>)serializer.Deserialize(reader);
                }
            }
            else
            {
                return new List<Backup>();
            }
        }
    }
}
