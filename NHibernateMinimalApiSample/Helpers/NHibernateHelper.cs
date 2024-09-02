using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernateMinimalApiSample.Models;
using ISession = NHibernate.ISession;

namespace NHibernateMinimalApiSample.Helpers
{
    public class NHibernateHelper
    {
        private readonly ISessionFactory _sessionFactory;

        public NHibernateHelper(string connectionString)
        {
            var config = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(connectionString))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Product>())
                .BuildConfiguration();

            _sessionFactory = config.BuildSessionFactory();
        }

        public ISessionFactory SessionFactory => _sessionFactory;

        public ISession OpenSession() => _sessionFactory.OpenSession();
    }
}
