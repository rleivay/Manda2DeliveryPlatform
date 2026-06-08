// ════════════════════════════════════════════════════════════════════════════
// ARCHIVO: MockDbSetHelper.cs
// NAMESPACE: Manda2.Tests
// CARPETA SUGERIDA: Manda2.Tests/Helpers/MockDbSetHelper.cs
//
// PROPÓSITO:
//   Helper compartido para crear mocks de DbSet<T> compatibles con
//   Entity Framework Core (EF Core) en pruebas unitarias con Moq.
//
//   Problema que resuelve:
//     EF Core usa IQueryable<T> internamente. Cuando mockeamos un DbSet<T>
//     con Moq directamente, las llamadas a .FirstOrDefaultAsync(),
//     .ToListAsync(), .Where(), .Include() fallan porque el proveedor
//     de LINQ no es el de EF Core sino el de memoria.
//
//   Solución:
//     Usamos TestAsyncQueryProvider + TestAsyncEnumerable para simular
//     el proveedor asíncrono de EF Core sobre una List<T> en memoria.
//     Esto permite que los handlers usen await _db.Drivers.FirstOrDefaultAsync(...)
//     sin cambiar una sola línea del código de producción.
//
// DEPENDENCIAS:
//   - Moq (NuGet)
//   - Microsoft.EntityFrameworkCore (NuGet)
//   - xUnit (NuGet)
//
// USO:
//   var mockSet = MockDbSetHelper.CreateMockDbSet(new List<Driver> { driver });
//   _mockContext.Setup(c => c.Drivers).Returns(mockSet.Object);
//
// NOTA IMPORTANTE:
//   .Include() en EF Core es ignorado silenciosamente cuando se usa este helper.
//   Para simular navegaciones (ej: group.DispatchAttempts), debes poblar
//   manualmente las propiedades de navegación en el objeto de prueba.
//   Ejemplo:
//     var group = new OrderGroup { DispatchAttempts = new List<DispatchAttempt> { ... } };
// ════════════════════════════════════════════════════════════════════════════

using Manda2.Tests.Dispatch.Feature.Dispatch.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Manda2.Tests.Helpers
{
    // ════════════════════════════════════════════════════════════════════
    // MÉTODO PRINCIPAL
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un Mock&lt;DbSet&lt;T&gt;&gt; a partir de una lista en memoria.
    /// Soporta: FirstOrDefaultAsync, ToListAsync, Where, Add, AddAsync.
    /// NO soporta: Include (se ignora silenciosamente — pobla navegaciones manualmente).
    /// </summary>
    /// <typeparam name="T">Tipo de entidad EF Core.</typeparam>
    /// <param name="data">Lista de entidades que simulan la tabla.</param>
    /// <returns>Mock configurado listo para usar con .Object.</returns>

    public static class MockDbSetHelper
    {
        // ════════════════════════════════════════════════════════════════════
        // MÉTODO PRINCIPAL
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Crea un Mock&lt;DbSet&lt;T&gt;&gt; a partir de una lista en memoria.
        /// Soporta: FirstOrDefaultAsync, ToListAsync, Where, Add, AddAsync.
        /// NO soporta: Include (se ignora silenciosamente — pobla navegaciones manualmente).
        /// </summary>
        /// <typeparam name="T">Tipo de entidad EF Core.</typeparam>
        /// <param name="data">Lista de entidades que simulan la tabla.</param>
        /// <returns>Mock configurado listo para usar con .Object.</returns>
        public static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            // Convertimos la lista a IQueryable para que LINQ funcione
            var queryable = data.AsQueryable();

            var mockSet = new Mock<DbSet<T>>();

            // ── Configurar IQueryable ────
            // EF Core usa estas 4 propiedades internamente para construir queries.
            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(queryable.Provider));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Expression)
                .Returns(queryable.Expression);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.ElementType)
                .Returns(queryable.ElementType);

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(() => queryable.GetEnumerator());

            // ── Configurar IAsyncEnumerable ────
            // Necesario para ToListAsync(), FirstOrDefaultAsync(), etc.
            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            // ── Configurar Add / AddAsync ────
            // Permite que el handler llame a _db.Drivers.Add(entity) sin error.
            mockSet.Setup(m => m.Add(It.IsAny<T>()))
                .Callback<T>(entity => data.Add(entity));

            mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .Callback<T, CancellationToken>((entity, _) => data.Add(entity))
                .ReturnsAsync((T entity, CancellationToken _) =>
                    (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>)null!);

            // ── Configurar Remove ────
            mockSet.Setup(m => m.Remove(It.IsAny<T>()))
                .Callback<T>(entity => data.Remove(entity));

            return mockSet;
        }

        // ════════════════════════════════════════════════════════════════════
        // PROVEEDOR ASÍNCRONO INTERNO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Proveedor de queries asíncronas que envuelve el proveedor LINQ estándar.
        /// Permite que EF Core ejecute FirstOrDefaultAsync, ToListAsync, etc.
        /// sobre una fuente de datos en memoria.
        /// </summary>
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            // Crea una query estándar (usada por Where, OrderBy, etc.)
            public IQueryable CreateQuery(Expression expression)
                => new TestAsyncEnumerable<TEntity>(expression);

            // Crea una query tipada
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
                => new TestAsyncEnumerable<TElement>(expression);

            // Ejecuta una query síncrona (usada internamente por LINQ)
            public object? Execute(Expression expression)
                => _inner.Execute(expression);

            // Ejecuta una query tipada síncrona
            public TResult Execute<TResult>(Expression expression)
                => _inner.Execute<TResult>(expression);

            // ── CLAVE: Ejecuta queries asíncronas de EF Core ────
            // FirstOrDefaultAsync, SingleOrDefaultAsync, CountAsync, etc.
            // llaman a este método internamente.
            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                // TResult es Task<T> — extraemos el tipo T del genérico
                var resultType = typeof(TResult).GetGenericArguments()[0];

                // ── FIX: selección explícita del overload Execute<TResult>(Expression)
                // Evita AmbiguousMatchException que ocurre con GetMethod("Execute", BindingFlags...)
                // porque IQueryProvider tiene dos overloads de Execute.
                var executeMethod = typeof(IQueryProvider)
                    .GetMethods()
                    .First(m =>
                        m.Name == nameof(IQueryProvider.Execute) &&
                        m.IsGenericMethodDefinition)          // solo el overload genérico Execute<T>
                    .MakeGenericMethod(resultType);

                // Ejecutamos la query de forma síncrona sobre la lista en memoria
                var syncResult = executeMethod.Invoke(_inner, new object[] { expression });

                // Envolvemos el resultado en Task.FromResult<T>(result)
                var fromResult = typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType);

                return (TResult)fromResult.Invoke(null, new[] { syncResult })!;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // ENUMERABLE ASÍNCRONO INTERNO
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// IQueryable asíncrono que permite que ToListAsync() funcione
        /// sobre una lista en memoria.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }

            // IQueryable<T>.Provider → devuelve nuestro proveedor asíncrono
            IQueryProvider IQueryable.Provider
                => new TestAsyncQueryProvider<T>(this);

            // IAsyncEnumerable<T>.GetAsyncEnumerator
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        /// <summary>
        /// Enumerador asíncrono que envuelve un IEnumerator&lt;T&gt; síncrono.
        /// Permite que foreach await y ToListAsync() funcionen en tests.
        /// </summary>
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            // Avanza al siguiente elemento de forma "asíncrona" (en realidad síncrona)
            public ValueTask<bool> MoveNextAsync()
                => ValueTask.FromResult(_inner.MoveNext());

            // Elemento actual
            public T Current => _inner.Current;

            // Libera recursos
            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return ValueTask.CompletedTask;
            }
        }
    }

}
