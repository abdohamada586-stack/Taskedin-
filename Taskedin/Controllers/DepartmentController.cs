using Microsoft.AspNetCore.Mvc;
using Taskedin.Domain.Entities;
using Taskedin.Domain.Models;
using Taskedin.Service.Interfaces;
using Taskedin.Domain.Common;

namespace Taskedin.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Department department)
    {
        var result = await _departmentService.CreateAsync(department);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        
        var result = await _departmentService.GetByIdAsync(id);
        return Ok(new ApiResponse<Department>
        {
            Success = true,
            Message = "Department retrieved successfully",
            Data = result
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Department department)
    {
        var result = await _departmentService.UpdateAsync(id, department);
        return Ok(result);
    }

    [HttpGet("{departmentId}/profiles")]
    public async Task<IActionResult> GetProfiles([FromQuery] GetProfilesRequest request)
    {
        var result = await _departmentService.GetProfilesAsync(request);
        return Ok(result);
    }
}