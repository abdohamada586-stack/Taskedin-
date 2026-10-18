using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taskedin.Domain.Entities;
using Taskedin.Infrastructure.Models;



namespace Taskedin.Service.Interfaces
{
    public interface IDepartmentService
    {
        Task<Department> CreateAsync(Department department);
        Task<Department?> GetByIdAsync(int id);

        /// <summary>
        /// Updates the department name. Returns false when the department does not exist.
        /// </summary>
        Task<bool> UpdateAsync(int id, Department department);

        /// <summary>
        /// Returns one page of profiles for a department, bounded by the optional date range.
        /// </summary>
        Task<List<Profile>> GetProfilesAsync(GetProfilesRequest request);
    }
}
