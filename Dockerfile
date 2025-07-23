# Utilise l'image officielle Bitnami OpenLDAP
FROM bitnami/openldap:latest

# Passer en root pour installer et générer les certificats
USER root

# Installer openssl (présent par défaut, ici pour garantir sa présence)
RUN install_packages openssl

# Créer les certificats auto-signés
RUN mkdir -p /certs && \
    openssl genrsa -out /certs/openldap.key 2048 && \
    openssl req -new -key /certs/openldap.key -out /certs/openldap.csr -subj "/CN=ldap.entreprisegroup.intra" && \
    openssl x509 -req -in /certs/openldap.csr -signkey /certs/openldap.key -out /certs/openldap.crt -days 365 && \
    cp /certs/openldap.crt /certs/openldapCA.crt

# Donner les bons droits (UID 1001 = openldap user dans bitnami)
RUN chown -R 1001:0 /certs && chmod 600 /certs/*

# Définir les variables d'environnement pour TLS
ENV LDAP_ENABLE_TLS=yes \
    LDAP_TLS_CERT_FILE=/certs/openldap.crt \
    LDAP_TLS_KEY_FILE=/certs/openldap.key \
    LDAP_TLS_CA_FILE=/certs/openldapCA.crt

# Revenir à l'utilisateur openldap
USER 1001