using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.UserFileModels
{
    public  class UserFilesRequest
    {
        public string UserId { get; set; }
        public int PageIndex { get; set; } = 1;
    }
}
