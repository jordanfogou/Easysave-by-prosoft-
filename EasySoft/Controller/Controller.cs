using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasySoft.view;
using EasySoft.model;
using Newtonsoft.Json;

namespace EasySoft.controller
{
    class Controller
    {
        private readonly Model model;
        private readonly View view;
        private int InputMenu;

        public Controller()
        {
            model = new Model();
            view = new View();

            // 1) Sélection de la langue
            view.ShowLanguageSelection();

            // 2) Affichage du bandeau de démarrage
            view.ShowStart();

            // 3) Boucle principale
            FirstMenu();
        }

        private void FirstMenu()
        {
            while (true)
            {
                model.CheckDataFile();

                // Affiche le menu
                view.ShowMenu();
                if (!int.TryParse(Console.ReadLine(), out InputMenu))
                {
                    Console.Clear();
                    continue;
                }
                Console.Clear();

                switch (InputMenu)
                {
                    case 0:
                        Environment.Exit(0);
                        break;

                    case 1:
                        OpenBackup();
                        break;

                    case 2:
                        CreateBackup();
                        break;

                    case 3:
                        ListBackups();
                        break;

                    default:
                        // touche invalide
                        break;
                }
            }
        }

        private void OpenBackup()
        {
            // ==== OUVRIR UNE SAUVEGARDE EXISTANTE ====
            view.ShowNameFile();
            var list = LoadAllBackups();

            // Affiche tous les noms
            foreach (var job in list)
                Console.WriteLine(" -- " + job.SaveName);

            // Boucle de saisie jusqu'à nom valide ou "0"
            while (true)
            {
                view.ShowFile();
                var chosen = Console.ReadLine() ?? "";

                if (chosen == "0")
                {
                    // Retour au menu principal
                    Console.Clear();
                    return;
                }

                // Recherche case-insensitive
                var job = list.FirstOrDefault(b =>
                    string.Equals(b.SaveName, chosen, StringComparison.OrdinalIgnoreCase));

                if (job == null)
                {
                    // Message d’erreur
                    view.ErrorMenu(view.SelectedLanguage == Language.English
                        ? "Backup not found. Please try again or enter 0 to go back."
                        : "Sauvegarde introuvable. Réessayez ou tapez 0 pour revenir.");
                    continue;
                }

                // Nom valide → exécution
                model.LoadSave(job.SaveName);

                // Affichage du message de succès
                if (view.SelectedLanguage == Language.English)
                    Console.WriteLine("\n Execution completed successfully! Press ENTER to return to main menu.");
                else
                    Console.WriteLine("\n Exécution réussie ! Appuyez sur ENTRÉE pour revenir au menu principal.");
                Console.ReadLine();
                Console.Clear();
                return;
            }
        }

        private void CreateBackup()
        {
            // ==== CRÉER UNE NOUVELLE SAUVEGARDE ====
            if (model.CheckDataBackup >= 5)
            {
                view.ErrorMenu(view.SelectedLanguage == Language.English
                    ? "You already have 5 backups."
                    : "Vous avez déjà 5 sauvegardes.");
                return;
            }

            view.ShowSubMenu();
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice is < 1 or > 2)
            {
                Console.Clear();
                return;
            }

            // Demander le format de sauvegarde
            Console.WriteLine(view.SelectedLanguage == Language.English
                ? "Choose backup format: 1) JSON  2) XML"
                : "Choisissez le format de sauvegarde : 1) JSON  2) XML");
            string formatInput = Console.ReadLine() ?? "1";
            string format = (formatInput == "2") ? "xml" : "json";

            // Nom
            view.ShowName();
            model.SaveName = Console.ReadLine() ?? "";

            // Source
            view.ShowResource();
            model.Resource = GetResourceInput();

            // Miroir, si différentielle
            if (choice == 2)
            {
                view.ShowMirrorResource();
                model.MirrorResource = GetMirrorResource();
            }

            // Cible
            view.ShowTargetResource();
            model.Targetresource = GetTargetResource();

            var type = choice == 1 ? "full" : "differential";
            var backup = new Backup(
                model.SaveName,
                model.Resource,
                model.Targetresource,
                type,
                model.MirrorResource
            );

            // Appel de la méthode AddSave avec le format choisi
            try
            {
                model.AddSave(backup, format);
                Console.WriteLine(view.SelectedLanguage == Language.English
                    ? "Backup added successfully!"
                    : "Sauvegarde ajoutée avec succès !");
            }
            catch (InvalidOperationException ex)
            {
                view.ErrorMenu(ex.Message);
                return;
            }
        }
                        
        private void ListBackups()
        {
            // ==== LISTER & FILTRER LES SAUVEGARDES ====
            view.ShowListHeader();

            var allJson = File.Exists(model.BackupListFile)
                ? File.ReadAllText(model.BackupListFile)
                : "";
            var allJobs = !string.IsNullOrWhiteSpace(allJson)
                ? JsonConvert.DeserializeObject<Backup[]>(allJson)!
                : Array.Empty<Backup>();

            view.ShowFilterMenu();
            if (!int.TryParse(Console.ReadLine(), out int filter))
                filter = 0;

            IEnumerable<Backup> toShow = filter switch
            {
                1 => allJobs.Where(b => b.Type == "full"),
                2 => allJobs.Where(b => b.Type == "differential"),
                _ => allJobs
            };

            Console.WriteLine();
            foreach (var job in toShow)
            {
                Console.WriteLine(
                    $"- {job.SaveName} [{job.Type}]  " +
                    $"Src: {job.ResourceBackup}  Dest: {job.TargetBackup}");
            }

            Console.WriteLine();
            Console.WriteLine(view.SelectedLanguage == Language.English
                ? "Press ENTER to return to main menu"
                : "Appuyez sur ENTRÉE pour revenir au menu principal");
            Console.ReadLine();
            Console.Clear();
        }

        private void SubMenu()
        {
            // (Cette méthode n'est plus utilisée directement; 
            //  CreateBackup() gère tout)
        }

        private string GetResourceInput()
        {
            while (true)
            {
                var input = Console.ReadLine() ?? "";
                var path = input.Replace("\"", "");
                if (Directory.Exists(path))
                    return path;

                view.ErrorMenu(view.SelectedLanguage == Language.English
                    ? "Incorrect Path."
                    : "Chemin incorrect.");
            }
        }

        private string GetTargetResource()
        {
            while (true)
            {
                var input = Console.ReadLine() ?? "";
                var path = input.Replace("\"", "");
                if (Directory.Exists(path))
                    return path;

                view.ErrorMenu(view.SelectedLanguage == Language.English
                    ? "Incorrect Path."
                    : "Chemin incorrect.");
            }
        }

        private string GetMirrorResource()
        {
            while (true)
            {
                var input = Console.ReadLine() ?? "";
                var path = input.Replace("\"", "");
                if (Directory.Exists(path))
                    return path;

                view.ErrorMenu(view.SelectedLanguage == Language.English
                    ? "Incorrect Path."
                    : "Chemin incorrect.");
            }
        }
        private List<Backup> LoadAllBackups() 
        {
            // Vérifie si le fichier de sauvegarde existe
            if (!File.Exists(model.BackupListFile))
                return new List<Backup>();

            // Lit le contenu du fichier
            var fileContent = File.ReadAllText(model.BackupListFile);

            // Désérialise le contenu JSON en une liste de sauvegardes
            return JsonConvert.DeserializeObject<List<Backup>>(fileContent) ?? new List<Backup>();
        }
    }
}
