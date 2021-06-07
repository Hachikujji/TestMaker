using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestMaker.UI.Services
{
    public interface ITokenHandler
    {
        public Task<bool> TryRefreshTokenAsync();
    }
}