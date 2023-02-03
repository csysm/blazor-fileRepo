using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Models.UserModels
{
    public class UserAddViewModel
    {
        [Required]
        [MaxLength(10)]
        [MinLength(2)]
        public string UserName { get; set; }

        [Required]
        //[EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        [MinLength(6)]
        public string Password { get; set; }
    }
}
