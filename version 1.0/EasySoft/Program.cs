using EasySoft.controller;
using EasySoft.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            // Charge la liste de tous les jobs
            var json = File.ReadAllText(model.BackupListFile);
            var allJobs = JsonConvert.DeserializeObject<Backup[]>(json) ?? Array.Empty<Backup>();

            foreach (var idx in ParseSelection(raw))
            {
                if (idx >= 1 && idx <= allJobs.Length)
                {
                    var job = allJobs[idx - 1];
                    Console.WriteLine($"--- Exécution de la sauvegarde {idx}: {job.SaveName} ---");
                    model.LoadSave(job.SaveName);
                    Console.WriteLine($"[{job.SaveName}] terminé.");
                }
                else
                {
                    Console.WriteLine($"Indice {idx} hors plage (1–{allJobs.Length}).");
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
    }
}
