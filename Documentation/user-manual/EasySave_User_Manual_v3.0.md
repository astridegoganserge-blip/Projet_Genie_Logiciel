# EasySave v3.0 — Manuel utilisateur

## Lancement

Double-cliquer sur **EasySave.GUI.exe**. La langue de l'interface (français / anglais) peut être changée à tout moment dans le menu **Paramètres**.

## Créer un travail de sauvegarde

1. Cliquer sur **Créer un job**.
2. Renseigner : nom du travail, dossier source, dossier cible, type (**Complète** ou **Différentielle**).
3. Cliquer sur **Enregistrer**.

## Exécuter des travaux

- **Un seul travail** : bouton ⚡ sur la ligne du travail.
- **Tous les travaux en parallèle** : bouton **Exécuter tout**.
- **Suivi en temps réel** : barre de progression et statut affichés dans la grille.

## Contrôler une exécution

Boutons disponibles par travail et globalement :

- ⏸ **Pause** — la pause s'applique après le fichier en cours ;
- ▶ **Reprendre** — relance un travail en pause ;
- ■ **Stop** — interrompt immédiatement le travail.

Les travaux passent automatiquement en pause lorsque le logiciel métier configuré démarre, et reprennent automatiquement à son arrêt.

## Paramètres principaux

| Paramètre | Effet |
|---|---|
| Langue | Français / anglais (changement immédiat). |
| Format de log | JSON ou XML. |
| Extensions à chiffrer | Les fichiers correspondants sont chiffrés via CryptoSoft. |
| Extensions prioritaires | Fichiers transférés en priorité (barrière globale entre les travaux). |
| Taille max fichier (Ko) | Au-delà, un seul fichier à la fois sur l'ensemble des travaux. |
| Logiciel métier | Nom du processus qui met en pause toutes les sauvegardes. |
| Mode log | Local / Docker / Les deux. |
| URL serveur Docker | URL du service de centralisation. |

## Fichiers générés (à côté de l'exécutable)

- `jobs.config.json` — liste des travaux ;
- `settings.json` — paramètres utilisateur ;
- `state.json` — état temps réel des travaux ;
- `logs/yyyy-MM-dd.json` ou `.xml` — historique journalier des transferts.

## Support technique

Voir **Documentation/support/EasySave_Support_Info_v3.0.md** ou contacter votre maintenance ProSoft (contrat 5/7, 8h–17h).
