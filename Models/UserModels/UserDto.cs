namespace FileRepoSys.Api.Models.UserModels
{
    public class UserDto
    {
        public Guid Id { get; set; }
        
        public string UserName { get; set; }

        public string Email { get; set; }

        public double MaxCapacity { get; set; }

        public double CurrentCapacity { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
