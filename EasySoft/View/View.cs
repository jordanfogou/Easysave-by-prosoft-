using System;
using System.Linq;

namespace EasySoft.view
{
    public enum Language
    {
        English = 1,
        French = 2
    }

    class View
    {
        // Langue sélectionnée
        public Language SelectedLanguage { get; private set; } = Language.English;

        /// <summary>
        /// Selection de la langue au démarrage
        /// </summary>
        public void ShowLanguageSelection()
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("In which language would you like to continue? / Dans quelle langue souhaitez-vous continuer ?");
            Console.WriteLine("1. English");
            Console.WriteLine("2. Français");
            Console.Write("Choice (1/2) : ");
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "1") { SelectedLanguage = Language.English; break; }
                if (input == "2") { SelectedLanguage = Language.French; break; }
                Console.Write("Please enter 1 or 2 / Veuillez saisir 1 ou 2 : ");
            }
            Console.Clear();
        }

        public void ShowStart()
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("|||                   EasySave v1.0                    |||");
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("     _____    _    ______   ______    ___     _______ ");
            Console.WriteLine("    | ____|  / \\  / ___\\ \\ / / ___|  / \\ \\   / / ____|");
            Console.WriteLine("    |  _|   / _ \\ \\___ \\\\ V /\\___ \\ / _ \\ \\ / /|  _|  ");
            Console.WriteLine("    | |___ / ___ \\ ___) || |  ___) / ___ \\ V / | |___ ");
            Console.WriteLine("    |_____/_/   \\_\\____/ |_| |____/_/   \\_\\_/  |_____|");
            Console.WriteLine("                                                      ");
            Console.WriteLine("----------------------------------------------------------");
        }

        public void ShowMenu()
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("|||                          Menu                      |||");
            Console.WriteLine("----------------------------------------------------------");
            if (SelectedLanguage == Language.English)
            {
                Console.WriteLine("|||  0. Exit                                         |||");
                Console.WriteLine("|||  1. Open a backup job                            |||");
                Console.WriteLine("|||  2. Create a backup job                          |||");
                Console.WriteLine("|||  3. List backups                                 |||");
                Console.Write("Enter menu number: ");
            }
            else
            {
                Console.WriteLine("|||  0. Quitter                                      |||");
                Console.WriteLine("|||  1. Ouvrir une sauvegarde                        |||");
                Console.WriteLine("|||  2. Créer une sauvegarde                         |||");
                Console.WriteLine("|||  3. Lister les sauvegardes                       |||");
                Console.Write("Entrez le numéro du menu : ");
            }
        }

        public void ShowSubMenu()
        {
            Console.WriteLine("----------------------------------------------------------");
            if (SelectedLanguage == Language.English)
            {
                Console.WriteLine("|||  0. Exit                                         |||");
                Console.WriteLine("|||  1. Complete Save                                 |||");
                Console.WriteLine("|||  2. Differential Save                             |||");
                Console.Write("Enter choice: ");
            }
            else
            {
                Console.WriteLine("|||  0. Quitter                                      |||");
                Console.WriteLine("|||  1. Sauvegarde complète                          |||");
                Console.WriteLine("|||  2. Sauvegarde différentielle                    |||");
                Console.Write("Entrez votre choix : ");
            }
        }

        public void ShowListHeader()
        {
            Console.WriteLine("----------------------------------------------------------");
            if (SelectedLanguage == Language.English)
                Console.WriteLine("|||              List of all backups                 |||");
            else
                Console.WriteLine("|||             Liste de toutes les sauvegardes      |||");
            Console.WriteLine("----------------------------------------------------------");
        }

        public void ShowFilterMenu()
        {
            Console.WriteLine("----------------------------------------------------------");
            if (SelectedLanguage == Language.English)
            {
                Console.WriteLine("|||  0. No filter – All backups                       |||");
                Console.WriteLine("|||  1. Full saves – Complete                          |||");
                Console.WriteLine("|||  2. Differential saves                              |||");
                Console.Write("Filter by type (enter number): ");
            }
            else
            {
                Console.WriteLine("|||  0. Aucun filtre – Toutes les sauvegardes         |||");
                Console.WriteLine("|||  1. Sauvegardes complètes                        |||");
                Console.WriteLine("|||  2. Sauvegardes différentielles                  |||");
                Console.Write("Filtrer par type (numéro): ");
            }
        }

        public void ShowName()
        {
            if (SelectedLanguage == Language.English)
                Console.WriteLine("------------------------------------------------\nPlease enter the name of your backup:");
            else
                Console.WriteLine("------------------------------------------------\nEntrez le nom de votre sauvegarde :");
        }

        public void ShowResource()
        {
            if (SelectedLanguage == Language.English)
            {
                Console.WriteLine("------------------------------------------------\nPlease enter the path of the folder to back up:");
                Console.WriteLine("You can drag and drop the folder into the console.");
            }
            else
            {
                Console.WriteLine("------------------------------------------------\nEntrez le chemin du dossier à sauvegarder :");
                Console.WriteLine("Vous pouvez glisser–déposer le dossier dans la console.");
            }
        }

        public void ShowTargetResource()
        {
            if (SelectedLanguage == Language.English)
                Console.WriteLine("------------------------------------------------\nPlease enter the destination path for the backup:");
            else
                Console.WriteLine("------------------------------------------------\nEntrez le chemin de destination de la sauvegarde :");
        }

        public void ShowMirrorResource()
        {
            if (SelectedLanguage == Language.English)
                Console.WriteLine("------------------------------------------------\nPlease enter the path of the last full backup (for differential):");
            else
                Console.WriteLine("------------------------------------------------\nEntrez le chemin de la dernière sauvegarde complète (pour la différentielle) :");
        }

        public void ErrorMenu(string ErrorReturn)
        {
            Console.WriteLine(ErrorReturn);
        }

        public void ShowFile()
        {
            if (SelectedLanguage == Language.English)
                Console.Write("------------------------------------------------\nEnter the name of the backup to open: ");
            else
                Console.Write("------------------------------------------------\nEntrez le nom de la sauvegarde à ouvrir : ");
        }

        public void ShowNameFile()
        {
            Console.Clear();
            if (SelectedLanguage == Language.English)
                Console.WriteLine("------------------------------------------------\nAvailable backups:\n");
            else
                Console.WriteLine("------------------------------------------------\nSauvegardes disponibles :\n");
        }
    }
}
