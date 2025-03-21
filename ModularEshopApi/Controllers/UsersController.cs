using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModularEshopApi.Data;
using ModularEshopApi.Models;

namespace ModularEshopApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly ApiDbContext _context;

        public UsersController(ApiDbContext context)
        {
            _context = context;
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
            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetUser", new { id = user.Id }, user);
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
                userToEdit.Password = user.Password;
                userToEdit.Role = user.Role;

                _context.Entry(userToEdit).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    return Ok(userToEdit);
                
            } catch (Exception ex)
            {
                return StatusCode(500, "internal server error: " + ex.Message);
            }
        }
    }
}
