using CahchingWebApiTest.Data;
using CahchingWebApiTest.Models;
using CahchingWebApiTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CahchingWebApiTest.Controllers;

[ApiController]
[Route("[controller]")]
public class DriversController : ControllerBase
{

    private readonly ILogger<DriversController> _logger;
    private readonly ICacheService _cacheService;
    private readonly AppDbContext _context;

    public DriversController(ILogger<DriversController> logger, ICacheService cacheService, AppDbContext context)
    {
        _logger = logger;
        _cacheService = cacheService;
        _context = context;
    }

    [HttpGet("drivers")]
    public async Task<IActionResult> Get()
    {
        //check cache data
        var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
        if (cacheData != null && cacheData.Count() > 0)
            return Ok(cacheData);

        cacheData = await _context.Drivers.ToListAsync();

        //ser expire time
        var expireTime = DateTimeOffset.Now.AddMinutes(5);
        _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expireTime);
        return Ok(cacheData);
    }

    [HttpPost("AddDriver")]
    public async Task<IActionResult> Post(Driver value)
    {
        var addObject = await _context.Drivers.AddAsync(value);
        var expireTime = DateTimeOffset.Now.AddMinutes(5);
        _cacheService.SetData<Driver>($"driver{value.id}", addObject.Entity, expireTime);
        await _context.SaveChangesAsync();
        return Ok(addObject.Entity);
    }
    [HttpDelete("DeleteDriver")]
    public async Task<IActionResult> Delete(int id)
    {
        var exist = await _context.Drivers.FirstOrDefaultAsync(x => x.id == id);
        if (exist != null)
        {
            _context.Remove(exist);
            _cacheService.RemoveData($"driver{id}");
            return NoContent();
        }
        return NotFound();
    }

}
