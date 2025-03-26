using Microsoft.EntityFrameworkCore;
using Moq;

public class MockDbSetHelper
{
  public static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> elements) where T : class
  {
    var queryable = elements.AsQueryable();
    var mockSet = new Mock<DbSet<T>>();

    mockSet.As<IQueryable<T>>().Setup( m => m.Provider ).Returns( queryable.Provider );
    mockSet.As<IQueryable<T>>().Setup( m => m.Expression ).Returns( queryable.Expression );
    mockSet.As<IQueryable<T>>().Setup( m => m.ElementType ).Returns( queryable.ElementType );
    mockSet.As<IQueryable<T>>().Setup( m => m.GetEnumerator() ).Returns( queryable.GetEnumerator() );

    mockSet.As<IAsyncEnumerable<T>>().Setup( m => m.GetAsyncEnumerator( It.IsAny<CancellationToken>() ) )
          .Returns( new TestAsyncEnumerator<T>( queryable.GetEnumerator() ) );

    return mockSet;
  }
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
  private readonly IEnumerator<T> _enumerator;

  public TestAsyncEnumerator(IEnumerator<T> enumerator)
  {
    _enumerator = enumerator;
  }

  public ValueTask DisposeAsync()
  {
    _enumerator.Dispose();
    return default;
  }

  public ValueTask<bool> MoveNextAsync()
  {
    return new ValueTask<bool>( _enumerator.MoveNext() );
  }

  public T Current => _enumerator.Current;
}
