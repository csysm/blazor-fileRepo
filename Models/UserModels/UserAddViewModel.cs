using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Models.UserModels
{
    public class UserAddViewModel
    {
        [Required]
        [Display(Name = "昵称")]
        [MaxLength(10)]
        [MinLength(2)]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "邮箱")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "密码")]
        [MaxLength(20)]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "验证码")]
        [StringLength(4)]
        public string VerifyCode { get; set; }

        [Required]
        public string VerifyKey { get; set; }
    }
}
