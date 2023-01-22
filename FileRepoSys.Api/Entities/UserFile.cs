using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Entities
{
    public class UserFile
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(30)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(256)]
        public string FilePath { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(10)]
        public string FileType { get; set; }

        [Required]
        public double FileSize { get; set; }

        [Required]
        [MaxLength(256)]
        public string Hash { get; set; }

        [MaxLength(50)]
        public string Profile { get; set; }//简介

        public DateTime CreateTime { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
