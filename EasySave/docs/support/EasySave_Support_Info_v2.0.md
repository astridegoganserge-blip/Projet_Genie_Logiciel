# EasySave v2.0 — Informations support technique

## 1. Version concernée

EasySave v2.0.0

## 2. Type d’application

EasySave v2.0 est une application de sauvegarde développée en C# avec .NET 10.0.

Elle contient :

- une interface graphique WPF ;
- une architecture MVVM ;
- un cœur métier séparé `EasySave.Core` ;
- une bibliothèque de journalisation `EasyLog` ;
- une intégration externe de `CryptoSoft`.

---

## 3. Projets principaux

```text
Projet_Genie_Logiciel/
├── EasySave/
├── EasySave.Core/
├── EasySave.GUI/
├── EasyLog/
└── CryptoSoft/