using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class MockDatabaseService : IDatabaseService
{
    private int _maxRetryAttempts = 3;
    private TimeSpan _initialDelay = TimeSpan.FromSeconds(1);
    private TimeSpan _maxDelay = TimeSpan.FromSeconds(10);
    private double _backoffMultiplier = 2;

    private List<Entity> _entities;

    public MockDatabaseService()
    {
        // Initialize or load entities from mock database
        _entities = new List<Entity>();
    }

    public IEnumerable<Entity> GetEntities()
    {
        return _entities;
    }

    public Entity GetEntityById(string id)
    {
        return _entities.FirstOrDefault(e => e.Id == id);
    }

    public IEnumerable<Entity> SearchEntities(string searchText)
    {
        // Implement search logic across entity fields
        return _entities.Where(e =>
            e.Names.Any(n => (n.FirstName + " " + n.MiddleName + " " + n.Surname).Contains(searchText)) ||
            e.Addresses.Any(a => (a.AddressLine + " " + a.Country).Contains(searchText))
        );
    }

    public IEnumerable<Entity> FilterEntities(string gender, DateTime? startDate, DateTime? endDate, string[] countries)
    {
        // Implement filtering logic based on provided parameters
        var filteredEntities = _entities.AsEnumerable();

        if (!string.IsNullOrEmpty(gender))
            filteredEntities = filteredEntities.Where(e => e.Gender == gender);

        if (startDate.HasValue)
            filteredEntities = filteredEntities.Where(e => e.Dates.Any(d => d.DateValue >= startDate.Value));

        if (endDate.HasValue)
            filteredEntities = filteredEntities.Where(e => e.Dates.Any(d => d.DateValue <= endDate.Value));

        if (countries != null && countries.Length > 0)
            filteredEntities = filteredEntities.Where(e => e.Addresses.Any(a => countries.Contains(a.Country)));

        return filteredEntities;
    }

    public void AddEntity(Entity entity)
    {
        // Retry mechanism for adding entity
        Retry(() =>
        {
            _entities.Add(entity);
        });
    }

    public void UpdateEntity(Entity entity)
    {
        // Retry mechanism for updating entity
        Retry(() =>
        {
            var existingEntity = _entities.FirstOrDefault(e => e.Id == entity.Id);
            if (existingEntity != null)
            {
                existingEntity = entity;
            }
            else
            {
                throw new InvalidOperationException("Entity not found.");
            }
        });
    }

    public void DeleteEntity(string id)
    {
        // Retry mechanism for deleting entity
        Retry(() =>
        {
            var entityToDelete = _entities.FirstOrDefault(e => e.Id == id);
            if (entityToDelete != null)
            {
                _entities.Remove(entityToDelete);
            }
            else
            {
                throw new InvalidOperationException("Entity not found.");
            }
        });
    }

    private void Retry(Action action)
    {
        int attempt = 0;
        do
        {
            try
            {
                attempt++;
                action.Invoke();
                return; // If successful, exit the method
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                if (attempt < _maxRetryAttempts)
                {
                    TimeSpan delay = CalculateDelay(attempt);
                    Console.WriteLine($"Retrying in {delay.TotalSeconds} seconds...");
                    Thread.Sleep(delay);
                }
                else
                {
                    Console.WriteLine($"Max retry attempts reached. Operation failed.");
                    throw; // Throw the exception if max retry attempts reached
                }
            }
        } while (attempt < _maxRetryAttempts);
    }

    private TimeSpan CalculateDelay(int attempt)
    {
        // Calculate delay using exponential backoff strategy
        double delayInSeconds = Math.Min(_initialDelay.TotalSeconds * Math.Pow(_backoffMultiplier, attempt - 1), _maxDelay.TotalSeconds);
        return TimeSpan.FromSeconds(delayInSeconds);
    }
}
