using System.DirectoryServices.Protocols;
using System.Net;

namespace Ldap.Test
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

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Ldap_Anonymous(bool secure)
		{
			int port = secure ? ldapFixture.LdapsPort : ldapFixture.LdapPort;
			LdapDirectoryIdentifier identifier = new("localhost", port);
			using LdapConnection connection = new(identifier)
			{
				AuthType = AuthType.Anonymous
			};
			connection.SessionOptions.ProtocolVersion = 3;
			connection.SessionOptions.SecureSocketLayer = secure;
			connection.SessionOptions.VerifyServerCertificate += (conn, cert) => secure;

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
			LdapDirectoryIdentifier identifier = new("localhost", port);
			string login = $"cn={username},ou=users,dc=example,dc=org";

			using var connection = new LdapConnection(identifier)
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