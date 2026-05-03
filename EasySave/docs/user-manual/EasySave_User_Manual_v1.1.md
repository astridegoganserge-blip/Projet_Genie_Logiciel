# EasySave v1.1 — Manuel utilisateur

## Présentation

EasySave est une application console permettant de créer et d’exécuter des travaux de sauvegarde complets ou différentiels.

La version 1.1 conserve le fonctionnement de la version 1.0 et ajoute le choix du format du fichier log journalier : JSON ou XML.

## Lancement

Depuis le dossier de l’application, lancer :

```bash
EasySave.exe
```

ou en développement :

```bash
dotnet run
```

Au démarrage, choisir la langue :

```text
1. Français
2. English
```

## Menu principal

```text
1. Créer un travail de sauvegarde
2. Exécuter un travail
3. Exécution séquentielle
4. Afficher les travaux configurés
5. Supprimer un travail
6. Paramètres
0. Quitter l'application
```

## Créer un travail de sauvegarde

Choisir l’option `1`, puis renseigner :

- l’identifiant du travail ;
- le nom de la sauvegarde ;
- le chemin source ;
- le chemin cible ;
- le type de sauvegarde.

Types disponibles :

```text
1 = sauvegarde complète
2 = sauvegarde différentielle
```

La sauvegarde complète copie tous les fichiers du répertoire source.  
La sauvegarde différentielle copie uniquement les fichiers nouveaux ou modifiés.

## Exécuter un travail

Choisir l’option `2`, puis saisir l’identifiant du travail à exécuter.

## Exécuter plusieurs travaux

Choisir l’option `3`, puis saisir une commande.

Exemples :

```text
1-3   exécute les travaux 1, 2 et 3
1;3   exécute les travaux 1 et 3
```

## Modifier le format du log

Choisir l’option `6`, puis sélectionner :

```text
1 = JSON
2 = XML
```

Le choix est sauvegardé dans `settings.json`.

## Fichiers générés

Les fichiers sont générés à côté de l’exécutable :

```text
jobs.config.json   configuration des travaux
settings.json      paramètres utilisateur
state.json         état temps réel
logs/              fichiers logs journaliers
```

Les logs journaliers sont créés au format choisi :

```text
AAAA-MM-JJ.json
AAAA-MM-JJ.xml
```

## Quitter l’application

Choisir l’option `0`.

## Remarque

EasySave v1.1 ne contient pas encore d’interface graphique.  
L’interface graphique est prévue pour EasySave v2.0.