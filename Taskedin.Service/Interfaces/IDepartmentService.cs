using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskedin.Domain.Entities;
using Taskedin.Domain.Models;


namespace Taskedin.Service.Interfaces
{
    public interface IDepartmentService
    {
        Task<Department> CreateAsync(Department department);
        Task<Department?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, Department department);
        Task<List<Profile>> GetProfilesAsync(GetProfilesRequest request);
    }
}
