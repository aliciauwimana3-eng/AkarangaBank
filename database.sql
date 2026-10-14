IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Agences] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Nom] nvarchar(100) NOT NULL,
        [Ville] nvarchar(50) NOT NULL,
        [EstSiege] bit NOT NULL,
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateModification] datetime2 NULL,
        CONSTRAINT [PK_Agences] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [TypesCompte] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Libelle] nvarchar(50) NOT NULL,
        [TauxInteret] decimal(5,2) NOT NULL DEFAULT 0.0,
        [SoldeMinimum] decimal(18,2) NOT NULL DEFAULT 0.0,
        [AutoriseDecouvert] bit NOT NULL,
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_TypesCompte] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Clients] (
        [Id] int NOT NULL IDENTITY,
        [NumeroClient] nvarchar(20) NOT NULL,
        [Nom] nvarchar(100) NOT NULL,
        [Prenom] nvarchar(100) NOT NULL,
        [DateNaissance] date NOT NULL,
        [NumeroCNI] nvarchar(30) NOT NULL,
        [Telephone] nvarchar(20) NULL,
        [Email] nvarchar(100) NULL,
        [Adresse] nvarchar(255) NULL,
        [AgenceId] int NOT NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'Actif',
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateModification] datetime2 NULL,
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Clients_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Parametres] (
        [Id] int NOT NULL IDENTITY,
        [Cle] nvarchar(50) NOT NULL,
        [Valeur] nvarchar(255) NOT NULL,
        [Description] nvarchar(255) NULL,
        [AgenceId] int NULL,
        [DateModification] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Parametres] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Parametres_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [RolesClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RolesClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolesClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Comptes] (
        [Id] int NOT NULL IDENTITY,
        [NumeroCompte] nvarchar(30) NOT NULL,
        [ClientId] int NOT NULL,
        [TypeCompteId] int NOT NULL,
        [AgenceId] int NOT NULL,
        [Solde] decimal(18,2) NOT NULL DEFAULT 0.0,
        [Devise] nvarchar(3) NOT NULL DEFAULT N'BIF',
        [Statut] nvarchar(20) NOT NULL DEFAULT N'Actif',
        [DateOuverture] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateFermeture] datetime2 NULL,
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateModification] datetime2 NULL,
        CONSTRAINT [PK_Comptes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comptes_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Comptes_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Comptes_TypesCompte_TypeCompteId] FOREIGN KEY ([TypeCompteId]) REFERENCES [TypesCompte] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Utilisateurs] (
        [Id] int NOT NULL IDENTITY,
        [NomComplet] nvarchar(100) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [AgenceId] int NOT NULL,
        [DeuxFA_Active] bit NOT NULL,
        [DeuxFA_Secret] nvarchar(255) NULL,
        [DerniereConnexion] datetime2 NULL,
        [EstActif] bit NOT NULL,
        [ClientId] int NULL,
        [DateCreation] datetime2 NOT NULL,
        [DateModification] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Utilisateurs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Utilisateurs_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Utilisateurs_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Cartes] (
        [Id] int NOT NULL IDENTITY,
        [NumeroCarte] nvarchar(20) NOT NULL,
        [CompteId] int NOT NULL,
        [ClientId] int NOT NULL,
        [TypeCarte] nvarchar(20) NOT NULL,
        [DateExpiration] datetime2 NOT NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'Active',
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Cartes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cartes_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Cartes_Comptes_CompteId] FOREIGN KEY ([CompteId]) REFERENCES [Comptes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [AuditLog] (
        [Id] int NOT NULL IDENTITY,
        [DateEvenement] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [UtilisateurId] int NULL,
        [AgenceId] int NULL,
        [Action] nvarchar(50) NOT NULL,
        [TableConcernee] nvarchar(50) NULL,
        [EnregistrementId] int NULL,
        [ValeurAvant] nvarchar(max) NULL,
        [ValeurApres] nvarchar(max) NULL,
        [AdresseIP] nvarchar(45) NULL,
        [UserAgent] nvarchar(255) NULL,
        [Details] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLog] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLog_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AuditLog_Utilisateurs_UtilisateurId] FOREIGN KEY ([UtilisateurId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Prets] (
        [Id] int NOT NULL IDENTITY,
        [Reference] nvarchar(30) NOT NULL,
        [ClientId] int NOT NULL,
        [CompteDebitId] int NOT NULL,
        [ValideParId] int NULL,
        [MontantPrincipal] decimal(18,2) NOT NULL,
        [TauxInteret] decimal(5,2) NOT NULL,
        [DureeMois] int NOT NULL,
        [Mensualite] decimal(18,2) NOT NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'EnAttente',
        [DateDemande] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateValidation] datetime2 NULL,
        [DateDebut] datetime2 NULL,
        [DateFin] datetime2 NULL,
        CONSTRAINT [PK_Prets] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prets_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Prets_Comptes_CompteDebitId] FOREIGN KEY ([CompteDebitId]) REFERENCES [Comptes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Prets_Utilisateurs_ValideParId] FOREIGN KEY ([ValideParId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [SessionsUtilisateur] (
        [Id] int NOT NULL IDENTITY,
        [UtilisateurId] int NOT NULL,
        [Token] nvarchar(255) NOT NULL,
        [AdresseIP] nvarchar(45) NULL,
        [UserAgent] nvarchar(255) NULL,
        [DeuxFA_Valide] bit NOT NULL,
        [DateConnexion] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateExpiration] datetime2 NOT NULL,
        [EstActive] bit NOT NULL,
        CONSTRAINT [PK_SessionsUtilisateur] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SessionsUtilisateur_Utilisateurs_UtilisateurId] FOREIGN KEY ([UtilisateurId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [UtilisateursClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_UtilisateursClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UtilisateursClaims_Utilisateurs_UserId] FOREIGN KEY ([UserId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [UtilisateursLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] int NOT NULL,
        CONSTRAINT [PK_UtilisateursLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UtilisateursLogins_Utilisateurs_UserId] FOREIGN KEY ([UserId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [UtilisateursRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_UtilisateursRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UtilisateursRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UtilisateursRoles_Utilisateurs_UserId] FOREIGN KEY ([UserId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [UtilisateursTokens] (
        [UserId] int NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_UtilisateursTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UtilisateursTokens_Utilisateurs_UserId] FOREIGN KEY ([UserId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [VerificationsKYC] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [TypeDocument] nvarchar(50) NOT NULL,
        [NumeroDocument] nvarchar(50) NOT NULL,
        [CheminDocument] nvarchar(500) NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'EnAttente',
        [VerifieParId] int NULL,
        [DateVerification] datetime2 NULL,
        [Commentaire] nvarchar(500) NULL,
        CONSTRAINT [PK_VerificationsKYC] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VerificationsKYC_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_VerificationsKYC_Utilisateurs_VerifieParId] FOREIGN KEY ([VerifieParId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Virements] (
        [Id] int NOT NULL IDENTITY,
        [Reference] nvarchar(30) NOT NULL,
        [CompteSourceId] int NOT NULL,
        [CompteDestinationId] int NOT NULL,
        [UtilisateurId] int NOT NULL,
        [Montant] decimal(18,2) NOT NULL,
        [Devise] nvarchar(3) NOT NULL DEFAULT N'BIF',
        [Statut] nvarchar(20) NOT NULL DEFAULT N'EnAttente',
        [Motif] nvarchar(255) NULL,
        [ControleAML] bit NOT NULL,
        [DateVirement] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateValidation] datetime2 NULL,
        CONSTRAINT [PK_Virements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Virements_Comptes_CompteDestinationId] FOREIGN KEY ([CompteDestinationId]) REFERENCES [Comptes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Virements_Comptes_CompteSourceId] FOREIGN KEY ([CompteSourceId]) REFERENCES [Comptes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Virements_Utilisateurs_UtilisateurId] FOREIGN KEY ([UtilisateurId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [EcheancesPret] (
        [Id] int NOT NULL IDENTITY,
        [PretId] int NOT NULL,
        [NumeroEcheance] int NOT NULL,
        [Capital] decimal(18,2) NOT NULL,
        [Interet] decimal(18,2) NOT NULL,
        [MontantTotal] decimal(18,2) NOT NULL,
        [DateEcheance] datetime2 NOT NULL,
        [DatePaiement] datetime2 NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'EnAttente',
        CONSTRAINT [PK_EcheancesPret] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_EcheancesPret_Prets_PretId] FOREIGN KEY ([PretId]) REFERENCES [Prets] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Operations] (
        [Id] int NOT NULL IDENTITY,
        [Reference] nvarchar(30) NOT NULL,
        [TypeOperation] nvarchar(30) NOT NULL,
        [CompteId] int NOT NULL,
        [UtilisateurId] int NOT NULL,
        [AgenceId] int NOT NULL,
        [VirementId] int NULL,
        [PretId] int NULL,
        [Montant] decimal(18,2) NOT NULL,
        [Devise] nvarchar(3) NOT NULL DEFAULT N'BIF',
        [Statut] nvarchar(20) NOT NULL DEFAULT N'EnAttente',
        [DateOperation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateValidation] datetime2 NULL,
        [Commentaire] nvarchar(255) NULL,
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        CONSTRAINT [PK_Operations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Operations_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Operations_Comptes_CompteId] FOREIGN KEY ([CompteId]) REFERENCES [Comptes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Operations_Prets_PretId] FOREIGN KEY ([PretId]) REFERENCES [Prets] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Operations_Utilisateurs_UtilisateurId] FOREIGN KEY ([UtilisateurId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Operations_Virements_VirementId] FOREIGN KEY ([VirementId]) REFERENCES [Virements] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [AlertesAML] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [OperationId] int NULL,
        [TypeAlerte] nvarchar(50) NOT NULL,
        [Niveau] nvarchar(20) NOT NULL,
        [Description] nvarchar(500) NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'Ouverte',
        [TraiteParId] int NULL,
        [DateAlerte] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [DateTraitement] datetime2 NULL,
        CONSTRAINT [PK_AlertesAML] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AlertesAML_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AlertesAML_Operations_OperationId] FOREIGN KEY ([OperationId]) REFERENCES [Operations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AlertesAML_Utilisateurs_TraiteParId] FOREIGN KEY ([TraiteParId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE TABLE [Ecritures] (
        [Id] int NOT NULL IDENTITY,
        [OperationId] int NOT NULL,
        [CompteId] int NOT NULL,
        [Sens] nvarchar(10) NOT NULL,
        [Montant] decimal(18,2) NOT NULL,
        [Devise] nvarchar(3) NOT NULL DEFAULT N'BIF',
        [DateEcriture] datetime2 NOT NULL DEFAULT (SYSDATETIME()),
        [Libelle] nvarchar(255) NULL,
        CONSTRAINT [PK_Ecritures] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Ecritures_Comptes_CompteId] FOREIGN KEY ([CompteId]) REFERENCES [Comptes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Ecritures_Operations_OperationId] FOREIGN KEY ([OperationId]) REFERENCES [Operations] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Agences_Code] ON [Agences] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AlertesAML_ClientId] ON [AlertesAML] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AlertesAML_OperationId] ON [AlertesAML] ([OperationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AlertesAML_Statut] ON [AlertesAML] ([Statut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AlertesAML_TraiteParId] ON [AlertesAML] ([TraiteParId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AuditLog_AgenceId] ON [AuditLog] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AuditLog_DateEvenement] ON [AuditLog] ([DateEvenement]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_AuditLog_UtilisateurId] ON [AuditLog] ([UtilisateurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Cartes_ClientId] ON [Cartes] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Cartes_CompteId] ON [Cartes] ([CompteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Cartes_NumeroCarte] ON [Cartes] ([NumeroCarte]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Clients_AgenceId] ON [Clients] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_NumeroClient] ON [Clients] ([NumeroClient]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Clients_NumeroCNI] ON [Clients] ([NumeroCNI]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Comptes_AgenceId] ON [Comptes] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Comptes_ClientId] ON [Comptes] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Comptes_NumeroCompte] ON [Comptes] ([NumeroCompte]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Comptes_TypeCompteId] ON [Comptes] ([TypeCompteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_EcheancesPret_DateEcheance] ON [EcheancesPret] ([DateEcheance]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EcheancesPret_PretId_NumeroEcheance] ON [EcheancesPret] ([PretId], [NumeroEcheance]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Ecritures_CompteId] ON [Ecritures] ([CompteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Ecritures_OperationId] ON [Ecritures] ([OperationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_AgenceId] ON [Operations] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_CompteId] ON [Operations] ([CompteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_DateOperation] ON [Operations] ([DateOperation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_PretId] ON [Operations] ([PretId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Operations_Reference] ON [Operations] ([Reference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_Statut] ON [Operations] ([Statut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_UtilisateurId] ON [Operations] ([UtilisateurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Operations_VirementId] ON [Operations] ([VirementId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Parametres_AgenceId] ON [Parametres] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Parametres_Cle_AgenceId] ON [Parametres] ([Cle], [AgenceId]) WHERE [AgenceId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Prets_ClientId] ON [Prets] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Prets_CompteDebitId] ON [Prets] ([CompteDebitId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Prets_Reference] ON [Prets] ([Reference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Prets_Statut] ON [Prets] ([Statut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Prets_ValideParId] ON [Prets] ([ValideParId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_RolesClaims_RoleId] ON [RolesClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SessionsUtilisateur_Token] ON [SessionsUtilisateur] ([Token]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_SessionsUtilisateur_UtilisateurId] ON [SessionsUtilisateur] ([UtilisateurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TypesCompte_Code] ON [TypesCompte] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [Utilisateurs] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Utilisateurs_AgenceId] ON [Utilisateurs] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Utilisateurs_ClientId] ON [Utilisateurs] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Utilisateurs_Role] ON [Utilisateurs] ([Role]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [Utilisateurs] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_UtilisateursClaims_UserId] ON [UtilisateursClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_UtilisateursLogins_UserId] ON [UtilisateursLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_UtilisateursRoles_RoleId] ON [UtilisateursRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_VerificationsKYC_ClientId] ON [VerificationsKYC] ([ClientId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_VerificationsKYC_VerifieParId] ON [VerificationsKYC] ([VerifieParId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Virements_CompteDestinationId] ON [Virements] ([CompteDestinationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Virements_CompteSourceId] ON [Virements] ([CompteSourceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Virements_Reference] ON [Virements] ([Reference]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Virements_Statut] ON [Virements] ([Statut]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    CREATE INDEX [IX_Virements_UtilisateurId] ON [Virements] ([UtilisateurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005101447_InitialIdentityWithInt'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005101447_InitialIdentityWithInt', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065144_PartieDoubleAvecComptesComptables'
)
BEGIN
    DECLARE @var nvarchar(max);
    SELECT @var = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Virements]') AND [c].[name] = N'ControleAML');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Virements] DROP CONSTRAINT ' + @var + ';');
    ALTER TABLE [Virements] ADD DEFAULT CAST(0 AS bit) FOR [ControleAML];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065144_PartieDoubleAvecComptesComptables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010065144_PartieDoubleAvecComptesComptables', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    DECLARE @var1 nvarchar(max);
    SELECT @var1 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ecritures]') AND [c].[name] = N'CompteId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Ecritures] DROP CONSTRAINT ' + @var1 + ';');
    ALTER TABLE [Ecritures] ALTER COLUMN [CompteId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    ALTER TABLE [Ecritures] ADD [CompteComptableId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    CREATE TABLE [ComptesComptables] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Libelle] nvarchar(100) NOT NULL,
        [Devise] nvarchar(3) NOT NULL,
        [EstActif] bit NOT NULL,
        CONSTRAINT [PK_ComptesComptables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    CREATE INDEX [IX_Ecritures_CompteComptableId] ON [Ecritures] ([CompteComptableId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    EXEC(N'ALTER TABLE [Ecritures] ADD CONSTRAINT [CK_Ecritures_UnSeulCompte] CHECK ((CompteId IS NULL AND CompteComptableId IS NOT NULL) OR (CompteId IS NOT NULL AND CompteComptableId IS NULL))');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ComptesComptables_Code] ON [ComptesComptables] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    ALTER TABLE [Ecritures] ADD CONSTRAINT [FK_Ecritures_ComptesComptables_CompteComptableId] FOREIGN KEY ([CompteComptableId]) REFERENCES [ComptesComptables] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065229_AjouterComptesComptables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010065229_AjouterComptesComptables', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065820_VirementsInterbancairesEtSeuilsConformite'
)
BEGIN
    DECLARE @var2 nvarchar(max);
    SELECT @var2 = QUOTENAME([d].[name])
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Virements]') AND [c].[name] = N'CompteDestinationId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Virements] DROP CONSTRAINT ' + @var2 + ';');
    ALTER TABLE [Virements] ALTER COLUMN [CompteDestinationId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065820_VirementsInterbancairesEtSeuilsConformite'
)
BEGIN
    ALTER TABLE [Virements] ADD [BanqueDestination] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065820_VirementsInterbancairesEtSeuilsConformite'
)
BEGIN
    ALTER TABLE [Virements] ADD [BeneficiaireExterne] nvarchar(150) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065820_VirementsInterbancairesEtSeuilsConformite'
)
BEGIN
    ALTER TABLE [Virements] ADD [EstInterbancaire] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065820_VirementsInterbancairesEtSeuilsConformite'
)
BEGIN
    ALTER TABLE [Virements] ADD [NumeroCompteExterne] nvarchar(34) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010065820_VirementsInterbancairesEtSeuilsConformite'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010065820_VirementsInterbancairesEtSeuilsConformite', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010081826_CompleterSuiviRemboursementsPret'
)
BEGIN
    ALTER TABLE [Prets] ADD [Commentaire] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010081826_CompleterSuiviRemboursementsPret'
)
BEGIN
    ALTER TABLE [Prets] ADD [MontantRembourse] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010081826_CompleterSuiviRemboursementsPret'
)
BEGIN
    ALTER TABLE [Prets] ADD [MotifRejet] nvarchar(500) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010081826_CompleterSuiviRemboursementsPret'
)
BEGIN
    ALTER TABLE [EcheancesPret] ADD [Penalite] decimal(18,2) NOT NULL DEFAULT 0.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010081826_CompleterSuiviRemboursementsPret'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010081826_CompleterSuiviRemboursementsPret', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    ALTER TABLE [Cartes] ADD [PlafondEnLigneJournalier] decimal(18,2) NOT NULL DEFAULT 300000.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    ALTER TABLE [Cartes] ADD [PlafondPaiementJournalier] decimal(18,2) NOT NULL DEFAULT 1000000.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    ALTER TABLE [Cartes] ADD [PlafondRetraitJournalier] decimal(18,2) NOT NULL DEFAULT 500000.0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    CREATE TABLE [TransactionsCartes] (
        [Id] int NOT NULL IDENTITY,
        [CarteId] int NOT NULL,
        [OperationId] int NULL,
        [Montant] decimal(18,2) NOT NULL,
        [TypeTransaction] nvarchar(20) NOT NULL,
        [Statut] nvarchar(20) NOT NULL,
        [MotifRefus] nvarchar(250) NULL,
        [DateTransaction] datetime2 NOT NULL,
        CONSTRAINT [PK_TransactionsCartes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransactionsCartes_Cartes_CarteId] FOREIGN KEY ([CarteId]) REFERENCES [Cartes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransactionsCartes_Operations_OperationId] FOREIGN KEY ([OperationId]) REFERENCES [Operations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    CREATE INDEX [IX_TransactionsCartes_CarteId_DateTransaction] ON [TransactionsCartes] ([CarteId], [DateTransaction]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    CREATE INDEX [IX_TransactionsCartes_OperationId] ON [TransactionsCartes] ([OperationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082318_AjouterTransactionsEtPlafondsCartes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010082318_AjouterTransactionsEtPlafondsCartes', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    ALTER TABLE [Prets] ADD [TypePretId] int NOT NULL DEFAULT 1;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    CREATE TABLE [TypesPret] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(30) NOT NULL,
        [Libelle] nvarchar(100) NOT NULL,
        [MontantMin] decimal(18,2) NOT NULL,
        [MontantMax] decimal(18,2) NOT NULL,
        [DureeMinMois] int NOT NULL,
        [DureeMaxMois] int NOT NULL,
        [TauxMin] decimal(5,2) NOT NULL,
        [TauxMax] decimal(5,2) NOT NULL,
        [EstActif] bit NOT NULL,
        CONSTRAINT [PK_TypesPret] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DureeMaxMois', N'DureeMinMois', N'EstActif', N'Libelle', N'MontantMax', N'MontantMin', N'TauxMax', N'TauxMin') AND [object_id] = OBJECT_ID(N'[TypesPret]'))
        SET IDENTITY_INSERT [TypesPret] ON;
    EXEC(N'INSERT INTO [TypesPret] ([Id], [Code], [DureeMaxMois], [DureeMinMois], [EstActif], [Libelle], [MontantMax], [MontantMin], [TauxMax], [TauxMin])
    VALUES (1, N''PERSONNEL'', 60, 3, CAST(1 AS bit), N''Prêt personnel'', 10000000.0, 100000.0, 16.0, 8.0),
    (2, N''EQUIPEMENT'', 120, 6, CAST(1 AS bit), N''Prêt équipement'', 50000000.0, 500000.0, 14.0, 6.0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DureeMaxMois', N'DureeMinMois', N'EstActif', N'Libelle', N'MontantMax', N'MontantMin', N'TauxMax', N'TauxMin') AND [object_id] = OBJECT_ID(N'[TypesPret]'))
        SET IDENTITY_INSERT [TypesPret] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    CREATE INDEX [IX_Prets_TypePretId] ON [Prets] ([TypePretId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TypesPret_Code] ON [TypesPret] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    ALTER TABLE [Prets] ADD CONSTRAINT [FK_Prets_TypesPret_TypePretId] FOREIGN KEY ([TypePretId]) REFERENCES [TypesPret] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082425_AjouterTypesDePret'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010082425_AjouterTypesDePret', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082631_AjouterMotifOppositionCarte'
)
BEGIN
    ALTER TABLE [Cartes] ADD [MotifBlocage] nvarchar(20) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082631_AjouterMotifOppositionCarte'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010082631_AjouterMotifOppositionCarte', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082743_ProtegerNumerosCarteEtDerives'
)
BEGIN
    ALTER TABLE [Cartes] ADD [DerniersChiffres] nvarchar(4) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082743_ProtegerNumerosCarteEtDerives'
)
BEGIN
    ALTER TABLE [Cartes] ADD [NumeroCarteChiffre] nvarchar(500) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082743_ProtegerNumerosCarteEtDerives'
)
BEGIN
    UPDATE Cartes SET DerniersChiffres = RIGHT(REPLACE(REPLACE(NumeroCarte, '-', ''), ' ', ''), 4)
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010082743_ProtegerNumerosCarteEtDerives'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010082743_ProtegerNumerosCarteEtDerives', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010090724_AjouterDocumentsPrivesEtNotifications'
)
BEGIN
    ALTER TABLE [VerificationsKYC] ADD [NomDocumentOriginal] nvarchar(255) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010090724_AjouterDocumentsPrivesEtNotifications'
)
BEGIN
    ALTER TABLE [VerificationsKYC] ADD [TailleDocument] bigint NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010090724_AjouterDocumentsPrivesEtNotifications'
)
BEGIN
    ALTER TABLE [VerificationsKYC] ADD [TypeMimeDocument] nvarchar(100) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010090724_AjouterDocumentsPrivesEtNotifications'
)
BEGIN
    CREATE TABLE [NotificationsUtilisateurs] (
        [Id] int NOT NULL IDENTITY,
        [UtilisateurId] int NOT NULL,
        [Titre] nvarchar(120) NOT NULL,
        [Message] nvarchar(1000) NOT NULL,
        [Url] nvarchar(500) NULL,
        [DateCreation] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
        [DateLecture] datetime2 NULL,
        CONSTRAINT [PK_NotificationsUtilisateurs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_NotificationsUtilisateurs_Utilisateurs_UtilisateurId] FOREIGN KEY ([UtilisateurId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010090724_AjouterDocumentsPrivesEtNotifications'
)
BEGIN
    CREATE INDEX [IX_NotificationsUtilisateurs_UtilisateurId_DateLecture_DateCreation] ON [NotificationsUtilisateurs] ([UtilisateurId], [DateLecture], [DateCreation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010090724_AjouterDocumentsPrivesEtNotifications'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010090724_AjouterDocumentsPrivesEtNotifications', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE TABLE [Documents] (
        [Id] int NOT NULL IDENTITY,
        [ClientId] int NOT NULL,
        [DeposeParId] int NULL,
        [Categorie] nvarchar(20) NOT NULL,
        [NomOriginal] nvarchar(255) NOT NULL,
        [CleStockage] nvarchar(32) NOT NULL,
        [TypeMime] nvarchar(150) NOT NULL,
        [TailleOctets] bigint NOT NULL,
        [HashSha256] nvarchar(64) NOT NULL,
        [Statut] nvarchar(20) NOT NULL DEFAULT N'Actif',
        [Version] int NOT NULL,
        [DateCreation] datetime2 NOT NULL,
        [DateArchivage] datetime2 NULL,
        [DateSuppression] datetime2 NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Documents_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Documents_Utilisateurs_DeposeParId] FOREIGN KEY ([DeposeParId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE TABLE [DocumentsAcces] (
        [Id] int NOT NULL IDENTITY,
        [DocumentId] int NOT NULL,
        [UtilisateurId] int NOT NULL,
        [Action] nvarchar(30) NOT NULL,
        [DateAcces] datetime2 NOT NULL,
        CONSTRAINT [PK_DocumentsAcces] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DocumentsAcces_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_DocumentsAcces_Utilisateurs_UtilisateurId] FOREIGN KEY ([UtilisateurId]) REFERENCES [Utilisateurs] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE INDEX [IX_Documents_ClientId_Categorie_Version] ON [Documents] ([ClientId], [Categorie], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE INDEX [IX_Documents_DeposeParId] ON [Documents] ([DeposeParId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE INDEX [IX_Documents_Statut_DateSuppression] ON [Documents] ([Statut], [DateSuppression]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE INDEX [IX_DocumentsAcces_DocumentId_DateAcces] ON [DocumentsAcces] ([DocumentId], [DateAcces]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    CREATE INDEX [IX_DocumentsAcces_UtilisateurId] ON [DocumentsAcces] ([UtilisateurId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010091932_Chapitre14DocumentsEtAcces'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010091932_Chapitre14DocumentsEtAcces', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [Canal] nvarchar(20) NOT NULL DEFAULT N'Interne';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [DateEnvoi] datetime2 NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [Erreur] nvarchar(1000) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [Statut] nvarchar(30) NOT NULL DEFAULT N'Envoyee';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [TemplateCode] nvarchar(80) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [Tentatives] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    CREATE TABLE [NotificationTemplates] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(80) NOT NULL,
        [Canal] nvarchar(20) NOT NULL,
        [Langue] nvarchar(10) NOT NULL,
        [Sujet] nvarchar(255) NOT NULL,
        [ContenuHtml] nvarchar(max) NULL,
        [ContenuTexte] nvarchar(max) NOT NULL,
        [Version] int NOT NULL,
        [EstActive] bit NOT NULL,
        [DateCreation] datetime2 NOT NULL,
        CONSTRAINT [PK_NotificationTemplates] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_NotificationTemplates_Code_Canal_Langue_Version] ON [NotificationTemplates] ([Code], [Canal], [Langue], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010092101_Chapitre14TemplatesNotificationsEtDocuments', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092812_Chapitre14CanauxEtPlanification'
)
BEGIN
    ALTER TABLE [Utilisateurs] ADD [PushToken] nvarchar(512) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092812_Chapitre14CanauxEtPlanification'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [Langue] nvarchar(10) NOT NULL DEFAULT N'fr-BI';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010092812_Chapitre14CanauxEtPlanification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010092812_Chapitre14CanauxEtPlanification', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010093111_Chapitre14JournalAccesDocuments'
)
BEGIN
    DROP INDEX [IX_Documents_ClientId_Categorie_Version] ON [Documents];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010093111_Chapitre14JournalAccesDocuments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Documents_ClientId_Categorie_Version] ON [Documents] ([ClientId], [Categorie], [Version]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010093111_Chapitre14JournalAccesDocuments'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010093111_Chapitre14JournalAccesDocuments', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010093752_Chapitre14VersionTemplateNotification'
)
BEGIN
    ALTER TABLE [NotificationsUtilisateurs] ADD [TemplateVersion] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010093752_Chapitre14VersionTemplateNotification'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010093752_Chapitre14VersionTemplateNotification', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010101827_Chapitre16AgenceEcritures'
)
BEGIN
    ALTER TABLE [Ecritures] ADD [AgenceId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010101827_Chapitre16AgenceEcritures'
)
BEGIN
    CREATE INDEX [IX_Ecritures_AgenceId] ON [Ecritures] ([AgenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010101827_Chapitre16AgenceEcritures'
)
BEGIN
    ALTER TABLE [Ecritures] ADD CONSTRAINT [FK_Ecritures_Agences_AgenceId] FOREIGN KEY ([AgenceId]) REFERENCES [Agences] ([Id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261010101827_Chapitre16AgenceEcritures'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261010101827_Chapitre16AgenceEcritures', N'10.0.12');
END;

COMMIT;
GO

