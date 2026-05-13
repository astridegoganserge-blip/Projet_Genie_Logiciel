# EasySave v3.0 — Informations support technique

## 1. Version concernée

EasySave v3.0.0

## 2. Type d'application

EasySave v3.0 est une application de sauvegarde multi-tâches concurrente développée en C# avec .NET 10.0.

Elle contient :

- une interface graphique WPF ;
- une architecture MVVM ;
- un cœur métier `EasySave.Core` (parallélisme, priorités, throttling) ;
- une bibliothèque de journalisation `EasyLog` ;
- une intégration externe `CryptoSoft` mono-instance ;
- un service de centralisation `EasySave.LogServer` containerisé.

---

## 3. Projets principaux

```text
Projet_Genie_Logiciel/
├── EasySave/              (application console v1.1)
├── EasySave.Core/         (cœur métier partagé)
├── EasySave.GUI/          (interface WPF v3.0)
├── EasySave.LogServer/    (service Docker de centralisation)
├── EasyLog/               (DLL de journalisation)
└── CryptoSoft/            (outil de chiffrement mono-instance)
```

---

## 4. Configuration minimale

| Élément | Valeur |
|---|---|
| Système d'exploitation | Windows 10 64-bit ou supérieur |
| Runtime | .NET 10.0 Desktop |
| Mémoire vive | 200 Mo libres minimum |
| Espace disque | 200 Mo hors données utilisateur |
| Droits | Lecture sur les sources, écriture sur les cibles, exécution de `CryptoSoft.exe` |
| Docker (optionnel) | Docker Desktop ou Docker Engine pour la centralisation des logs |

---

## 5. Emplacement par défaut

- Exécutable : `EasySave.GUI.exe`
- Dossier de travail : dossier de l'exécutable
- CryptoSoft : sous-dossier `CryptoSoft/` à côté de l'exécutable (copié automatiquement en sortie de build)
- Logs locaux : sous-dossier `logs/` à côté de l'exécutable
- Logs centralisés (si activé) : volume Docker `centralized-logs` du container `easysave-logserver`

---

## 6. Fichiers de configuration et de logs

Tous les fichiers d'application sont au format JSON et placés dans le dossier d'exécution.

| Fichier | Rôle |
|---|---|
| `jobs.config.json` | Liste des travaux (Guid, numéro, nom, chemins, type, dernière exécution) |
| `settings.json` | Paramètres globaux (langue, format log, chiffrement, priorités, taille max, logiciel métier, mode log, URL Docker) |
| `state.json` | État temps réel des travaux |
| `logs/yyyy-MM-dd.json` ou `.xml` | Historique journalier des transferts |
| `crash.log` | Trace d'exception non gérée (créé seulement en cas d'incident) |

---

## 7. Démarrage du service de centralisation Docker

À la racine du dépôt :

```bash
docker compose up -d
```

Le service écoute sur le port `5000`. URL à renseigner côté EasySave : `http://<adresse-serveur>:5000`.

Vérification :

```bash
curl http://localhost:5000/
```

Réponse attendue : `{"service":"EasySave Log Server","status":"Running"}`.

---

## 8. Dépannage rapide

| Symptôme | Vérification |
|---|---|
| Aucun travail listé | Vérifier la présence et la lisibilité de `jobs.config.json`. |
| Sauvegarde refusée au lancement | Vérifier qu'aucun logiciel métier configuré n'est en cours d'exécution. |
| Chiffrement échoue (`EncryptionTimeMs < 0` dans le log) | Vérifier la présence de `CryptoSoft.exe` dans le sous-dossier `CryptoSoft/`. |
| Chiffrement bloqué pendant 30 secondes | Une autre instance CryptoSoft est en cours. Comportement attendu (mono-instance). |
| Logs Docker manquants en mode `Both` | Vérifier l'URL serveur dans les paramètres et que le container `easysave-logserver` tourne (`docker ps`). |
| Le watcher du logiciel métier ne réagit plus | Le nom du processus a été changé dans les paramètres. Redémarrer l'application. |
| `state.json` paraît bloqué | Vérifier `crash.log`. Vérifier l'absence de fichier `state.json.tmp` ou `state.json.corrupted`. |
| Application figée pendant un gros fichier | Comportement attendu : un seul fichier > seuil à la fois. Les autres travaux continuent les petits fichiers. |

---

## 9. Contrat de maintenance

Contrat annuel à tacite reconduction, support 5/7 de 8h à 17h, mises à jour incluses.

Tarif : 12 % du prix d'achat unitaire (200 € HT), revalorisé annuellement sur la base de l'indice SYNTEC.

---

## 10. Contact

Support technique ProSoft — équipe EasySave.
