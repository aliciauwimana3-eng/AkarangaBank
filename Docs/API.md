# API REST v1

Les routes sont protégées par la session ASP.NET Identity de l'application. Il faut d'abord se connecter via l'écran de connexion Web et envoyer le cookie d'authentification avec chaque requête. Une route API non authentifiée renvoie `401`; un utilisateur sans autorisation reçoit `403`.

Base path : `/api/v1`

| Méthode | Route | Résultat |
|---|---|---|
| GET | `/comptes` | Comptes actifs visibles par l'utilisateur connecté |
| GET | `/comptes/{id}/solde` | Solde du compte si l'utilisateur y a accès |
| GET | `/comptes/{id}/releve` | Solde et écritures du relevé autorisé |
| GET | `/virements` | Virements visibles par l'utilisateur connecté |
| POST | `/virements` | Crée une demande de virement à valider |

Exemple de demande de virement interne :

```json
{
  "compteSourceId": 12,
  "compteDestinationId": 18,
  "estInterbancaire": false,
  "montant": 25000,
  "motif": "Facture"
}
```

Exemple de demande interbancaire :

```json
{
  "compteSourceId": 12,
  "estInterbancaire": true,
  "banqueDestination": "Banque destinataire",
  "numeroCompteExterne": "BI00BANK00012345",
  "beneficiaireExterne": "Nom du bénéficiaire",
  "montant": 25000,
  "motif": "Facture"
}
```

Le POST renvoie `202 Accepted` avec l'identifiant et l'état `EnAttente`. Le serveur recalcule les droits sur le compte source ; il ne fait pas confiance aux identifiants seuls envoyés par le client.
