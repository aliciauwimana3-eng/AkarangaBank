# Akaranga Bank

Application pédagogique de banque commerciale développée avec ASP.NET Core MVC. Elle regroupe l'authentification et les rôles, les clients et contrôles KYC, les comptes et opérations comptabilisées en partie double, la conformité AML, les virements, les prêts, les cartes, les documents privés, les notifications et le reporting.

## Prérequis

- .NET SDK 10.0 (le projet cible `net10.0`).
- SQL Server 2022 ou une instance SQL Server compatible avec EF Core 10.
- Pour le déploiement conteneurisé : Docker Engine et Docker Compose.
- Pour les canaux externes : un compte SMTP, Twilio et Firebase; ces fournisseurs sont facultatifs pour le développement local.

## Configuration locale

Les mots de passe, chaînes de connexion et clés des fournisseurs doivent être fournis par User Secrets ou des variables d'environnement. Ne place pas ces valeurs dans `appsettings.json` et ne versionne pas `.env`.

Depuis la racine du projet :

```powershell
dotnet tool install --global dotnet-ef
dotnet user-secrets set "ConnectionStrings:AkarangaBank" "Server=localhost;Database=AkarangaBank;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
dotnet user-secrets set "AdminInitial:Email" "admin@akaranga.local"
dotnet user-secrets set "AdminInitial:Password" "Replace-With-A-Unique-Password-2026!"
dotnet ef database update
dotnet run
```

L'application crée les rôles manquants et l'agence siège sur une base initialisée, puis crée l'administrateur initial si ses deux paramètres sont définis. Une base neuve doit recevoir ses migrations avant le premier démarrage, sauf si `Database:ApplyMigrationsOnStartup=true` est activé. Les clés ASP.NET Data Protection et les fichiers applicatifs sont conservés dans `App_Data`; garde ce dossier persistant et protégé en production.

L'application est disponible sur l'adresse locale affichée par `dotnet run`. La route `/health/live` vérifie le processus et `/health/ready` vérifie la connexion SQL.

### Services externes facultatifs

- Email : `Email:SmtpHost`, `Email:SmtpPort`, `Email:SmtpUsername`, `Email:SmtpPassword`, `Email:From`.
- SMS : `Twilio:AccountSid`, `Twilio:AuthToken`, `Twilio:From`.
- Push : `Firebase:ServiceAccountPath` doit désigner un fichier de compte de service fourni hors du dépôt.

Sans ces paramètres, les fonctionnalités internes et les autres pages restent utilisables; l'envoi du canal correspondant nécessite sa configuration.

## Exécution avec Docker Compose

1. Copie `.env.example` vers `.env`.
2. Remplace les mots de passe d'exemple par des valeurs uniques et longues; `.env` est ignoré par Git.
3. Lance `docker compose up --build`.
4. Ouvre `http://localhost:8080`; Compose attend que SQL Server soit prêt et demande à l'application d'appliquer les migrations.

Les volumes Compose conservent SQL Server et `App_Data`. Le mot de passe SA ainsi que l'administrateur initial sont des paramètres de démonstration à remplacer avant tout déploiement.

## Structure du projet

| Dossier | Contenu |
|---|---|
| `Controllers`, `Views`, `ViewModels` | Parcours MVC et interfaces Razor |
| `Models`, `Data`, `Migrations` | Modèle métier, EF Core et historique du schéma |
| `Services` | Règles métier, KYC, conformité, documents, notifications, PDF et rapports |
| `Areas/Identity` | Pages d'authentification et de gestion d'identité |
| `Hubs` | Notifications internes SignalR |
| `Resources` | Ressources de localisation |
| `tests/AkarangaBank.Web.Tests` | Tests unitaires et tests d'intégration |
| `.github/workflows` | Compilation et pipeline CI |

## Technologies et sécurité

- ASP.NET Core MVC, Razor Pages, EF Core et SQL Server.
- ASP.NET Core Identity, rôles, TOTP et contrôle MFA sur les opérations sensibles.
- Partie double, journal d'audit applicatif, documents privés avec hash SHA-256.
- Virements inter-agences comptabilisés sur quatre lignes, avec comptes de compensation et rattachement de chaque écriture à son agence.
- Surveillance AML avec seuils par défaut de 5 M BIF (vigilance), 10 M BIF (déclaration) et 50 M BIF (blocage), ainsi que des détections de fractionnement, smurfing, layering et vélocité.
- QuestPDF, ClosedXML, CsvHelper, MailKit, Twilio, Firebase Admin, SignalR et Hangfire.
- Cultures disponibles : `fr-BI`, `rn-BI`, `en-US` et `sw-KE`.
- Limitation globale : 300 requêtes par minute et par adresse IP; les requêtes excédentaires reçoivent HTTP 429.
- En production, HTTPS et des secrets externes au dépôt sont requis. Configure `ForwardedHeaders:KnownProxies` uniquement avec les adresses de proxy de confiance.

## Tests

Le projet de tests est `tests/AkarangaBank.Web.Tests/AkarangaBank.Web.Tests.csproj`. Pour lancer la suite :

```powershell
dotnet test tests/AkarangaBank.Web.Tests/AkarangaBank.Web.Tests.csproj
```

Le pipeline GitHub Actions restaure, compile et exécute cette suite, puis vérifie une couverture minimale de 60 % des lignes à partir des rapports Cobertura. Il peut ensuite déployer sur Azure si les variables et secrets correspondants sont configurés.

## Périmètre et limites connues

- L'API mobile reste conceptuelle; aucun endpoint API REST public n'est livré.
- Les synthèses de résultat et bilan reposent sur les préfixes descriptifs présents dans les comptes (par exemple `CAISSE`, `PRETS`, `PRODUITS` et `DEPENSES`) et affichent les agrégats en BIF. Un plan comptable réglementaire complet et multi-devises reste à définir.
- Le document CNRF produit par l'application est un dossier préparatoire à valider; l'application ne le transmet pas à la CNRF.
- Les canaux SMTP, SMS et push dépendent de comptes et secrets fournisseur que l'équipe de déploiement doit fournir.

## Licence et auteurs

Projet de formation Akaranga Bank. Aucun fichier de licence ni nom d'auteur individuel n'a été fourni dans le dépôt; ces mentions sont à compléter par l'équipe propriétaire avant distribution.
