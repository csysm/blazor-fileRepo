using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(10)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        public string Password { get; set; }
        
        public bool IsActive { get; set; }

        public double MaxCapacity { get; set; }

        public double CurrentCapacity { get; set; }

        public DateTime CreateTime { get; set; }

        public bool IsDeleted { get; set; }

        public IEnumerable<UserFile>? UserFiles { get; set; }
    }
}
