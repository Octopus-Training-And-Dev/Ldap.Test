using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Ldap.DirectoryServices.Test
{
	public class LdapFixture : IDisposable
	{
		#region Fields

		public IContainer _ldapContainer;

		#endregion Fields

		#region Public Constructors

		public LdapFixture() => InitializeAsync().GetAwaiter().GetResult();

		#endregion Public Constructors

		#region Properties

		public string Hostname { get; private set; }
		public ushort LdapPort { get; private set; }
		public ushort LdapsPort { get; private set; }

		public TestcontainersStates TestcontainersStates
		{
			get
			{
				if (_ldapContainer == null)
				{
					return TestcontainersStates.Exited;
				}

				return _ldapContainer.State;
			}
		}

		#endregion Properties

		#region Public Methods

		public async void Dispose()
		{
			if (_ldapContainer == null)
			{
				return;
			}

			_ldapContainer.DisposeAsync().GetAwaiter().GetResult();
		}

		public async Task InitializeAsync()
		{
			string certsPathWindows = Path.GetFullPath("./Certs"); // chemin absolu Windows
			string certsPathUnix = certsPathWindows.Replace("\\", "/"); // chemin en slash UNIX

			_ldapContainer = new ContainerBuilder()
					.WithImage("bitnami/openldap:latest")
					.WithPortBinding(1389, true)
					.WithPortBinding(1636, true)
					.WithEnvironment("LDAP_ADMIN_USERNAME", "admin")
					.WithEnvironment("LDAP_ADMIN_PASSWORD", "adminpassword")
					.WithEnvironment("LDAP_USERS", "smaussion,user01,user02")
					.WithEnvironment("LDAP_PASSWORDS", "P@ssw0rd,password1,password2")
					.WithEnvironment("LDAP_ENABLE_TLS", "yes")
					.WithEnvironment("LDAP_TLS_CERT_FILE", "/opt/bitnami/openldap/certs/openldap.crt")
					.WithEnvironment("LDAP_TLS_KEY_FILE", "/opt/bitnami/openldap/certs/openldap.key")
					.WithEnvironment("LDAP_TLS_CA_FILE", "/opt/bitnami/openldap/certs/openldapCA.crt")
					.WithEnvironment("LDAP_TLS_VERIFY_CLIENT", "never")
					.WithBindMount(certsPathUnix, "/opt/bitnami/openldap/certs", accessMode: AccessMode.ReadOnly)
					.WithWaitStrategy(Wait.ForUnixContainer()
						.UntilMessageIsLogged("slapd starting")
						.UntilPortIsAvailable(1389))
					.WithCleanUp(true)
					.Build();

			await _ldapContainer.StartAsync();

			// Récupérer les ports mappés
			LdapPort = _ldapContainer.GetMappedPublicPort(1389);
			LdapsPort = _ldapContainer.GetMappedPublicPort(1636);
			Hostname = _ldapContainer.Hostname;

			Console.WriteLine($"Mapped ports => LDAP: {LdapPort}, LDAPS: {LdapsPort}");
			Console.WriteLine($"Hostname => {Hostname}");
		}

		#endregion Public Methods
	}
}