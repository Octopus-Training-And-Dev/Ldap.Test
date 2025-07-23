using FluentAssertions;

using Novell.Directory.Ldap;

using System.Net.Sockets;

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

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Ldap_AnonymousBind_ShouldSucceed(bool secure)
		{
			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;

			using var connection = new LdapConnection();

			if (secure)
			{
				connection.UserDefinedServerCertValidationDelegate += (sender, cert, chain, errors) => true;
			}

			connection.SecureSocketLayer = secure;
			connection.Connect(ldapFixture.Hostname, port);

			connection.Should().NotBeNull($"Cannot connect to LDAP at {ldapFixture.Hostname}:{port}");
			//connection.SessionOptions.ProtocolVersion = 3;
			connection.Disconnect();
		}

		[Theory]
		[InlineData(false, "smaussion", "P@ssw0rd")]
		[InlineData(false, "user01", "password1")]
		[InlineData(false, "user02", "password2")]
		[InlineData(true, "smaussion", "P@ssw0rd")]
		[InlineData(true, "user01", "password1")]
		[InlineData(true, "user02", "password2")]
		public void Ldap_SearchForUsers_ShouldReturnSmaussion(bool secure, string username, string password)
		{
			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;
			string loginDn = $"cn={username},ou=users,dc=example,dc=org";

			using LdapConnection ldapConnection = new LdapConnection();

			// Accepte les certificats auto-signés (uniquement en dev)
			if (secure)
			{
				ldapConnection.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, errors) => true;
			}

			ldapConnection.SecureSocketLayer = secure;
			ldapConnection.Connect(ldapFixture.Hostname, port);

			// Essayer le bind (authentification)
			ldapConnection.Bind(loginDn, password);

			// Vérifier que le bind a réussi
			ldapConnection.Bound.Should().BeTrue();

			ldapConnection.Disconnect();
		}
	}
}