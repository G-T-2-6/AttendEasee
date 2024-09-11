
using AttendEase.Models;

public interface IProjectRepository
{ 
    Project GetProjectByProjectId(int projectId);
    Project GetProjectByProjectCode(string projectCode);
    IEnumerable<string> GetAllByProjectCode();
    IEnumerable<int> GetProjectsIds();
    IEnumerable<Project> GetAllProjects();
    void AddProject(Project project);
    bool UpdateProject(Project project, string name, string location);
    bool RemoveProjectByProjectCode(string ProjectCode);

    void Save();
}

