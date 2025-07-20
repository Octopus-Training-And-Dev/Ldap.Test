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
