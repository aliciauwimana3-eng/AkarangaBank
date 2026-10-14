# Prêts et cartes (chapitre 12)

## Prêts

- Les produits `PERSONNEL` et `EQUIPEMENT` sont initialisés avec leurs montants, durées et taux minimum/maximum.
- Une demande est réservée aux rôles guichetier, gestionnaire et administrateur. Le client doit être actif, son KYC validé et son compte de versement actif.
- Le taux initial est la moyenne des bornes du produit. Un gestionnaire ou administrateur peut appliquer un taux personnalisé à l'approbation. Le rejet exige un motif.
- L'approbation crédite le compte et crée les écritures en partie double dans une transaction EF Core; le calcul de l'échéancier ajuste le capital de la dernière mensualité.
- La mise à jour des retards est accessible depuis le contrôleur des prêts pour un gestionnaire ou administrateur avec MFA. Elle applique 0,5 % par mois, avec un minimum de 500 BIF, et classe le prêt `Impayé` à partir de trois échéances en retard.
- Le remboursement débite montant et pénalité, comptabilise capital, intérêts et pénalité, puis passe le prêt à `Solde` lorsque toutes les échéances sont payées.

## Cartes

- L'émission nécessite un compte actif, un client actif, un KYC validé et MFA. Visa, MasterCard et Locale génèrent un PAN à 16 chiffres avec préfixe et chiffre de contrôle Luhn.
- Le PAN est chiffré avec ASP.NET Data Protection en base. Les écrans et journaux n'affichent que `**** **** **** 1234`; le CVV n'est ni généré ni conservé.
- Trois plafonds journaliers distincts sont enregistrés (retrait, paiement et paiement en ligne). Une autorisation contrôle statut, expiration, solde, cumul journalier et surveillance AML.
- Une transaction refusée est conservée avec son motif. Une transaction autorisée, le débit du compte et les deux écritures comptables sont enregistrés atomiquement.
- L'opposition accepte Perte, Vol, Fraude ou Autre et conserve le motif. Le renouvellement expire l'ancienne carte et crée une nouvelle carte avec les mêmes plafonds.
- L'historique liste les transactions autorisées et refusées pour les comptes du périmètre utilisateur.

## Migration et vérification

Les migrations `CompleterSuiviRemboursementsPret`, `AjouterTransactionsEtPlafondsCartes`, `AjouterTypesDePret`, `AjouterMotifOppositionCarte` et `ProtegerNumerosCarteEtDerives` ajoutent le schéma du chapitre. Appliquer avec `dotnet ef database update` avant utilisation.

Les tests couvrent les mensualités à 0 % et 12 %, les autres durées, ainsi que la somme des capitaux et la dernière échéance. Les tests d'autorisation carte avec base relationnelle et les essais manuels de bout en bout restent à compléter.
