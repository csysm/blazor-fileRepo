﻿namespace FileRepoSys.Api.Models
{
    public class UserFileDto
    {
        public Guid Id { get; set; }

        public string FileName { get; set; }

        public string FileType { get; set; }

        public string Suffix { get; set; }

        public double FileSize { get; set; }

        public string Profile { get; set; }//简介

        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}
