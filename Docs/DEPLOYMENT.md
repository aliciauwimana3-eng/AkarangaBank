# Déploiement

## Configuration

Ne publiez pas les secrets de développement. En production, fournissez ces valeurs par variables d'environnement ou par le coffre de secrets de l'hébergeur :

- `ConnectionStrings__AkarangaBank` : chaîne SQL Server avec chiffrement TLS et validation du certificat.
- `AdminInitial__Email` et `AdminInitial__Password` : identité utilisée uniquement pour créer le premier administrateur si aucun n'existe.
- `ASPNETCORE_ENVIRONMENT=Production`.
- `ForwardedHeaders__KnownProxies__0` : adresse IP du proxy inverse de confiance (et `__1`, etc. s'il y en a plusieurs). Ne configurez pas de proxy non maîtrisé comme proxy de confiance.

Le compte SQL doit disposer des droits nécessaires pour appliquer les migrations pendant la phase de déploiement. Les migrations ne sont pas lancées au démarrage de l'application.

## Publication

1. Sauvegarder la base et vérifier le certificat TLS SQL Server.
2. Installer l'outil EF Core CLI si nécessaire avec `dotnet tool install --global dotnet-ef`, puis, depuis le dossier du projet, appliquer le schéma avec `dotnet ef database update`.
3. Publier avec `dotnet publish -c Release -o publish`.
4. Héberger derrière HTTPS et configurer le proxy inverse avec les en-têtes transmis uniquement depuis des proxys de confiance.
5. Vérifier `GET /health/live` et `GET /health/ready`. Le second renvoie `503` si la base n'est pas joignable.

Les cookies d'authentification sont `Secure` hors développement. Les API renvoient `401` et `403` au lieu de rediriger vers la page de connexion.

## Docker local

1. Copier `.env.example` vers `.env`, puis remplacer les deux mots de passe d'exemple par des valeurs locales uniques.
2. Démarrer l'environnement avec `docker compose up --build -d`.
3. Vérifier `http://localhost:8080/health/live` et `http://localhost:8080/health/ready`.
4. Arrêter avec `docker compose down`. Ajouter `-v` uniquement si la base locale doit aussi être supprimée.

Le service web attend que SQL Server réponde, puis applique les migrations uniquement parce que la configuration du compose local active `Database__ApplyMigrationsOnStartup`. Cette option reste désactivée par défaut dans l'application. En production, appliquer les migrations explicitement avant de publier.

## GitHub Actions

Le workflow `.github/workflows/ci.yml` restaure, compile, teste et publie les résultats de test pour les pull requests et les branches `main`/`develop`. Pour autoriser le déploiement Azure sur `main`, créer la variable `AZURE_WEBAPP_NAME` et le secret `AZURE_WEBAPP_PUBLISH_PROFILE` dans les paramètres du dépôt GitHub. Aucun secret n'est enregistré dans le dépôt.
