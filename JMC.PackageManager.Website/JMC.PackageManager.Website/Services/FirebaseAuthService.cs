using Firebase.Auth;
using Firebase.Auth.Providers;
using FirebaseAdmin.Auth;

namespace JMC.PackageManager.Website.Services
{
    public class FirebaseAuthService : FirebaseService
    {
        public static FirebaseAuthConfig FirebaseAuthConfig { get; set; } = new()
        {
            ApiKey = ApiKey,
            AuthDomain = AuthDomain,
            Providers = new FirebaseAuthProvider[]
            {
                new GoogleProvider().AddScopes("email"),
                new EmailProvider()
            },
        };
        private FirebaseAuthClient AuthClient { get; }
        private FirebaseAuth FirebaseAuth { get; }
        public FirebaseAuthService()
        {
            AuthClient = new(FirebaseAuthConfig);
            FirebaseAuth = FirebaseAuth.DefaultInstance;
        }
        /// <summary>
        /// Create user with email and ps
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<UserCredential> SignUp(string username, string password)
        {
            return await AuthClient.CreateUserWithEmailAndPasswordAsync(username, password);
        }
        public async Task SignInWithGoogle()
        {

        }
    }
}
