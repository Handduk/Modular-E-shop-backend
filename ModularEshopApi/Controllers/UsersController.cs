using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularEshopApi.Data;
using ModularEshopApi.Models;
using AutoMapper;
using ModularEshopApi.Dto;

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

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDTO>(user);
                return CreatedAtAction("GetUser", new { id = user.Id }, userDto);
            } catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex);
            }
        }

        // Put: api/Users/1
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(int id, User user)
        {
            try
            {
                var userToEdit = await _context.Users.FindAsync(id);
                if (userToEdit == null)
                {
                    return NotFound("User not found");
                }
                if (userToEdit.Id != id)
                {
                    return BadRequest("Id mismatch");
                }

                userToEdit.Name = user.Name;
                userToEdit.Email = user.Email;
                userToEdit.Role = user.Role;

                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    userToEdit.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                }

                _context.Entry(userToEdit).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDTO>(userToEdit);
                return Ok(userDto);
                
            } catch (Exception ex)
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
                if(user == null)
                {
                    return NotFound();
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                var userDto = _mapper.Map<UserDTO>(user);
                return Ok(userDto);
            } catch (Exception ex)
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
                if(user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
                {
                    return Unauthorized("Wrong email or password");
                }

                var userDto = _mapper.Map<UserDTO>(user);
                return Ok(userDto);
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
