using BidenSurfer.Infras;

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
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.USER_CLAIM_TYPE);
                return claim != null ? long.Parse(claim.Value) : 0;
            }
        }

        public int Role
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.ROLE_CLAIM_TYPE);
                return claim != null ? int.Parse(claim.Value) : 0;
            }
        }        
    }
}
