# Release note — EasySave v2.0.0

## Version concernée

EasySave v2.0.0

## Type de version

Version majeure.

Cette version introduit l’interface graphique WPF fondée sur une architecture MVVM, tout en conservant la compatibilité avec les acquis de la version console EasySave v1.1.

---

## Objectif de la version

EasySave v2.0 a pour objectif de rendre l’application plus ergonomique, plus extensible et plus conforme aux attentes fonctionnelles de la version graphique.

La version 2.0 permet notamment :

- de gérer un nombre illimité de travaux de sauvegarde ;
- d’exécuter un ou plusieurs travaux de sauvegarde depuis une interface graphique ;
- d’exécuter les travaux de manière séquentielle ;
- de suivre l’état d’exécution en temps réel ;
- de choisir la langue de l’interface ;
- de configurer les extensions à chiffrer ;
- d’intégrer CryptoSoft pour le chiffrement ;
- de bloquer l’exécution lorsqu’un logiciel métier est détecté.

---

## Nouveautés principales

### Interface graphique WPF

EasySave dispose désormais d’une interface graphique développée en WPF.

L’interface permet de :

- consulter la liste des travaux ;
- créer de nouveaux travaux ;
- exécuter un travail sélectionné ;
- exécuter tous les travaux de manière séquentielle ;
- consulter l’état d’exécution ;
- modifier les paramètres généraux.

---

### Architecture MVVM

La version 2.0 introduit une séparation claire entre :

- les vues WPF ;
- les ViewModels ;
- les services applicatifs ;
- le cœur métier ;
- les repositories ;
- les stratégies de sauvegarde.

Cette organisation prépare l’application à des évolutions futures.

---

### Projet EasySave.Core

Un nouveau projet `EasySave.Core` a été ajouté.

Il contient les éléments métier de la version 2.0 :

- modèles ;
- repositories ;
- services ;
- stratégies ;
- manager de sauvegarde.

Le projet graphique `EasySave.GUI` dépend désormais de `EasySave.Core`.

---

### Nombre de travaux illimité

La limite de cinq travaux de sauvegarde n’est plus appliquée dans la version graphique.

Chaque travail possède :

- un identifiant technique `Guid` ;
- un numéro visible auto-incrémenté ;
- un nom ;
- un chemin source ;
- un chemin cible ;
- un type de sauvegarde ;
- une date de dernière exécution.

Dans l’interface, les travaux sont affichés avec une numérotation lisible :

```text
01
02
03