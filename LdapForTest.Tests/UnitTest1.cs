using LdapForNet;

using static LdapForNet.Native.Native;

namespace LdapForTest.Tests
{
	public class LdapConnectionTests
	{
		private const string LdapHost = "localhost";        // ou IP docker
		private const int LdapPort = 3;                    // 636 pour LDAPS, 389 pour LDAP simple
		private const string BindDn = "cn=smaussion,ou=users,dc=example,dc=org";
		private const string Password = "P@ssw0rd";

		[Fact]
		public void Test_Ldap_Ldaps_Connection_Should_Succeed()
		{
			using LdapConnection connection = new();

			// Connexion LDAPS
			connection.Connect(LdapHost, LdapPort, LdapSchema.LDAP);

			// Pour accepter certificat auto-signé (uniquement en dev)
			connection.TrustAllCertificates();
			LdapCredential ldapCredential = new()
			{
				UserName = BindDn,
				Password = Password
			};

			// Bind simple avec DN et mot de passe
			connection.Bind(LdapAuthType.Simple, ldapCredential);
		}
	}
}