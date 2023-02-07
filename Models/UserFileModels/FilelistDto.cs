using FileRepoSys.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.UserFileModels
{
    public class FilelistDto
    {
        public List<UserFileDto> CurrentPageFiles { get; set; }

        public int TotalCount { get; set; }
    }
}
