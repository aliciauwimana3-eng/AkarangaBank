# Journal des modifications

Les changements notables sont consignés ici. Le dépôt est en cours de réalisation pédagogique; les chapitres indiquent les fonctionnalités intégrées au projet.

## [0.16.0] — Projet final

- Ajout du reporting et de la génération des documents du chapitre 15 : tableaux, exports PDF/Excel/CSV et journalisation des exports.
- Ajout du tableau de bord final et des filtres et exports comptables, AML et prêts.
- Ajout des documents de remise : README, guide Docker, sécurité et périmètre fonctionnel.
- Ajout d'une limitation globale de débit HTTP et renforcement de la longueur minimale des mots de passe.
- Initialisation automatique de l'agence siège lorsque la base ne contient encore aucune agence.
- Ajout du seuil AML critique configurable à 50 M BIF et de règles de détection smurfing/layering.
- Ajout des quatre écritures et du suivi d'agence pour les virements internes inter-agences (migration `Chapitre16AgenceEcritures`).
- Ajout dans la CI d'un seuil minimal de couverture de lignes de 60 %.

## [0.14.0] — Documents et notifications

- Ajout du stockage privé KYC, du suivi d'accès, des modèles de notification et des canaux email, SMS, push et internes.
- Ajout des traitements planifiés des notifications et de purge documentaire.

## [0.12.0] — Prêts et cartes

- Ajout du cycle de prêt, des échéanciers, des remboursements et du suivi des retards.
- Ajout de l'émission et du contrôle des cartes et de leurs transactions.

## [0.08.0] — Comptabilité et conformité

- Ajout des écritures en partie double, des contrôles comptables, des alertes AML et de la préparation des déclarations.
- Ajout des virements et de leur contrôle de conformité.

## [0.01.0] — Fondations

- Mise en place de l'application ASP.NET Core MVC, d'EF Core, d'Identity, des clients, des comptes et des opérations bancaires.
