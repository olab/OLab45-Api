using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using OLab.Api.Model;

namespace OLab.Api.Tests;

public class OLabDBContext_SystemCountersTests
{
    // Helper to create a mock DbSet<T> from an IEnumerable<T>
    private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> items) where T : class
    {
        var queryable = items.AsQueryable();

        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

        return mockSet;
    }

    private static SystemCounters CreateFullSystemCounter(
        uint id,
        int status,
        string imageableType = "map",
        uint imageableId = 123,
        string name = "Counter",
        string description = "desc",
        byte[] startValue = null,
        byte[] value = null,
        int? iconId = 7,
        string prefix = "pre",
        string suffix = "suf",
        sbyte? visible = 1,
        int? outOf = 100,
        int? isSystem = 0,
        DateTime? createdAt = null,
        DateTime? updatedAt = null)
    {
        return new SystemCounters
        {
            Id = id,
            Name = name,
            Description = description,
            StartValue = startValue ?? new byte[] { 1, 2, 3 },
            Value = value ?? new byte[] { 4, 5, 6 },
            IconId = iconId,
            Prefix = prefix,
            Suffix = suffix,
            Visible = visible,
            OutOf = outOf,
            Status = status,
            ImageableId = imageableId,
            ImageableType = imageableType,
            IsSystem = isSystem,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            UpdatedAt = updatedAt ?? DateTime.UtcNow
        };
    }

    [Fact]
    public void GetAll_ReturnsAllRecords_WithAllPropertiesSet()
    {
        // Arrange - seed two full SystemCounters
        var seed = new List<SystemCounters>
        {
            CreateFullSystemCounter(1, status: 0, name: "C1"),
            CreateFullSystemCounter(2, status: 1, name: "C2")
        };

        var mockSet = CreateMockDbSet(seed);

        var options = new DbContextOptions<OLabDBContext>();
        var mockContext = new Mock<OLabDBContext>(options);
        mockContext.Setup(c => c.SystemCounters).Returns(mockSet.Object);

        // Act
        var result = mockContext.Object.SystemCounters.ToList();

        // Assert - ensure items and properties present
        Assert.Equal(2, result.Count);

        var c1 = result.Single(r => r.Id == 1u);
        Assert.Equal("C1", c1.Name);
        Assert.NotNull(c1.StartValue);
        Assert.NotNull(c1.Value);
        Assert.Equal(0, c1.Status);
        Assert.Equal("map", c1.ImageableType);
        Assert.Equal(123u, c1.ImageableId);

        var c2 = result.Single(r => r.Id == 2u);
        Assert.Equal("C2", c2.Name);
    }

    [Fact]
    public void QueryByStatus_ReturnsMatchingRecords()
    {
        // Arrange - seed multiple counters with different status values
        var seed = new List<SystemCounters>
        {
            CreateFullSystemCounter(10, status: 5, name: "FiveA"),
            CreateFullSystemCounter(11, status: 5, name: "FiveB"),
            CreateFullSystemCounter(12, status: 9, name: "Nine")
        };

        var mockSet = CreateMockDbSet(seed);
        var options = new DbContextOptions<OLabDBContext>();
        var mockContext = new Mock<OLabDBContext>(options);
        mockContext.Setup(c => c.SystemCounters).Returns(mockSet.Object);

        // Act
        var fives = mockContext.Object.SystemCounters.Where(sc => sc.Status == 5).ToList();

        // Assert
        Assert.Equal(2, fives.Count);
        Assert.All(fives, sc => Assert.Equal(5, sc.Status));
        Assert.Contains(fives, sc => sc.Name == "FiveA");
        Assert.Contains(fives, sc => sc.Name == "FiveB");
    }

    [Fact]
    public void SingleOrDefault_ByImageableTypeAndId_ReturnsCorrectItem()
    {
        // Arrange - seed counters and ensure one matches ImageableType+ImageableId
        var seed = new List<SystemCounters>
        {
            CreateFullSystemCounter(20, status: 2, imageableType: "map", imageableId: 555, name: "Target"),
            CreateFullSystemCounter(21, status: 2, imageableType: "user", imageableId: 666, name: "Other")
        };

        var mockSet = CreateMockDbSet(seed);
        var options = new DbContextOptions<OLabDBContext>();
        var mockContext = new Mock<OLabDBContext>(options);
        mockContext.Setup(c => c.SystemCounters).Returns(mockSet.Object);

        // Act
        var found = mockContext.Object.SystemCounters
            .SingleOrDefault(sc => sc.ImageableType == "map" && sc.ImageableId == 555);

        // Assert
        Assert.NotNull(found);
        Assert.Equal("Target", found.Name);

        var notFound = mockContext.Object.SystemCounters
            .SingleOrDefault(sc => sc.ImageableType == "map" && sc.ImageableId == 999);
        Assert.Null(notFound);
    }
}