# Chapitre 14 - Documents et notifications

L'enonce detaille du chapitre demande un espace documentaire general, des traces d'acces et des notifications multicanal. Les anciens documents KYC restent geres par leur flux specifique; la table `Documents` couvre les autres depots classes.

## Documents KYC

- Le formulaire recoit un fichier en multipart et accepte PDF, PNG et JPEG, avec une limite de 5 Mo.
- L'extension autorisee et la signature du contenu sont controlees cote serveur. Le nom du fichier ne sert jamais de chemin.
- Chaque document recoit une cle GUID opaque. Les octets sont chiffres avec ASP.NET Data Protection et conserves hors de `wwwroot`, dans `App_Data/KycDocuments` ou dans `Storage:KycDocumentsPath`.
- Le telechargement passe par une action authentifiee reservee aux roles `admin`, `gestionnaire` et `guichetier`, qui controle la cle, choisit un type MIME autorise et renvoie `X-Content-Type-Options: nosniff`.
- Les fichiers KYC ne sont pas publies comme fichiers statiques. Les cles Data Protection et les fichiers restent dans le volume `akaranga_app_data` avec Docker.

## Notifications

- Les utilisateurs possedant un compte client associe recoivent une notification interne lors du depot, de la validation ou du rejet d'un justificatif.
- Les administrateurs actifs et les gestionnaires/guichetiers actifs de l'agence du client sont avertis quand un justificatif est depose. Le menu affiche le nombre de notifications non lues.
- La page `/Notifications` ne retourne que les notifications de l'utilisateur connecte. Le marquage comme lu est une requete POST avec jeton anti-CSRF.
- Les liens enregistres dans les notifications sont limites aux chemins locaux.
- Les templates sont identifies par code, canal, langue et version. Les variables injectees dans le HTML sont encodees.
- Les envois Email, SMS et Push sont mis en file dans Hangfire. Trois executions au total sont possibles; l'etat et l'erreur sont traces dans `NotificationsUtilisateurs`.
- Le hub SignalR est expose sur `/hubs/notifications`; les clients authentifies rejoignent leur groupe prive.
- Le rappel SMS d'echeance est planifie trois jours avant l'echeance. Les notifications partent uniquement si le canal est configure.

## Espace documentaire

- Categories : KYC, Contrat, Comptable, AML, RH et Audit. Formats : PDF, JPG, PNG, DOCX et XLSX; maximum 10 Mo.
- Les types Office sont verifies comme archives OOXML contenant leur document principal attendu. Chaque fichier est chiffre au repos, stocke hors de `wwwroot`, nomme par GUID, et son hash SHA-256 est controle avant telechargement.
- Les depots, telechargements, archivages et suppressions logiques sont inscrits dans `DocumentsAcces`. Une purge Hangfire retire du disque et de la base les elements marques Supprime au-dela de `Documents:DeletedRetentionDays` (90 jours par defaut).
- Les versions sont numerotees par client et categorie. Le telechargement est limite au client proprietaire ou au personnel de son agence; l'administrateur peut acceder a tous les documents.

## Configuration des services externes

Ne pas enregistrer les valeurs secretes dans `appsettings.json`. Configurer les cles suivantes dans les User Secrets en local, ou dans un gestionnaire de secrets en production : `Email:SmtpHost`, `Email:SmtpPort`, `Email:From`, `Email:SmtpUsername`, `Email:SmtpPassword`, `Twilio:AccountSid`, `Twilio:AuthToken`, `Twilio:From`, `Firebase:ServiceAccountPath`. Le compte de service Firebase doit rester hors du depot et etre monte en lecture seule. Le jeton FCM de l'utilisateur est stocke dans `ApplicationUser.PushToken`.

Les templates initiaux sont ajoutes au demarrage quand `Database:ApplyMigrationsOnStartup` vaut `true`. Langues incluses : fr-BI, rn-BI, en-US et sw-KE. Hangfire utilise la meme connexion SQL Server; le dashboard `/hangfire` est reserve aux administrateurs. Un client connecte peut enregistrer son jeton FCM via POST `/Notifications/PushToken` avec un jeton anti-CSRF.

## Base et exploitation

Appliquer les migrations avant de publier. En production, sauvegarder le volume `App_Data`: sa perte rendrait les fichiers et leurs cles de chiffrement indisponibles. Les fournisseurs sont inactifs tant que leurs configurations ne sont pas renseignees; les envois externes non configures echouent et sont journalises avec les tentatives.
