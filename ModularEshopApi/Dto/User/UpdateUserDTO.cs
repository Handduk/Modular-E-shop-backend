namespace ModularEshopApi.Dto.User
{
    public class UpdateUserDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? Password { get; set; }
    }
}
