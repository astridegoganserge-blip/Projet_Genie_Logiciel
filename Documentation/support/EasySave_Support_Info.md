# EasySave — Informations support technique

## Version concernée

EasySave v1.1.0

## Type d’application

EasySave v1.1 est une application console développée en C# avec .NET 10.0.

Le logiciel permet de créer et d’exécuter jusqu’à 5 travaux de sauvegarde en mode complet ou différentiel.

---

## Configuration minimale

- Système d’exploitation : Windows 10 ou supérieur
- Framework : .NET 10.0
- Droits nécessaires :
  - lecture sur le répertoire source ;
  - écriture sur le répertoire cible ;
  - écriture dans le répertoire d’exécution pour les fichiers de configuration, logs et état.

---

## Emplacement par défaut de l’application

L’application est exécutée depuis le dossier de build ou d’installation.

En environnement de développement :

```text
EasySave/bin/Debug/net10.0/
```

En environnement de livraison :

```text
EasySave/bin/Release/net10.0/
```

---

## Fichiers générés

Les fichiers générés sont placés à côté de l’exécutable, via `AppContext.BaseDirectory`.

---

## Fichier de configuration des travaux

```text
jobs.config.json
```

Ce fichier contient la liste des travaux de sauvegarde configurés :

- identifiant du travail ;
- nom de la sauvegarde ;
- chemin source ;
- chemin cible ;
- type de sauvegarde ;
- date de dernière exécution.

Exemple :

```json
[
  {
    "Id": 1,
    "Name": "Test_Backup",
    "SourcePath": "C:\\Users\\HP\\Desktop\\source_test",
    "TargetPath": "C:\\Users\\HP\\Desktop\\target_test",
    "Type": 0,
    "LastExecutionTime": "2026-05-01T13:22:00+02:00"
  }
]
```

> `Type = 0` : sauvegarde complète  
> `Type = 1` : sauvegarde différentielle

---

## Fichier de paramètres

```text
settings.json
```

Ce fichier contient les paramètres utilisateur :

- langue sélectionnée ;
- format de log choisi : JSON ou XML.

Exemple :

```json
{
  "LogFormat": 1,
  "Language": "fr"
}
```

> `LogFormat = 0` : JSON  
> `LogFormat = 1` : XML

---

## Fichier d’état temps réel

```text
state.json
```

Ce fichier contient l’état d’avancement du travail de sauvegarde.

Informations enregistrées :

- nom du travail ;
- horodatage de la dernière action ;
- statut du travail ;
- nombre total de fichiers ;
- taille totale des fichiers ;
- nombre de fichiers restants ;
- taille restante ;
- fichier source en cours ;
- fichier cible en cours ;
- progression.

Exemple :

```json
{
  "BackupName": "Test_Backup",
  "LastActionTime": "2026-05-01T13:22:00+02:00",
  "Status": "Terminé",
  "TotalFiles": 1,
  "TotalSize": 14,
  "RemainingFiles": 0,
  "RemainingSize": 0,
  "CurrentSourceFile": "",
  "CurrentTargetFile": "",
  "Progression": 100
}
```

Les statuts utilisés sont :

```text
Actif
Terminé
Erreur
```

---

## Fichiers logs journaliers

Les logs sont stockés dans le dossier :

```text
logs/
```

Les fichiers sont générés avec la date du jour.

Formats possibles :

```text
AAAA-MM-JJ.json
AAAA-MM-JJ.xml
```

Chaque entrée de log contient :

- horodatage ;
- nom du travail ;
- chemin complet du fichier source ;
- chemin complet du fichier cible ;
- taille du fichier ;
- temps de transfert en millisecondes ;
- temps de chiffrement avec `EncryptionTimeMs`.

Exemple JSON :

```json
[
  {
    "Timestamp": "2026-05-01T13:22:00+02:00",
    "BackupName": "Test_Backup",
    "SourceFile": "C:\\Users\\HP\\Desktop\\source_test\\test.txt",
    "TargetFile": "C:\\Users\\HP\\Desktop\\target_test\\test.txt",
    "FileSize": 14,
    "TransferTimeMs": 1,
    "EncryptionTimeMs": 0
  }
]
```

Exemple XML :

```xml
<?xml version="1.0" encoding="utf-16"?>
<ArrayOfLogEntry>
  <LogEntry>
    <Timestamp>2026-05-01T13:22:00+02:00</Timestamp>
    <BackupName>Test_Backup</BackupName>
    <SourceFile>C:\Users\HP\Desktop\source_test\test.txt</SourceFile>
    <TargetFile>C:\Users\HP\Desktop\target_test\test.txt</TargetFile>
    <FileSize>14</FileSize>
    <TransferTimeMs>1</TransferTimeMs>
    <EncryptionTimeMs>0</EncryptionTimeMs>
  </LogEntry>
</ArrayOfLogEntry>
```

Pour EasySave v1.1, `EncryptionTimeMs` vaut `0`, car le chiffrement est prévu pour une version future.

---

## Codes d’interprétation

### TransferTimeMs

```text
>= 0 : transfert réussi
< 0  : erreur de transfert
```

### EncryptionTimeMs

```text
0   : aucun chiffrement
> 0 : durée du chiffrement en millisecondes
< 0 : erreur de chiffrement
```

---

## Ligne de commande

EasySave peut être exécuté avec un argument pour lancer automatiquement un ou plusieurs travaux.

Exemples :

```bash
EasySave.exe 1
EasySave.exe 1-3
EasySave.exe "1;3"
```

Interprétation :

```text
1     : exécute uniquement le travail n°1
1-3   : exécute les travaux 1, 2 et 3
1;3   : exécute les travaux 1 et 3
```

---

## Emplacement des fichiers de langue

Les fichiers de langue sont stockés dans :

```text
languages/
```

Fichiers disponibles :

```text
fr.json
en.json
```

Ils sont copiés automatiquement dans le dossier de sortie grâce au fichier `EasySave.csproj`.

---

## Problèmes fréquents

### Le répertoire source est introuvable

Cause probable :

- le chemin source n’existe pas ;
- l’utilisateur n’a pas les droits de lecture ;
- le lecteur réseau n’est pas connecté.

Action recommandée :

- vérifier le chemin source ;
- vérifier les droits d’accès ;
- tester l’ouverture du dossier manuellement.

---

### Le répertoire cible est inaccessible

Cause probable :

- le chemin cible est incorrect ;
- l’utilisateur n’a pas les droits d’écriture ;
- le lecteur cible est indisponible.

Action recommandée :

- vérifier le chemin cible ;
- vérifier les droits d’écriture ;
- créer le dossier cible si nécessaire.

---

### Aucun fichier log n’est généré

Cause probable :

- aucun travail n’a encore été exécuté ;
- l’application n’a pas les droits d’écriture ;
- le dossier `logs/` n’a pas pu être créé.

Action recommandée :

- exécuter un travail de sauvegarde ;
- vérifier les droits d’écriture dans le dossier d’exécution ;
- vérifier la présence du dossier `logs/`.

---

### Les libellés apparaissent entre crochets

Exemple :

```text
[AppTitle]
[MenuCreate]
[ChoicePrompt]
```

Cause probable :

- les fichiers de langue ne sont pas chargés ;
- le dossier `languages/` est absent du dossier d’exécution ;
- les clés attendues sont absentes dans `fr.json` ou `en.json`.

Action recommandée :

Vérifier la présence des fichiers suivants dans le dossier d’exécution :

```text
languages/fr.json
languages/en.json
```

---

### Le format XML n’est pas pris en compte

Cause probable :

- `settings.json` n’est pas correctement enregistré ;
- l’application utilise encore une ancienne instance du logger.

Action recommandée :

- vérifier le contenu de `settings.json` ;
- vérifier que `LogFormat` vaut `1` pour XML ;
- relancer un travail de sauvegarde après modification du paramètre.

Exemple attendu :

```json
{
  "LogFormat": 1,
  "Language": "fr"
}
```

---

## Architecture supportée

EasySave v1.1 est organisé selon une architecture console MVC.

```text
Program.cs
    ↓
ConsoleApplicationController
    ↓
Views + Controllers
    ↓
Repositories + Strategies + Services
```

Rôle des couches :

- `Program.cs` : point d’entrée léger de l’application ;
- `ConsoleApplicationController` : orchestration du flux console ;
- `Views` : affichage et saisies console ;
- `Controllers` : coordination des actions utilisateur ;
- `Repositories` : persistance JSON des données ;
- `Services` : services techniques ;
- `Strategies` : algorithmes de sauvegarde complète et différentielle ;
- `EasyLog` : bibliothèque séparée pour la journalisation.

---

## Choix technique .NET 10

Le cahier des charges mentionne .NET 8.0 comme socle attendu.

Le projet est construit avec .NET 10.0 afin d’utiliser un environnement plus récent, tout en conservant les principes attendus :

- architecture console MVC pour EasySave 1.x ;
- bibliothèque EasyLog séparée ;
- logs JSON/XML ;
- fichier d’état temps réel ;
- architecture préparée pour la future version graphique MVVM.

---

## Contact support

En cas d’incident, fournir au support technique :

- la version d’EasySave utilisée ;
- le contenu de `settings.json` ;
- le contenu de `jobs.config.json` ;
- le dernier fichier de log dans `logs/` ;
- le fichier `state.json` ;
- une description du scénario ayant provoqué l’erreur.