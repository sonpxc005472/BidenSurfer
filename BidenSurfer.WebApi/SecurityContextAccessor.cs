using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;

namespace BidenSurfer.WebApi
{
    public class SecurityContextAccessor : ISecurityContextAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SecurityContextAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public long UserId
        {
            get
            {
                UserDto? user = _httpContextAccessor?.HttpContext?.Items["User"] as UserDto;                
                return user != null ? user.Id : 0;
            }
        }

        public int Role
        {
            get
            {
                UserDto? user = _httpContextAccessor?.HttpContext?.Items["User"] as UserDto;
                return user != null ? user.Role : 0;
            }
        }        
    }
}
