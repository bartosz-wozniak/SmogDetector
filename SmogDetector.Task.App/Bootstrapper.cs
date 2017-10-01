using System.Reflection;
using Autofac;
using SmogDetector.DataAccess.IRepositories;
using SmogDetector.DataAccess.Models;
using SmogDetector.DataAccess.Repositories;
using SmogDetector.Task.App.Commands;
using SmogDetector.Task.App.Helpers;
using SmogDetector.Task.Business;

namespace SmogDetector.Task.App
{
    public static class Bootstrapper
    {
        public static IContainer Configure()
        {
            var assembly = Assembly.GetExecutingAssembly();      
            var builder = new ContainerBuilder();
            builder.RegisterType<GenericRepository<ImgwClassification>>().As<IRepository<ImgwClassification>>();
            builder.RegisterType<GenericRepository<ImgwStation>>().As<IRepository<ImgwStation>>();
            builder.RegisterType<GenericRepository<ImgwWeatherData>>().As<IRepository<ImgwWeatherData>>();
            builder.RegisterType<GenericRepository<SmogData>>().As<IRepository<SmogData>>();
            builder.RegisterType<GenericRepository<ProcessedSmogData>>().As<IRepository<ProcessedSmogData>>();
            builder.RegisterType<GenericRepository<ProcessedWeatherData>>().As<IRepository<ProcessedWeatherData>>();
            builder.RegisterType<GenericRepository<NormalizedWeatherData>>().As<IRepository<NormalizedWeatherData>>();
            builder.RegisterType<GenericRepository<KnnDistancesData>>().As<IRepository<KnnDistancesData>>();
            builder.RegisterType<GenericRepository<KnnResultsData>>().As<IRepository<KnnResultsData>>();
            builder.RegisterType<GenericRepository<RandomForestResult>>().As<IRepository<RandomForestResult>>();
            builder.RegisterType<GenericRepository<RandomForestStats>>().As<IRepository<RandomForestStats>>();
            builder.RegisterType<ImgwDataSynchronizer>().As<IImgwDataSynchronizer>();
            builder.RegisterType<SmogDataSynchronizer>().As<ISmogDataSynchronizer>();
            builder.RegisterType<DataNormalizer>().As<IDataNormalizer>();
            builder.RegisterType<KnnComputation>().As<IKnnComputation>();
            builder.RegisterType<RandomForest>().As<IRandomForest>();
            builder.RegisterAssemblyTypes(assembly)
                .Where(CommandHelper.IsCommand)
                .Named<ICommand>(a=>CommandHelper.GetCommandName(a).Name);
            return builder.Build();
        }
    }
}
