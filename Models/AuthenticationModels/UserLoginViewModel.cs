using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Models.AuthenticationModels
{
    public class UserLoginViewModel
    {
        [Required]
        [Display(Name ="邮箱")]
        [StringLength(20, MinimumLength =3)]
        public string Email { get; set;}

        [Required]
        [Display(Name="密码")]
        [StringLength(20, MinimumLength = 3)]
        public string Password { get; set;}
    }
}
