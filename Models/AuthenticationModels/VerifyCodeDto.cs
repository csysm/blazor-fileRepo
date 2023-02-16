using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.AuthenticationModels
{
    public class VerifyCodeDto
    {
        public VerifyCodeDto(byte[] image,string verifyKey)
        {
            this.image = image;
            this.verifyKey = verifyKey;
        }

        byte[]? image;

        string? verifyKey;
    }
}
