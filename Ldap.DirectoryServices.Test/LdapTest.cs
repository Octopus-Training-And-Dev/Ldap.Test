using FluentAssertions;

using System.DirectoryServices.Protocols;
using System.Net;
using System.Net.Sockets;

namespace Ldap.DirectoryServices.Test
{
	public class LdapTest : IClassFixture<LdapFixture>
	{
		#region Fields

		private readonly LdapFixture ldapFixture;

		#endregion Fields

		#region Public Constructors

		public LdapTest(LdapFixture ldapFixture)
		{
			this.ldapFixture = ldapFixture;
		}

		#endregion Public Constructors

		#region Public Methods

		[Fact]
		public void CheckLdapContainer()
		{
			int port = ldapFixture.LdapsPort;

			using var tcpClient = new TcpClient();

			var connectTask = tcpClient.ConnectAsync(ldapFixture.Hostname, port);
			bool connected = connectTask.Wait(TimeSpan.FromSeconds(5)); // timeout 5s

			(tcpClient.Connected && connected).Should().BeTrue($"Cannot connect to LDAP at {ldapFixture.Hostname}:{port}");
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Ldap_Anonymous(bool secure)
		{
			//ldapFixture.TestcontainersStates.Should().Be(DotNet.Testcontainers.Containers.TestcontainersStates.Running, "the LDAP container should be running");
			//ldapFixture.Hostname.Should().Be("Localhost", "the LDAP container should be running on localhost");

			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;

			using var tcpClient = new TcpClient();

			var connectTask = tcpClient.ConnectAsync(ldapFixture.Hostname, port);
			bool connected = connectTask.Wait(TimeSpan.FromSeconds(5)); // timeout 5s

			Assert.True(connected && tcpClient.Connected, $"Cannot connect to LDAP at {ldapFixture.Hostname}:{port}");

			LdapDirectoryIdentifier identifier = new(ldapFixture.Hostname, port);
			using LdapConnection connection = new(identifier)
			{
				AuthType = AuthType.Anonymous
			};

			connection.SessionOptions.ProtocolVersion = 3;
			connection.SessionOptions.SecureSocketLayer = secure;
			connection.SessionOptions.VerifyServerCertificate += (conn, cert) => true;

			connection.Bind();
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
			LdapDirectoryIdentifier identifier = new(ldapFixture.Hostname, port);
			string login = $"cn={username},ou=users,dc=example,dc=org";

			using LdapConnection connection = new LdapConnection(identifier)
			{
				AuthType = AuthType.Basic,
				Credential = new NetworkCredential(login, password)
			};

			connection.SessionOptions.ProtocolVersion = 3;
			connection.SessionOptions.SecureSocketLayer = secure;
			connection.SessionOptions.VerifyServerCertificate += (conn, cert) => secure;

			connection.Bind();
		}

		#endregion Public Methods
	}
}