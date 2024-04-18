namespace BidenSurfer.WebApi
{
    public interface ISecurityContextAccessor
    {
        public long UserId { get; }
        public int Role { get; }
    }
}
