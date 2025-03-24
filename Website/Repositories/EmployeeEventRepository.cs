﻿using Dapper;
using System.Data;
using Website.DTOs;
using Website.Models;
using Website.Persistence;

namespace Website.Repositories
{
    public class EmployeeEventRepository : RepositoryBase
    {
        public EmployeeEventRepository(IDbConnectionProvider connectionProvider) : base(connectionProvider)
        {
        }

        /// <summary>
        /// Creates the employeeEvent table, if it doesn't already exist.
        /// </summary>
        /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
        /// <returns>An awaitable task</returns>
        public async Task CreateTableIfNotExistsAsync(CancellationToken cancellationToken)
        {
            var command = new CommandDefinition(
                @"
                CREATE TABLE IF NOT EXISTS [EmployeeEvent] (                    
                    [Id] INTEGER PRIMARY KEY AUTOINCREMENT,
                    [EmployeeId] INTEGER NOT NULL,
                    [EventId] INTEGER NOT NULL,
                    FOREIGN KEY ([EmployeeId]) REFERENCES [Employee]([Id]) ON DELETE CASCADE,
                    FOREIGN KEY ([EventId]) REFERENCES [Event]([Id]) ON DELETE CASCADE
                );
                ",
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            await Connection.ExecuteAsync(command);
        }


        /// <summary>
        /// Gets all employeeEvents.
        /// </summary>
        /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
        /// <returns>An awaitable task whose result is the events found.</returns>
        public async Task<IReadOnlyCollection<EmployeeEventDTO>> GetAllAsync(int? eventId = null, int? employeeId = null, CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                @"
                SELECT  [EE].[Id],
                        [EVT].[Id] as EventId,
                        [EVT].[Description] AS EventDescription,
                        [EVT].[StartDateTime] AS EventStartDateTime,
                        [EVT].[EndDateTime] AS EventEndDateTime,
                        [EVT].[MaximumCapacity] AS EventMaximumCapacity,
                        [EMP].[Id] AS EmployeeId,
                        [EMP].[FirstName] AS EmployeeFirstName,
                        [EMP].[LastName] AS EmployeeLastName,
                        [EMP].[DateOfBirth] AS EmployeeDateOfBirth,
                        [EMP].[FavouriteDrink] AS EmployeeFavouriteDrink
                FROM    [EmployeeEvent] AS [EE]
                INNER JOIN [Employee] AS [EMP] ON EE.EmployeeId = EMP.Id
                INNER JOIN [Event] AS [EVT] ON EE.EventId = EVT.Id
                WHERE   (@EventId IS NULL OR EE.EventId = @EventId)
                AND     (@EmployeeId IS NULL OR EE.EmployeeId = @EmployeeId)
                ",
                parameters: new
                {
                    EventId = eventId,
                    EmployeeId = employeeId
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            var employeeEvents = await Connection.QueryAsync<EmployeeEventDTO>(command);

            return employeeEvents.ToArray();
        }

        public async Task<IReadOnlyCollection<Employee>> GetAllEmployeesNotInEventAsync(int eventId, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                  @"
                    SELECT  [EMP].[Id],
                            [EMP].[FirstName],
                            [EMP].[LastName],
                            [EMP].[DateOfBirth]
                    FROM    [Employee] AS [EMP]
                    WHERE   NOT EXISTS (
                                SELECT 1 
                                FROM [EmployeeEvent] AS [EE]
                                WHERE EE.EmployeeId = EMP.Id
                                AND EE.EventId = @EventId
                            )
                    AND     (@searchTerm IS NULL OR (FIRSTNAME || ' ' || LASTNAME) LIKE @searchPattern);
                    ",
                parameters: new
                {
                    EventId = eventId,
                    searchTerm,
                    searchPattern = $"{searchTerm}%"
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            var employees = await Connection.QueryAsync<Employee>(command);

           return employees.ToArray();
        }

        /// <summary>
        /// Gets all employeeEvents.
        /// </summary>
        /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
        /// <returns>An awaitable task whose result is the events found.</returns>
        public async Task<EmployeeEventDTO> GetOneAsync(int id, CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                @"
                SELECT  [EE].[Id],
                        [EVT].[Id] as EventId,
                        [EVT].[Description] AS EventDescription,
                        [EVT].[StartDateTime] AS EventStartDateTime,
                        [EVT].[EndDateTime] AS EventEndDateTime,
                        [EVT].[MaximumCapacity] AS EventMaximumCapacity,
                        [EMP].[Id] AS EmployeeId,
                        [EMP].[FirstName] AS EmployeeFirstName,
                        [EMP].[LastName] AS EmployeeLastName,
                        [EMP].[DateOfBirth] AS EmployeeDateOfBirth
                FROM    [EmployeeEvent] AS [EE]
                INNER JOIN [Employee] AS [EMP] ON EE.EmployeeId = EMP.Id
                INNER JOIN [Event] AS [EVT] ON EE.EventId = EVT.Id
                WHERE   (@Id = EE.Id)
                ",
                parameters: new
                {
                    Id = id,
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            var employeeEvent = await Connection.QuerySingleAsync<EmployeeEventDTO>(command);

            return employeeEvent;
        }

        public async Task<int> CreateAsync(EmployeeEvent employeeEvent, CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                @"
                INSERT INTO [EmployeeEvent]
                (
                    [EventId],
                    [EmployeeId]
                )
                VALUES
                (
                    @EventId,
                    @EmployeeId
                )
                RETURNING
                (
                    [Id]
                )
            ",
                parameters: new
                {
                    employeeEvent.EventId,
                    employeeEvent.EmployeeId,
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            var createdId = await Connection.QuerySingleAsync<int>(command);

            return createdId;
        }

        /// <summary>
        /// Determines whether an employeeEvent with the given ID exists.
        /// </summary>
        /// <param name="id">The ID of the employeeEvent to check.</param>
        /// <param name="cancellationToken">A token which can be used to cancel asynchronous operations.</param>
        /// <returns>An awaitable task whose result indicates whether the employeeEvent exists.</returns>
        public async ValueTask<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            var q = "Select * from EmployeeEvent";

            var ee = await Connection.QueryAsync(q, cancellationToken);

            var command = new CommandDefinition(
                @"SELECT  CAST(CASE
                     WHEN EXISTS (
                                     SELECT 1
                                     FROM   [EmployeeEvent] AS [E]
                                     WHERE  [E].Id = @Id
                                 ) THEN 1
                     ELSE 0
                 END AS bit);
            ",
                parameters: new
                {
                    Id = id
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            return await Connection.ExecuteScalarAsync<bool>(command);
        }


        /// <summary>
        /// Deletes an existing employeeEvent.
        /// </summary>
        /// <param name="eventId">The ID of the employeeEvent to delete.</param>
        /// <returns>An awaitable task.</returns>
        public async Task DeleteAsync(int eventId, CancellationToken cancellationToken = default)
        {
            var command = new CommandDefinition(
                @"
                DELETE FROM [EmployeeEvent]
                WHERE   [Id] = @Id;
            ",
                parameters: new
                {
                    Id = eventId
                },
                commandType: CommandType.Text,
                cancellationToken: cancellationToken);

            await Connection.ExecuteAsync(command);
        }
    }
}
