using System.ComponentModel.DataAnnotations;

namespace FileRepoSys.Api.Entities
{
    public class UserFile
    {
        public Guid Id { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(64)]
        public string FileName { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(64)]
        public string FileStorageName { get; set; }

        [Required]
        [MaxLength(256)]
        public string FilePath { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(128)]
        public string MimeType { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(10)]
        public string Suffix { get; set; }

        [Required]
        public long FileSize { get; set; }

        public DateTime CreateTime { get; set; }=DateTime.Now;

        public bool IsDeleted { get; set; }=false;

        public Guid UserId { get; set; }
        public User User { get; set; }

        //[MaxLength(50)]
        //public string Profile { get; set; }//简介
    }
}
