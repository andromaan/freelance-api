namespace BLL;

public static class Settings
{
    public static class Roles
    {
        public const string AnyAuthenticated = "AnyAuthenticated";
        public const string AdminOrModerator = "AdminOrModerator";
        public const string AdminOrEmployer = "AdminOrEmployer";
        public const string AdminOrFreelancer = "AdminOrFreelancer";
        
        public const string AdminRole = "admin";
        public const string AdminId = "11111111-1111-1111-1111-111111111111";
        public const string EmployerRole = "employer";
        public const string FreelancerRole = "freelancer";
        public const string ModeratorRole = "moderator";
    
        public static readonly List<string> ListOfRoles = new()
        {
            AdminRole,
            EmployerRole,
            FreelancerRole,
            ModeratorRole
        };
    }
    
    public static class ImagesPathSettings
    {
        public static string HtmlPagesPath = "templates";
        public static string UserImagesPath = "images/user";

        public const string ImagesPath = "wwwroot/images";
        public const string StaticFileRequestPath = "images";

        public const string UserAvatarImagesPath = "wwwroot/images/users/avatars";
        public const string UserAvatarImagesPathForUrl = "images/users/avatars";
    }
}