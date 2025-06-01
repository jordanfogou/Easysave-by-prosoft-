using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace CryptoSoft
{
    class Program
    {
        /// <summary>
        /// Usage : CryptoSoft.exe -in "<fichierEntrée>" -out "<fichierSortie>" -pwd "<motDePasse>"
        /// </summary>
        static int Main(string[] args)
        {
            // Si on n'a pas au moins 6 arguments, on affiche l'aide
            if (args.Length < 6)
            {
                Console.WriteLine("Usage: CryptoSoft.exe -in <fichierEntrée> -out <fichierSortie> -pwd <motDePasse>");
                return -1;
            }

            string inputFile = null;
            string outputFile = null;
            string password = null;

            // On parcourt les arguments deux par deux (clé / valeur)
            for (int i = 0; i < args.Length; i += 2)
            {
                string key = args[i].ToLowerInvariant();
                if (i + 1 >= args.Length)
                {
                    Console.WriteLine($"Argument manquant pour {key}");
                    return -2;
                }

                string value = args[i + 1];
                switch (key)
                {
                    case "-in":
                        inputFile = value;
                        break;
                    case "-out":
                        outputFile = value;
                        break;
                    case "-pwd":
                        password = value;
                        break;
                    default:
                        Console.WriteLine($"Argument inconnu : {key}");
                        return -3;
                }
            }

            // Vérification basique
            if (string.IsNullOrEmpty(inputFile)
                || string.IsNullOrEmpty(outputFile)
                || password == null)
            {
                Console.WriteLine("Arguments invalides. Assurez-vous de passer -in, -out et -pwd.");
                return -4;
            }

            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Fichier d’entrée introuvable : {inputFile}");
                return -5;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();

                // Ex. de sel fixe pour la dérivation de clé (en production on peut rendre ça paramétrable)
                byte[] salt = new byte[] { 0x21, 0x45, 0x72, 0x19, 0xEF, 0xAC, 0x34, 0xDD };

                // Dérivation de la clé et de l’IV à partir du mot de passe, via PBKDF2
                using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] key = deriveBytes.GetBytes(32); // 256 bits
                byte[] iv = deriveBytes.GetBytes(16); // 128 bits

                // Lecture du fichier source et chiffrement en AES
                using FileStream fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                using FileStream fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                using var aes = new AesManaged() { Key = key, IV = iv };
                using CryptoStream cryptoStream = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write);

                fsInput.CopyTo(cryptoStream);

                cryptoStream.FlushFinalBlock();
                stopwatch.Stop();

                long elapsedMs = stopwatch.ElapsedMilliseconds;

                // Affiche le temps (en ms) sur la sortie standard,
                // qu’EasySaveApp lira pour renseigner EncryptionTimeLog
                Console.WriteLine(elapsedMs);
                return 0;
            }
            catch (Exception ex)
            {
                // En cas d’erreur, on affiche “ERREUR:<message>” puis on renvoie un code < 0
                Console.WriteLine($"ERREUR:{ex.Message}");
                return -6;
            }
        }
    }
}
