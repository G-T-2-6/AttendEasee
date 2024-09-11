using AttendEase.Data;
using AttendEase.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDBContext _context;

    public ProjectRepository(ApplicationDBContext context)
    {
        _context = context;
    }

    public IEnumerable<string> GetAllByProjectCode()
    {
        return _context.Projects.Select(p => p.ProjectCode).ToList();
    }

    public Project GetProjectByProjectId(int projectId)
    {
        return _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
    }

    public Project GetProjectByProjectCode(string projectCode)
    {
        return _context.Projects.FirstOrDefault(p => p.ProjectCode == projectCode);
    }

    public IEnumerable<int> GetProjectsIds()
    {
        return _context.Projects.Select(p => p.ProjectId).ToList();
    }

    public IEnumerable<Project> GetAllProjects() {
        return _context.Projects.ToList();
    }

    public void AddProject(Project project)
    {
        _context.Projects.Add(project);
        _context.SaveChanges();
    }
    public bool UpdateProject(Project project, string name, string location)
    {
        bool isUpdated = false;
        if (name != null)
        {
            project.Name = name;
            isUpdated = true;
        }

        if (location != null)
        {
            project.Location = location;
            isUpdated = true;
        }

        if (isUpdated)
        {
            _context.SaveChanges();
            return true;
        }
        return false;
    }

    public bool RemoveProjectByProjectCode(string projectCode)
    {
        var fetched = GetProjectByProjectCode(projectCode);
        if(fetched != null)
        {
            bool isUpdated = false;
            if (projectCode != null)
            {
                _context.Projects.Remove(fetched);
                isUpdated = true;
            }

            if (isUpdated)
            {
                _context.SaveChanges();
                return true;
            }
        }
        return false;
    }

    public void Save()
    {
        _context.SaveChanges();
    }
}
