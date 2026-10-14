# Chapitre 13 - Accessibilite et internationalisation

## Realisation du chapitre

- Cultures prises en charge : `fr-BI` par defaut, `rn-BI`, `en-US` et `sw-KE`. La selection est limitee a cette liste; le cookie dure un an, les redirections restent locales et les formats de dates et nombres suivent la culture.
- Les ressources partagees sont disponibles dans toutes les vues. Navigation, commandes communes et principaux ecrans clients, comptes, agences, virements, operations, comptabilite, conformite, audit, alertes AML, prets et cartes utilisent ces ressources.
- Le layout fournit la langue HTML, le lien de saut, la cible principale, un focus clavier visible et des noms accessibles pour les commandes.
- Les tableaux ont des noms accessibles et des en-tetes `scope="col"`. Les erreurs sont exposees comme alertes et les confirmations comme statut live.
- Les fichiers RESX sont valides, sans cles dupliquees, avec le meme ensemble de cles par culture. La compilation Release reussit.

## Limite de verification

Les controles NVDA, axe-core/Lighthouse, contraste et zoom 200 % demandent un navigateur et des outils qui ne sont pas disponibles dans cet environnement. Les tests applicatifs n'ont pas ete relances apres les derniers changements d'interface. Le build affiche deux avertissements NuGet de faible gravite.