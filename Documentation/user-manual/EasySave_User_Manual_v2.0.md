# Manuel utilisateur — EasySave v2.0

## 1. Présentation

EasySave v2.0 est une application de sauvegarde dotée d’une interface graphique WPF.

Elle permet de :

- créer des travaux de sauvegarde ;
- exécuter un travail sélectionné ;
- exécuter plusieurs travaux de manière séquentielle ;
- suivre l’état d’avancement ;
- choisir la langue de l’interface ;
- choisir le format des logs ;
- configurer les extensions à chiffrer ;
- bloquer l’exécution si un logiciel métier est ouvert.

---

## 2. Lancer l’application

Depuis la racine du dépôt :

```bash
dotnet run --project EasySave.GUI/EasySave.GUI.csproj