using BExIS.Pmm.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Vaiona.Persistence.Api;
using PlotChartX = BExIS.Pmm.Entities.PlotHistory;

namespace BExIS.Dlm.Services
{
    public class PlotHistoryManager : IDisposable
    {
        #region Attributes
        
        public IReadOnlyRepository<PlotChartX> Repo { get; private set; }

        #endregion

        #region Ctors

        private IUnitOfWork guow = null;
        public PlotHistoryManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            this.Repo = guow.GetReadOnlyRepository<PlotChartX>();
        }

        private bool isDisposed = false;
        ~PlotHistoryManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }




        #endregion

        #region Methods
        public PlotChartX Create(string plotId, string plotType, string latitude, string longitude, string coordinate, string coordinateType, string geometryType, string geometryText, long logedId, String action, DateTime dateTime, string referencePoint = "")
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(plotId));
            Contract.Requires(!string.IsNullOrWhiteSpace(plotType) != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(latitude) != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(longitude) != null);
            Contract.Ensures(Contract.Result<PlotChartX>() != null && Contract.Result<PlotChartX>().Id >= 0);

            //PartyStatus initialStatus = new PartyStatus();
            //initialStatus.Timestamp = DateTime.UtcNow;
            //initialStatus.Description = "Created";
            //initialStatus.StatusType = statusType;

            PlotChartX entity = new PlotChartX()
            {
                PlotId = plotId,
                Latitude = latitude,
                Longitude = longitude,
                GeometryType = geometryType,
                GeometryText = geometryText,
                Coordinate = coordinate,
                CoordinateType = coordinateType,
                Status = 1,
                VersionNo = 1,
                Extra = null,
                PlotType = "",
                LogedId = logedId,
                LogTime = dateTime,
                Action = action,
                ReferencePoint = referencePoint
                //Area = null
            };
            //entity.History.Add(initialStatus);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PlotChartX> repo = uow.GetRepository<PlotChartX>();
                repo.Put(entity); // must store the status objects too
                uow.Commit();
            }
            return (entity);
        }

        public bool Delete(PlotChartX entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PlotChartX> repo = uow.GetRepository<PlotChartX>();
                
                entity = repo.Reload(entity);

                
                //delete the entity
                repo.Delete(entity);

                // commit changes
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }

        public bool Delete(IEnumerable<PlotChartX> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (PlotChartX e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (PlotChartX e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PlotChartX> repo = uow.GetRepository<PlotChartX>();
                
                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);

                    //delete the unit
                    repo.Delete(latest);
                }
                // commit changes
                uow.Commit();
            }
            return (true);
        }

        public PlotChartX Update(PlotChartX entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permanent ID");

            Contract.Ensures(Contract.Result<PlotChartX>() != null && Contract.Result<PlotChartX>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<PlotChartX> repo = uow.GetRepository<PlotChartX>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }


        #endregion

              
        #region Associations
        #endregion
    }
}
