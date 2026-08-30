using Microsoft.EntityFrameworkCore;
using Moq;
using OLab.Api.Model;
using System.Collections;
using System.Reflection;

namespace OLab.Test;

public static class OlabDbContextTest
{
  // Helper to create a mock DbSet<T> from an IEnumerable<T>
  public static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> items) where T : class
  {
    var queryable = items.AsQueryable();

    var mockSet = new Mock<DbSet<T>>();
    mockSet.As<IQueryable<T>>().Setup( m => m.Provider ).Returns( queryable.Provider );
    mockSet.As<IQueryable<T>>().Setup( m => m.Expression ).Returns( queryable.Expression );
    mockSet.As<IQueryable<T>>().Setup( m => m.ElementType ).Returns( queryable.ElementType );
    mockSet.As<IQueryable<T>>().Setup( m => m.GetEnumerator() ).Returns( () => queryable.GetEnumerator() );

    return mockSet;
  }

  // Create a single sample instance of T and populate common properties with deterministic sample values.
  public static T CreateSample<T>(int seed = 1) where T : class, new()
  {
    var instance = new T();
    var type = typeof( T );

    foreach ( var prop in type.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
    {
      if ( !prop.CanWrite )
        continue;

      var propType = prop.PropertyType;
      var underlying = Nullable.GetUnderlyingType( propType );
      var targetType = underlying ?? propType;

      // strings
      if ( targetType == typeof( string ) )
      {
        prop.SetValue( instance, $"Sample{prop.Name}{seed}" );
        continue;
      }

      // byte arrays
      if ( targetType == typeof( byte[] ) )
      {
        prop.SetValue( instance, new byte[] { (byte)(seed & 0xFF) } );
        continue;
      }

      // DateTime
      if ( targetType == typeof( DateTime ) )
      {
        prop.SetValue( instance, DateTime.UtcNow.AddTicks( seed ) );
        continue;
      }

      // bool
      if ( targetType == typeof( bool ) )
      {
        prop.SetValue( instance, seed % 2 == 0 );
        continue;
      }

      // Collections - set to empty List<TItem> for most navigation collections
      if ( typeof( IEnumerable ).IsAssignableFrom( targetType ) && targetType.IsGenericType )
      {
        try
        {
          var gen = targetType.GetGenericTypeDefinition();
          var args = targetType.GetGenericArguments();
          if ( (gen == typeof( ICollection<> ) || gen == typeof( IEnumerable<> ) || gen == typeof( IList<> )) && args.Length == 1 )
          {
            var listType = typeof( List<> ).MakeGenericType( args[ 0 ] );
            var listInstance = Activator.CreateInstance( listType );
            prop.SetValue( instance, listInstance );
            continue;
          }
        }
        catch
        {
          // ignore collection initialization errors and continue
        }
      }

      // skip complex reference types (entity navigation properties that are classes)
      if ( targetType.IsClass && targetType != typeof( string ) && targetType != typeof( byte[] ) )
        continue;

      // Enums
      if ( targetType.IsEnum )
      {
        var values = Enum.GetValues( targetType );
        if ( values.Length > 0 )
          prop.SetValue( instance, values.GetValue( 0 ) );
        continue;
      }

      // Numeric and other primitive-like types
      object? value = null;
      try
      {
        if ( targetType == typeof( int ) )
          value = seed;
        else if ( targetType == typeof( uint ) )
          value = (uint)seed;
        else if ( targetType == typeof( long ) )
          value = (long)seed;
        else if ( targetType == typeof( ulong ) )
          value = (ulong)seed;
        else if ( targetType == typeof( short ) )
          value = (short)seed;
        else if ( targetType == typeof( ushort ) )
          value = (ushort)seed;
        else if ( targetType == typeof( byte ) )
          value = (byte)(seed & 0xFF);
        else if ( targetType == typeof( sbyte ) )
          value = (sbyte)(seed % sbyte.MaxValue);
        else if ( targetType == typeof( decimal ) )
          value = (decimal)seed;
        else if ( targetType == typeof( float ) )
          value = (float)seed;
        else if ( targetType == typeof( double ) )
          value = (double)seed;
        else if ( targetType == typeof( char ) )
          value = (char)('A' + (seed % 26));
        else if ( targetType == typeof( Guid ) )
          value = Guid.NewGuid();
        else
        {
          // attempt generic conversion
          value = Convert.ChangeType( seed, targetType );
        }
      }
      catch
      {
        value = null;
      }

      if ( value != null )
      {
        // if property is nullable, Convert.ChangeType may be needed for the underlying type
        if ( underlying != null )
        {
          try
          {
            var boxed = Convert.ChangeType( value, underlying );
            prop.SetValue( instance, boxed );
          }
          catch
          {
            prop.SetValue( instance, value );
          }
        }
        else
        {
          prop.SetValue( instance, value );
        }
      }
    }

    // ensure common Id property is set (uint/int/long/string/Guid) if present
    var idProp = type.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                     .FirstOrDefault( p => string.Equals( p.Name, "Id", StringComparison.OrdinalIgnoreCase ) && p.CanWrite );
    if ( idProp != null )
    {
      var idType = Nullable.GetUnderlyingType( idProp.PropertyType ) ?? idProp.PropertyType;
      try
      {
        if ( idType == typeof( uint ) )
          idProp.SetValue( instance, (uint)seed );
        else if ( idType == typeof( int ) )
          idProp.SetValue( instance, seed );
        else if ( idType == typeof( long ) )
          idProp.SetValue( instance, (long)seed );
        else if ( idType == typeof( Guid ) )
          idProp.SetValue( instance, Guid.NewGuid() );
        else if ( idType == typeof( string ) )
          idProp.SetValue( instance, seed.ToString() );
      }
      catch
      {
        // ignore id set failures
      }
    }

    return instance;
  }

  // Create multiple sample instances
  public static List<T> CreateMany<T>(int count) where T : class, new()
  {
    var list = new List<T>( count );
    for ( var i = 1; i <= count; i++ )
      list.Add( CreateSample<T>( i ) );
    return list;
  }

  // Shortcut to create a mocked DbSet<T> using existing test helper
  public static Mock<DbSet<T>> CreateMockDbSetFor<T>(int count) where T : class, new()
  {
    var items = CreateMany<T>( count );
    return OlabDbContextTest.CreateMockDbSet( items );
  }

  /* Pseudocode / Plan:
  1. Method: CreateMockDbContextWithDbSet<T>(IEnumerable<T> items, string? dbSetPropertyName = null)
     - T : class
  2. Create a mocked DbSet<T> using existing CreateMockDbSet(items).
  3. Create a Mock<OLabDBContext> instance (pass default DbContextOptions).
  4. Setup the context so that Set<T>() returns the mocked DbSet (mockContext.Setup(c => c.Set<T>()).Returns(mockSet.Object)).
  5. Locate a DbSet<T> property on OLabDBContext:
     - If dbSetPropertyName supplied, try to find that property.
     - Otherwise find the first public instance property whose PropertyType == typeof(DbSet<T>).
  6. If a property is found, construct an Expression<Func<OLabDBContext, DbSet<T>>> for that property using System.Linq.Expressions.
  7. Use reflection to call Mock<OLabDBContext>.SetupGet<DbSet<T>>(expression) and then call Returns(mockSet.Object) on the setup.
     - This avoids compile-time knowledge of the property name and supports any DbSet property.
  8. Return the configured Mock<OLabDBContext>.
  Notes:
  - Use reflection and expression-tree construction to avoid requiring compile-time property access.
  - Keep method generic and optional property name for explicit selection.
  */

  public static Mock<OLabDBContext> CreateMockDbContextWithDbSet<T>(IEnumerable<T> items, string? dbSetPropertyName = null) where T : class
  {
    // Create mock DbSet<T>
    var mockSet = CreateMockDbSet( items );

    // Create mock DbContext
    var options = new Microsoft.EntityFrameworkCore.DbContextOptions<OLabDBContext>();
    var mockContext = new Mock<OLabDBContext>( options );

    // Setup DbContext.Set<T>() to return our mock set
    mockContext.Setup( c => c.Set<T>() ).Returns( mockSet.Object );

    // Attempt to find a DbSet<T> property on OLabDBContext
    var dbSetType = typeof( Microsoft.EntityFrameworkCore.DbSet<> ).MakeGenericType( typeof( T ) );
    var ctxType = typeof( OLabDBContext );

    System.Reflection.PropertyInfo? prop = null;
    if ( !string.IsNullOrEmpty( dbSetPropertyName ) )
    {
      prop = ctxType.GetProperty( dbSetPropertyName!, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy );
      if ( prop != null && prop.PropertyType != dbSetType )
        prop = null; // ignore if type doesn't match
    }

    if ( prop == null )
    {
      prop = ctxType.GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance )
                    .FirstOrDefault( p => p.PropertyType == dbSetType );
    }

    if ( prop != null && prop.CanRead )
    {
      // Build a typed expression: ctx => ctx.TheProperty
      var param = System.Linq.Expressions.Expression.Parameter( ctxType, "ctx" );
      var body = System.Linq.Expressions.Expression.Property( param, prop );
      var delegateType = typeof( Func<,> ).MakeGenericType( ctxType, prop.PropertyType );
      var lambda = System.Linq.Expressions.Expression.Lambda( delegateType, body, param );

      // Find the generic SetupGet<TProperty>(Expression<Func<TMock, TProperty>>) method
      var setupGetMethod = typeof( Mock<OLabDBContext> ).GetMethods()
                              .Where( m => m.Name == "SetupGet" && m.IsGenericMethodDefinition && m.GetParameters().Length == 1 )
                              .FirstOrDefault();

      if ( setupGetMethod != null )
      {
        var genericSetupGet = setupGetMethod.MakeGenericMethod( prop.PropertyType );
        var setupResult = genericSetupGet.Invoke( mockContext, new object[] { lambda } );

        if ( setupResult != null )
        {
          // Call Returns(propertyValue) on the ISetupGetter<,> returned by SetupGet
          var returnsMethod = setupResult.GetType().GetMethod( "Returns", new Type[] { prop.PropertyType } );
          if ( returnsMethod != null )
          {
            returnsMethod.Invoke( setupResult, new object[] { mockSet.Object } );
          }
        }
      }
    }

    return mockContext;
  }

}
