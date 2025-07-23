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

			using TcpClient tcpClient = new();

			Task connectTask = tcpClient.ConnectAsync(ldapFixture.Hostname, port);
			bool connected = connectTask.Wait(TimeSpan.FromSeconds(5)); // timeout 5s

			_ = (tcpClient.Connected && connected).Should().BeTrue($"Cannot connect to LDAP at {ldapFixture.Hostname}:{port}");
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Ldap_Anonymous(bool secure)
		{
			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;

			using TcpClient tcpClient = new();

			Task connectTask = tcpClient.ConnectAsync(ldapFixture.Hostname, port);
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

			using LdapConnection connection = new(identifier)
			{
				AuthType = AuthType.Basic,
				Credential = new NetworkCredential(login, password)
			};

			connection.SessionOptions.ProtocolVersion = 3;
			connection.SessionOptions.SecureSocketLayer = secure;
			connection.SessionOptions.VerifyServerCertificate += (conn, cert) => secure;

			connection.Bind();
		}

		//[Theory]
		//[InlineData(false, "smaussion", "P@ssw0rd")]
		//[InlineData(false, "user01", "password1")]
		//[InlineData(false, "user02", "password2")]
		//[InlineData(true, "smaussion", "P@ssw0rd")]
		//[InlineData(true, "user01", "password1")]
		//[InlineData(true, "user02", "password2")]
		//public void LdapForNet_SearchForUsers_ShouldReturnSmaussion(bool secure, string username, string password)
		//{
		//	int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;
		//	string login = $"cn={username},ou=users,dc=example,dc=org";
		//	using LdapForNet.LdapConnection cn = new LdapForNet.LdapConnection();
		//	{
		//		// Connexion LDAPS
		//		cn.Connect(ldapFixture.Hostname);

		//		cn.TrustAllCertificates();
		//		LdapForNet.LdapCredential ldapCredential = new()
		//		{
		//			UserName = login,
		//			Password = password
		//		};

		//		cn.Bind(LdapAuthType.Simple, ldapCredential);
		//	}
		//}


		#endregion Public Methods
	}
}