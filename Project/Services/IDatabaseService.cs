using System.Collections.Generic;

public interface IDatabaseService
{
    IEnumerable<Entity> GetEntities();
    Entity GetEntityById(string id); // Add this method signature
    IEnumerable<Entity> SearchEntities(string searchText);
    IEnumerable<Entity> FilterEntities(string gender, DateTime? startDate, DateTime? endDate, string[] countries);
}
