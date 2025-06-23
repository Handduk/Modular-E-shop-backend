using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularEshopApi.Data;
using ModularEshopApi.Models;
using AutoMapper;
using ModularEshopApi.Dto.User;

namespace ModularEshopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public UsersController(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ActionResult<UserDTO>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userList = users.Select(User => new UserDTO
            {
                Id = User.Id,
                Name = User.Name,
                Email = User.Email,
                Role = User.Role
            }).ToList();
            if (userList == null)
            {
                return NotFound("No users found");
            }
            return Ok(userList);
        }
        // GET: api/Users/1
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var userDto = _mapper.Map<UserDTO>(user);
            return Ok(userDto);
        }

        // GET: api/Users/Role/{email}
        [HttpGet("Role/{role}")]
        public async Task<ActionResult<List<UserDTO>>> GetUsersByRole(string role)
        {
            try
            {
                var users = await _context.Users.Where(u => u.Role.ToLower() == role.ToLower()).ToListAsync();
                if (users == null || !users.Any())
                {
                    return NotFound("No users found with the specified role");
                }
                var userDtos = _mapper.Map<List<UserDTO>>(users);
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromForm] User user)
        {
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDTO>(user);
                return CreatedAtAction("GetUser", new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }

        // POST: api/Users/reseller
        [HttpPost("reseller")]
        public async Task<ActionResult<UserDTO>> CreateReseller([FromForm] CreateResellerDTO dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
                {
                    return BadRequest("Name and Email are required");
                }

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    Role = "Reseller",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("defaultPassword") // Default password, should be changed by the user
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDTO>(user);
                return CreatedAtAction("GetUser", new { id = user.Id }, userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // Put: api/Users/1
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDTO>> UpdateUser(int id, [FromForm] UpdateUserDTO userDto)
        {
            try
            {
                if (userDto == null)
                {
                    return BadRequest("User data is required");
                }

                var userToEdit = await _context.Users.FindAsync(id);
                if (userToEdit == null)
                {
                    return NotFound("User not found");
                }

                userToEdit.Name = userDto.Name;
                userToEdit.Email = userDto.Email;
                userToEdit.Role = userDto.Role;

                if (!string.IsNullOrWhiteSpace(userDto.Password))
                {
                    userToEdit.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
                }

                _context.Entry(userToEdit).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<UserDTO>(userToEdit);
                return Ok(resultDto);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "internal server error: " + ex.Message);
            }
        }

        // DELETE: api/Users/1
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                var userDto = _mapper.Map<UserDTO>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<User>> LoginUser([FromBody] LoginDTO loginDTO)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower());
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
                {
                    return Unauthorized("Wrong email or password");
                }

                var userDto = _mapper.Map<UserDTO>(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
