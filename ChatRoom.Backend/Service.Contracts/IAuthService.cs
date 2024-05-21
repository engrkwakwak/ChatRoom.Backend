using Shared.DataTransferObjects.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts {
    public interface IAuthService {
        Task<bool> ValidateUser(SignInDto userForAuth);
        string CreateToken();
    }
}
