using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerse.Sharde.CommenResult
{
    public enum ErrorType
    {
        failure=0, 
        validatio=1,
        NotFound=2, 
        Unauthorized=3,
        Forbidden=4,    
        InvalidCredentials=5,
        
    }
}
