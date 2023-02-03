using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Models.UserModels
{
    public class UserProfileUpdateViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(10)]
        [MinLength(2)]
        public string UserName { get; set; }
    }
}
