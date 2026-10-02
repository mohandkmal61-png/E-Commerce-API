using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Commerse.Sharde.CommenResult
{
    public class Error
    {
        public string Code { get;  }
        public string Description { get; }
        
        public ErrorType Type { get; } 


        private  Error(string code, string description, ErrorType type)
        {
            Code = code;
            Description = description;  
            Type = type;
        }


        public static Error Failure(string code = "Genral Failure", string description="Genral Failure Occurde")
        {
            return new Error(code, description, ErrorType.failure);
        }

        public static Error Validation(string code = "Validation Failure", string description = "Validation Failure Occurde")
        {
            return new Error(code, description, ErrorType.validatio);
        }

        public static Error NotFound(string code = "Not Found", string description = "Resource Not Found")
        {
            return new Error(code, description, ErrorType.NotFound);
        }

        public static Error Unauthorized(string code = "Unauthorized", string description = "Authentication is required")
        {
            return new Error(code, description, ErrorType.Unauthorized);
        }

    }
}
