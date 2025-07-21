using FluentAssertions;

using Novell.Directory.Ldap;

using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace ldap.Novell.Tests
{
	public class LdapTest : IClassFixture<LdapFixture>
	{
		private readonly LdapFixture ldapFixture;

		public LdapTest(LdapFixture ldapFixture)
		{
			this.ldapFixture = ldapFixture;
		}

		[Fact]
		public void CheckLdapContainer()
		{
			int port = ldapFixture.LdapsPort;

			using TcpClient tcpClient = new();

			Task connectTask = tcpClient.ConnectAsync(ldapFixture.Hostname, port);
			bool connected = connectTask.Wait(TimeSpan.FromSeconds(5)); // timeout 5s

			_ = (tcpClient.Connected && connected).Should().BeTrue($"Cannot connect to LDAP at {ldapFixture.Hostname}:{port}");
		}

		//[Theory]
		//[InlineData(false)]
		//[InlineData(true)]
		public void Ldap_Anonymous(bool secure)
		{
			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;
			using LdapConnection connection = new();
			connection.SecureSocketLayer = secure;
			connection.Connect(ldapFixture.Hostname, port);
			// Bind anonyme (pas d'identifiants)
			connection.Bind("cn=admin,dc=example,dc=com", "adminpassword");

			// Bind anonyme
			bool result = connection.Bound;
			result.Should().BeTrue($"Cannot bind anonymously to LDAP at {ldapFixture.Hostname}:{port}");
			// Optionnel : vérifier que la connexion est active
			connection.Disconnect();
		}

		[Theory]
		[InlineData(false, "smaussion", "P@ssw0rd")]
		[InlineData(false, "user01", "password1")]
		[InlineData(false, "user02", "password2")]
		[InlineData(true, "smaussion", "P@ssw0rd")]
		//[InlineData(true, "user01", "password1")]
		//[InlineData(true, "user02", "password2")]
		public void Ldap_SearchForUsers_ShouldReturnSmaussion(bool secure, string username, string password)
		{
			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;
			string loginDn = $"cn={username},ou=users,dc=example,dc=org";

			using LdapConnection ldapConnection = new();
			ldapConnection.SecureSocketLayer = secure;
			ldapConnection.Connect(ldapFixture.Hostname, port);
			ldapConnection.Bind(loginDn, password);

			// Si bind OK, test passe. Sinon exception levée.
			_ = ldapConnection.Bound.Should().BeTrue();
			ldapConnection.Disconnect();
		}
	}
}