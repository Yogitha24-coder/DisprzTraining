using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DisprzTraining.Data;
using DisprzTraining.Models;
using DisprzTraining.DTOs;
using System.Security.Claims;

namespace DisprzTraining.Controllers
{
    [ApiController]
    [Route("appointments")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentsContext _context;

        public AppointmentsController(AppointmentsContext context)
        {
            _context = context;
        }

        // ✅ Safe helper to extract userId from JWT
private int GetUserId()
{
    var claim = User.FindFirst("userId") ?? User.FindFirst("id");

    if (claim == null)
        throw new UnauthorizedAccessException("User id not found in token");

    return int.Parse(claim.Value);
}


        // ✅ GET all (with optional filter by date)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointments([FromQuery] DateTime? date)
        {
            var userId = GetUserId();
            var query = _context.Appointments.Where(a => a.UserId == userId);

            if (date.HasValue)
                query = query.Where(a => a.Date.Date == date.Value.Date);

            return await query.ToListAsync();
        }

        // ✅ GET by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<Appointment>> GetAppointment(int id)
        {
            var userId = GetUserId();
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment == null)
                return NotFound(new { message = $"Appointment {id} not found for this user" });

            return appointment;
        }

        // ✅ POST (Create new appointment)
        [HttpPost]
public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
{
    if (dto == null)
        return BadRequest("Appointment data is missing");

    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var userId = GetUserId();

    var appointment = new Appointment
    {
        Title = dto.Title,
        Description = dto.Description,
        Date = dto.Date,
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        UserId = userId
    };

    _context.Appointments.Add(appointment);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, appointment);
}


        // ✅ PUT (Update appointment)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] CreateAppointmentDto dto)
        {
            var userId = GetUserId();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (appointment == null)
                return NotFound(new { message = $"Appointment {id} not found for this user" });

            // update fields
            appointment.Title = dto.Title;
            appointment.Description = dto.Description;
            appointment.Date = dto.Date;
            appointment.StartTime = dto.StartTime;
            appointment.EndTime = dto.EndTime;

            _context.Entry(appointment).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE (Remove appointment)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var userId = GetUserId();

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            if (appointment == null)
                return NotFound(new { message = $"Appointment {id} not found for this user" });

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Appointment {id} deleted" });
        }
    }
}
