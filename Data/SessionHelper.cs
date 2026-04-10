namespace TransportationManagement.Data
{
    public static class SessionHelper
    {
        public static bool IsLoggedIn(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetString("Username") != null;

        public static string GetRole(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetString("Role") ?? "";

        public static string GetUsername(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetString("Username") ?? "";
    }
}
