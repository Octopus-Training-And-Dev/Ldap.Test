# README

## Qu'est-ce que LDAP ?

LDAP (Lightweight Directory Access Protocol) est un protocole standard utilisé pour accéder à des services d'annuaire sur un réseau. Il permet de rechercher, modifier et authentifier des objets dans un annuaire, comme des utilisateurs, des groupes ou des ressources.

### Fonctionnement :
- **Bind LDAP** : Une connexion au serveur LDAP pour s'authentifier.
- **Requêtes LDAP** : Utilisées pour interroger l'annuaire et récupérer des informations comme les noms d'utilisateur, les groupes, les UID, etc.
- **Sécurité** : LDAP peut être sécurisé via TLS ou des mécanismes comme LDAP Signing.

Pour en savoir plus, consultez [la documentation sur LDAP](https://learn.microsoft.com/en-us/azure/azure-netapp-files/lightweight-directory-access-protocol).

---

## Installation d'OpenSSL via Chocolatey

Pour installer OpenSSL sur Windows en utilisant Chocolatey, suivez les étapes ci-dessous :

1. Assurez-vous que Chocolatey est installé sur votre machine. Si ce n'est pas le cas, consultez [le guide d'installation de Chocolatey](https://chocolatey.org/install).
2. Ouvrez une invite de commande ou PowerShell en mode administrateur.
3. Exécutez la commande suivante pour installer OpenSSL :
```
choco install openssl
``` 
5. Une fois l'installation terminée, vérifiez que `openssl.exe` est accessible dans votre variable PATH en exécutant :
```
openssl version
```


Pour plus d'informations, consultez la documentation officielle de Chocolatey : [OpenSSL sur Chocolatey](https://community.chocolatey.org/packages/openssl).

### Lancement script pour generer les Certificat autosignés
Pour générer des certificats autosignés pour les tests LDAP, vous pouvez utiliser le script PowerShell suivant :
Vous trouvez le script dans le fichier `GenerateCertificates.ps1`. Ce script crée un dossier `certs`, génère une clé privée RSA, une requête de signature de certificat (CSR) et un certificat auto-signé. Il copie également le certificat dans le même dossier sous le nom `openldapCA.crt`.

```powershell
# Crée le dossier certs s'il n'existe pas
$certsPath = ".\certs"
if (-Not (Test-Path $certsPath)) {
    New-Item -ItemType Directory -Path $certsPath | Out-Null
}

# Génère la clé privée RSA 2048 bits
openssl genrsa -out "$certsPath\openldap.key" 2048

# Génère la requête CSR avec le sujet CN=ldap.entreprisegroup.intra
openssl req -new -key "$certsPath\openldap.key" -out "$certsPath\openldap.csr" -subj "/CN=ldap.entreprisegroup.intra"

# Génère le certificat auto-signé (valide 365 jours)
openssl x509 -req -in "$certsPath\openldap.csr" -signkey "$certsPath\openldap.key" -out "$certsPath\openldap.crt" -days 365

# Copie le certificat comme CA (pour Bitnami OpenLDAP)
Copy-Item "$certsPath\openldap.crt" "$certsPath\openldapCA.crt"

Write-Host "Certificats générés dans le dossier $certsPath"
```
# LDAP Tests avec OpenLDAP

## Description des tests

### Fichier `LdapTest.cs`

Ce fichier contient deux tests principaux pour valider les interactions avec un serveur LDAP :

1. **Ldap_Anonymous** :
- Vérifie la connexion anonyme au serveur LDAP.
- Teste les connexions sécurisées (LDAPS) et non sécurisées (LDAP).
- Utilise `AuthType.Anonymous` pour établir une connexion sans authentification.

2. **Ldap_SearchForUsers_ShouldReturnSmaussionAsync** :
- Valide l'authentification des utilisateurs avec des identifiants spécifiques.
- Teste les connexions sécurisées et non sécurisées.
- Utilise `AuthType.Basic` avec des informations d'identification (`username` et `password`) pour se connecter au serveur LDAP.
- Vérifie que les utilisateurs définis dans le serveur LDAP (ex. `smaussion`, `user01`, `user02`) peuvent s'authentifier correctement.

Ces tests utilisent un conteneur OpenLDAP configuré via `DotNet.Testcontainers` pour simuler un serveur LDAP.

---

## Configuration des tests

Les tests dépendent de la classe `LdapFixture`, qui initialise un conteneur OpenLDAP avec les paramètres suivants :
- Ports exposés : `1389` (LDAP) et `1636` (LDAPS).
- Utilisateurs et mots de passe définis via des variables d'environnement.
- Certificats TLS montés dans le conteneur pour les connexions sécurisées.

Pour exécuter les tests, assurez-vous que Docker est installé et en cours d'exécution sur votre machine.

---
