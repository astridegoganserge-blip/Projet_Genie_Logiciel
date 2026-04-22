using System;
using System.IO;
using System.Linq;
using EasyLog;
using EasySave.Managers;
using EasySave.Models;
using EasySave.Services;

namespace EasySave.UI
{
    /// <summary>
    /// Interface utilisateur en mode console pour EasySave.
    /// Gère l'affichage du menu, la saisie utilisateur et l'appel aux fonctions du BackupManager.
    /// </summary>
    public class ConsoleInterface
    {
        // Gestionnaire des travaux de sauvegarde (création, exécution, suppression)
        private BackupManager _manager;

        // Logger utilisé pour enregistrer les opérations de sauvegarde
        private EasyLog.EasyLog _logger;

        /// <summary>
        /// Initialise le logger et le gestionnaire de sauvegardes.
        /// Le logger écrit ses fichiers dans le sous-dossier "logs" du répertoire d'exécution.
        /// </summary>
        public ConsoleInterface()
        {
            string logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            _logger = new EasyLog.EasyLog(logPath);
            _manager = new BackupManager();
        }

        /// <summary>
        /// Point d'entrée de l'interface.
        /// Si un argument de ligne de commande est fourni, exécute la commande directement.
        /// Sinon, affiche le menu interactif en boucle jusqu'à ce que l'utilisateur choisisse de quitter.
        /// </summary>
        /// <param name="args">Arguments transmis depuis Main (optionnel : commande d'exécution séquentielle).</param>
        public void Run(string[] args)
        {
            // Mode ligne de commande : exécution directe sans menu interactif
            if (args.Length > 0)
            {
                RunCommandLine(args[0]);
                return;
            }

            // Boucle principale du menu interactif
            while (true)
            {
                ShowMenu();
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": CreateJob(); break;
                    case "2": ExecuteJob(); break;
                    case "3": ExecuteSequential(); break;
                    case "4": ShowJobs(); break;
                    case "5": DeleteJob(); break;
                    case "0": return; // Quitter l'application
                    default:
                        Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                        Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Exécute une séquence de travaux en mode ligne de commande (sans menu).
        /// </summary>
        /// <param name="command">Commande transmise en argument au lancement (ex. : "1-3" ou "1;3").</param>
        private void RunCommandLine(string command)
        {
            _manager.ExecuteSequential(command, _logger);
        }

        /// <summary>
        /// Affiche le menu principal dans la console.
        /// Utilise les traductions chargées par le LanguageManager.
        /// </summary>
        private void ShowMenu()
        {
            Console.Clear();
            Console.WriteLine(LanguageManager.GetString("AppTitle"));
            Console.WriteLine(LanguageManager.GetString("MenuCreate"));
            Console.WriteLine(LanguageManager.GetString("MenuExecute"));
            Console.WriteLine(LanguageManager.GetString("MenuSequential"));
            Console.WriteLine(LanguageManager.GetString("MenuShow"));
            Console.WriteLine(LanguageManager.GetString("MenuDelete"));
            Console.WriteLine(LanguageManager.GetString("MenuExit"));
            Console.Write(LanguageManager.GetString("ChoicePrompt"));
        }

        /// <summary>
        /// Invite l'utilisateur à saisir les informations d'un nouveau travail de sauvegarde,
        /// puis l'ajoute via le BackupManager.
        /// Bloqué si le nombre maximum de 5 travaux est atteint, ou si l'ID est déjà utilisé.
        /// </summary>
        private void CreateJob()
        {
            // Vérifie que la limite de 5 travaux simultanés n'est pas dépassée
            if (_manager.Jobs.Count >= 5)
            {
                Console.WriteLine(LanguageManager.GetString("MaxJobsReached"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            // Saisie et validation de l'identifiant numérique du travail
            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            // Vérifie que l'ID n'est pas déjà attribué à un autre travail
            if (!_manager.IsJobIdAvailable(id))
            {
                Console.WriteLine(LanguageManager.GetString("IdUsed"));
                Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
                Console.ReadKey();
                return;
            }

            // Saisie des paramètres du travail
            Console.Write(LanguageManager.GetString("EnterName"));
            string name = Console.ReadLine();
            Console.Write(LanguageManager.GetString("EnterSource"));
            string source = Console.ReadLine();
            Console.Write(LanguageManager.GetString("EnterTarget"));
            string target = Console.ReadLine();

            // Saisie du type de sauvegarde : 1 = Complète (défaut), 2 = Différentielle
            Console.Write(LanguageManager.GetString("EnterType"));
            string typeChoice = Console.ReadLine();

            var job = new BackupJob
            {
                Id = id,
                Name = name,
                SourcePath = source,
                TargetPath = target,
                Type = typeChoice == "2" ? BackupType.Differential : BackupType.Complete
            };

            // Tente d'ajouter le travail et affiche le résultat
            if (_manager.AddJob(job))
                Console.WriteLine(LanguageManager.GetString("JobCreated"));
            else
                Console.WriteLine(LanguageManager.GetString("BackupFailed"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        /// <summary>
        /// Affiche la liste des travaux existants, puis demande à l'utilisateur
        /// d'en choisir un par ID pour l'exécuter individuellement.
        /// </summary>
        private void ExecuteJob()
        {
            ShowJobs();
            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            // Exécute le travail et affiche le résultat (succès ou échec)
            if (_manager.ExecuteJob(id, _logger))
                Console.WriteLine(LanguageManager.GetString("BackupCompleted"));
            else
                Console.WriteLine(LanguageManager.GetString("BackupFailed"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        /// <summary>
        /// Demande à l'utilisateur une commande de sélection multiple (ex. : "1-3" ou "1;2;4"),
        /// puis exécute les travaux correspondants de manière séquentielle.
        /// </summary>
        private void ExecuteSequential()
        {
            Console.Write(LanguageManager.GetString("MenuSequential"));
            Console.Write(" ");
            string command = Console.ReadLine();

            // Exécute la séquence et affiche le résultat global
            if (_manager.ExecuteSequential(command, _logger))
                Console.WriteLine(LanguageManager.GetString("SequenceCompleted"));
            else
                Console.WriteLine(LanguageManager.GetString("SequenceFailed"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        /// <summary>
        /// Récupère et affiche tous les travaux de sauvegarde enregistrés.
        /// </summary>
        private void ShowJobs()
        {
            var jobs = _manager.GetAllJobs();
            Console.WriteLine("\n--- " + LanguageManager.GetString("AppTitle") + " ---");

            // Affiche chaque travail via sa méthode ToString()
            foreach (var job in jobs)
            {
                Console.WriteLine(job.ToString());
            }

            Console.WriteLine("-------------------");
            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }

        /// <summary>
        /// Affiche la liste des travaux, puis invite l'utilisateur à en choisir un par ID
        /// pour le supprimer définitivement.
        /// </summary>
        private void DeleteJob()
        {
            ShowJobs();
            Console.Write(LanguageManager.GetString("EnterJobId"));
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine(LanguageManager.GetString("InvalidChoice"));
                Console.ReadKey();
                return;
            }

            // Tente la suppression et informe l'utilisateur du résultat
            if (_manager.RemoveJob(id))
                Console.WriteLine(LanguageManager.GetString("JobDeleted"));
            else
                Console.WriteLine(LanguageManager.GetString("JobNotFound"));

            Console.WriteLine(LanguageManager.GetString("PressAnyKey"));
            Console.ReadKey();
        }
    }
}