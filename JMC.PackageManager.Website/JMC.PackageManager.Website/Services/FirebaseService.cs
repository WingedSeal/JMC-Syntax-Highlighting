namespace JMC.PackageManager.Website.Services
{
    public abstract class FirebaseService
    {
        protected static string ApiKey = Environment.GetEnvironmentVariable("FIREBASE_APIKEY")!;
        protected static string Bucket = Environment.GetEnvironmentVariable("FIREBASE_BUCKET")!;
        protected static string AuthDomain = Environment.GetEnvironmentVariable("FIREBASE_AUTHDOMAIN")!;
    }
}
