using Microsoft.EntityFrameworkCore;
using Taskedin.Domain.Entities;
using Taskedin.Infrastructure.Models;
using Taskedin.Persistence.Context;
using Taskedin.Service.Interfaces;

namespace Taskedin.Service.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly TaskedinDbContext _db;

        public DepartmentService(TaskedinDbContext db)
        {
            _db = db;
        }

        public async Task<Department> CreateAsync(Department department)
        {
            _db.Departments.Add(department);
            await _db.SaveChangesAsync();
            return department;
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _db.Departments
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> UpdateAsync(int id, Department department)
        {
            var existing = await _db.Departments.FindAsync(id);

            if (existing == null)
                return false;

            existing.Name = department.Name;
            

            await _db.SaveChangesAsync();
            return true;
        }

        // Returns one page of the department's profiles, optionally bounded by a creation date range.
        public async Task<List<Profile>> GetProfilesAsync(
            GetProfilesRequest request)
        {
            var query = _db.Profiles
                .Where(p => p.DepartmentId == request.DepartmentId);

            if (request.FromDate.HasValue)
                query = query.Where(p => p.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(p => p.CreatedAt <= request.ToDate.Value);

            return await query
                .OrderBy(p => p.Id)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
        }
    }
}