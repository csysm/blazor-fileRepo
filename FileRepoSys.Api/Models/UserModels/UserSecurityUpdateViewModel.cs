using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Models.UserModels
{
    public class UserSecurityUpdateViewModel
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        public string OldPassword { get; set; }

        [MinLength(6)]
        [MaxLength(20)]
        [Required]
        public string  NewPassword{ get; set; }
    }
}
